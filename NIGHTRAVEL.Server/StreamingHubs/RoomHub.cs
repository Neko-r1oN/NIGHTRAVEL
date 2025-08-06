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
using Shared.Interfaces.StreamingHubs;
using System.Text.RegularExpressions;
using UnityEngine;
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
                else if(this.roomContext.JoinedUserList.Count == 0)
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

            // ルームデータに追加
            this.roomContext.AddPlayerData(this.ConnectionId);

            //　ルームに参加
            this.roomContext.Group.Add(this.ConnectionId, Client);

            // 参加したことを自分以外に通知
            //this.roomContext.Group.All.Onjoin(roomContext.JoinedUserList[this.ConnectionId],
            //    roomContext.JoinedUserList);
            this.roomContext.Group.Except([this.ConnectionId]).Onjoin(roomContext.JoinedUserList[this.ConnectionId]);

            // ルームデータから接続IDを指定して自身のデータを取得
            var playerData = this.roomContext.GetPlayerData(this.ConnectionId);
            playerData.IsDead = false; // 死亡判定をfalseにする

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
            //　退室するユーザーを取得
            var joinedUser = this.roomContext.JoinedUserList[this.ConnectionId];

            ////マスタークライアントだったら次の人に譲渡する
            if (joinedUser.IsMaster == true)
            {
                await MasterLostAsync(this.ConnectionId);
            }

            // ルーム参加者全員に、ユーザーの退室通知を送信
            this.roomContext.Group.All.OnLeave(joinedUser);

            //　ルームから退室
            this.roomContext.Group.Remove(this.ConnectionId);

            //コンテキストからユーザーを削除
            roomContext.RemoveUser(this.ConnectionId);
            // ルームデータから自身のデータを削除
            roomContext.RemovePlayerData(this.ConnectionId);
        }

        /// <summary>
        /// 準備完了
        /// Author:Nishiura
        /// </summary>
        /// <returns></returns>
        public async Task ReadyAsync()
        {
            bool canStartGame = true; // ゲーム開始可能判定変数

            // 自身のデータを取得
            var joinedUser = roomContext.JoinedUserList[this.ConnectionId];
            joinedUser.IsReady = true; // 準備完了にする

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
        #endregion

        #region ゲーム内での処理
        /// <summary>
        /// プレイヤー位置、回転、アニメーション同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="anim"></param>
        /// <returns></returns>
        public async Task MovePlayerAsync(PlayerData playerData)
        {
            //// ルームデータから接続IDを指定して自身のデータを取得
            //var playerData = this.roomContext.GetPlayerData(this.ConnectionId);

            //playerData.Position = pos; // 位置を渡す
            //playerData.Rotation = rot; // 回転を渡す
            //playerData.State = anim;   // アニメーションIDを渡す

            // ルーム参加者全員に、ユーザ情報通知を送信
            //this.roomContext.Group.All.OnMovePlayer(playerData);

            //ルームの自分以外に、ユーザ情報通知を送信
            this.roomContext.Group.Except([this.ConnectionId]).OnUpdatePlayer(playerData);
        }



        /// <summary>
        /// プレイヤーの更新
        /// Author:Nishiura
        /// </summary>
        /// <param name="playerData"></param>
        /// <returns></returns>
        public async Task UpdatePlayerAsync(PlayerData playerData)
        {
            // ルームデータから接続IDを指定して自身のデータを取得
            var gottenData = this.roomContext.GetPlayerData(playerData.ConnectionId);

            // 取得したデータを受け取ったデータに置き換える
            gottenData = playerData;

            // キャラクターデータリストに自身のデータがない場合
            if (!this.roomContext.characterDataList.ContainsKey(this.ConnectionId))
            {
                // 新たなキャラクターデータを追加
                this.roomContext.AddCharacterData(this.ConnectionId);
            }
            else // 既に存在している場合
            {
                // キャラクターデータを更新
                this.roomContext.UpdateCharacterData(this.ConnectionId, playerData);
            }

            // ルームの自分以外に、ユーザ情報通知を送信
            this.roomContext.Group.Except([this.ConnectionId]).OnUpdatePlayer(playerData);
        }

        /// <summary>
        /// マスタークライアントの更新
        /// Author:木田晃輔
        /// </summary>
        /// <param name="masterClientData"></param>
        /// <returns></returns>
        public async Task UpdateMasterClientAsync(MasterClientData masterClientData)
        {
            // ルームデータから接続IDを指定して自身のデータを取得
            var gottenPlayerData = this.roomContext.GetPlayerData(this.ConnectionId);

            // 取得したデータを受け取ったマスターデータに置き換え
            gottenPlayerData = masterClientData.PlayerData;

            // ルームデータから敵のリストを取得
            var gottenEnemyDataList = this.roomContext.enemyDataList;

            // 敵データの個数分ループして更新
            for (int i = 0; i < masterClientData.EnemyDatas.Count; i++)
            {
                // 指定データが存在している場合、代入する
                if (gottenEnemyDataList[i] != null)
                {
                    gottenEnemyDataList[i] = masterClientData.EnemyDatas[i];
                }
            }

            // キャラクターデータリストに自身のデータがない場合
            if (!this.roomContext.characterDataList.ContainsKey(this.ConnectionId))
            {
                // 新たなキャラクターデータを追加
                this.roomContext.AddCharacterData(this.ConnectionId);
            }
            else // 既に存在している場合
            {
                // キャラクターデータを更新
                this.roomContext.UpdateCharacterData(this.ConnectionId, masterClientData.PlayerData);
            }

            // ルームの自分以外に、マスタークライアントの状態の更新通知を送信
            this.roomContext.Group.Except([this.ConnectionId]).OnUpdateMasterClient(masterClientData);
        }

        /// <summary>
        /// レリックID設定、位置同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="pos">位置</param>
        /// <returns></returns>
        public async Task DropRelicAsync(Vector2 pos)
        {
            GameDbContext dbContext = new GameDbContext();

            int relicID = new Random().Next(1, dbContext.Relics.ToArray().Length); // 取得するレリックのIDを乱数で指定

            // レリック情報を検索
            var relic = dbContext.Relics.Where(relic => relic.id == relicID).First();

            DropRelicData dropRelicData = new DropRelicData();

            // データを更新
            dropRelicData.Id = Guid.NewGuid().ToString();
            dropRelicData.Name = relic.name;
            dropRelicData.ExplanationText = relic.explanation;
            dropRelicData.Effect = (int)relic.effect;
            dropRelicData.RelicType = (EnumManager.RELIC_TYPE)relicID;
            dropRelicData.SpawnPos = pos;
            dropRelicData.DroppedVec = new Vector2();

            // ドロップしたレリックリストに追加
            this.roomContext.dropRelicDataList.Add(dropRelicData);

            // ルーム参加者全員に、レリックのIDと生成位置を送信
            this.roomContext.Group.All.OnDropRelic(this.roomContext.dropRelicDataList);
        }

        /// <summary>
        /// レリック取得同期処理
        /// Author:Nishiura
        /// </summary>
		/// <param name="relicID">レリックID</param>
        /// <param name="relicName">レリック名</param>
        /// <returns></returns>
        public async Task GetRelicAsync(int relicID, string relicName)
        {
            // ルーム参加者全員に、取得したレリック名を送信
            this.roomContext.Group.All.OnGetRelic(relicID, relicName);
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
            this.roomContext.SetSpawnedEnemyData(spawnEnemyData);

            // 自分以外に、取得した敵情報と生成位置を送信
            this.roomContext.Group.Except([this.ConnectionId]).OnSpawnEnemy(spawnEnemyData);
        }

        /// <summary>
        /// ギミック起動同期処理
        /// Autho:Nishiura
        /// </summary>
        /// <param name="gimID">ギミック識別ID</param>
        /// <returns></returns>
        public async Task BootGimmickAsync(int gimID)
        {
            // ルームデータからギミックのIDを指定してギミックデータを取得
            GimmickData gimmickData = this.roomContext.GetGimmickData(gimID);

            // 参加者全員にギミック情報を通知？？？？
            this.roomContext.Group.All.OnBootGimmick(gimmickData);
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
        /// <param name="stageID"></param>
        /// <param name="isBossStage">ボスステージ判定</param>
        /// <returns></returns>
        public async Task AdvanceNextStageAsync(int stageID, bool isBossStage)
        {
            lock (roomContextRepository) // 排他制御
            {

                if ((stageID == 3 && isBossStage) || stageID != 3) stageID++;   // ステージが3かつラスボスへ進む場合または通常ステージの場合、ステージIDを加算
                else if (stageID == 3 && !isBossStage) stageID = 1;             // ラスボスへ進まない場合、ステージ1へ移動

                // 生成された敵リストをクリア
                this.roomContext.spawnedEnemyDataList.Clear();

                // 獲得したアイテムリストをクリア
                this.roomContext.gottenItemList.Clear();

                // 起動した端末リストをクリア
                this.roomContext.bootedTerminalList.Clear();

                // 成功した端末リストをクリア
                this.roomContext.succededTerminalList.Clear();

                // 参加者全員にステージの進行を通知
                this.roomContext.Group.All.OnAdanceNextStage(stageID);
            }
        }

        /// <summary>
        /// ステージ進行完了同期処理
        /// </summary>
        /// <param name="stageID">ステージID</param>
        /// <param name="isBossStage">ボスステージ判定</param>
        /// <returns></returns>
        public async Task AdvancedStageAsync(int stageID, bool isBossStage)
        {
            bool canAdvenceStage = true; // ステージ進行済み判定変数

            // 自身のデータを取得
            var joinedUser = roomContext.JoinedUserList[this.ConnectionId];
            joinedUser.IsAdvance = true; // 準備完了にする

            // 自分以外の参加者全員に、自分が準備完了した通知を送信
            this.roomContext.Group.Except([this.ConnectionId]).OnAdvancedStage(this.ConnectionId);

            foreach (var user in this.roomContext.JoinedUserList)
            { // 現在の参加者数分ループ
                if (user.Value.IsAdvance != true) canAdvenceStage = false; // もし一人でも準備完了していなかった場合、進行させない
            }

            // 進行できる場合、進行通知をする
            if (canAdvenceStage) await AdvanceNextStageAsync(stageID, isBossStage);

        }

        /// <summary>
        /// ステージクリア待機同期処理
        /// </summary>
        /// <param name="conID">接続ID</param>
        /// <param name="isTimeUp">時間切れ判定</param>
        /// <param name="stageID">ステージID</param>
        /// <param name="isBossStage">ボスステージ判定</param>
        /// <returns></returns>
        public async Task WaitStageClearAsync(Guid? conID, bool isTimeUp, int stageID, bool isBossStage)
        {
            // 時間切れの場合、即座に進行処理をする
            if (isTimeUp) await AdvanceNextStageAsync(stageID, isBossStage);

            bool canAdvenceStage = true; // ステージ進行判定変数

            // 自身のデータを取得
            var joinedUser = roomContext.JoinedUserList[(Guid)conID];
            joinedUser.IsTouchBossTerm = true; // 接触したことにする

            foreach (var user in this.roomContext.JoinedUserList)
            { // 現在の参加者数分ループ
                if (user.Value.IsTouchBossTerm != true) canAdvenceStage = false; // もし一人でも接触していなかった場合、進行させない
            }

            // 進行できる場合、進行通知をする
            if (canAdvenceStage) await AdvanceNextStageAsync(stageID, isBossStage);
        }

        /// <summary>
        /// プレイヤー体力増減同期処理
        /// Autho:Nishiura
        /// </summary>
        /// <param name="playerID">敵識別ID</param>
        /// <param name="playerHP">敵体力</param>
        /// <returns></returns>
        public async Task PlayerHealthAsync(int playerID, float playerHP)
        {
            // 参加者全員に受け取ったIDのプレイヤーが受け取ったHPになったことを通知
            this.roomContext.Group.All.OnPlayerHealth(playerID, playerHP);
        }

        /// <summary>
        /// 敵体力増減同期処理
        /// Autho:Nishiura
        /// </summary>
        /// <param name="enemID">敵識別ID</param>
        /// <param name="conID">ダメージを与えたPLの接続ID</param>
        /// <param name="giverATK">PLの攻撃力</param>
        /// <returns></returns>
        public async Task EnemyHealthAsync(int enemID, Guid conID, float giverATK)
        {
            // ID指定で敵情報を取得
            var enemData =  this.roomContext.GetEnemyData(enemID);
            if (enemData.State.hp <= 0) return;   // すでに対象の敵HPが0の場合は処理しない

            // 受け取った情報でダメージ計算をする
            enemData.State.hp -= (int)((giverATK / 2) - (enemData.State.defence / 4));

            // 敵被弾データを新しく作成
            EnemyDamegeData enemDmgData = new EnemyDamegeData();

            // 作成したデータに各情報を代入
            enemDmgData.AttackerId = conID;
            enemDmgData.HitEnemyId = enemID;
            enemDmgData.RemainingHp = enemData.State.hp;

            // 敵のHPが0以下になった場合
            if(enemData.State.hp <= 0)
            {
                //enemDmgData.Exp = enemData.Exp; // 獲得経験値を代入
                this.roomContext.ExpManager.nowExp += enemData.Exp; // 被弾クラスにExpを代入

                // 所持経験値が必要経験値に満ちた場合
                if(this.roomContext.ExpManager.nowExp >= this.roomContext.ExpManager.RequiredExp)
                {
                    LevelUp(roomContext.ExpManager); // レベルアップ処理
                }
            }

            // 自分以外の参加者に受け取ったIDの敵が受け取ったHPになったことを通知
            this.roomContext.Group.All.OnEnemyHealth(enemDmgData);
        }

        /// <summary>
        /// 敵死亡同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemID">敵識別ID</param>
        /// <returns></returns>
        public async Task KilledEnemyAsync(int enemID)
        {
            // 参加者全員に受け取ったIDの敵が死亡したことを通知
            this.roomContext.Group.All.OnKilledEnemy(enemID);
        }

        /// <summary>
        /// 経験値同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="exp">経験値</param>
        /// <returns></returns>
        public async Task EXPAsync(int exp)
        {
            // 参加者全員に受け取ったEXPの値を通知
            this.roomContext.Group.All.OnEXP(exp);
        }

        /// <summary>
        /// レベルアップ同期処理
        /// Author:Nishiura
        /// </summary>
        /// <returns></returns>
        public async Task LevelUpAsync()
        {
            // 参加者全員にレベルアップしたことを通知
            this.roomContext.Group.All.OnLevelUp();
        }

        /// <summary>
        /// 時間同期処理
        /// </summary>
        /// <param name="tiemrType">タイマーの辞典</param>
        /// <returns></returns>
        public async Task TimeAsync(Dictionary<EnumManager.TIME_TYPE, int> tiemrType)
        {
            // 参加者全員にレベルアップしたことを通知
            this.roomContext.Group.All.OnTimer(tiemrType);
        }

        /// <summary>
        /// プレイヤー死亡同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID">プレイヤーID</param>
        /// <returns></returns>
        public async Task PlayerDeadAsync(Guid conID)
        {
            // 全滅判定変数
            bool isAllDead = true;
            // ルームデータから接続IDを指定して自身のデータを取得
            var playerData = this.roomContext.GetPlayerData(conID);
            playerData.IsDead = true; // 死亡判定をtrueにする

            // 死亡者以外の参加者全員に対象者が死亡したことを通知
            this.roomContext.Group.Except([conID]).OnPlayerDead(conID);

            foreach(var player in this.roomContext.playerDataList)
            {
                if (player.Value.IsDead == false) // もし誰かが生きていた場合
                {
                    isAllDead = false;
                    break;
                }
            }

            //if(isAllDead) this.roomContext.Group.All.OnGameEnd();
        }

        /// <summary>
        /// ダメージ表記同期処理
        /// </summary>
        /// <param name="dmg"></param>
        /// <returns></returns>
        public async Task DamageAsync(int dmg)
        {
            // 参加者全員に与えたダメージを通知
            this.roomContext.Group.All.OnDamage(dmg);
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
        /// <param name="itemID">識別ID(文字列)</param>
        /// <returns></returns>
        public async Task GetItemAsync(string itemID)
        {
            // すでに取得済みである場合、処理しない
            if(this.roomContext.gottenItemList.Contains(itemID)) return;

            // 取得済みアイテムリストに入れる
            this.roomContext.gottenItemList.Add(itemID);
            
            // アイテムの獲得を全員に通知
            this.roomContext.Group.All.OnGetItem(itemID);
        }

        /// <summary>
        /// ステータス強化選択
        /// </summary>
        /// <param name="conID">接続ID</param>
        /// <param name="upgradeOpt">強化項目</param>
        /// <returns></returns>
        public async Task<CharacterStatusData> ChooseUpgrade(Guid conID, EnumManager.STAT_UPGRADE_OPTION upgradeOpt)
        {
            // ルームデータから接続IDを指定して自身のデータを取得
            var playerData = this.roomContext.characterDataList[conID];

            // 各最大値を10%増加(仮)
            playerData.Status.hp = (int)(playerData.Status.hp * 1.1f);
            playerData.Status.power = (int)(playerData.Status.power * 1.1f);
            playerData.Status.defence = (int)(playerData.Status.defence * 1.1f);
            playerData.Status.jumpPower *= 1.1f;
            playerData.Status.moveSpeed *= 1.1f;
            playerData.Status.healRate *= 1.1f;

            CharacterStatusData newStatus = new CharacterStatusData();

            // 最大値を更新
            newStatus.hp = playerData.Status.hp;
            newStatus.power = playerData.Status.power;
            newStatus.defence = playerData.Status.defence;
            newStatus.jumpPower = playerData.Status.jumpPower;
            newStatus.moveSpeed = playerData.Status.moveSpeed;
            newStatus.healRate = playerData.Status.healRate;

            return newStatus;
        }

        #endregion

        /// <summary>
        /// マスタークライアント譲渡処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID"></param>
        /// <returns></returns>
        public async Task MasterLostAsync(Guid conID)
        {
            // 参加者リストをループ
            foreach (var user in this.roomContext.JoinedUserList)
            {
                // 対象がマスタークライアントでない場合
                if (user.Value.IsMaster == false)
                {
                    // その対象をマスタークライアントとし、通知を送る。ループを抜ける
                    user.Value.IsMaster = true;
                    this.roomContext.Group.Only([user.Key]).OnChangeMasterClient();
                    break;
                }
            }

            // マスタークライアントを剥奪
            this.roomContext.JoinedUserList[conID].IsMaster = false;
        }

        // <summary>
        /// レベルアップ処理
        /// </summary>
        protected void LevelUp(ExpManager expManager)
        {
            // レベルアップ処理
            expManager.Level++; // 現在のレベルを上げる
            expManager.nowExp = expManager.nowExp - expManager.RequiredExp;    // 超過した分の経験値を現在の経験値量として保管

            // 次のレベルまで必要な経験値量を計算 （必要な経験値量 = 次のレベルの3乗 - 今のレベルの3乗）
            expManager.RequiredExp = (int)Math.Pow(expManager.Level + 1, 3) - (int)Math.Pow(expManager.Level, 3);

            // リストが3になるまでループ
            do
            {
                // ランダムな選択肢を乱数で設定
                var rndOption = new Random().Next(0, (int)EnumManager.STAT_UPGRADE_OPTION.Max);

                // 設定された選択肢がリスト内に無い場合
                if (!this.roomContext.statusOptionList.Contains((EnumManager.STAT_UPGRADE_OPTION)rndOption))
                {
                    // 選択肢をリストに入れる
                    this.roomContext.statusOptionList.Add((EnumManager.STAT_UPGRADE_OPTION)rndOption);
                }
            } while (this.roomContext.statusOptionList.Count <= 3);

            // ルームデータから接続IDを指定して自身のデータを取得
            var playerData = this.roomContext.characterDataList[this.ConnectionId];

            // 各最大値を10%増加(仮)
            playerData.Status.hp = (int)(playerData.Status.hp * 1.1f);
            playerData.Status.power = (int)(playerData.Status.power * 1.1f);
            playerData.Status.defence = (int)(playerData.Status.defence * 1.1f);
            playerData.Status.jumpPower *= 1.1f;
            playerData.Status.moveSpeed *= 1.1f;
            playerData.Status.healRate *= 1.1f;

            CharacterStatusData newStatus = new CharacterStatusData();

            // 最大値を更新
            newStatus.hp = playerData.Status.hp;
            newStatus.power = playerData.Status.power;
            newStatus.defence = playerData.Status.defence;
            newStatus.jumpPower = playerData.Status.jumpPower;
            newStatus.moveSpeed = playerData.Status.moveSpeed;
            newStatus.healRate = playerData.Status.healRate;

            this.roomContext.characterStatusDataList.Add(this.ConnectionId, newStatus);
        }
    }
}
