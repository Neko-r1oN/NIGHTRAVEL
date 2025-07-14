using Cysharp.Runtime.Multicast;
using Shared.Interfaces.StreamingHubs;

namespace NIGHTRAVEL.Server.Model.Context
{
    public class RoomContext
    {
        public Guid Id { get;} //コンテキストID
        public string Name { get;} //ルーム名
        public IMulticastSyncGroup<Guid, IRoomHubReceiver> Group {  get;}
        public Dictionary<Guid, JoinedUser> JoinedUserList { get; } = new(); //参加ユーザー一覧
        //[その他、ゲームのルームデータをフィールドに保存]

        public RoomContext(IMulticastGroupProvider groupProvider,string roomName)
        {
            Id = Guid.NewGuid();
            Name = roomName;
            Group =
                groupProvider.GetOrAddSynchronousGroup<Guid, IRoomHubReceiver>(roomName);
        }
        public void Dispose() { 
            Group.Dispose();
        }
    }
}
