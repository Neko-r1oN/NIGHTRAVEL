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
    public class RoomHub :  StreamingHubBase<IRoomHub,IRoomHubReceiver>,IRoomHub
    {
       private IGroup<IRoomHubReceiver> room;

        //入室
        public async Task<JoinedUser[]> JoinedAsync(string roomName, int userId)
        {
            //ルームに参加＆ルームを保持
            this.room = await this.Group.AddAsync(roomName);

            //DBからユーザー情報取得
            GameDbContext context = new GameDbContext();
            var user = context.Users.Where(user => user.id == userId).First();

            //グループストレージにユーザーデータを格納
            var roomStorage = room.CountAsync();
            var joinedUser = new JoinedUser() { ConnectionId = this.ConnectionId, UserData = user };

            ////1人目をマスタークライアントにする
            //if (roomStorage.AllValues.Count == 0) { joinedUser.IsMaster = true; }

            //ルーム参加者全員に、ユーザーの入室通知を送信
            this.room.Except(this.ConnectionId).Onjoin(joinedUser);

            var roomData = new RoomData() { JoinedUser = joinedUser };
            roomStorage.AsTask(this.ConnectionId, roomData);



            RoomData[] roomDataList = roomStorage.AllValues.ToArray<RoomData>();

            //参加中のユーザー情報を返す
            JoinedUser[] joinedUserList = new JoinedUser[roomDataList.Length];

            for (int i = 0; i < roomDataList.Length; i++)
            {
                joinedUserList[i] = roomDataList[i].JoinedUser;
            }

            return joinedUserList;
        }

        //退室
        public async Task LeavedAsync()
        {
            //グループストレージからRoomData取得
            var roomStorage = this.room.GetInMemoryStorage<RoomData>();
            var roomData = roomStorage.Get(this.ConnectionId);

            ////マスタークライアントだったら次の人に譲渡する
            //if (roomData.JoinedUser.IsMaster == true)
            //{
            //    await MasterLostAsync();
            //}

            //グループデータから削除
            this.room.GetInMemoryStorage<RoomData>().Remove(this.ConnectionId);

            var joinedUser = new JoinedUser() { ConnectionId = this.ConnectionId };

            //ルーム参加者全員に、ユーザーの退室通知を送信
            this.Broadcast(room).OnLeave(joinedUser);

            //ルーム内のメンバーから自分を削除
            await room.RemoveAsync(this.Context);

        }
    }
}
