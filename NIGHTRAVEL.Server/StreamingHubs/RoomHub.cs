////////////////////////////////////////////////////////////////
///
/// クライアントからサーバーへの通信を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////
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
        public async Task<Dictionary<Guid,JoinedUser>> JoinedAsync(string roomName, int userId)
        {
            lock (roomContextRepository)
            {//同時に生成しないように排他制御
                //ルームに参加＆ルームを保持
                this.roomContext = roomContextRepository.GetContext(roomName);
                if (this.roomContext == null)
                {//無かったら生成
                    this.roomContext = roomContextRepository.CreateContext(roomName);
                }
            }

            //DBからユーザー情報取得
            GameDbContext dbContext = new GameDbContext();
            var user = dbContext.Users.Where(user => user.Id == userId).First();

            //グループストレージにユーザーデータを格納
            var joinedUser = new JoinedUser() { ConnectionId = this.ConnectionId, UserData = user };

            ////1人目をマスタークライアントにする
            //if (roomStorage.AllValues.Count == 0) { joinedUser.IsMaster = true; }

            //ルームコンテキストに参加ユーザーを保存
            this.roomContext.JoinedUserList[ConnectionId]=joinedUser;
            // ルームデータに追加
            this.roomContext.AddRoomData(this.ConnectionId);

            //通知
            this.Client.Onjoin(roomContext.JoinedUserList[ConnectionId]);

            //参加中のユーザー情報を返す
            return this.roomContext.JoinedUserList;
        }

        /// <summary>
        /// 退室処理
        /// Author:Kida
        /// </summary>
        /// <returns></returns>
        public async Task LeavedAsync()
        {
            // すでに自身のデータが場合、処理しない
            if (roomContext.JoinedUserList[this.ConnectionId] == null) return;

            ////マスタークライアントだったら次の人に譲渡する
            //if (roomData.JoinedUser.IsMaster == true)
            //{
            //    await MasterLostAsync();
            //}

            // ルーム参加者全員に、ユーザーの退室通知を送信
            this.Client.OnLeave(roomContext.JoinedUserList[this.ConnectionId]);

            // グループデータから自身を削除
            roomContext.JoinedUserList.Remove(this.ConnectionId);

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
        /// レリック取得
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
        /// 敵生成
        /// Author:Nishiura
        /// </summary>
        /// <param name="enemID">敵識別ID</param>
        /// <param name="pos">位置</param>
        /// <returns></returns>
        public async Task SpawnEnemyAsync(int enemID, Vector2 pos)
        {
            // ルームデータから接続IDを指定して自身のデータを取得
            EnemyData enemData = this.roomContext.GetEnemyData(enemID);

            // ルーム参加者全員に、取得したレリック名を送信
            this.Client.OnSpawnEnemy(enemData, pos);
        }

        /// <summary>
        /// ギミック起動
        /// Autho:Nishiura
        /// </summary>
        /// <param name="gimID">ギミック識別ID</param>
        /// <returns></returns>
        public async Task BootGimmickAsync(int gimID)
        {
            GimmickData gimmickData = this.roomContext.GetGimmickData(gimID);

            this.Client.OnBootGimmick(gimmickData);
        }
    }
}
