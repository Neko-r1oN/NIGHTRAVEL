////////////////////////////////////////////////////////////////
///
/// RoomHub�ւ̐ڑ����Ǘ�����X�N���v�g
/// 
/// Aughter:�ؓc�W��
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

    //�ڑ�ID
    public Guid ConnectionId { get; set; }
    //���[�U�[�ڑ��ʒm
    public Action<JoinedUser> OnJoinedUser { get; set; }

    //���[�U�[�ގ��ʒm
    public Action<JoinedUser> OnLeavedUser { get; set; }

    //�ʒu��]����
    public Action<JoinedUser, Vector2, Quaternion, CharacterState> OnMovePlayerSyn { get; set; }

    //�E�o�ʒm
    public Action<JoinedUser> OnEscapeCharacter { get; set; }

    //�G�̏o������
    public Action<EnemyData, Vector3> OnSpawndEnemy { get; set; }

    //�Ă���Id����
    public Action<int> OnIdAsyncEnemy { get; set; }

    //�G�̈ړ�����
    public Action<int , Vector2 , Quaternion , EnemyAnimState> OnMoveEnemySyn { get; set; }

    //�G�̌��j����
    public Action<string> OnExcusionedEnemy { get; set; }

    //�}�X�^�[�N���C�A���g���n
    public Action<JoinedUser> OnMasteredClient { get; set; }

    //�I�u�W�F�N�g�̈ړ���]����
    public Action<string, Vector3, Quaternion> OnMovedObject { get; set; }

    //�����b�N�̐�������
    public Action<int, Vector2> OnSpawnedRelic {  get; set; }

    //�����b�N�̎擾����
    public Action<int , string> OnGotRelic {  get; set; }

    //�M�~�b�N�̋N������
    public Action<GimmickData> OnBootedGimmick { get; set; }

    //��Փx�㏸����
    public Action<int> OnAscendDifficultySyn {  get; set; }

    //���X�e�[�W�i�s����
    public Action<int> OnAdanceNextStageSyn { get; set; }

    //�G�̗͑�������
    public Action<int,float> OnEnemyHealthSyn { get; set; }

    //MagicOnion�ڑ�����
    public async UniTask ConnectAsync()
    {
        var channel = GrpcChannelx.ForAddress(ServerURL);
        roomHub = await StreamingHubClient.ConnectAsync<IRoomHub, IRoomHubReceiver>(channel, this);
    }

    //MagicOnion�ؒf����
    public async UniTask DisconnectAsync()
    {
        if (roomHub != null) await roomHub.DisposeAsync();
        if (channel != null) await channel.ShutdownAsync();
        roomHub = null; channel = null;
    }

    //�j������
    async void OnDestroy()
    {
        DisconnectAsync();
    }

    /// <summary>
    /// ����
    /// Aughter:�ؓc�W��
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

    //�����ʒm(IRoomHubReceiver�C���^�[�t�F�C�X�̎���)
    public void Onjoin(JoinedUser joinedUser)
    {
        OnJoinedUser?.Invoke(joinedUser);
    }

    //�ގ�
    public async UniTask LeaveAsync()
    {
        await roomHub.LeavedAsync();
    }

    //�ގ��ʒm
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
    /// �v���C���[�̈ړ��ʒm
    /// Aughter:�ؓc�W��
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
    /// �G�̈ړ��ʒm
    /// Aughter:�ؓc�W��
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
    /// �����b�N����
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="relicID"></param>
    /// <param name="pos"></param>
    public void OnSpawnRelic(int relicID, Vector2 pos)
    {
        OnSpawnedRelic(relicID, pos);
    }

    /// <summary>
    /// �����b�N�擾
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="relicID"></param>
    /// <param name="rekicName"></param>
    public void OnGetRelic(int relicID, string rekicName)
    {
        OnGotRelic(relicID, rekicName);
    }

    /// <summary>
    /// �G�̐���
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="enemData"></param>
    /// <param name="pos"></param>
    public void OnSpawnEnemy(EnemyData enemData, Vector2 pos)
    {
        OnSpawndEnemy(enemData, pos);
    }

    /// <summary>
    /// �M�~�b�N�̋N��
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="gimmickData"></param>
    public void OnBootGimmick(GimmickData gimmickData)
    {
        OnBootedGimmick(gimmickData);
    }

    /// <summary>
    /// ��Փx�㏸
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="difID"></param>
    public void OnAscendDifficulty(int difID)
    {
        OnAscendDifficultySyn(difID);
    }

    /// <summary>
    /// ���X�e�[�W�i�s
    /// Aughter:�ؓc�W��
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
    /// �G�̗͑���
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="enemID"></param>
    /// <param name="enemHP"></param>
    public void OnEnemyHealth(int enemID, float enemHP)
    {
        OnEnemyHealthSyn(enemID, enemHP);
    }

    /// <summary>
    /// �G���S�ʒm
    /// Author:Nishiura
    /// </summary>
    /// <param name="enemID">�G����ID</param>
    public void OnKilledEnemy(int enemID)
    {

    }

    /// <summary>
    /// �o���l�ʒm
    /// Author:Nishiura
    /// </summary>
    /// <param name="exp">�o���l</param>
    public void OnEXP(int exp)
    {

    }

    /// <summary>
    /// ���x���A�b�v�ʒm
    /// </summary>
    public void OnLevelUp()
    {

    }

    public void OnPlayerDead(int playerID)
    {

    }

    ////�G�̏o��
    //public async UniTask SpawnEnemyAsync(string enemyName, Vector3 pos)
    //{
    //    await roomHub.SpawnAsync(enemyName, pos);
    //}

    ////�Ă���Id���M
    //public async UniTask EnemyIdAsync(int enemyId)
    //{
    //    await roomHub.EnemyIdAsync(enemyId);
    //}

    ////�G��ID�ʒm
    //public void OnIdEnemy(int enemyId)
    //{
    //    OnIdAsyncEnemy(enemyId);
    //}

    ////�G�̈ړ���]
    //public void OnMoveEnemy(string enemyName, Vector3 pos, Quaternion rot)
    //{
    //    OnMovedEnemy(enemyName, pos, rot);
    //}

    ////�G�̈ړ���]����
    //public async UniTask MoveEnemyAsync(string enemyName, Vector3 pos, Quaternion rot)
    //{
    //    await roomHub.EnemyMoveAsync(enemyName, pos, rot);
    //}

    ////�G�̌��j
    //public void OnExcusionEnemy(string enemyName)
    //{
    //    OnExcusionedEnemy(enemyName);
    //}

    ////�G�̌��j����
    //public async UniTask ExcusionEnemyAsync(string enemyName)
    //{
    //    await roomHub.EnemyExcusionAsync(enemyName);
    //}

    ////�}�X�^�[�N���C�A���g���n
    //public async UniTask MasterLostAsync()
    //{
    //    await roomHub.MasterLostAsync();
    //}

    ////�I�u�W�F�N�g�̐�������
    //public async UniTask ObjectSpawnAsync(string objectName, Vector3 pos, Quaternion rot, Vector3 fow)
    //{
    //    await roomHub.ObjectSpawnAsync(ConnectionId, objectName, pos, rot, fow);
    //}

    ////�I�u�W�F�N�g�̐���
    //public void OnObjectSpawn(Guid connectionId, string objectName, Vector3 pos, Quaternion rot, Vector3 fow)
    //{
    //    OnSpawnObject(connectionId, objectName, pos, rot, fow);
    //}

    ////�I�u�W�F�N�g�̈ړ�����
    //public async UniTask ObjectMoveAsync(string objectName, Vector3 pos, Quaternion rot)
    //{
    //    await roomHub.ObjectMoveAsync(objectName, pos, rot);
    //}

    ////�I�u�W�F�N�g�̈ړ�
    //public void OnObjectMove(string objectName, Vector3 pos, Quaternion rot)
    //{
    //    OnMovedObject(objectName, pos, rot);
    //}
}
