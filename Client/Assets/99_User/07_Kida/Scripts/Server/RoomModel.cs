////////////////////////////////////////////////////////////////
///
/// RoomHubへの接続を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using Grpc.Net.Client;
using MagicOnion.Client;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.IRoomHubReceiver;
using static UnityEditor.PlayerSettings;
using static UnityEngine.Rendering.DebugUI.Table;

public class RoomModel : BaseModel, IRoomHubReceiver
{
    private GrpcChannel channel;
    private IRoomHub roomHub;
    public bool IsMaster { get; set; }

    //接続ID
    public Guid ConnectionId { get; set; }
    //ユーザー接続通知
    public Action<JoinedUser> OnJoinedUser { get; set; }

    //ユーザー退室通知
    public Action<JoinedUser> OnLeavedUser { get; set; }

    //位置回転同期
    public Action<JoinedUser, Vector2, Quaternion, CharacterState> OnMovePlayerSyn { get; set; }

    //脱出通知
    public Action<JoinedUser> OnEscapeCharacter { get; set; }

    //敵の出現処理
    public Action<List<EnemyData>, Vector3> OnSpawndEnemy { get; set; }

    //てきのId同期
    public Action<int> OnIdAsyncEnemy { get; set; }

    //敵の移動同期
    public Action<int , Vector2 , Quaternion , EnemyAnimState> OnMoveEnemySyn { get; set; }

    //敵の撃破同期
    public Action<string> OnExcusionedEnemy { get; set; }

    //マスタークライアント譲渡
    public Action<JoinedUser> OnMasteredClient { get; set; }

    ////オブジェクトの生成同期
    //public Action<Guid, string, Vector3, Quaternion, Vector3> OnSpawnObject { get; set; }

    //オブジェクトの移動回転同期
    public Action<string, Vector3, Quaternion> OnMovedObject { get; set; }

    //レリックの生成同期
    public Action<int, Vector2> OnSpawnedRelic {  get; set; }

    //レリックの取得同期
    public Action<int , string> OnGotRelic {  get; set; }

    //MagicOnion接続処理
    public async UniTask ConnectAsync()
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        channel = GrpcChannel.ForAddress(ServerURL,
            new GrpcChannelOptions() { HttpHandler = handler });
        roomHub = await StreamingHubClient.ConnectAsync<IRoomHub, IRoomHubReceiver>(channel, this);
    }

    //MagicOnion切断処理
    public async UniTask DisconnectAsync()
    {
        if (roomHub != null) await roomHub.DisposeAsync();
        if (channel != null) await channel.ShutdownAsync();
        roomHub = null; channel = null;
    }

    //破棄処理
    async void OnDestroy()
    {
        DisconnectAsync();
    }

    ////入室
    //public async UniTask JoinedAsync(string roomName, int userId)
    //{
    //    JoinedUser[] users = await roomHub.JoinedAsync(roomName, userId);
    //    foreach (var user in users)
    //    {
    //        if (user.UserData.Id == userId)
    //            this.ConnectionId = user.ConnectionId;
    //        this.IsMaster = user.IsMaster;
    //        OnJoinedUser(user);
    //    }

    //}

    //入室通知(IRoomHubReceiverインターフェイスの実装)
    public void Onjoin(JoinedUser joinedUser)
    {
        OnJoinedUser(joinedUser);
    }

    //退室
    public async UniTask LeaveAsync()
    {
        await roomHub.LeavedAsync();
    }

    //退室通知
    public void OnLeave(JoinedUser user)
    {
        OnLeavedUser(user);
    }

    //public void OnMasterClient(JoinedUser user)
    //{
    //    OnMasteredClient(user);
    //}

    ////移動
    //public async Task MoveAsync(Vector3 pos, Quaternion rot, int anim)
    //{
    //    await roomHub.MoveAsync(pos, rot, anim);
    //}

    /// <summary>
    /// プレイヤーの移動通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="user"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    /// <param name="animID"></param>
    public void OnMovePlayer(JoinedUser user, Vector2 pos, Quaternion rot, CharacterState animID)
    {
        OnMovePlayerSyn(user, pos, rot, animID);
    }

    /// <summary>
    /// 敵の移動通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="enemID"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    /// <param name="animID"></param>
    public void OnMoveEnemy(int enemID, Vector2 pos, Quaternion rot, EnemyAnimState animID)
    {
        OnMoveEnemySyn(enemID, pos, rot, animID);
    }

    /// <summary>
    /// レリック生成
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="relicID"></param>
    /// <param name="pos"></param>
    public void OnSpawnRelic(int relicID, Vector2 pos)
    {
        OnSpawnedRelic(relicID, pos);
    }

    /// <summary>
    /// レリック取得
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="relicID"></param>
    /// <param name="rekicName"></param>
    public void OnGetRelic(int relicID, string rekicName)
    {
        OnGotRelic(relicID, rekicName);
    }

    public void OnSpawnEnemy(List<EnemyData> enemData, Vector2 pos)
    {
        OnSpawndEnemy(enemData, pos);
    }



    ////敵の出現通知
    //public void OnSpawn(string enemyName, Vector3 pos)
    //{
    //    OnSpawnEnemy(enemyName, pos);
    //}

    ////敵の出現
    //public async UniTask SpawnEnemyAsync(string enemyName, Vector3 pos)
    //{
    //    await roomHub.SpawnAsync(enemyName, pos);
    //}

    ////てきのId送信
    //public async UniTask EnemyIdAsync(int enemyId)
    //{
    //    await roomHub.EnemyIdAsync(enemyId);
    //}

    ////敵のID通知
    //public void OnIdEnemy(int enemyId)
    //{
    //    OnIdAsyncEnemy(enemyId);
    //}

    ////敵の移動回転
    //public void OnMoveEnemy(string enemyName, Vector3 pos, Quaternion rot)
    //{
    //    OnMovedEnemy(enemyName, pos, rot);
    //}

    ////敵の移動回転同期
    //public async UniTask MoveEnemyAsync(string enemyName, Vector3 pos, Quaternion rot)
    //{
    //    await roomHub.EnemyMoveAsync(enemyName, pos, rot);
    //}

    ////敵の撃破
    //public void OnExcusionEnemy(string enemyName)
    //{
    //    OnExcusionedEnemy(enemyName);
    //}

    ////敵の撃破同期
    //public async UniTask ExcusionEnemyAsync(string enemyName)
    //{
    //    await roomHub.EnemyExcusionAsync(enemyName);
    //}

    ////マスタークライアント譲渡
    //public async UniTask MasterLostAsync()
    //{
    //    await roomHub.MasterLostAsync();
    //}

    ////オブジェクトの生成同期
    //public async UniTask ObjectSpawnAsync(string objectName, Vector3 pos, Quaternion rot, Vector3 fow)
    //{
    //    await roomHub.ObjectSpawnAsync(ConnectionId, objectName, pos, rot, fow);
    //}

    ////オブジェクトの生成
    //public void OnObjectSpawn(Guid connectionId, string objectName, Vector3 pos, Quaternion rot, Vector3 fow)
    //{
    //    OnSpawnObject(connectionId, objectName, pos, rot, fow);
    //}

    ////オブジェクトの移動同期
    //public async UniTask ObjectMoveAsync(string objectName, Vector3 pos, Quaternion rot)
    //{
    //    await roomHub.ObjectMoveAsync(objectName, pos, rot);
    //}

    ////オブジェクトの移動
    //public void OnObjectMove(string objectName, Vector3 pos, Quaternion rot)
    //{
    //    OnMovedObject(objectName, pos, rot);
    //}
}
