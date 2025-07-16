//=============================
// ルームコンテキストスクリプト
// Author:木田晃輔
//=============================
using Cysharp.Runtime.Multicast;
using NIGHTRAVEL.Server.StreamingHubs;
using Shared.Interfaces.StreamingHubs;

namespace NIGHTRAVEL.Server.Model.Context
{
    public class RoomContext
    {   
        /// <summary>
        /// コンテキストID
        /// Author:Kida
        /// </summary>
        public Guid Id { get;}

        /// <summary>
        /// ルーム名
        /// Author:Kida
        /// </summary>
        public string Name { get;}

        /// <summary>
        /// グループ
        /// Author:Kida
        /// </summary>
        public IMulticastSyncGroup<Guid, IRoomHubReceiver> Group {  get;}

        /// <summary>
        /// 参加者リスト
        /// Author:Kida
        /// </summary>
        public JoinedUser[] JoinedUserList { get; set; }

        /// <summary>
        /// ルームデータリスト
        /// Author:Nishiura
        /// </summary>
        public Dictionary<Guid, RoomData> roomDataList { get; } = new();

        /// <summary>
        /// エネミーデータリスト
        /// Author:Nishiura
        /// </summary>
        public Dictionary<int, EnemyData> enemyDataList { get; } = new();

        /// <summary>
        /// ギミックデータリスト
        /// Author:Nishiura
        /// </summary>
        public Dictionary<int, GimmickData> gimmickDataList { get; } = new();

        //[その他、ゲームのルームデータをフィールドに保存]

        public RoomContext(IMulticastGroupProvider groupProvider,string roomName)
        {
            Id = Guid.NewGuid();
            Name = roomName;
            Group =
                groupProvider.GetOrAddSynchronousGroup<Guid, IRoomHubReceiver>(roomName);
        }

        /// <summary>
        /// データ追加処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID">自身の接続ID</param>
        public void AddRoomData(Guid conID)
        {
            // データリストに自身のデータを追加
            roomDataList.Add(conID,new RoomData());
        }

        /// <summary>
        /// データ削除処理
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID">自身の接続ID</param>
        public void RemoveRoomData(Guid conID)
        {
            // データリストから自身のデータを消去
            roomDataList.Remove(conID);
        }

        public void Dispose() { 
            Group.Dispose();
        }

        /// <summary>
        /// ユーザ情報を渡す関数
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID"></param>
        public RoomData GetRoomData(Guid conID)
        {
            return roomDataList[conID];
        }

        /// <summary>
        /// 敵情報を渡す関数
        /// Author:Nishiura
        /// </summary>
        /// <param name="conID"></param>
        public EnemyData GetEnemyData(int enemID)
        {
            return enemyDataList[enemID];
        }

        /// <summary>
        /// ギミック情報を渡す関数
        /// Author:Nishiura
        /// </summary>
        /// <param name="gimID"></param>
        /// <returns></returns>
        public GimmickData GetGimmickData(int gimID)
        {
            return gimmickDataList[gimID];
        }


        /// <summary>
        /// ユーザーの退出処理
        /// Aughter:木田晃輔
        /// </summary>
        /// <param name="joinedUser"></param>
        /// <returns></returns>
        public void RemoveUser(int joinOrder)
        {
            if(JoinedUserList==null)
            {

            }
            else
            {
                //退出したユーザーを特定して削除
                JoinedUserList.Where( value => value == JoinedUserList[joinOrder]).ToArray();
            }
            
        }
    }
}
