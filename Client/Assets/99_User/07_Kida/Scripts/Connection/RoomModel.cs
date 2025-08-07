////////////////////////////////////////////////////////////////
///
/// RoomHub�ւ̐ڑ����Ǘ�����X�N���v�g
/// 
/// Aughter:�ؓc�W��
///
////////////////////////////////////////////////////////////////

#region using�ꗗ
using Cysharp.Threading.Tasks;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using static Shared.Interfaces.StreamingHubs.IRoomHubReceiver;
using Vector2 = UnityEngine.Vector2;
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
    public Action<PlayerData> OnUpdatePlayerSyn { get; set; }

    //�}�X�^�[�N���C�A���g�̕ύX�ʒm
    public Action OnChangedMasterClient {  get; set; }

    //�}�X�^�[�N���C�A���g�̍X�V�ʒm
    public Action<MasterClientData> OnUpdateMasterClientSyn { get; set; }

    //�E�o�ʒm
    public Action<JoinedUser> OnEscapeCharacter { get; set; }

    //�G�̏o���ʒm
    public Action<List<SpawnEnemyData>> OnSpawndEnemy { get; set; }

    //�G�̍X�V�ʒm
    public Action<List<EnemyData>> OnUpdatedEnemy { get; set; }

    //�G�̌��j�ʒm
    public Action<string> OnExcusionedEnemy { get; set; }

    //�}�X�^�[�N���C�A���g���n
    public Action<JoinedUser> OnMasteredClient { get; set; }

    //�I�u�W�F�N�g�̈ړ���]�ʒm
    public Action<string, UnityEngine.Vector3, UnityEngine.Quaternion> OnMovedObject { get; set; }

    //�����b�N�̐����ʒm
    public Action<List<DropRelicData>> OnDropedRelic {  get; set; }

    //�����b�N�̎擾�ʒm
    public Action<int , string> OnGotRelic {  get; set; }

    //�M�~�b�N�̋N���ʒm
    public Action<GimmickData> OnBootedGimmick { get; set; }

    //��Փx�㏸�ʒm
    public Action<int> OnAscendDifficultySyn {  get; set; }

    //���X�e�[�W�i�s�ʒm
    public Action<Guid,bool,STAGE_TYPE> OnAdanceNextStageSyn { get; set; }

    //�v���C���[�̗͑����ʒm
    public Action<int,float> OnPlayerHealthSyn {  get; set; }

    //�G�̗͑����ʒm
    public Action<EnemyDamegeData> OnEnemyHealthSyn { get; set; }

    //�G���S�ʒm
    public Action<int> OnKilledEnemySyn {  get; set; }

    //�o���l�ʒm
    public Action<int> OnEXPSyn {  get; set; }

    //���x���A�b�v�ʒm
    public Action OnLevelUpSyn { get; set; }

    //�v���C���[�_�E���ʒm
    public Action<Guid> OnPlayerDeadSyn {  get; set; }

    //�_���[�W�\�L�ʒm
    public Action<int> OnDamaged {  get; set; }

    //�^�C�}�[�ʒm
    public Action<Dictionary<EnumManager.TIME_TYPE, int>> OnTimerSyn { get; set; }

    //�X�e�[�W�i�s�ʒm
    public Action<Guid> OnAdvancedStageSyn { get; set; }

    //�Q�[���I���ʒm
    public Action<ResultData> OnGameEndSyn { get; set; }

    //�[���N���ʒm
    public Action<int> OnBootedTerminal {  get; set; }

    //�[�����ʒʒm
    public Action<int,bool> OnTerminalsResultSyn { get; set; }

    //�A�C�e���l���ʒm
    public Action<string> OnGetItemSyn {  get; set; }
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
                this.IsMaster = user.Value.IsMaster;
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
        this.IsMaster = false;
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
    /// �v���C���[�̍X�V����
    /// </summary>
    /// <param name="playerData"></param>
    /// <returns></returns>
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
    public async Task PlayerDeadAsync(Guid playerID)
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
    /// �G�̐�������
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="enemID"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public async Task SpawnEnemyAsync(List<SpawnEnemyData> spawnEnemyDatas)
    {
        await roomHub.SpawnEnemyAsync(spawnEnemyDatas);
    }

    /// <summary>
    /// �G�̗͑�������
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="enemID"></param>
    /// <param name="enemHP"></param>
    /// <returns></returns>
    public async Task EnemyHealthAsync(int enemID,Guid guid, float enemHP, List<DEBUFF_TYPE> debuffList)
    {
        await roomHub.EnemyHealthAsync(enemID,guid, enemHP, debuffList);
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
    public async Task DropRelicAsync(Stack<Vector2> pos)
    {
        await roomHub.DropRelicAsync(pos);
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
    /// <returns></returns>
    public async Task AscendDifficultyAsync()
    {
        await roomHub.AscendDifficultyAsync();
    }

    /// <summary>
    /// ���X�e�[�W�i�s�̓���
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="stageID"></param>
    //public async Task AdvanceNextStageAsync(int stageID, bool isBossStage)
    //{
    //    await roomHub.AdvanceNextStageAsync(stageID, isBossStage);
    //}

    /// <summary>
    /// �X�e�[�W�i�s�����̓���
    /// </summary>
    /// <param name="stageID"></param>
    /// <param name="isBossStage"></param>
    /// <returns></returns>
    public async Task AdvancedStageAsync(Guid conID, bool isAdvance)
    {
        await roomHub.AdvancedStageAsync(conID, isAdvance);
    }

    /// <summary>
    /// �X�e�[�W�N���A�ҋ@�̓���
    /// </summary>
    /// <param name="conID"></param>
    /// <param name="isTimeUp"></param>
    /// <param name="isBossStage"></param>
    /// <returns></returns>
    public async Task WaitStageClearAsync(Guid? conID, bool isTimeUp,bool isBossStage)
    {
        await roomHub.WaitStageClearAsync(conID, isTimeUp,isBossStage);
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

    /// <summary>
    /// �^�C�}�[�̓���
    /// </summary>
    /// <param name="tiemrType"></param>
    /// <returns></returns>
    public async Task TimeAsync(Dictionary<EnumManager.TIME_TYPE, int> tiemrType)
    {
        await roomHub.TimeAsync(tiemrType);
    }
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
    public void OnUpdatePlayer(PlayerData playerData)
    {
        OnUpdatePlayerSyn(playerData);
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
    public void OnPlayerDead(Guid playerID)
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
    /// <param name="enemyData"></param>
    public void OnUpdateEnemy(List<EnemyData> enemyDatas)
    {
        OnUpdatedEnemy(enemyDatas);
    }

    /// <summary>
    /// �G�̐����ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="enemyData"></param>
    /// <param name="pos"></param>
    public void OnSpawnEnemy(List<SpawnEnemyData> spawnEnemyDatas)
    {
        OnSpawndEnemy(spawnEnemyDatas);
    }

    /// <summary>
    /// �G�̗͑����ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnEnemyHealth(EnemyDamegeData enemyDamegeData)
    {
        OnEnemyHealthSyn(enemyDamegeData);
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
    public void OnDropRelic(List<DropRelicData> relicDatas)
    {
        OnDropedRelic(relicDatas);
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
    /// �[���N���ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="termID"></param>
    public void OnBootTerminal(int termID)
    {
        OnBootedTerminal(termID);
    }

    /// <summary>
    /// �[�����ʒʒm
    /// Author:�ؓc�W��
    /// </summary>
    /// <param name="termID"></param>
    /// <param name="result"></param>
    public void OnTerminalsResult(int termID, bool result)
    {
        OnTerminalsResultSyn(termID, result);
    }

    /// <summary>
    /// �A�C�e���l���ʒm
    /// Author:�ؓc�W��
    /// </summary>
    public void OnGetItem(string itemID)
    {
        OnGetItemSyn(itemID);
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
    public void OnAdanceNextStage(Guid conID, bool isAdvance, STAGE_TYPE stageType)
    {
        OnAdanceNextStageSyn(conID,isAdvance,stageType);
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

    /// <summary>
    /// �^�C�}�[
    /// </summary>
    /// <param name="timerType"></param>
    public void OnTimer(Dictionary<EnumManager.TIME_TYPE, int> timerType)
    {
        OnTimerSyn(timerType);
    }
    #endregion

    /// <summary>
    /// �X�e�[�W�i�s�ʒm
    /// </summary>
    /// <param name="conID"></param>
    public void OnAdvancedStage(Guid conID)
    {
        OnAdvancedStageSyn(conID);
    }

    /// <summary>
    /// �Q�[���I���ʒm
    /// </summary>
    /// <param name="result"></param>
    public void OnGameEnd(ResultData result)
    {
        OnGameEndSyn(result);
    }
    #endregion
}
