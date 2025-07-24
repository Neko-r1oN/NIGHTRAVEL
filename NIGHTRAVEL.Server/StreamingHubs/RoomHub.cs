//=============================
// クライアントからサーバーへの通信を管理するスクリプト
// Author:木田晃輔
//=============================
using MagicOnion.Server.Hubs;
using NIGHTRAVEL.Server.Model.Context;
using NIGHTRAVEL.Server.StreamingHubs;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Shared.Interfaces.StreamingHubs;
using System.Text.RegularExpressions;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.IRoomHubReceiver;

namespace StreamingHubs
{
    public class RoomHub(RoomContextRepository roomContextRepository) : StreamingHubBase<IRoomHub, IRoomHubReceiver>, IRoomHub
    {
        //コンテキスト定義
        private RoomContext roomContext;
        RoomContextRepository roomContextRepos;

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

        /// <summary>
        /// 入室処理
        /// Author:Kida
        /// </summary>
        /// <param name="roomName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Dictionary<Guid,JoinedUser>> JoinedAsync(string roomName, int userId)
        {
            lock (roomContextRepository)
            { //同時に生成しないように排他制御
                // ルームに参加＆ルームを保持
                this.roomContext = roomContextRepository.GetContext(roomName);
                if (this.roomContext == null)
                { //無かったら生成
                    this.roomContext = roomContextRepository.CreateContext(roomName);
                }
            }

            //DBからユーザー情報取得
            GameDbContext dbContext = new GameDbContext();
            var user = dbContext.Users.Where(user => user.Id == userId).First();

            // グループストレージにユーザーデータを格納
            var joinedUser = new JoinedUser() { ConnectionId = this.ConnectionId, UserData = user };

            if(joinedUser.JoinOrder==0)
            {//部屋を作った人だった場合

                //参加人数の初期化
                joinedUser.JoinOrder = 1;

                //1人目をマスタークライアントにする
                joinedUser.IsMaster = true;
            }
            else
            {
                //参加人数を足す
                joinedUser.JoinOrder++;
            }
          
            // ルームコンテキストに参加ユーザーを保存
            this.roomContext.JoinedUserList[this.ConnectionId]=joinedUser;

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
            ////マスタークライアントだったら次の人に譲渡する
            //if (PlayerData.JoinedUser.IsMaster == true)
            //{
            //    await MasterLostAsync();
            //}

            var joinedUser = this.roomContext.JoinedUserList[this.ConnectionId];

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

            // ゲームが開始できる場合、開始通知をする
            if (canStartGame) this.roomContext.Group.All.OnStartGame();
        }

        /// <summary>
        /// プレイヤー位置、回転、アニメーション同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="anim"></param>
        /// <returns></returns>
        public async Task MovePlayerAsync(Vector2 pos, Quaternion rot, CharacterState anim)
        {
            // ルームデータから接続IDを指定して自身のデータを取得
            var playerData = this.roomContext.GetPlayerData(this.ConnectionId);

            playerData.Position = pos; // 位置を渡す
            playerData.Rotation = rot; // 回転を渡す
            playerData.State = anim;   // アニメーションIDを渡す
            
            // ルーム参加者全員に、ユーザ情報通知を送信
            this.roomContext.Group.All.OnMovePlayer(playerData.JoinedUser, pos, rot, anim);
        }

        /// <summary>
        ///  敵位置、回転、アニメーション同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemIDList">敵ID</param>
        /// <param name="pos">敵位置値</param>
        /// <param name="rot">敵回転値</param>
        /// <param name="anim">敵アニメーションID</param>
        /// <returns></returns>
        public async Task MoveEnemyAsync(List<int> enemIDList, Vector2 pos, Quaternion rot, EnemyAnimState anim)
        {
            foreach (var enemID in enemIDList)
            {
                // ルームデータから接続IDを指定して自身のデータを取得
                var enemData = this.roomContext.GetEnemyData(enemID);

                enemData.Position = pos; // 位置を渡す
                enemData.Rotation = rot; // 回転を渡す
                enemData.State = anim;   // アニメーションIDを渡す

                // ルーム参加者全員に、ユーザ情報通知を送信
                this.roomContext.Group.All.OnMoveEnemy(enemID, pos, rot, anim);
            }
        }

        /// <summary>
        /// レリックID設定、位置同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="pos">位置</param>
        /// <returns></returns>
        public async Task SpawnRelicAsync(Vector2 pos)
        {  
            GameDbContext dbContext = new GameDbContext();
            Random rand = new Random();

            // レリックテーブルから全ての要素を取得し、その個数を取得する
            int relicMax = dbContext.Relics.ToArray().Length;
            int relicID = rand.Next(1, relicMax); // 取得するレリックのIDを乱数で指定

            var relicFind = dbContext.Relics.Where(relic => relic.id == relicID);   // 生成した乱数で検索

            if (relicFind.Count() <= 0) return; // 結果が空の場合処理しない

            // ルーム参加者全員に、レリックのIDと生成位置を送信
            this.roomContext.Group.All.OnSpawnRelic(relicID, pos);
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
        public async Task SpawnEnemyAsync(List<int> enemIDList, Vector2 pos)
        {
            // 受け取った敵ID数分ループ
            foreach (var enemID in enemIDList)
            {
                // DBからIDを指定して敵の情報を取得する
                GameDbContext dbContext = new GameDbContext();
                var enemies = dbContext.Enemies.Where(enemies => enemies.id == enemID).First();

                // 取得したものをエネミーデータに格納する
                this.roomContext.SetEnemyData(enemies);

                // ルームデータから敵のIDを指定して生成した敵データを取得
                EnemyData enemData = this.roomContext.GetEnemyData(enemID);

                enemData.Position = pos; // 敵の位置に渡された位置を代入

                // ルーム参加者全員に、取得した敵情報と生成位置を送信
                this.roomContext.Group.All.OnSpawnEnemy(enemData, pos);
            }
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
        /// <param name="difID">難易度値</param>
        /// <returns></returns>
        public async Task AscendDifficultyAsync(int difID)
        {
            difID++; // 受け取った難易度に+1
            // 参加者全員に難易度の上昇を通知
            this.roomContext.Group.All.OnAscendDifficulty(difID);
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
            if ((stageID == 3 && isBossStage) || stageID != 3) stageID++;   // ステージが3かつラスボスへ進む場合または通常ステージの場合、ステージIDを加算
            else if (stageID == 3 && !isBossStage) stageID = 1;             // ラスボスへ進まない場合、ステージ1へ移動

            // 参加者全員にステージの進行を通知
            this.roomContext.Group.All.OnAdanceNextStage(stageID);
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
        /// <param name="enemHP">敵体力</param>
        /// <returns></returns>
        public async Task EnemyHealthAsync(int enemID, float enemHP)
        {
            // 参加者全員に受け取ったIDの敵が受け取ったHPになったことを通知
            this.roomContext.Group.All.OnEnemyHealth(enemID, enemHP);
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
        /// プレイヤー死亡同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="playerID">プレイヤーID</param>
        /// <returns></returns>
        public async Task PlayerDeadAsync(int playerID)
        {
            // ルームデータから接続IDを指定して自身のデータを取得
            var playerData = this.roomContext.GetPlayerData(this.ConnectionId);
            playerData.IsDead = false; // 死亡判定をtrueにする

            // 参加者全員に対象者が死亡したことを通知
            this.roomContext.Group.All.OnPlayerDead(playerID);
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
    }
}
