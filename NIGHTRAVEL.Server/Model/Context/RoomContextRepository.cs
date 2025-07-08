using Cysharp.Runtime.Multicast;
using System.Collections.Concurrent;

namespace NIGHTRAVEL.Server.Model.Context
{
    public class RoomContextRepository(IMulticastGroupProvider groupProvider)
    {
        private readonly ConcurrentDictionary<string, RoomContext> contexts = new();
        //ゲームコンテキストの作成
        public RoomContext CreateContext(string roomName)
        {
            var context = new RoomContext(groupProvider, roomName);
            contexts[roomName] = context;
            return context;
        }
        //ゲームコンテキストの取得
        public RoomContext GetContext(string roomName)
        {
            return contexts[roomName];
        }
        //ゲームコンテキストの削除
        public void RemoveContext(string roomName)
        {
            if(contexts.Remove(roomName, out var roomContext))
            {
                roomContext?.Dispose();
            }
        }
    }
}
