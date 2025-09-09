//=============================
// クライアントからサーバーへの通信を管理するスクリプト
// Author:木田晃輔
//=============================

#region using一覧
using MagicOnion.Server.Hubs;
using Microsoft.EntityFrameworkCore;
using NIGHTRAVEL.Server.Model.Context;
using NIGHTRAVEL.Server.StreamingHubs;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using static Shared.Interfaces.StreamingHubs.IRoomHubReceiver;
#endregion

namespace StreamingHubs
{
    public class RoomHub(RoomContextRepository roomContextRepository) : StreamingHubBase<IRoomHub, IRoomHubReceiver>, IRoomHub
    {
        //コンテキスト定義
        private RoomContext roomContext;
        RoomContextRepository roomContextRepos;

        #region 接続・切断処理
        //接続した場合
        protected override ValueTask OnConnected()
        {
            roomContextRepos = roomContextRepository;
            return default;
        }

        // 切断された場合
        protected override ValueTask OnDisconnected()
        {
            // 退室処理を実行
            LeavedAsync(); return CompletedTask;
        }
        #endregion

        #region マッチングしてからゲーム開始までの処理
        /// <summary>
        /// 入室処理
        /// Author:Kida
        /// </summary>
        /// <param name="roomName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Dictionary<Guid, JoinedUser>> JoinedAsync(string roomName, int userId)
        {
            lock (roomContextRepository)
            { //同時に生成しないように排他制御
                // ルームに参加＆ルームを保持
                this.roomContext = roomContextRepository.GetContext(roomName);
                if (this.roomContext == null)
                { //無かったら生成
                    this.roomContext = roomContextRepository.CreateContext(roomName);
                }
                else if (this.roomContext.JoinedUserList.Count == 0)
                { //ルーム情報が入ってかつ参加人数が0人の場合
                    roomContextRepository.RemoveContext(roomName);                      //ルーム情報を削除
                    this.roomContext = roomContextRepository.CreateContext(roomName);   //ルームを生成
                }
            }

            //DBからユーザー情報取得
            GameDbContext dbContext = new GameDbContext();
            var user = dbContext.Users.Where(user => user.Id == userId).First();

            // グループストレージにユーザーデータを格納
            var joinedUser = new JoinedUser() { ConnectionId = this.ConnectionId, UserData = user };

            if (roomContext.JoinedUserList.Count == 0)
            {//roomContext内の参加人数が0である場合

                //参加順番の初期化
                joinedUser.JoinOrder = 1;

                //1人目をマスタークライアントにする
                joinedUser.IsMaster = true;
            }
            else
            {
                //参加順番の設定
                joinedUser.JoinOrder = roomContext.JoinedUserList.Count + 1;
            }

            // ルームコンテキストに参加ユーザーを保存
            this.roomContext.JoinedUserList[this.ConnectionId] = joinedUser;

            //　ルームに参加
            this.roomContext.Group.Add(this.ConnectionId, Client);

            this.roomContext.Group.Except([this.ConnectionId]).Onjoin(roomContext.JoinedUserList[this.ConnectionId]);

            this.roomContext.NowStage = EnumManager.STAGE_TYPE.Rust;

            // 参加中のユーザー情報を返す
            return this.roomContext.JoinedUserList;
        }

        /// <summary>
        /// 退室処理
        /// Author:Kida
        /// </summary>
        /// <returns></returns>
        public async Task LeavedAsync()
        {
            lock (roomContextRepository) // 排他制御
            {
                // Nullチェック入れる
                //　退室するユーザーを取得
                var joinedUser = this.roomContext.JoinedUserList[this.ConnectionId];

                ////マスタークライアントだったら次の人に譲渡する
                if (joinedUser.IsMaster == true)
                {
                    MasterLostAsync(this.ConnectionId);
                    foreach (var user in this.roomContext.JoinedUserList)
                    {
                        if (user.Value.IsMaster == true)
                        {
                            this.roomContext.Group.Only([user.Key]).OnChangeMasterClient();
                        }
                    }
                }

                // ルーム参加者全員に、ユーザーの退室通知を送信
                this.roomContext.Group.All.OnLeave(joinedUser);

                //　ルームから退室
                this.roomContext.Group.Remove(this.ConnectionId);

                //コンテキストからユーザーを削除
                roomContext.RemoveUser(this.ConnectionId);

                // ルームデータから自身のデータを削除
                roomContext.RemoveCharacterData(this.ConnectionId);

            }
        }

        /// <summary>
        /// 準備完了
        /// Author:Nishiura
        /// </summary>
        /// <returns></returns>
        public async Task ReadyAsync(int characterId)
        {
            lock (roomContextRepository) // 排他制御
            {
                bool canStartGame = true; // ゲーム開始可能判定変数

                // 自身のデータを取得
                var joinedUser = roomContext.JoinedUserList[this.ConnectionId];
                joinedUser.IsReady = true; // 準備完了にする
                joinedUser.CharacterID = characterId; //キャラクターIDを保存

                // ルーム参加者全員に、自分が準備完了した通知を送信
                this.roomContext.Group.All.OnReady(this.ConnectionId);

                foreach (var user in this.roomContext.JoinedUserList)
                { // 現在の参加者数分ループ
                    if (user.Value.IsReady != true) canStartGame = false; // もし一人でも準備完了していなかった場合、開始させない
                }
                // 難易度を初期値にする
                this.roomContext.NowDifficulty = 0;
                // ゲームが開始できる場合、開始通知をする
                if (canStartGame) this.roomContext.Group.All.OnStartGame();
            }
        }
        #endregion

        #region ゲーム内での処理

        /// <summary>
        /// プレイヤーの更新
        /// Author:Nishiura
        /// </summary>
        /// <param name="playerData"></param>
        /// <returns></returns>
        public async Task UpdatePlayerAsync(PlayerData playerData)
        {
            lock (roomContextRepository) // 排他制御
            {

                // キャラクターデータリストに自身のデータがない場合
                if (!this.roomContext.characterDataList.ContainsKey(this.ConnectionId))
                {
                    // 新たなキャラクターデータを追加
                    this.roomContext.AddCharacterData(this.ConnectionId, playerData);
                }
                else // 既に存在している場合
                {
                    // キャラクターデータを更新
                    this.roomContext.UpdateCharacterData(this.ConnectionId, playerData);
                }

                // ルームの自分以外に、ユーザ情報通知を送信
                this.roomContext.Group.Except([this.ConnectionId]).OnUpdatePlayer(playerData);
            }
        }

        /// <summary>
        /// マスタークライアントの更新
        /// Author:木田晃輔
        /// </summary>
        /// <param name="masterClientData"></param>
        /// <returns></returns>
        public async Task UpdateMasterClientAsync(MasterClientData masterClientData)
        {
            lock (roomContextRepository) // 排他制御
            {

                // ルームデータから敵のリストを取得し、該当する要素を更新する
                var gottenEnemyDataList = this.roomContext.enemyDataList;
                foreach (var enemyData in masterClientData.EnemyDatas)
                {
                    if (gottenEnemyDataList.ContainsKey(enemyData.UniqueId))
                    {
                        gottenEnemyDataList[enemyData.UniqueId] = enemyData;
                    }
                }

                foreach (var item in masterClientData.GimmickDatas)
                {
                    // すでにルームコンテキストにギミックが含まれている場合
                    if (this.roomContext.gimmickList.ContainsKey(item.GimmickID))
                    {
                        // そのギミックを更新する
                        this.roomContext.gimmickList[item.GimmickID] = item;
                    }
                    else // 含まれていない場合
                    {
                        // そのギミックを追加する
                        this.roomContext.gimmickList.Add(item.GimmickID, item);
                    }
                }

                // キャラクターデータリストに自身のデータがない場合
                if (!this.roomContext.characterDataList.ContainsKey(this.ConnectionId))
                {
                    // 新たなキャラクターデータを追加
                    this.roomContext.AddCharacterData(this.ConnectionId, masterClientData.PlayerData);
                }
                else // 既に存在している場合
                {
                    // キャラクターデータを更新
                    this.roomContext.UpdateCharacterData(this.ConnectionId, masterClientData.PlayerData);
                }

                // ルームの自分以外に、マスタークライアントの状態の更新通知を送信
                this.roomContext.Group.Except([this.ConnectionId]).OnUpdateMasterClient(masterClientData);
            }
        }

        /// <summary>
        /// レアリティ抽選処理
        /// </summary>
        /// <param name="isBoss"></param>
        /// <returns></returns>
        RARITY_TYPE DrawRarity(bool includeBossRarity)
        {
            // レアリティの種類
            Dictionary<RARITY_TYPE, int> raritys = new Dictionary<RARITY_TYPE, int>()
            {
                { RARITY_TYPE.Legend, 2 },
                { RARITY_TYPE.Unique, 6 },
                { RARITY_TYPE.Rare, 12},
                { RARITY_TYPE.Uncommon, 35},
                { RARITY_TYPE.Common, 45 },
                { RARITY_TYPE.Boss, 45 }
            };

            if (includeBossRarity) raritys.Remove(RARITY_TYPE.Common);
            else raritys.Remove(RARITY_TYPE.Boss);

            RARITY_TYPE result = RARITY_TYPE.Common;
            int totalWeight = raritys.Values.Sum();
            int currentWeight = 0;
            int rndPoint = new Random().Next(1, totalWeight);

            foreach(var rarity in raritys)
            {
                currentWeight += rarity.Value;
                if(rndPoint <= currentWeight)
                {
                    result = rarity.Key;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// ステータス強化選択肢を複数取得する
        /// </summary>
        /// <param name="rarity"></param>
        /// <returns></returns>
        List<STAT_UPGRADE_OPTION> DrawStatusUpgrateOption(int elementCnt)
        {
            GameDbContext dbContext = new GameDbContext();
            List <STAT_UPGRADE_OPTION> drawIds = new List<STAT_UPGRADE_OPTION>();

            // 重複なしで指定個数分のステータス強化の選択肢を取得する
            for (int i = 0; i < elementCnt; i++)
            {
                var rarity = DrawRarity(false);
                var option = dbContext.Status_Enhancements.Where(option => !drawIds.Contains((STAT_UPGRADE_OPTION)option.id)).First();
                drawIds.Add((STAT_UPGRADE_OPTION)option.id);
            }

            return drawIds;
        } 

        /// <summary>
        /// レリック抽選処理
        /// </summary>
        /// <param name="rarity"></param>
        /// <returns></returns>
        Relic DrawRelic(RARITY_TYPE rarity)
        {
            GameDbContext dbContext = new GameDbContext();
            List<Relic> ralics = dbContext.Relics.Where(relic => relic.rarity == (int)rarity).ToList();
            return ralics[new Random().Next(0, ralics.Count)];
        }

        /// <summary>
        /// レリックID設定、位置同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="pos">位置</param>
        /// <returns></returns>
        public async Task DropRelicAsync(Stack<Vector2> pos, bool includeBossRarity)
        {
            var result = new Dictionary<string, DropRelicData>();

            // 参加人数分ループ
            for (int i = 0; i < this.roomContext.JoinedUserList.Count; i++)
            {
                var rarity = DrawRarity(includeBossRarity);
                var relic = DrawRelic(rarity);

                // データを更新
                DropRelicData dropRelicData = new DropRelicData();
                dropRelicData.uniqueId = Guid.NewGuid().ToString();
                dropRelicData.Name = relic.name;
                dropRelicData.ExplanationText = relic.explanation;
                dropRelicData.RelicType = (EnumManager.RELIC_TYPE)relic.id;
                dropRelicData.RarityType = rarity;
                dropRelicData.SpawnPos = pos.Pop();

                result.Add(dropRelicData.uniqueId, dropRelicData);

                // ドロップしたレリックリストに追加
                this.roomContext.dropRelicDataList.Add(dropRelicData.uniqueId, dropRelicData);
            }

            // ルーム参加者全員に、レリックのIDと生成位置を送信
            this.roomContext.Group.All.OnDropRelic(result);
        }

        /// <summary>
        /// 敵生成同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemID">敵識別ID</param>
        /// <param name="pos">位置</param>
        /// <returns></returns>
        public async Task SpawnEnemyAsync(List<SpawnEnemyData> spawnEnemyData)
        {
            lock (roomContextRepository) // 排他制御
            {
                for(int i = 0; i <  spawnEnemyData.Count; i++)
                {
                    // 個体識別用のIDを設定
                    spawnEnemyData[i].UniqueId = Guid.NewGuid().ToString();

                    // DBからIDを指定して敵を取得
                    GameDbContext dbContext = new GameDbContext();
                    var enemy = dbContext.Enemies.Where(enemy => enemy.id == (int)spawnEnemyData[i].TypeId).First();

                    // 設定した情報をルームデータに保存
                    this.roomContext.SetEnemyData(spawnEnemyData[i].UniqueId, enemy);
                }
            }

            // 自分以外に、取得した敵情報と生成位置を送信
            this.roomContext.Group.All.OnSpawnEnemy(spawnEnemyData);
        }

        /// <summary>
        /// ギミック起動同期処理
        /// Autho:Nishiura
        /// </summary>
        /// <param name="gimID">ギミック識別ID</param>
        /// <returns></returns>
        public async Task BootGimmickAsync(int gimID)
        {
            lock (roomContextRepository)
            {
                // 対象ギミックが存在しているかつ起動可能である場合
                if (this.roomContext.gimmickList[gimID] != null && !this.roomContext.gimmickList[gimID].IsActivated)
                {
                    this.roomContext.gimmickList[gimID].IsActivated = true;

                    // 参加者全員にギミック情報を通知
                    this.roomContext.Group.All.OnBootGimmick(gimID);
                }
            }
        }

        /// <summary>
        /// 難易度上昇同期処理
        /// Autho:Nishiura
        /// </summary>
        /// <returns></returns>
        public async Task AscendDifficultyAsync()
        {
            this.roomContext.NowDifficulty++;
            // 参加者全員に難易度の上昇を通知
            this.roomContext.Group.All.OnAscendDifficulty(this.roomContext.NowDifficulty);
        }

        /// <summary>
        /// 次ステージ進行同期処理
        /// Autho:Nishiura
        /// </summary>
        /// <param name="conID">接続ID</param>
        /// <param name="isAdvance">ステージ進行判定</param>
        /// <returns></returns>
        public async Task StageClear(bool isAdvance)
        {
            // すでに申請済みの場合処理しない
            if (this.roomContext.isAdvanceRequest == true) return;
            lock (roomContextRepository) // 排他制御
            {
                // 進行申請を申請済みにするにする
                this.roomContext.isAdvanceRequest = true;

                if (isAdvance)
                {
                    if((int)this.roomContext.NowStage == 3)
                    {
                        this.roomContext.NowStage = EnumManager.STAGE_TYPE.Rust;
                    }else this.roomContext.NowStage++; // 現在のステージを加算

                    // 獲得したアイテムリストをクリア
                    this.roomContext.gottenItemList.Clear();

                    // 起動した端末リストをクリア
                    this.roomContext.bootedTerminalList.Clear();

                    // 成功した端末リストをクリア
                    this.roomContext.succededTerminalList.Clear();

                    // 生成した敵のリストを初期化
                    this.roomContext.enemyDataList.Clear();

                    // 参加者全員にステージの進行を通知
                    this.roomContext.Group.All.OnAdanceNextStage(isAdvance, this.roomContext.NowStage);

                    // 各進行判定変数の値をfalseにする
                    this.roomContext.JoinedUserList[this.ConnectionId].IsAdvance = false;
                }
                else
                {
                    // ゲーム終了を全員に通知
                    Result();
                }
            }
        }

        /// <summary>
        /// ステージ進行完了同期処理
        /// </summary>
        /// <param name="conID">接続ID</param>
        /// <param name="isAdvance">ステージ進行判定</param>
        /// <returns></returns>
        public async Task AdvancedStageAsync()
        {
            lock (roomContextRepository)
            {
                bool canAdvenceStage = true; // ステージ進行済み判定変数

                // 自身のデータを取得
                var joinedUser = roomContext.JoinedUserList[this.ConnectionId];
                joinedUser.IsAdvance = true; // 準備完了にする

                foreach (var user in this.roomContext.JoinedUserList)
                { // 現在の参加者数分ループ
                    if (user.Value.IsAdvance != true) canAdvenceStage = false; // もし一人でも準備完了していなかった場合、進行させない
                }

                // 進行できる場合、進行通知をする
                if (canAdvenceStage)
                {
                    this.roomContext.Group.All.OnSameStart();

                    joinedUser.IsAdvance = false; // 準備完了を解除する
                    canAdvenceStage = false;
                    roomContext.isAdvanceRequest = false;
                }
            }
        }

        /// <summary>
        /// 敵体力増減同期処理
        /// Autho:Nishiura
        /// </summary>
        /// <param name="enemID">敵識別ID</param>
        /// <param name="conID">ダメージを与えたPLの接続ID</param>
        /// <param name="giverATK">PLの攻撃力</param>
        /// <param name="debuffType">デバフの種類</param>
        /// <returns></returns>
        public async Task EnemyHealthAsync(string enemID, float giverATK, List<EnumManager.DEBUFF_TYPE> debuffType)
        {
            lock (roomContextRepository) // 排他制御
            {
                // ID指定で敵情報を取得
                var enemData = this.roomContext.GetEnemyData(enemID);
                if (enemData.State.hp <= 0) return;   // すでに対象の敵HPが0の場合は処理しない

                // 受け取った情報でダメージ計算をする
                enemData.State.hp -= (int)((giverATK / 2) - (enemData.State.defence / 4));

                // 敵被弾データを新しく作成
                EnemyDamegeData enemDmgData = new EnemyDamegeData();

                // 作成したデータに各情報を代入
                enemDmgData.AttackerId = this.ConnectionId; // 攻撃者ID
                enemDmgData.Damage = (int)((giverATK / 2) - (enemData.State.defence / 4));  // 付与ダメージ
                enemDmgData.HitEnemyId = enemID;    // 被弾敵ID
                enemDmgData.RemainingHp = enemData.State.hp;    // HP残量
                enemDmgData.DebuffList = debuffType;    // 付与デバフ

                // 合計付与ダメージを加算
                this.roomContext.totalGaveDamage += enemDmgData.Damage;

                // 敵のHPが0以下になった場合
                if (enemDmgData.RemainingHp <= 0)
                {
                    float addExpRate = this.roomContext.playerStatusDataList[this.ConnectionId].Item2.AddExpRate;   // 獲得可能経験値倍率
                    this.roomContext.ExpManager.nowExp += enemData.Exp + (int)(enemData.Exp * addExpRate); // 被弾クラスにExpを代入
                    // 合計キル数を加算
                    this.roomContext.totalKillCount++;

                    // 所持経験値が必要経験値に満ちた場合
                    if (this.roomContext.ExpManager.nowExp >= this.roomContext.ExpManager.RequiredExp)
                    {
                        LevelUp(roomContext.ExpManager); // レベルアップ処理
                    }
                }

                // 自分以外の参加者に受け取ったIDの敵が受け取ったHPになったことを通知
                this.roomContext.Group.All.OnEnemyHealth(enemDmgData);
            }
        }

        /// <summary>
        /// 敵の被ダメージ同期処理   プレイヤーによるダメージ以外
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemID">敵識別ID</param>
        /// <param name="dmgAmount">ダメージ量</param>
        /// <returns></returns>
        public async Task ApplyDamageToEnemyAsync(string enemID, int dmgAmount)
        {
            lock (roomContextRepository) // 排他制御
            {
                // 敵被弾データを新しく作成
                EnemyDamegeData enemDmgData = new EnemyDamegeData();

                // ID指定で敵情報を取得
                var enemData = this.roomContext.GetEnemyData(enemID);
                if (enemData.State.hp <= 0) return;   // すでに対象の敵HPが0の場合は処理しない

                // 現在のHPを受け取ったダメージ量分減算
                enemData.State.hp -= dmgAmount;

                enemDmgData.Damage = dmgAmount;  // 付与ダメージ
                enemDmgData.HitEnemyId = enemID;    // 被弾敵ID
                enemDmgData.RemainingHp = enemData.State.hp;    // HP残量

                if (enemDmgData.RemainingHp <= 0)
                {
                    this.roomContext.ExpManager.nowExp += enemData.Exp; // 被弾クラスにExpを代入

                    // 所持経験値が必要経験値に満ちた場合
                    if (this.roomContext.ExpManager.nowExp >= this.roomContext.ExpManager.RequiredExp)
                    {
                        LevelUp(roomContext.ExpManager); // レベルアップ処理
                    }
                }

                // 参加者全員に受け取ったIDの敵が受け取ったHPになったことを通知
                this.roomContext.Group.All.OnEnemyHealth(enemDmgData);
            }
        }

        /// <summary>
        /// プレイヤー死亡同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID">プレイヤーID</param>
        /// <returns></returns>
        public async Task PlayerDeadAsync()
        {
            lock (roomContextRepository) // 排他制御
            {
                // 蘇生アイテムを持っているかチェック
                var relicStatusData = this.roomContext.playerStatusDataList[this.ConnectionId].Item2;
                if (relicStatusData.BuckupHDMICnt > 0)
                {
                    relicStatusData.BuckupHDMICnt--;
                    this.roomContext.characterDataList[this.ConnectionId].State = this.roomContext.characterDataList[this.ConnectionId].Status;
                    return;
                }

                // 全滅判定変数
                bool isAllDead = true;
                // ルームデータから接続IDを指定して自身のデータを取得
                var playerData = this.roomContext.GetPlayerData(this.ConnectionId);
                playerData.IsDead = true; // 死亡判定をtrueにする

                // 死亡者以外の参加者全員に対象者が死亡したことを通知
                this.roomContext.Group.Except([this.ConnectionId]).OnPlayerDead(this.ConnectionId);

                foreach (var player in this.roomContext.characterDataList)
                {
                    if (player.Value.IsDead == false) // もし誰かが生きていた場合
                    {
                        isAllDead = false;
                        break;
                    }
                }

                // 全滅した場合、ゲーム終了通知を全員に出す
                if (isAllDead) Result();
            }
        }

        /// <summary>
        /// 端末起動同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="termID">端末識別ID</param>
        /// <returns></returns>
        public async Task BootTerminalAsync(int termID)
        {
            // 受け取った端末が起動済みである場合処理しない
            if (this.roomContext.bootedTerminalList.Contains(termID)) return;

            // 起動済み端末リストに入れる
            this.roomContext.bootedTerminalList.Add(termID);

            // 参加者全員に端末の起動を通知
            this.roomContext.Group.All.OnBootTerminal(termID);
        }

        /// <summary>
        /// 端末成功同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="termID">端末識別ID</param>
        /// <param name="result">端末結果</param>
        /// <returns></returns>
        public async Task TerminalsResultAsync(int termID, bool result)
        {
            // 受け取った端末がクリア済みである場合処理しない
            if (this.roomContext.succededTerminalList.Contains(termID)) return;

            if (result == true) // クリアの場合
            {
                // クリア済みとしてリストに入れる
                this.roomContext.succededTerminalList.Add(termID);

                // 参加者全員に端末の結果を通知
                this.roomContext.Group.All.OnTerminalsResult(termID, result);
            }
            else if (result == false) // 失敗の場合
            {
                // 参加者全員に端末の結果を通知
                this.roomContext.Group.All.OnTerminalsResult(termID, result);
            }
        }

        /// <summary>
        /// アイテム獲得同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="itemType">アイテムの種類</param>
        /// <param name="itemID">識別ID(文字列)</param>
        /// <returns></returns>
        public async Task GetItemAsync(EnumManager.ITEM_TYPE itemType, string itemID)
        {
            lock (roomContextRepository) // 排他制御
            {
                // すでに取得済みである場合、処理しない
                if (this.roomContext.gottenItemList.Contains(itemID)) return;

                switch (itemType)   // アイテムの種類に応じて処理を分ける
                {
                    case EnumManager.ITEM_TYPE.Relic:       // レリックの場合
                        var relic = this.roomContext.dropRelicDataList[itemID];

                        //DBからレリック情報取得
                        GameDbContext dbContext = new GameDbContext();
                        var relicData = dbContext.Relics.Where(data => data.id == (int)relic.RelicType).First();

                        // 取得したレリックをリストに入れる
                        this.roomContext.relicDataList.Add(relicData);

                        // レリック強化を付与
                        GetStatusWithRelics();
                        break;

                    case EnumManager.ITEM_TYPE.DataCube:    // データキューブの場合
                        this.roomContext.ExpManager.nowExp += (int)(this.roomContext.ExpManager.RequiredExp * 0.05f); // 要求経験値の5%を渡す
                        break;
                    
                    case EnumManager.ITEM_TYPE.DataBox:     // データボックスの場合
                        this.roomContext.ExpManager.nowExp += (int)(this.roomContext.ExpManager.RequiredExp * 0.25f); // 要求経験値の25%を渡す
                        break;
                }

                // 取得済みアイテムリストに入れる
                this.roomContext.gottenItemList.Add(itemID);

                // アイテムの獲得を全員に通知
                this.roomContext.Group.All.OnGetItem(this.ConnectionId, itemID);
            }
        }

        /// <summary>
        /// ステータス強化選択
        /// </summary>
        /// <param name="conID">接続ID</param>
        /// <param name="upgradeOpt">強化項目</param>
        /// <returns></returns>
        public async Task ChooseUpgrade(EnumManager.STAT_UPGRADE_OPTION upgradeOpt)
        {
            // ルームデータから接続IDを指定して最大ステータスのインスタンス取得
            var status = this.roomContext.playerStatusDataList[this.ConnectionId].Item1;

            //DBからステータス強化選択肢情報取得
            GameDbContext dbContext = new GameDbContext();
            var upgrade = dbContext.Status_Enhancements.Where(status => status.id == (int)upgradeOpt).First();

            switch (upgrade.type)   // 各強化をタイプで識別
            {
                case (int)EnumManager.STATUS_TYPE.HP:   // 体力の場合
                    // 第1効果
                    status.hp += (int)upgrade.const_effect1;
                    status.hp *= (int)upgrade.rate_effect1;
                    // 第2効果
                    status.hp += (int)upgrade.const_effect2;
                    status.hp *= (int)upgrade.rate_effect2;

                    if(status.hp <= 0)status.hp = 1; // HPが0を下回った場合、1にする
                    break;

                case (int)EnumManager.STATUS_TYPE.Power:    // 攻撃力の場合
                    // 第1効果
                    status.power += (int)upgrade.const_effect1;
                    status.power *= (int)upgrade.rate_effect1;
                    // 第2効果
                    status.power += (int)upgrade.const_effect2;
                    status.power *= (int)upgrade.rate_effect2;

                    if (status.power <= 0) status.power = 1; // 攻撃力が0を下回った場合、1にする
                    break;
                
                case (int)EnumManager.STATUS_TYPE.Defense:  // 防御力の場合
                    // 第1効果
                    status.defence += (int)upgrade.const_effect1;
                    status.defence *= (int)upgrade.rate_effect1;
                    // 第2効果
                    status.defence += (int)upgrade.const_effect2;
                    status.defence *= (int)upgrade.rate_effect2;

                    if (status.defence <= 0) status.defence = 1; // 防御力が0を下回った場合、1にする
                    break;
                
                case (int)EnumManager.STATUS_TYPE.JumpPower:    // ジャンプ力の場合
                    // 第1効果
                    status.jumpPower += (int)upgrade.const_effect1;
                    status.jumpPower *= (int)upgrade.rate_effect1;
                    // 第2効果
                    status.jumpPower += (int)upgrade.const_effect2;
                    status.jumpPower *= (int)upgrade.rate_effect2;

                    if (status.jumpPower <= 0) status.jumpPower = 1; // 跳躍力が0を下回った場合、1にする
                    break;
                
                case (int)EnumManager.STATUS_TYPE.MoveSpeed:    // 移動速度の場合
                    // 第1効果
                    status.moveSpeed += (int)upgrade.const_effect1;
                    status.moveSpeed *= (int)upgrade.rate_effect1;
                    // 第2効果
                    status.moveSpeed += (int)upgrade.const_effect2;
                    status.moveSpeed *= (int)upgrade.rate_effect2;

                    if (status.moveSpeed <= 0) status.moveSpeed = 1; // 移動速度が0を下回った場合、1にする
                    break;
                
                case (int)EnumManager.STATUS_TYPE.HealRate: // 自動回復速度の場合
                    // 第1効果
                    status.healRate += (int)upgrade.const_effect1;
                    status.healRate *= (int)upgrade.rate_effect1;
                    // 第2効果
                    status.healRate += (int)upgrade.const_effect2;
                    status.healRate *= (int)upgrade.rate_effect2;

                    if (status.healRate <= 0) status.healRate = 0.001f; // 自動回復速度が0を下回った場合、1にする
                    break;
                
                case (int)EnumManager.STATUS_TYPE.AttackSpeedFactor:    // 攻撃速度の場合
                    // 第1効果
                    status.attackSpeedFactor += (int)upgrade.const_effect1;
                    status.attackSpeedFactor *= (int)upgrade.rate_effect1;
                    // 第2効果
                    status.attackSpeedFactor += (int)upgrade.const_effect2;
                    status.attackSpeedFactor *= (int)upgrade.rate_effect2;

                    if (status.attackSpeedFactor <= 0) status.attackSpeedFactor = 0.1f; // 攻撃速度が0を下回った場合、1にする
                    break;
            }

            GetStatusWithRelics();
        }

        /// <summary>
        /// 弾発射同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="spawnPos">生成位置</param>
        /// <param name="shootVec">発射ベクトル</param>
        public async Task ShootBulletAsync(EnumManager.PROJECTILE_TYPE type, List<EnumManager.DEBUFF_TYPE> debuffs, int power, Vector2 spawnPos, Vector2 shootVec, Quaternion rotation)
        {
            // 参加者全員に端末の結果を通知
            this.roomContext.Group.All.OnShootBullet(type, debuffs, power, spawnPos, shootVec, rotation);
        }

        #endregion

        /// <summary>
        /// マスタークライアント譲渡処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID"></param>
        /// <returns></returns>
        void MasterLostAsync(Guid conID)
        {
            // 参加者リストをループ
            foreach (var user in this.roomContext.JoinedUserList)
            {
                // 対象がマスタークライアントでない場合
                if (user.Value.IsMaster == false)
                {
                    // その対象をマスタークライアントとし、通知を送る。ループを抜ける
                    user.Value.IsMaster = true;
                    //this.roomContext.Group.Only([user.Key]).OnChangeMasterClient();
                    break;
                }
            }

            // マスタークライアントを剥奪
            this.roomContext.JoinedUserList[conID].IsMaster = false;
        }

        /// <summary>
        /// レリックを適用させたステータスに加工して通知する
        /// </summary>
        void GetStatusWithRelics()
        {
            var statusData = this.roomContext.playerStatusDataList[this.ConnectionId];
            CharacterStatusData status = new CharacterStatusData(statusData.Item1);     // 実際に更新はしない
            PlayerRelicStatusData relicStatus = statusData.Item2;   // インスタンス取得

            // ここで所持レリックを基にresultDataを更新する
            foreach (var relic in this.roomContext.relicDataList)
            {
                if (relic.status_type <= (int)STATUS_TYPE.HealRate) // タイプがステータス上昇の場合
                {
                    // タイプ別ステータス上昇
                    switch (relic.status_type)
                    {
                        case (int)STATUS_TYPE.HP:                   // HPの場合
                            status.hp += relic.const_effect;
                            status.hp += (int)(status.hp * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.Defense:              // 防御力の場合
                            status.defence += relic.const_effect;
                            status.defence += (int)(status.defence * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.Power:                // 攻撃力の場合
                            status.power += relic.const_effect;
                            status.power += (int)(status.power * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.JumpPower:            // 跳躍力の場合
                            status.jumpPower += relic.const_effect;
                            status.jumpPower += (int)(status.jumpPower * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.MoveSpeed:            // 移動速度の場合
                            status.moveSpeed += relic.const_effect;
                            status.moveSpeed += (int)(status.moveSpeed * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.AttackSpeedFactor:    // 攻撃速度の場合
                            status.attackSpeedFactor += relic.const_effect;
                            status.attackSpeedFactor += (int)(status.attackSpeedFactor * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.HealRate:             // 自動回復速度の場合
                            status.healRate += relic.const_effect;
                            status.healRate += (int)(status.healRate * relic.rate_effect);
                            break;
                    }
                }
                else // タイプがレリックステータスの場合
                {
                    switch (relic.status_type)
                    {
                        case (int)STATUS_TYPE.ScatterBugCnt:    //スキャッターバグの場合
                            relicStatus.ScatterBugCnt += relic.const_effect;
                            relicStatus.ScatterBugCnt += (int)(relicStatus.ScatterBugCnt * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.DigitalMeatCnt:   // デジタルミートの場合
                            relicStatus.DigitalMeatCnt += relic.const_effect;
                            relicStatus.DigitalMeatCnt += (int)(relicStatus.DigitalMeatCnt * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.BuckupHDMICnt:    // バックアップHDMIの場合
                            relicStatus.BuckupHDMICnt += relic.const_effect;
                            relicStatus.BuckupHDMICnt += (int)(relicStatus.BuckupHDMICnt * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.ChargedCoreCnt:   // 感電オーブの場合
                            relicStatus.ChargedCoreCnt += relic.const_effect;
                            relicStatus.ChargedCoreCnt += (int)(relicStatus.ChargedCoreCnt * relic.rate_effect);
                            break;

                        case (int)DEBUFF_TYPE.Burn:                 // 炎上確率の場合
                            relicStatus.GiveDebuffRates[DEBUFF_TYPE.Burn] += relic.const_effect;
                            relicStatus.GiveDebuffRates[DEBUFF_TYPE.Burn] += (int)(relicStatus.GiveDebuffRates[DEBUFF_TYPE.Burn] * relic.rate_effect);
                            break;

                        case (int)DEBUFF_TYPE.Freeze:               // 凍結確率の場合
                            relicStatus.GiveDebuffRates[DEBUFF_TYPE.Freeze] += relic.const_effect;
                            relicStatus.GiveDebuffRates[DEBUFF_TYPE.Freeze] += (int)(relicStatus.GiveDebuffRates[DEBUFF_TYPE.Freeze] * relic.rate_effect);
                            break;

                        case (int)DEBUFF_TYPE.Shock:                // 感電確率の場合
                            relicStatus.GiveDebuffRates[DEBUFF_TYPE.Shock] += relic.const_effect;
                            relicStatus.GiveDebuffRates[DEBUFF_TYPE.Shock] += (int)(relicStatus.GiveDebuffRates[DEBUFF_TYPE.Shock] * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.AddExpRate:           // 付与経験値率の場合
                            relicStatus.AddExpRate += relic.const_effect;
                            relicStatus.AddExpRate += (int)(relicStatus.AddExpRate * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.RegainCodeRate:       // 与ダメージ回復率の場合
                            relicStatus.RegainCodeRate += relic.const_effect;
                            relicStatus.RegainCodeRate += (int)(relicStatus.RegainCodeRate * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.HolographicArmorRate: // 回避率の場合
                            relicStatus.HolographicArmorRate += relic.const_effect;
                            relicStatus.HolographicArmorRate += (int)(relicStatus.HolographicArmorRate * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.MouseRate:            // クールダウン短縮率の場合
                            relicStatus.MouseRate += relic.const_effect;
                            relicStatus.MouseRate += (int)(relicStatus.MouseRate * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.FirewallRate:         // 被ダメ軽減率の場合
                            relicStatus.FirewallRate += relic.const_effect;
                            relicStatus.FirewallRate += (int)(relicStatus.FirewallRate * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.LifeScavengerRate:    // キル時HP回復率の場合
                            relicStatus.LifeScavengerRate += relic.const_effect;
                            relicStatus.LifeScavengerRate += (int)(relicStatus.LifeScavengerRate * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.RugrouterRate:        // DA率の場合
                            relicStatus.RugrouterRate += relic.const_effect;
                            relicStatus.RugrouterRate += (int)(relicStatus.RugrouterRate * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.IdentificationAIRate: // デバフ的に対するダメUP率の場合
                            relicStatus.IdentificationAIRate += relic.const_effect;
                            relicStatus.IdentificationAIRate += (int)(relicStatus.IdentificationAIRate * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.DanborDollRate:       // 防御貫通率の場合
                            relicStatus.DanborDollRate += relic.const_effect;
                            relicStatus.DanborDollRate += (int)(relicStatus.DanborDollRate * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.IllegalScriptRate:    // クリティカルオーバーキル発生率の場合
                            relicStatus.IllegalScriptRate += relic.const_effect;
                            relicStatus.IllegalScriptRate += (int)(relicStatus.IllegalScriptRate * relic.rate_effect);
                            break;

                    }
                }
            }

            // 最新の状態に更新する
            statusData.Item2 = relicStatus;

            // 基のステータスにレリックを適用したステータスをリクエスト者に通知
            this.roomContext.Group.Except([this.ConnectionId]).OnUpdateStatus(status, relicStatus);
        }

        // <summary>
        /// レベルアップ処理
        /// Author:Nishiura
        /// </summary>
        protected void LevelUp(ExpManager expManager)
        {
            // 強化選択肢リストを初期化
            this.roomContext.statusOptionList.Clear();

            // レベルアップ処理
            expManager.Level++; // 現在のレベルを上げる
            expManager.nowExp = expManager.nowExp - expManager.RequiredExp;    // 超過した分の経験値を現在の経験値量として保管

            // 次のレベルまで必要な経験値量を計算 （必要な経験値量 = 次のレベルの3乗 - 今のレベルの3乗）
            expManager.RequiredExp = (int)Math.Pow(expManager.Level + 1, 3) - (int)Math.Pow(expManager.Level, 3);

            // 強化選択肢格納リスト
            List<STAT_UPGRADE_OPTION> statusOptionList = DrawStatusUpgrateOption(3);

            // 強化後ステータス格納リスト
            Dictionary<Guid, CharacterStatusData> characterStatusDataList = new Dictionary<Guid, CharacterStatusData>();

            // 参加者リストをループ
            foreach (var user in this.roomContext.JoinedUserList)
            {
                // 参加者リストのキーから接続IDを受け取り対応ユーザのデータを取得
                var playerData = this.roomContext.playerStatusDataList[user.Key].Item1;

                // 各最大値を10%増加(仮)
                playerData.hp = (int)(playerData.hp * 1.1f);
                playerData.power = (int)(playerData.power * 1.1f);
                playerData.defence = (int)(playerData.defence * 1.1f);
                playerData.jumpPower *= 1.1f;
                playerData.moveSpeed *= 1.1f;
                playerData.healRate *= 1.1f;

                // 強化後のステータスをGuidをキーにして格納
                characterStatusDataList.Add(user.Key, playerData);
            }

            // 参加者全員にレベルアップしたことを通知
            this.roomContext.Group.All.OnLevelUp(expManager.Level, expManager.nowExp, characterStatusDataList, this.roomContext.statusOptionList);
        }

        /// <summary>
        /// リザルト作成処理
        /// Author:Nishiura
        /// </summary>
        /// <returns></returns>
        public async void Result()
        {
            ResultData resultData = new ResultData();
            var playerData = this.roomContext.GetPlayerData(this.ConnectionId);

            // 必要なデータを代入
            resultData.Difficulty = this.roomContext.NowDifficulty; // 難易度
            resultData.Level = this.roomContext.ExpManager.Level;   //レベル
            resultData.AliveTime = 66666;   // 生存時間(仮)
            resultData.PlayerClass = playerData.Class;  // プレイヤーのクラス
            resultData.TotalGottenItem = this.roomContext.gottenItemList.Count; // 総獲得アイテム数
            resultData.TotalActivedTerminal = this.roomContext.bootedTerminalList.Count;    // 総起動端末数
            resultData.TotalGaveDamage = this.roomContext.totalGaveDamage;  // 総付与ダメージ数
            resultData.EnemyKillCount = this.roomContext.totalKillCount;    // 総キルカウント
            resultData.GottenRelicList = this.roomContext.relicDataList;    // 獲得レリックリスト
            resultData.TotalReceivedDamage = this.roomContext.totalGainDamage;  // 合計被弾値

            this.roomContext.Group.All.OnGameEnd(resultData);
        }
    }
}
