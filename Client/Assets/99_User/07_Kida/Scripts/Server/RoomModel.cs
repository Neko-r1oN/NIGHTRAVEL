//////////////////////////////////////////////////////////////////
/////
///// RoomHubへの接続を管理するスクリプト
///// 
///// Aughter:木田晃輔
/////
//////////////////////////////////////////////////////////////////

//using Cysharp.Net.Http;
//using Cysharp.Threading.Tasks;
//using Grpc.Net.Client;
//using MagicOnion.Client;
//using Shared.Interfaces.StreamingHubs;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Unity.VisualScripting;
//using UnityEngine;
//using static Shared.Interfaces.StreamingHubs.IRoomHubReceiver;

//public class RoomModel : BaseModel, IRoomHubReceiver
//{
//    private GrpcChannel channel;
//    private IRoomHub roomHub;
//    public bool IsMaster { get; set; }

//    //接続ID
//    public Guid ConnectionId { get; set; }
//    //ユーザー接続通知
//    public Action<JoinedUser> OnJoinedUser { get; set; }

//    //ユーザー退室通知
//    public Action<JoinedUser> OnLeavedUser { get; set; }

//    //位置回転同期
//    public Action<JoinedUser, Vector3, Quaternion, int> OnMoveCharacter { get; set; }

//    //脱出通知
//    public Action<JoinedUser> OnEscapeCharacter { get; set; }

//    //敵の出現処理
//    public Action<string, Vector3> OnSpawnEnemy { get; set; }

//    //てきのId同期
//    public Action<int> OnIdAsyncEnemy { get; set; }

//    //敵の移動同期
//    public Action<string, Vector3, Quaternion> OnMovedEnemy { get; set; }

//    //敵の撃破同期
//    public Action<string> OnExcusionedEnemy { get; set; }

//    //マスタークライアント譲渡
//    public Action<JoinedUser> OnMasteredClient { get; set; }

//    ////オブジェクトの生成同期
//    //public Action<Guid, string, Vector3, Quaternion, Vector3> OnSpawnObject { get; set; }

//    //オブジェクトの移動回転同期
//    public Action<string, Vector3, Quaternion> OnMovedObject { get; set; }

//    //MagicOnion接続処理
//    public async UniTask ConnectAsync()
//    {
//        var handler = new YetAnotherHttpHandler() { Http2Only = true };
//        channel = GrpcChannel.ForAddress(ServerURL,
//            new GrpcChannelOptions() { HttpHandler = handler });
//        roomHub = await StreamingHubClient.ConnectAsync<IRoomHub, IRoomHubReceiver>(channel, this);
//    }

//    //MagicOnion切断処理
//    public async UniTask DisconnectAsync()
//    {
//        if (roomHub != null) await roomHub.DisposeAsync();
//        if (channel != null) await channel.ShutdownAsync();
//        roomHub = null; channel = null;
//    }

//    //破棄処理
//    async void OnDestroy()
//    {
//        DisconnectAsync();
//    }

//    ////入室
//    //public async UniTask JoinedAsync(string roomName, int userId)
//    //{
//    //    JoinedUser[] users = await roomHub.JoinedAsync(roomName, userId);
//    //    foreach (var user in users)
//    //    {
//    //        if (user.UserData.Id == userId)
//    //            this.ConnectionId = user.ConnectionId;
//    //        this.IsMaster = user.IsMaster;
//    //        OnJoinedUser(user);
//    //    }

//    //}

//    //入室通知(IRoomHubReceiverインターフェイスの実装)
//    public void Onjoin(Dictionary<Guid, JoinedUser> keys)
//    {
//        //OnJoinedUser(k);
//    }

//    //退室
//    public async UniTask LeaveAsync()
//    {
//        await roomHub.LeavedAsync();
//    }

//    //退室通知
//    public void OnLeave(JoinedUser user)
//    {
//        OnLeavedUser(user);
//    }

//    public void OnMasterClient(JoinedUser user)
//    {
//        OnMasteredClient(user);
//    }

//    ////移動
//    //public async Task MoveAsync(Vector3 pos, Quaternion rot, int anim)
//    //{
//    //    await roomHub.MoveAsync(pos, rot, anim);
//    //}

//    //移動通知
//    public void OnMovePlayer(JoinedUser user, Vector3 pos, Quaternion rot, CharacterState character)
//    {
//        //OnMoveCharacter(user, pos, rot, anim);
//    }
    
//    //移動通知
//    public void OnMoveEnemy(int enemy, Vector3 pos, Quaternion rot, EnemyAnimState character)
//    {
//        //OnMoveCharacter(user, pos, rot, anim);
//    }

//    ////敵の出現通知
//    //public void OnSpawn(string enemyName, Vector3 pos)
//    //{
//    //    OnSpawnEnemy(enemyName, pos);
//    //}

//    ////敵の出現
//    //public async UniTask SpawnEnemyAsync(string enemyName, Vector3 pos)
//    //{
//    //    await roomHub.SpawnAsync(enemyName, pos);
//    //}

//    ////てきのId送信
//    //public async UniTask EnemyIdAsync(int enemyId)
//    //{
//    //    await roomHub.EnemyIdAsync(enemyId);
//    //}

//    ////敵のID通知
//    //public void OnIdEnemy(int enemyId)
//    //{
//    //    OnIdAsyncEnemy(enemyId);
//    //}

//    ////敵の移動回転
//    //public void OnMoveEnemy(string enemyName, Vector3 pos, Quaternion rot)
//    //{
//    //    OnMovedEnemy(enemyName, pos, rot);
//    //}

//    ////敵の移動回転同期
//    //public async UniTask MoveEnemyAsync(string enemyName, Vector3 pos, Quaternion rot)
//    //{
//    //    await roomHub.EnemyMoveAsync(enemyName, pos, rot);
//    //}

//    ////敵の撃破
//    //public void OnExcusionEnemy(string enemyName)
//    //{
//    //    OnExcusionedEnemy(enemyName);
//    //}

//    ////敵の撃破同期
//    //public async UniTask ExcusionEnemyAsync(string enemyName)
//    //{
//    //    await roomHub.EnemyExcusionAsync(enemyName);
//    //}

//    ////マスタークライアント譲渡
//    //public async UniTask MasterLostAsync()
//    //{
//    //    await roomHub.MasterLostAsync();
//    //}

//    ////オブジェクトの生成同期
//    //public async UniTask ObjectSpawnAsync(string objectName, Vector3 pos, Quaternion rot, Vector3 fow)
//    //{
//    //    await roomHub.ObjectSpawnAsync(ConnectionId, objectName, pos, rot, fow);
//    //}

//    ////オブジェクトの生成
//    //public void OnObjectSpawn(Guid connectionId, string objectName, Vector3 pos, Quaternion rot, Vector3 fow)
//    //{
//    //    OnSpawnObject(connectionId, objectName, pos, rot, fow);
//    //}

//    ////オブジェクトの移動同期
//    //public async UniTask ObjectMoveAsync(string objectName, Vector3 pos, Quaternion rot)
//    //{
//    //    await roomHub.ObjectMoveAsync(objectName, pos, rot);
//    //}

//    ////オブジェクトの移動
//    //public void OnObjectMove(string objectName, Vector3 pos, Quaternion rot)
//    //{
//    //    OnMovedObject(objectName, pos, rot);
//    //}
//}
