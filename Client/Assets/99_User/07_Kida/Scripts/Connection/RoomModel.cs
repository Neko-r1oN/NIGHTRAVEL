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
using MagicOnion;
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
    public Action<EnemyData, Vector3> OnSpawndEnemy { get; set; }

    //てきのId同期
    public Action<int> OnIdAsyncEnemy { get; set; }

    //敵の移動同期
    public Action<int , Vector2 , Quaternion , EnemyAnimState> OnMoveEnemySyn { get; set; }

    //敵の撃破同期
    public Action<string> OnExcusionedEnemy { get; set; }

    //マスタークライアント譲渡
    public Action<JoinedUser> OnMasteredClient { get; set; }

    //オブジェクトの移動回転同期
    public Action<string, Vector3, Quaternion> OnMovedObject { get; set; }

    //レリックの生成同期
    public Action<int, Vector2> OnSpawnedRelic {  get; set; }

    //レリックの取得同期
    public Action<int , string> OnGotRelic {  get; set; }

    //ギミックの起動同期
    public Action<GimmickData> OnBootedGimmick { get; set; }

    //難易度上昇同期
    public Action<int> OnAscendDifficultySyn {  get; set; }

    //次ステージ進行同期
    public Action<int> OnAdanceNextStageSyn { get; set; }

    //敵体力増減同期
    public Action<int,float> OnEnemyHealthSyn { get; set; }

    //MagicOnion接続処理
    public async UniTask ConnectAsync()
    {
        var channel = GrpcChannelx.ForAddress(ServerURL);
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

    /// <summary>
    /// 入室
    /// Aughter:木田晃輔
    /// </summary>
    /// <returns></returns>
    public async UniTask JoinAsync(string roomName,int userId)
    {
       JoinedUser[] users = await roomHub.JoinedAsync(roomName,userId);
        foreach (var user in users)
        {
            if (user.UserData.Id == userId)
            {
                this.ConnectionId=user.ConnectionId;
                OnJoinedUser(user);
            }
        }
    }

    //入室通知(IRoomHubReceiverインターフェイスの実装)
    public void Onjoin(JoinedUser joinedUser)
    {
        OnJoinedUser?.Invoke(joinedUser);
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

    public void OnReady(Guid conID)
    {

    }

    public void OnStartGame()
    {

    }

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

    /// <summary>
    /// 敵の生成
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="enemData"></param>
    /// <param name="pos"></param>
    public void OnSpawnEnemy(EnemyData enemData, Vector2 pos)
    {
        OnSpawndEnemy(enemData, pos);
    }

    /// <summary>
    /// ギミックの起動
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="gimmickData"></param>
    public void OnBootGimmick(GimmickData gimmickData)
    {
        OnBootedGimmick(gimmickData);
    }

    /// <summary>
    /// 難易度上昇
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="difID"></param>
    public void OnAscendDifficulty(int difID)
    {
        OnAscendDifficultySyn(difID);
    }

    /// <summary>
    /// 次ステージ進行
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="stageID"></param>
    public void OnAdanceNextStage(int stageID)
    {
        OnAdanceNextStageSyn(stageID);
    }

    public void OnPlayerHealth(int playerID, float playerHP)
    {

    }

    /// <summary>
    /// 敵体力増減
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="enemID"></param>
    /// <param name="enemHP"></param>
    public void OnEnemyHealth(int enemID, float enemHP)
    {
        OnEnemyHealthSyn(enemID, enemHP);
    }

    /// <summary>
    /// 敵死亡通知
    /// Author:Nishiura
    /// </summary>
    /// <param name="enemID">敵識別ID</param>
    public void OnKilledEnemy(int enemID)
    {

    }

    /// <summary>
    /// 経験値通知
    /// Author:Nishiura
    /// </summary>
    /// <param name="exp">経験値</param>
    public void OnEXP(int exp)
    {

    }

    /// <summary>
    /// レベルアップ通知
    /// </summary>
    public void OnLevelUp()
    {

    }

    public void OnPlayerDead(int playerID)
    {

    }

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
