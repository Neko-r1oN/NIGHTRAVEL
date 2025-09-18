//=============================
// クライアントからサーバーへの通信を管理するスクリプト
// Author:木田晃輔
//=============================

#region using一覧
using MagicOnion.Server.Hubs;
using Microsoft.EntityFrameworkCore;
using NIGHTRAVEL.Server.Model.Context;
using NIGHTRAVEL.Server.Services;
using NIGHTRAVEL.Server.StreamingHubs;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using Swashbuckle.AspNetCore.SwaggerGen;
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
        Room room = new Room();
        RoomService roomService = new RoomService();

        // ターミナル関連定数 (MAXの値はRandで用いるため、上限+1の数)
        private const int MIN_TERMINAL_NUM = 3;
        private const int MAX_TERMINAL_NUM = 7;
        private const int MIN_TERMINAL_ID = 1;
        private const int MAX_TERMINAL_ID = 6;

        // レリック関連定数
        private const int MAX_DAMAGE = 99999; 

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

                GameDbContext dbContext = new GameDbContext();
                //DBからユーザー情報取得
                var user = dbContext.Users.Where(user => user.Id == userId).First();


                // ルームに参加＆ルームを保持
                this.roomContext = roomContextRepository.GetContext(roomName);
                if (this.roomContext == null)
                { //無かったら生成
                    this.roomContext = roomContextRepository.CreateContext(roomName);
                    //DBに生成
                    room.roomName = roomName;
                    room.userName = user.Name;
                    roomService.RegistRoom(room.roomName, room.userName);
                }
                else if (this.roomContext.JoinedUserList.Count == 0)
                { //ルーム情報が入ってかつ参加人数が0人の場合
                    roomContextRepository.RemoveContext(roomName);                      //ルーム情報を削除
                    this.roomContext = roomContextRepository.CreateContext(roomName);   //ルームを生成
                    //DBに生成
                    room.roomName = roomName;
                    room.userName = user.Name;
                    roomService.RegistRoom(room.roomName, room.userName);
                }

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

                this.roomContext.resultDataList.Add(this.ConnectionId, new ResultData());

                
                
               
                this.roomContext.Group.Add(this.ConnectionId, Client);

                
                this.roomContext.Group.Only([this.ConnectionId]).OnRoom();
                
                //　ルームに参加
                this.roomContext.Group.Except([this.ConnectionId]).Onjoin(roomContext.JoinedUserList[this.ConnectionId]);

                this.roomContext.NowStage = EnumManager.STAGE_TYPE.Rust;

                // 参加中のユーザー情報を返す
                return this.roomContext.JoinedUserList;
            }
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
                if (this.roomContext==null) return;

                GameDbContext context = new GameDbContext();

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

                //最後の1人の場合ルームを削除
                if(this.roomContext.JoinedUserList.Count == 1)
                {
                    roomService.RemoveRoom(this.roomContext.Name);
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
                this.roomContext.Group.All.OnReady(joinedUser);

                foreach (var user in this.roomContext.JoinedUserList)
                { // 現在の参加者数分ループ
                    if (user.Value.IsReady != true) canStartGame = false; // もし一人でも準備完了していなかった場合、開始させない
                }
                // 難易度を初期値にする
                this.roomContext.NowDifficulty = 0;

                // ゲームが開始できる場合、開始通知をする
                if (canStartGame)
                {
                    this.roomContext.Group.All.OnStartGame();

                    // 現在時刻を代入
                    this.roomContext.startTime = DateTime.Now;
                }
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

                // ルームデータから端末情報を取得し、アクティブ状態の端末を更新
                roomContext.terminalList = masterClientData.TerminalDatas; 

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
        List<StatusUpgrateOptionData> DrawStatusUpgrateOption(int elementCnt)
        {
            GameDbContext dbContext = new GameDbContext();
            List <STAT_UPGRADE_OPTION> drawIds = new List<STAT_UPGRADE_OPTION>();
            List<StatusUpgrateOptionData> result = new List<StatusUpgrateOptionData>();

            // 重複なしで指定個数分のステータス強化の選択肢を取得する
            for (int i = 0; i < elementCnt; i++)
            {
                var rarity = DrawRarity(false);
                var option = dbContext.Status_Enhancements.Where(option => !drawIds.Contains((STAT_UPGRADE_OPTION)option.id)).First();

                var createData = new StatusUpgrateOptionData()
                {
                    TypeId = (STAT_UPGRADE_OPTION)option.id,
                    Name = option.name,
                    Rarity = (RARITY_TYPE)option.rarity,
                    Explanation = option.explanation,
                    StatusType = (STATUS_TYPE)option.type
                };
                result.Add(createData);
                drawIds.Add((STAT_UPGRADE_OPTION)option.id);
            }

            return result;
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
                for (int i = 0; i < spawnEnemyData.Count; i++)
                {
                    // 個体識別用のIDを設定
                    spawnEnemyData[i].UniqueId = Guid.NewGuid().ToString();

                    // DBからIDを指定して敵を取得
                    GameDbContext dbContext = new GameDbContext();
                    var enemy = dbContext.Enemies.Where(enemy => enemy.id == (int)spawnEnemyData[i].TypeId).First();

                    // 設定した情報をルームデータに保存
                    this.roomContext.SetEnemyData(spawnEnemyData[i].UniqueId, enemy);

                    // 端末IDが設定されている場合は、その端末の生成した敵リストに追加
                    if (spawnEnemyData[i].TerminalID != -1)
                    {
                        var terminal = this.roomContext.terminalList.Where(term => term.ID == spawnEnemyData[i].TerminalID).First();
                        terminal.EnemyList.Add(spawnEnemyData[i].UniqueId);
                    }
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
        public async Task BootGimmickAsync(int gimID, bool triggerOnce)
        {
            lock (roomContextRepository)
            {
                // 対象ギミックが存在しているかつ起動可能である場合
                if (this.roomContext.gimmickList.ContainsKey(gimID))
                {
                    if (triggerOnce)
                    {
                        this.roomContext.gimmickList.Remove(gimID);
                    }

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
                this.roomContext.totalClearStageCount++;

                if (isAdvance)
                {
                    if((int)this.roomContext.NowStage == 3)
                    {
                        this.roomContext.NowStage = STAGE_TYPE.Rust;
                    }else this.roomContext.NowStage++; // 現在のステージを加算

                    // 獲得したアイテムリストをクリア
                    this.roomContext.gottenItemList.Clear();

                    // 起動した端末リストをクリア
                    this.roomContext.bootedTerminalList.Clear();

                    // 成功した端末リストをクリア
                    this.roomContext.succededTerminalList.Clear();

                    // 生成した端末リストをクリア
                    this.roomContext.terminalList.Clear();

                    // 生成した敵のリストを初期化
                    this.roomContext.enemyDataList.Clear();

                    // 参加者全員にステージの進行を通知
                    this.roomContext.Group.All.OnAdanceNextStage(this.roomContext.NowStage);

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
                    // 端末データを抽選・保存
                    var terminals = LotteryTerminal();
                    roomContext.terminalList = terminals;

                    // 同期開始通知と共に端末データを送信
                    this.roomContext.Group.All.OnSameStart(terminals);

                    joinedUser.IsAdvance = false; // 準備完了を解除する
                    canAdvenceStage = false;
                    roomContext.isAdvanceRequest = false;
                }
            }
        }

        /// <summary>
        /// オブジェクト生成処理
        /// </summary>
        /// <returns></returns>
        public async Task SpawnObjectAsync(OBJECT_TYPE type, Vector2 spawnPos)
        {
            lock (roomContextRepository)
            {
                int uniqueId = this.roomContext.gimmickList.Count;
                GimmickData gimmickData = new GimmickData()
                {
                    GimmickID = this.roomContext.gimmickList.Count,
                    Position = spawnPos,
                };
                this.roomContext.gimmickList.Add(uniqueId, gimmickData);
                this.roomContext.Group.All.OnSpawnObject(type, spawnPos, uniqueId);
            }
        }

        /// <summary>
        /// 端末データ抽選処理
        /// Autho:Nakamoto
        /// </summary>
        /// <returns></returns>
        private List<TerminalData> LotteryTerminal()
        {
            // ID1,2は固定で設定
            List<TerminalData> terminals = new List<TerminalData>()
            {
                new TerminalData(){ ID = 1, Type = TERMINAL_TYPE.Boss, State = TERMINAL_STATE.Inactive},
                new TerminalData(){ ID = 2, Type = TERMINAL_TYPE.Speed, State = TERMINAL_STATE.Inactive},
            };

            // 3以降は抽選
            Random rand = new Random();
            int terminalCount = rand.Next(MIN_TERMINAL_NUM, MAX_TERMINAL_NUM); // 3～6個の端末を抽選

            for(int i = 3; i <= terminalCount; i++ )
            {
                int termID = 0;
                
                while(termID == 0 || termID == 2 || termID == 6)
                {   // SpeedとBossは固定で設定しているため、抽選から除外
                    termID = rand.Next(MIN_TERMINAL_ID, MAX_TERMINAL_ID);
                }

                terminals.Add(new TerminalData() { ID = i, Type = (TERMINAL_TYPE)termID, State = TERMINAL_STATE.Inactive });
            }

            return terminals;
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

                // レリックステータス取得
                var statusData = this.roomContext.playerStatusDataList[this.ConnectionId];
                PlayerRelicStatusData relicStatus = statusData.Item2;   // インスタンス取得

                // 受け取った情報で基礎ダメージ計算をする
                int damage = (int)((giverATK / 2) - (enemData.State.defence / 4));

                // ダメージにレリック効果適用

                // 「識別AI」効果（デバフ状態の敵に対するダメージ倍率UP）
                if (roomContext.enemyDataList[enemID].DebuffList.Count != 0) damage = (int)(damage * relicStatus.IdentificationAIRate);
                // レリック「イリーガルスクリプト」適用時、ダメージを99999にする
                damage = (LotteryIllegalScript(relicStatus.IllegalScriptRate)) ? MAX_DAMAGE : damage;

                // ダメージ適用
                enemData.State.hp -= damage;

                // 敵被弾データを新しく作成
                EnemyDamegeData enemDmgData = new EnemyDamegeData();

                // 作成したデータに各情報を代入
                enemDmgData.AttackerId = this.ConnectionId; // 攻撃者ID
                enemDmgData.Damage = damage;                // 付与ダメージ
                enemDmgData.HitEnemyId = enemID;            // 被弾敵ID
                enemDmgData.RemainingHp = enemData.State.hp;// HP残量
                enemDmgData.DebuffList = debuffType;        // 付与デバフ
                enemDmgData.Exp = 0;

                // 合計付与ダメージを加算
                this.roomContext.totalGaveDamage += enemDmgData.Damage;

                // リザルトデータを更新
                this.roomContext.resultDataList[this.ConnectionId].TotalGaveDamage += enemDmgData.Damage;

                // 敵のHPが0以下になった場合
                if (enemDmgData.RemainingHp <= 0)
                {
                    // 獲得可能な経験値量を設定
                    float addExpRate = this.roomContext.playerStatusDataList[this.ConnectionId].Item2.AddExpRate;   // レリックによる獲得可能経験値倍率
                    enemDmgData.Exp = enemData.Exp + (int)(enemData.Exp * addExpRate);
                    this.roomContext.ExpManager.nowExp += enemDmgData.Exp;

                    // 合計キル数を加算
                    DeleteEnemyData(enemID);
                    this.roomContext.totalKillCount++;

                    // リザルトデータを更新
                    this.roomContext.resultDataList[this.ConnectionId].EnemyKillCount++;

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
        /// 違法スクリプト適用判定
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool LotteryIllegalScript(float value)
        {
            Random rand = new Random();
            var result = rand.Next(0, 100);

            if (result <= value) return true;
            else return false;

        }

        /// <summary>
        /// 指定された敵の除外
        /// </summary>
        /// <param name="uniqueId"></param>
        public void DeleteEnemyData(string uniqueId)
        {
            this.roomContext.enemyDataList.Remove(uniqueId);

            // 以下に端末生成の敵の処理を記載
            if (roomContext.terminalList == null) return;

            foreach (var item in roomContext.terminalList)
            {
                if (item.Type == EnumManager.TERMINAL_TYPE.Enemy && item.State == EnumManager.TERMINAL_STATE.Active ||
                    item.Type == EnumManager.TERMINAL_TYPE.Elite && item.State == EnumManager.TERMINAL_STATE.Active)
                {   // エネミーかエリートがアクティブになってる場合
                    foreach (var enemyID in item.EnemyList)
                    {   // 引数のユニークIDと一致する敵を検索、削除
                        if (enemyID == uniqueId)
                        {
                            item.EnemyList.Remove(uniqueId);

                            if (item.EnemyList.Count == 0)
                            {   // 端末の敵を全部倒した時
                                this.roomContext.Group.All.OnTerminalsSuccess(item.ID);
                            }
                        }
                    }
                }
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
                if (isAllDead)
                {
                    // 経過時間を算出
                    this.roomContext.resultDataList[this.ConnectionId].AliveTime = DateTime.Now - this.roomContext.startTime;

                    Result();
                }
            }
        }

        /// <summary>
        /// 端末起動同期処理
        /// </summary>
        /// <param name="termID">端末識別ID</param>
        /// <returns></returns>
        public async Task BootTerminalAsync(int termID)
        {
            // 渡ってきた端末の種類に応じてステータスを変更
            lock (roomContextRepository)
            {
                // 引数の端末IDから端末データを取得
                var terminal = this.roomContext.terminalList.Where(term => term.ID == termID).First();
                terminal.State = TERMINAL_STATE.Active; // 端末の状態をアクティブにする

                // リクエスト者に対してディール・ジャンブルの効果適用
                if (terminal.Type == TERMINAL_TYPE.Deal)
                {
                    terminal.State = TERMINAL_STATE.Success;
                }
                else if(terminal.Type == TERMINAL_TYPE.Jumble)
                {
                    // リクエスト者に対してジャンブルの効果適用
                    JumbleRelic(this.ConnectionId);
                    this.roomContext.Group.Single(this.ConnectionId).OnTerminalJumble(CastDropRelicData(roomContext.relicDataList[this.ConnectionId]));
                    GetStatusWithRelics();
                    terminal.State = TERMINAL_STATE.Success;
                }

                // リザルトデータを更新
                this.roomContext.resultDataList[this.ConnectionId].TotalActivedTerminal++;

                // 参加者全員に端末が起動したことを通知
                this.roomContext.Group.All.OnBootTerminal(termID);
            }
        }

        /// <summary>
        /// リクエスト者に対してジャンブルの効果適用
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        private void JumbleRelic(Guid connectionId)
        {
            var haveCnt = this.roomContext.relicDataList[connectionId].Count;
            this.roomContext.relicDataList[connectionId].Clear();

            for(int i = 0; i < haveCnt; i++)
            {
                this.roomContext.relicDataList[connectionId].Add(DrawRelic(DrawRarity(true)));
            }
        }

        /// <summary>
        /// RelicDataをDropRelicDataに変換
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<DropRelicData> CastDropRelicData(List<Relic> list)
        {
            var relicList = new List<DropRelicData>();

            foreach (var rel in list)
            {
                var data = new DropRelicData();
                data.uniqueId = rel.id.ToString();
                data.RarityType = (RARITY_TYPE)rel.rarity;
                data.Name = rel.name;
                relicList.Add(data);
            }

            return relicList;
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

                        // DBからレリック情報取得
                        GameDbContext dbContext = new GameDbContext();
                        var relicData = dbContext.Relics.Where(data => data.id == (int)relic.RelicType).First();

                        // 取得したレリックをリストに入れる
                        this.roomContext.relicDataList[this.ConnectionId].Add(relicData);

                        // リザルトデータを更新
                        this.roomContext.resultDataList[this.ConnectionId].GottenRelicList.Add((RELIC_TYPE)relicData.id);

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

                // リザルトデータを更新
                this.roomContext.resultDataList[this.ConnectionId].TotalGottenItem++;

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
        public async Task ChooseUpgrade(Guid optionsKey, STAT_UPGRADE_OPTION upgradeOpt)
        {
            lock (roomContextRepository)
            {
                if (!this.roomContext.statusOptionList[this.ConnectionId].ContainsKey(optionsKey)) return;
                this.roomContext.statusOptionList[this.ConnectionId].Remove(optionsKey);

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

                        if (status.hp <= 0) status.hp = 1; // HPが0を下回った場合、1にする
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
        }

        /// <summary>
        /// 弾発射同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="spawnPos">生成位置</param>
        /// <param name="shootVec">発射ベクトル</param>
        public async Task ShootBulletsAsync(params ShootBulletData[] shootBulletDatas)
        {
            // 参加者全員に端末の結果を通知
            this.roomContext.Group.All.OnShootBullets(shootBulletDatas);
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
            foreach (var relic in this.roomContext.relicDataList[this.ConnectionId])
            {
                if (relic.status_type <= (int)STATUS_TYPE.HealRate) // タイプがステータス上昇の場合
                {
                    // タイプ別ステータス上昇
                    switch (relic.status_type)
                    {
                        case (int)STATUS_TYPE.HP:                   // HPの場合
                            if (status.hp >= relic.max) break;
                            status.hp += relic.const_effect;
                            status.hp += (int)(status.hp * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.Defense:              // 防御力の場合
                            if (status.defence >= relic.max) break;
                            status.defence += relic.const_effect;
                            status.defence += (int)(status.defence * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.Power:                // 攻撃力の場合
                            if (status.power >= relic.max) break;
                            status.power += relic.const_effect;
                            status.power += (int)(status.power * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.JumpPower:            // 跳躍力の場合
                            if (status.jumpPower >= relic.max) break;
                            status.jumpPower += relic.const_effect;
                            status.jumpPower += (int)(status.jumpPower * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.MoveSpeed:            // 移動速度の場合
                            if (status.moveSpeed >= relic.max) break;
                            status.moveSpeed += relic.const_effect;
                            status.moveSpeed += (int)(status.moveSpeed * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.AttackSpeedFactor:    // 攻撃速度の場合
                            if (status.attackSpeedFactor >= relic.max) break;
                            status.attackSpeedFactor += relic.const_effect;
                            status.attackSpeedFactor += (int)(status.attackSpeedFactor * relic.rate_effect);
                            break;

                        case (int)STATUS_TYPE.HealRate:             // 自動回復速度の場合
                            if (status.healRate >= relic.max) break;
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
                            if (relicStatus.ScatterBugCnt >= relic.max) break;
                            relicStatus.ScatterBugCnt += relic.const_effect;
                            relicStatus.ScatterBugCnt += (int)relic.rate_effect;
                            break;

                        case (int)STATUS_TYPE.DigitalMeatCnt:   // デジタルミートの場合
                            if (relicStatus.DigitalMeatCnt >= relic.max) break;
                            relicStatus.DigitalMeatCnt += relic.const_effect;
                            relicStatus.DigitalMeatCnt += (int)relic.rate_effect;
                            break;

                        case (int)STATUS_TYPE.BuckupHDMICnt:    // バックアップHDMIの場合
                            //if (relicStatus.BuckupHDMICnt >= relic.max) break;
                            relicStatus.BuckupHDMICnt += relic.const_effect;
                            relicStatus.BuckupHDMICnt += (int)relic.rate_effect;
                            break;

                        case (int)STATUS_TYPE.ChargedCoreCnt:   // 感電オーブの場合
                            if (relicStatus.ChargedCoreCnt >= relic.max) break;
                            relicStatus.ChargedCoreCnt += relic.const_effect;
                            relicStatus.ChargedCoreCnt += (int)relic.rate_effect;
                            break;

                        case (int)DEBUFF_TYPE.Burn:                 // 炎上確率の場合
                            if (relicStatus.GiveDebuffRates[DEBUFF_TYPE.Burn] >= relic.max) break;
                            relicStatus.GiveDebuffRates[DEBUFF_TYPE.Burn] += relic.const_effect;
                            relicStatus.GiveDebuffRates[DEBUFF_TYPE.Burn] += (int)relic.rate_effect;
                            break;

                        case (int)DEBUFF_TYPE.Freeze:               // 凍結確率の場合
                            if (relicStatus.GiveDebuffRates[DEBUFF_TYPE.Freeze] >= relic.max) break;
                            relicStatus.GiveDebuffRates[DEBUFF_TYPE.Freeze] += relic.const_effect;
                            relicStatus.GiveDebuffRates[DEBUFF_TYPE.Freeze] += (int)relic.rate_effect;
                            break;

                        case (int)DEBUFF_TYPE.Shock:                // 感電確率の場合
                            if (relicStatus.GiveDebuffRates[DEBUFF_TYPE.Shock] >= relic.max) break;
                            relicStatus.GiveDebuffRates[DEBUFF_TYPE.Shock] += relic.const_effect;
                            relicStatus.GiveDebuffRates[DEBUFF_TYPE.Shock] += (int)relic.rate_effect;
                            break;

                        case (int)STATUS_TYPE.AddExpRate:           // 付与経験値率の場合
                            if (relicStatus.AddExpRate >= relic.max) break;
                            relicStatus.AddExpRate += relic.const_effect;
                            relicStatus.AddExpRate += (int)relic.rate_effect;
                            break;

                        case (int)STATUS_TYPE.RegainCodeRate:       // 与ダメージ回復率の場合
                            if (relicStatus.RegainCodeRate >= relic.max) break;
                            relicStatus.RegainCodeRate += relic.const_effect;
                            relicStatus.RegainCodeRate += (int)relic.rate_effect;
                            break;

                        case (int)STATUS_TYPE.HolographicArmorRate: // 回避率の場合
                            if (relicStatus.HolographicArmorRate >= relic.max) break;
                            relicStatus.HolographicArmorRate += relic.const_effect;
                            relicStatus.HolographicArmorRate += (int)relic.rate_effect;
                            break;

                        case (int)STATUS_TYPE.MouseRate:            // クールダウン短縮率の場合
                            if (relicStatus.MouseRate >= relic.max) break;
                            relicStatus.MouseRate += relic.const_effect;
                            relicStatus.MouseRate += (int)relic.rate_effect;
                            break;

                        case (int)STATUS_TYPE.FirewallRate:         // 被ダメ軽減率の場合
                            if (relicStatus.FirewallRate >= relic.max) break;
                            relicStatus.FirewallRate += relic.const_effect;
                            relicStatus.FirewallRate += (int)relic.rate_effect;
                            break;

                        case (int)STATUS_TYPE.LifeScavengerRate:    // キル時HP回復率の場合
                            if (relicStatus.LifeScavengerRate >= relic.max) break;
                            relicStatus.LifeScavengerRate += relic.const_effect;
                            relicStatus.LifeScavengerRate += (int)relic.rate_effect;
                            break;

                        case (int)STATUS_TYPE.RugrouterRate:        // DA率の場合
                            if (relicStatus.RugrouterRate >= relic.max) break;
                            relicStatus.RugrouterRate += relic.const_effect;
                            relicStatus.RugrouterRate += (int)relic.rate_effect;
                            break;

                        case (int)STATUS_TYPE.IdentificationAIRate: // デバフ的に対するダメUP率の場合
                            if (relicStatus.IdentificationAIRate >= relic.max) break;
                            relicStatus.IdentificationAIRate += relic.const_effect;
                            relicStatus.IdentificationAIRate += (int)relic.rate_effect;
                            break;

                        case (int)STATUS_TYPE.DanborDollRate:       // 防御貫通率の場合
                            if (relicStatus.DanborDollRate >= relic.max) break;
                            relicStatus.DanborDollRate += relic.const_effect;
                            relicStatus.DanborDollRate += (int)relic.rate_effect;
                            break;

                        case (int)STATUS_TYPE.IllegalScriptRate:    // クリティカルオーバーキル発生率の場合
                            if (relicStatus.IllegalScriptRate >= relic.max) break;
                            relicStatus.IllegalScriptRate += relic.const_effect;
                            relicStatus.IllegalScriptRate += (int)relic.rate_effect;
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
            // レベルアップ処理
            expManager.Level++; // 現在のレベルを上げる
            expManager.nowExp = expManager.nowExp - expManager.RequiredExp;    // 超過した分の経験値を現在の経験値量として保管

            // 最終レベルに現在のレベルを代入
            this.roomContext.resultLevel = expManager.Level;

            // リザルトデータを更新
            //this.roomContext.resultDataList[this.ConnectionId].MaxLevel = this.roomContext.resultLevel;

            // 次のレベルまで必要な経験値量を計算 （必要な経験値量 = 次のレベルの3乗 - 今のレベルの3乗）
            expManager.RequiredExp = (int)Math.Pow(expManager.Level + 1, 3) - (int)Math.Pow(expManager.Level, 3);

            // 強化選択肢格納リスト
            List<StatusUpgrateOptionData> statusOptionList = DrawStatusUpgrateOption(3);
            Guid optionsKey = Guid.NewGuid();

            // 参加者リストをループ
            foreach (var user in this.roomContext.JoinedUserList)
            {
                // ステータス強化選択肢をルームデータで管理
                this.roomContext.statusOptionList[user.Key][optionsKey].AddRange(statusOptionList);

                // 参加者リストのキーから接続IDを受け取り対応ユーザのデータを取得
                var playerData = this.roomContext.playerStatusDataList[user.Key].Item1;

                // 各最大値を更新
                const float LEVEL_UP_RATE = 0.05f;
                playerData.hp = playerData.hp + (int)(playerData.hp * LEVEL_UP_RATE);
                playerData.power = playerData.power + (int)(playerData.power * LEVEL_UP_RATE);
                playerData.defence = playerData.defence + (int)(playerData.defence * LEVEL_UP_RATE);

                // ユーザー毎にレベルアップ通知
                this.roomContext.Group.Except([user.Key]).OnLevelUp(expManager.Level, expManager.nowExp, playerData, optionsKey, statusOptionList);
            }
        }

        /// <summary>
        /// リザルト作成処理
        /// Author:Nishiura
        /// </summary>
        /// <returns></returns>
        public async void Result()
        {
            foreach (var conectionId in this.roomContext.JoinedUserList.Keys)
            {   
                var playerData = this.roomContext.GetPlayerData(conectionId);
                var resultData = this.roomContext.resultDataList[conectionId];

                // 必要なデータを代入
                resultData.TotalClearStageCount = this.roomContext.totalClearStageCount;
                resultData.DifficultyLevel = this.roomContext.NowDifficulty;
                resultData.AliveTime = DateTime.Now - this.roomContext.startTime;
                resultData.PlayerClass = playerData.Class;                                      // プレイヤーのクラス

                // 合計スコア
                resultData.TotalScore = (resultData.TotalGottenItem * 10) +
                            (resultData.TotalActivedTerminal * 10) +
                            (resultData.EnemyKillCount * 10) +
                            (resultData.TotalGaveDamage * 2) +
                            (resultData.TotalClearStageCount * 100) *
                            (resultData.DifficultyLevel / 2);

                this.roomContext.Group.Except([conectionId]).OnGameEnd(resultData);
            }
        }
    }
}
