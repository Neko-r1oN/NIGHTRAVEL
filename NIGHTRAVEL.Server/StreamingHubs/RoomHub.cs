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
        public async Task<JoinedUser[]> JoinedAsync(string roomName, int userId)
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

            if(joinedUser.JoinOrder==null)
            {//部屋を作った人だった場合

                //参加人数の初期化
                joinedUser.JoinOrder = 1;

                //1人目をマスタークライアントにする
                joinedUser.IsMaster = true;
            }

            //参加人数を足す
            joinedUser.JoinOrder++;

            // ルームコンテキストに参加ユーザーを保存
            this.roomContext.JoinedUserList[joinedUser.JoinOrder-1]=joinedUser;

            // ルームデータに追加
            this.roomContext.AddRoomData(this.ConnectionId);

            // 参加したことを全員に通知
            this.Client.Onjoin(roomContext.JoinedUserList[joinedUser.JoinOrder-1]);

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
            //if (roomData.JoinedUser.IsMaster == true)
            //{
            //    await MasterLostAsync();
            //}

            var joinedUser = new JoinedUser() { ConnectionId = this.ConnectionId };

            // ルーム参加者全員に、ユーザーの退室通知を送信
            this.Client.OnLeave(joinedUser);

            //コンテキストからユーザーを削除
            roomContext.RemoveUser(joinedUser.JoinOrder);

            // ルームデータから自身のデータを削除
            roomContext.RemoveRoomData(this.ConnectionId);
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
            var roomData = this.roomContext.GetRoomData(this.ConnectionId);

            roomData.Position = pos; // 位置を渡す
            roomData.Rotation = rot; // 回転を渡す
            roomData.State = anim;   // アニメーションIDを渡す
            
            // ルーム参加者全員に、ユーザ情報通知を送信
            this.Client.OnMovePlayer(roomData.JoinedUser, pos, rot, anim);
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
                this.Client.OnMoveEnemy(enemID, pos, rot, anim);
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
            this.Client.OnSpawnRelic(relicID, pos);
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
            this.Client.OnGetRelic(relicID, relicName);
        }

        /// <summary>
        /// 敵生成同期処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemID">敵識別ID</param>
        /// <param name="pos">位置</param>
        /// <returns></returns>
        public async Task SpawnEnemyAsync(int enemID, Vector2 pos)
        {
            // ルームデータから敵のIDを指定して生成した敵データを取得
            EnemyData enemData = this.roomContext.GetEnemyData(enemID);

            enemData.Position = pos; // 敵の位置に渡された位置を代入

            // ルーム参加者全員に、取得した敵情報と生成位置を送信
            this.Client.OnSpawnEnemy(enemData, pos);
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
            this.Client.OnBootGimmick(gimmickData);
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
            this.Client.OnAscendDifficulty(difID);
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
            this.Client.OnAdanceNextStage(stageID);
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
            this.Client.OnEnemyHealth(enemID, enemHP);
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
            this.Client.OnKilledEnemy(enemID);
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
            this.Client.OnEXP(exp);
        }

        /// <summary>
        /// レベルアップ同期処理
        /// Author:Nishiura
        /// </summary>
        /// <returns></returns>
        public async Task LevelUpAsync()
        {
            // 参加者全員にレベルアップしたことを通知
            this.Client.OnLevelUp();
        }
    }
}
