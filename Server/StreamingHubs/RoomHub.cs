////////////////////////////////////////////////////////////////////////////
///
///  ルーム処理スクリプト
///  Author : 川口京佑  2025.01/28
///
////////////////////////////////////////////////////////////////////////////

using MagicOnion.Server.Hubs;
using MagicOnionServer.Model.Context;
using Newtonsoft.Json.Linq;
using Server.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using Shared.Model.Entity;
using System.Diagnostics;
using System;
using UnityEngine;


namespace StreamingHubs
{
    /// <summary>
    /// ルーム(内部処理)クラス
    /// </summary>
    public class RoomHub : StreamingHubBase<IRoomHub,IRoomHubReceiver>,IRoomHub
    {
        private IGroup room;

        //プレイヤー番号リスト
        private int[] numberList = {1,2,3,4};
        //ルーム最大収容人数
        private int MAX_PLAYER = 4;

     
        /// <summary>
        /// ユーザー入室処理
        /// </summary>
        /// <param name="roomName">参加ルーム名</param>
        /// <param name="userId">ユーザーID</param>
        /// <returns>参加者リスト</returns>
        public async Task<JoinedUser[]>JoinAsync(string roomName,int userId)
        {
            //ルームに参加＆ルームを保持
            this.room = await this.Group.AddAsync(roomName);

            //DBからユーザー情報を取得
            GameDbContext context = new GameDbContext();
            var user = context.Users.Where(user => user.Id == userId).First();

            //グループストレージにユーザー情報を格納
            var roomStorage = this.room.GetInMemoryStorage<RoomData>();

            //同時実行されないように lock で設定(排他的処理)
            lock (roomStorage)
            {
                //自身の入室番号を取得
                int joinOrderNum = GetJoinOrder(roomStorage.AllValues.ToArray<RoomData>());

                //参加ユーザーの情報を挿入
                var joinedUser = new JoinedUser()
                {
                    ConnectionId = this.ConnectionId,
                    UserData = user,
                    IsSelf = true,
                    JoinOrder = joinOrderNum
                };


                //ルームデータにユーザーとゲーム状態を挿入
                var roomData = new RoomData() { JoinedUser = joinedUser, UserState = new UserState(), MoveData = new MoveData() };

                //ストレージに保存
                roomStorage.Set(this.ConnectionId, roomData);

                //プライベートマッチで入室した際
                if (roomName != "Lobby")
                {
                    //通常通り通知
                    this.BroadcastExceptSelf(room).OnJoin(joinedUser);
                }

                Console.WriteLine("参加者名:" + joinedUser.UserData.Name);


                RoomData[] roomDataList = roomStorage.AllValues.ToArray<RoomData>();

                //参加中のユーザー情報を流す
                JoinedUser[] joinedUserList = new JoinedUser[roomDataList.Length];

                Debug.WriteLine("接続ID:" + this.ConnectionId);

                //RoomDataList内のJoinedUserを格納
                for (int i = 0; i < joinedUserList.Length; i++)
                {
                    joinedUserList[i] = roomDataList[i].JoinedUser;
                }

                //参加者が上限に
                if (roomDataList.Length == MAX_PLAYER && roomName != "Lobby")
                {
                    Console.WriteLine("ルーム名:" + roomName);
                    Console.WriteLine("ゲーム開始");

                    //準備確認を通知
                    this.Broadcast(room).OnStand();
                }
                return joinedUserList;
            }
        }

        /// <summary>
        /// マスタークライアント確認処理
        /// </summary>
        /// <param name="user">送信元ユーザー</param>
        /// <returns></returns>
        public async Task MastaerCheckAsync(JoinedUser user)
        {
            this.Broadcast(room).OnMasterCheck(user);
        }

        /// <summary>
        /// ロビー入室処理
        /// </summary>
        /// <param name="userId">参加ユーザーID</param>
        /// <returns></returns>
        public async Task JoinLobbyAsync(int userId)
        {
            JoinedUser[] joinedUserList = await JoinAsync("Lobby", userId);

            Console.WriteLine("参加");

            Console.WriteLine(joinedUserList.Length);

            //参加人数が４人になったら
            if (joinedUserList.Length == MAX_PLAYER)
            {
                //書式を桁無し指定にして再入室
                this.Broadcast(room).OnMatch(Guid.NewGuid().ToString("N"));
            }
        }


        /// <summary>
        /// 強制退出(切断)処理
        /// </summary>
        /// <returns></returns>
        protected override ValueTask OnDisconnected()
        {
            //ルームデータ削除
            this.room.GetInMemoryStorage<RoomData>().Remove(this.ConnectionId);
            //退出したことを全メンバーに通知
            this.Broadcast(room).Leave(this.ConnectionId);
            //ルーム内のメンバーから削除
            room.RemoveAsync(this.Context);

            return CompletedTask;
        }

        /// <summary>
        /// 退出処理
        /// </summary>
        /// <returns></returns>
        public async Task LeaveAsync()
        {
            //グループデータから削除
            this.room.GetInMemoryStorage<RoomData>().Remove(this.ConnectionId);

            //ルーム内のメンバーから自分を削除
            await room.RemoveAsync(this.Context);

            //ルーム参加者全員にユーザーの退出通知を送信
            this.BroadcastExceptSelf(room).Leave(this.ConnectionId);
        }

        /// <summary>
        /// 入室番号取得処理
        /// </summary>
        /// <param name="roomData">ルーム情報</param>
        /// <returns>対象者の入室番号</returns>
        int GetJoinOrder(RoomData[] roomData)
        {
            int joinOrder = 1;     //参加順変数

            int loop = 0;          //ループ用変数
            while (loop < roomData.Length)
            {
                loop = 0;
                for (int i = roomData.Length - 1; i >= 0; i--, loop++)
                {
                    if (roomData[i].JoinedUser.JoinOrder == joinOrder)
                    {
                        joinOrder++;
                        break;
                    }
                }
            }
            return joinOrder;
        }

        /// <summary>
        /// マッチング処理
        /// </summary>
        /// <param name="roomName">4番目に入室したユーザーのID</param>
        /// <returns></returns>
        public async Task MatchAsync(string roomName)
        {
            //ルーム参加者全員にマッチング通知を送信
           this.BroadcastExceptSelf(room).OnMatch(roomName);
        }

        public async Task StandAcync()
        {
            //準備状態通知
            this.Broadcast(room).OnStand();
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <returns></returns>
        public async Task MoveAsync(MoveData moveData)
        {
            //移動情報を自分のRoomDataに保存
            var roomStrage = this.room.GetInMemoryStorage<RoomData>();
            var roomData = roomStrage.Get(this.ConnectionId);
            roomData = new RoomData() { MoveData = moveData };

            //ルーム参加者全員にユーザーの移動通知を送信
            this.BroadcastExceptSelf(room).OnMove(moveData);
        }

      



        //ランキング取得処理
        int GetRanking(RoomData[] roomData)
        {
            int rankNum = 1;        //ランキング変数

            int loop = 0;           //ループ変数
            while (loop < roomData.Length)
            {
                loop = 0;
                for (int i = roomData.Length - 1; i >= 0; i--, loop++)
                {
                    if (roomData[i].UserState.Ranking == rankNum)
                    {
                        rankNum++;
                        break;
                    }
                }
            }
            return rankNum;
        }
    }
}