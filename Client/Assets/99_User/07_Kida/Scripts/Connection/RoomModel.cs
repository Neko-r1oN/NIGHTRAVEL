////////////////////////////////////////////////////////////////
///
/// RoomHub�ւ̐ڑ����Ǘ�����X�N���v�g
/// 
/// Aughter:�ؓc�W��
///
////////////////////////////////////////////////////////////////

#region using�ꗗ
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
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.IRoomHubReceiver;
using static UnityEditor.PlayerSettings;
using static UnityEngine.Rendering.DebugUI.Table;
#endregion

public class RoomModel : BaseModel, IRoomHubReceiver
{
    private GrpcChannel channel;  //�T�[�o�[URL
    private IRoomHub roomHub;     //roomHub�̊֐����Ăяo�����Ɏg��

    //�}�X�^�[�N���C�A���g���ǂ���
    public bool IsMaster { get; set; }

    //�ڑ�ID
    public Guid ConnectionId { get; private set; }

    #region �ʒm��`�ꗗ

    public Dictionary<Guid, JoinedUser> joinedUserList { get; private set; } = new Dictionary<Guid, JoinedUser>();

    //���[�U�[�ڑ��ʒm
    public Action<JoinedUser> OnJoinedUser { get; set; }

    //���[�U�[�ގ��ʒm
    public Action<JoinedUser> OnLeavedUser { get; set; }

    //���������ʒm
    public Action<Guid> OnReadySyn {  get; set; }

    //�Q�[���J�n�ʒm
    public Action OnStartedGame { get; set; }

    //�v���C���[�ʒu��]�ʒm
    public Action<PlayerData> OnMovePlayerSyn { get; set; }

    //�}�X�^�[�N���C�A���g�̕ύX�ʒm
    public Action OnChangedMasterClient {  get; set; }

    //�}�X�^�[�N���C�A���g�̍X�V�ʒm
    public Action<MasterClientData> OnUpdateMasterClientSyn { get; set; }

    //�E�o�ʒm
    public Action<JoinedUser> OnEscapeCharacter { get; set; }

    //�G�̏o���ʒm
    public Action<EnemyData, Vector3> OnSpawndEnemy { get; set; }

    //�G�̈ړ��ʒm
    public Action<int , Vector2 , Quaternion , EnemyAnimState> OnMoveEnemySyn { get; set; }

    //�G�̌��j�ʒm
    public Action<string> OnExcusionedEnemy { get; set; }

    //�}�X�^�[�N���C�A���g���n
    public Action<JoinedUser> OnMasteredClient { get; set; }

    //�I�u�W�F�N�g�̈ړ���]�ʒm
    public Action<string, Vector3, Quaternion> OnMovedObject { get; set; }

    //�����b�N�̐����ʒm
    public Action<int, Vector2> OnSpawnedRelic {  get; set; }

    //�����b�N�̎擾�ʒm
    public Action<int , string> OnGotRelic {  get; set; }

    //�M�~�b�N�̋N���ʒm
    public Action<GimmickData> OnBootedGimmick { get; set; }

    //��Փx�㏸�ʒm
    public Action<int> OnAscendDifficultySyn {  get; set; }

    //���X�e�[�W�i�s�ʒm
    public Action<int> OnAdanceNextStageSyn { get; set; }

    //�v���C���[�̗͑����ʒm
    public Action<int,float> OnPlayerHealthSyn {  get; set; }

    //�G�̗͑����ʒm
    public Action<int,float> OnEnemyHealthSyn { get; set; }

    //�G���S�ʒm
    public Action<int> OnKilledEnemySyn {  get; set; }

    //�o���l�ʒm
    public Action<int> OnEXPSyn {  get; set; }

    //���x���A�b�v�ʒm
    public Action OnLevelUpSyn { get; set; }

    //�v���C���[�_�E���ʒm
    public Action<int> OnPlayerDeadSyn {  get; set; }

    //�_���[�W�\�L�ʒm
    public Action<int> OnDamaged {  get; set; }
    #endregion

    #region RoomModel�C���X�^���X����
    private static RoomModel instance;
    public static RoomModel Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            // �C���X�^���X���������݂��Ȃ��悤�ɁA���ɑ��݂��Ă����玩�g����������
            Destroy(gameObject);
        }
    }

    //public static RoomModel Instance
    //{
    //    get
    //    {
    //        // GET�v���p�e�B���Ă΂ꂽ�Ƃ��ɃC���X�^���X���쐬����(����̂�)
    //        if (instance == null)
    //        {
    //            GameObject gameObj = new GameObject("RoomModel"+DateTime.Now.ToString());
    //            instance = gameObj.AddComponent<RoomModel>();
    //            DontDestroyOnLoad(gameObj);
    //        }
    //        return instance;
    //    }
    //}
    #endregion

    private void OnDisable()
    {
        Debug.Log("���ɂ܂���");
    }

    #region MagicOnion�ڑ��E�ؒf����
    /// <summary>
    /// MagicOnion�ڑ�����
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <returns></returns>
    public async UniTask ConnectAsync()
    {
        var channel = GrpcChannelx.ForAddress(ServerURL);
        roomHub = await StreamingHubClient.ConnectAsync<IRoomHub, IRoomHubReceiver>(channel, this);
    }

    /// <summary>
    /// MagicOnion�ؒf����
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <returns></returns>
    public async UniTask DisconnectAsync()
    {
        if (roomHub != null) await roomHub.DisposeAsync();
        if (channel != null) await channel.ShutdownAsync();
        roomHub = null; channel = null;
    }
    #endregion

    /// <summary>
    /// �j������
    /// Aughter:�ؓc�W��
    /// </summary>
    async void OnDestroy()
    {
        DisconnectAsync();
    }

    #region �����̏���
    #region �����E�ގ��E������������
    /// <summary>
    /// ��������
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <returns></returns>
    public async UniTask JoinAsync(string roomName, int userId)
    {
        joinedUserList = await roomHub.JoinedAsync(roomName, userId);
        foreach (var user in joinedUserList)
        {
            if (user.Value.UserData.Id == userId)
            {
                this.ConnectionId = user.Value.ConnectionId;
                Debug.Log("���f���F" + RoomModel.Instance.ConnectionId);
            }
        }
    }

    /// <summary>
    /// �}�X�^�[�N���C�A���g�̍X�V����
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="masterClient"></param>
    /// <returns></returns>
    public async UniTask UpdateMasterClientAsync(MasterClientData masterClient)
    {
        await roomHub.UpdateMasterClientAsync(masterClient);
    }

    /// <summary>
    /// �ގ��̓���
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <returns></returns>
    public async UniTask LeaveAsync()
    {
        await roomHub.LeavedAsync();
        //���������X�g�������
        joinedUserList.Clear();
    }

    /// <summary>
    /// ������������
    /// </summary>
    /// <returns></returns>
    public async Task ReadyAsync()
    {
        await roomHub.ReadyAsync();
    }
    #endregion
    #region �v���C���[�����֘A
    /// <summary>
    /// �v���C���[�̈ړ�����
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <returns></returns>
    public async Task MovePlayerAsync(PlayerData playerData)
    {
        await roomHub.MovePlayerAsync(playerData);
    }

    public async Task UpdatePlayerAsync(PlayerData playerData)
    {
        await roomHub.UpdatePlayerAsync(playerData);
    }

    /// <summary>
    /// �v���C���[�̗̑͑�������
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="playerHP"></param>
    /// <returns></returns>
    public async Task PlayerHealthAsync(int playerID, float playerHP)
    {
        await roomHub.PlayerHealthAsync(playerID, playerHP);
    }

    /// <summary>
    /// �v���C���[�̃��x���A�b�v����
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <returns></returns>
    public async Task LevelUpAsync()
    {
        await roomHub.LevelUpAsync();
    }

    /// <summary>
    /// �v���C���[�_�E������
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="playerID"></param>
    /// <returns></returns>
    public async Task PlayerDeadAsync(int playerID)
    {
        await roomHub.PlayerDeadAsync(playerID);
    }

    /// <summary>
    /// �o���l����
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public async Task EXPAsync(int exp)
    {
        await roomHub.EXPAsync(exp);
    }

    #endregion
    #region �G�����֘A
    /// <summary>
    /// �G�̈ʒu����
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="enemIDList"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    /// <param name="anim"></param>
    /// <returns></returns>
    public async Task MoveEnemyAsync(List<int> enemIDList, Vector2 pos, Quaternion rot, EnemyAnimState anim)
    {
        await roomHub.MoveEnemyAsync(enemIDList, pos, rot, anim);
    }

    /// <summary>
    /// �G�̐�������
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="enemID"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public async Task SpawnEnemyAsync(List<int> enemID, Vector2 pos)
    {
        await roomHub.SpawnEnemyAsync(enemID, pos);
    }

    /// <summary>
    /// �G�̗͑�������
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="enemID"></param>
    /// <param name="enemHP"></param>
    /// <returns></returns>
    public async Task EnemyHealthAsync(int enemID, float enemHP)
    {
        await roomHub.EnemyHealthAsync(enemID, enemHP);
    }

    /// <summary>
    /// �G���S����
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="enemID"></param>
    /// <returns></returns>
    public async Task KilledEnemyAsync(int enemID)
    {
        await roomHub?.KilledEnemyAsync(enemID);
    }
    #endregion
    #region �����b�N�����֘A
    /// <summary>
    /// �����b�N��������
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public async Task SpawnRelicAsync(Vector2 pos)
    {
        await roomHub.SpawnRelicAsync(pos);
    }

    /// <summary>
    /// �����b�N�擾����
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="relicID"></param>
    /// <param name="relicName"></param>
    /// <returns></returns>
    public async Task GetRelicAsync(int relicID, string relicName)
    {
        await roomHub.GetRelicAsync(relicID, relicName);
    }
    #endregion
    #region �Q�[����UI�E�d�l�̓����֘A
    /// <summary>
    /// �M�~�b�N�̋N������
    ///  Aughter:�ؓc�W��
    /// </summary>
    /// <param name="gimID"></param>
    /// <returns></returns>
    public async Task BootGimmickAsync(int gimID)
    {
        await roomHub.BootGimmickAsync(gimID);
    }

    /// <summary>
    /// ��Փx�㏸�̓���
    /// </summary>
    /// <param name="difID"></param>
    /// <returns></returns>
    public async Task AscendDifficultyAsync(int difID)
    {
        await roomHub.AscendDifficultyAsync(difID);
    }

    /// <summary>
    /// ���X�e�[�W�i�s�̓���
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="stageID"></param>
    public async Task AdvanceNextStageAsync(int stageID, bool isBossStage)
    {
        await roomHub.AdvanceNextStageAsync(stageID, isBossStage);
    }

    /// <summary>
    /// �_���[�W�\�L����
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="dmg"></param>
    /// <returns></returns>
    public async Task DamageAsync(int dmg)
    {
        await roomHub.DamageAsync(dmg);
    }

    #endregion
    #endregion

    #region �ʒm�̏���
    #region �����E�ގ��E���������ʒm
    /// <summary>
    /// �����ʒm(IRoomHubReceiver�C���^�[�t�F�C�X�̎���)
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="joinedUser"></param>
    public void Onjoin(JoinedUser joinedUser)
    {
        //OnJoinedUser?.Invoke(joinedUser);

        if (!joinedUserList.ContainsKey(joinedUser.ConnectionId))
            joinedUserList.Add(joinedUser.ConnectionId, joinedUser);

        //�����ʒm
        OnJoinedUser(joinedUser);

    }

    /// <summary>
    /// �ގ��ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="user"></param>
    public void OnLeave(JoinedUser joinedUser)
    {
        if (joinedUserList.ContainsKey(joinedUser.ConnectionId))
            joinedUserList.Remove(joinedUser.ConnectionId);
        OnLeavedUser(joinedUser);
    }

    /// <summary>
    /// ���������ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="conID"></param>
    public void OnReady(Guid conID)
    {
        OnReadySyn(conID);
    }
    #endregion
    #region �v���C���[�ʒm�֘A
    /// <summary>
    /// �v���C���[�̈ړ��ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="user"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    /// <param name="animID"></param>
    public void OnMovePlayer(PlayerData playerData)
    {
        OnMovePlayerSyn(playerData);
    }

    /// <summary>
    /// �}�X�^�[�N���C�A���g�̕ύX�ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnChangeMasterClient()
    {
        OnChangedMasterClient();
    }

    /// <summary>
    /// �}�X�^�[�N���C�A���g�̍X�V�ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="masterClientData"></param>
    public void OnUpdateMasterClient(MasterClientData masterClientData)
    {
        OnUpdateMasterClientSyn(masterClientData);
    }

    /// <summary>
    /// �v���C���[�̗̑͑����ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="playerHP"></param>
    public void OnPlayerHealth(int playerID, float playerHP)
    {
        OnPlayerHealthSyn(playerID, playerHP);
    }

    /// <summary>
    /// �v���C���[�_�E���ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="playerID"></param>
    public void OnPlayerDead(int playerID)
    {
        OnPlayerDeadSyn(playerID);
    }


    /// <summary>
    /// �v���C���[�̌o���l�ʒm
    /// Author:Nishiura
    /// </summary>
    /// <param name="exp">�o���l</param>
    public void OnEXP(int exp)
    {
        OnEXPSyn(exp);
    }

    /// <summary>
    /// �v���C���[�̃��x���A�b�v�ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnLevelUp()
    {
        OnLevelUpSyn();
    }
    #endregion
    #region �G�ʒm�֘A
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
    /// �G�̐����ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="enemData"></param>
    /// <param name="pos"></param>
    public void OnSpawnEnemy(EnemyData enemData, Vector2 pos)
    {
        OnSpawndEnemy(enemData, pos);
    }

    /// <summary>
    /// �G�̗͑����ʒm
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
        OnKilledEnemySyn(enemID);
    }
    #endregion
    #region �����b�N�ʒm�֘A
    /// <summary>
    /// �����b�N�����ʒm
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
    #endregion
    #region �Q�[����UI�E�d�l�̓����֘A
    /// <summary>
    /// �Q�[���J�n�ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnStartGame()
    {
        OnStartedGame();
    }

    /// <summary>
    /// �M�~�b�N�̋N���ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="gimmickData"></param>
    public void OnBootGimmick(GimmickData gimmickData)
    {
        OnBootedGimmick(gimmickData);
    }

    /// <summary>
    /// ��Փx�㏸�̒ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="difID"></param>
    public void OnAscendDifficulty(int difID)
    {
        OnAscendDifficultySyn(difID);
    }

    /// <summary>
    /// ���X�e�[�W�i�s�̒ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="stageID"></param>
    public void OnAdanceNextStage(int stageID)
    {
        OnAdanceNextStageSyn(stageID);
    }

    /// <summary>
    /// �_���[�W�\�L�ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="dmg"></param>
    public void OnDamage(int dmg)
    {
        OnDamaged(dmg);
    }
    #endregion
    #endregion
}
