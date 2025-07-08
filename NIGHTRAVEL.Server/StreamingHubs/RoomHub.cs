using MagicOnion.Server.Hubs;
using NIGHTRAVEL.Server.Model.Context;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using UnityEngine;

using Shared.Interfaces.StreamingHubs;
using NIGHTRAVEL.Server.StreamingHubs;
using System.Text.RegularExpressions;
using Cysharp.Runtime.Multicast;

namespace StreamingHubs
{
    public class RoomHub(RoomContextRepository roomContextRepository) : StreamingHubBase<IRoomHub, IRoomHubReceiver>, IRoomHub
    {
        //コンテキスト定義
        private RoomContext roomContext;

        //入室
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
            var user = dbContext.Users.Where(user => user.id == userId).First();

            //グループストレージにユーザーデータを格納
            var joinedUser = new JoinedUser() { ConnectionId = this.ConnectionId, UserData = user };

            ////1人目をマスタークライアントにする
            //if (roomStorage.AllValues.Count == 0) { joinedUser.IsMaster = true; }

            //ルームコンテキストに参加ユーザーを保存
            this.roomContext.JoinedUserList[ConnectionId]=joinedUser;

            //通知
            this.Client.Onjoin(joinedUser);

            //参加中のユーザー情報を返す
            return this.roomContext.JoinedUserList;
        }

        ////退室
        //public async Task LeavedAsync()
        //{
        //    //グループストレージからRoomData取得
        //    var roomStorage = this.room.GetInMemoryStorage<RoomData>();
        //    var roomData = roomStorage.Get(this.ConnectionId);

        //    ////マスタークライアントだったら次の人に譲渡する
        //    //if (roomData.JoinedUser.IsMaster == true)
        //    //{
        //    //    await MasterLostAsync();
        //    //}

        //    //グループデータから削除
        //    this.room.GetInMemoryStorage<RoomData>().Remove(this.ConnectionId);

        //    var joinedUser = new JoinedUser() { ConnectionId = this.ConnectionId };

        //    //ルーム参加者全員に、ユーザーの退室通知を送信
        //    this.Broadcast(room).OnLeave(joinedUser);

        //    //ルーム内のメンバーから自分を削除
        //    await room.RemoveAsync(this.Context);

        //}
    }
}
