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

    // ���݂̎Q���ҏ��
    public Dictionary<Guid, JoinedUser> joinedUserList { get; private set; } = new Dictionary<Guid, JoinedUser>();

    #region �ʒm��`�ꗗ

    #region �V�X�e��

    //���[�U�[�ڑ��ʒm
    public Action<JoinedUser> OnJoinedUser { get; set; }

    //���[�U�[�ގ��ʒm
    public Action<JoinedUser> OnLeavedUser { get; set; }

    //���������ʒm
    public Action<Guid> OnReadySyn { get; set; }

    //�Q�[���J�n�ʒm
    public Action OnStartedGame { get; set; }

    //�����J�n�ʒm
    public Action OnSameStartSyn { get; set; }

    //��Փx�㏸�ʒm
    public Action<int> OnAscendDifficultySyn { get; set; }

    //���X�e�[�W�i�s�ʒm
    public Action<bool, STAGE_TYPE> OnAdanceNextStageSyn { get; set; }

    //���x���A�b�v�ʒm
    public Action<int, int, Dictionary<Guid, CharacterStatusData>, List<EnumManager.STAT_UPGRADE_OPTION>> OnLevelUpSyn { get; set; }

    //�X�e�[�W�i�s�ʒm
    public Action OnAdvancedStageSyn { get; set; }

    //�Q�[���I���ʒm
    public Action<ResultData> OnGameEndSyn { get; set; }

    #endregion

    #region �v���C���[�E�}�X�^�N���C�A���g

    //�}�X�^�[�N���C�A���g���n
    public Action<JoinedUser> OnMasteredClient { get; set; }

    //�}�X�^�[�N���C�A���g�̕ύX�ʒm
    public Action OnChangedMasterClient { get; set; }

    //�}�X�^�[�N���C�A���g�̍X�V�ʒm
    public Action<MasterClientData> OnUpdateMasterClientSyn { get; set; }

    //�v���C���[�ʒu��]�ʒm
    public Action<PlayerData> OnUpdatePlayerSyn { get; set; }

    // �v���C���[�̃X�e�[�^�X�X�V�ʒm
    public Action<CharacterStatusData, PlayerRelicStatusData> OnUpdateStatusSyn { get; set; }

    //�v���C���[�_�E���ʒm
    public Action<Guid> OnPlayerDeadSyn { get; set; }

    #endregion

    #region �G

    //�G�̏o���ʒm
    public Action<List<SpawnEnemyData>> OnSpawndEnemy { get; set; }

    //�G�̗͑����ʒm
    public Action<EnemyDamegeData> OnEnemyHealthSyn { get; set; }


    #endregion

    #region �A�C�e��

    //�����b�N�̐����ʒm
    public Action<Dictionary<string, DropRelicData>> OnDropedRelic { get; set; }

    //�A�C�e���l���ʒm
    public Action<Guid, string> OnGetItemSyn { get; set; }

    #endregion

    #region �M�~�b�N

    //�M�~�b�N�̋N���ʒm
    public Action<int> OnBootedGimmick { get; set; }


    #endregion

    #region �[��

    //�[���N���ʒm
    public Action<int> OnBootedTerminal { get; set; }

    //�[�����ʒʒm
    public Action<int, bool> OnTerminalsResultSyn { get; set; }

    #endregion

    #region ���˕�

    // ���˕��̐����ʒm
    public Action<PROJECTILE_TYPE, List<DEBUFF_TYPE>, int, Vector2, Vector2, Quaternion> OnShootedBullet { get; set; }

    #endregion

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
    /// �v���C���[�̃X�e�[�^�X�X�V�ʒm
    /// </summary>
    public void OnUpdateStatus(CharacterStatusData characterStatus, PlayerRelicStatusData prsData)
    {
        OnUpdateStatusSyn(characterStatus, prsData);
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
    /// �v���C���[�̃��x���A�b�v�ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnLevelUp(int level, int nowExp, Dictionary<Guid, CharacterStatusData> characterStatusDataList, List<EnumManager.STAT_UPGRADE_OPTION> statusOptionList)
    {
       // OnLevelUpSyn(level,nowExp,characterStatusDataList,statusOptionList);
    }

    /// <summary>
    /// ���˕��̐����ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnShootBullet(PROJECTILE_TYPE type, List<DEBUFF_TYPE> debuffs, int power, Vector2 spawnPos, Vector2 shootVec, Quaternion rotation)
    {
        OnShootedBullet(type, debuffs, power, spawnPos, shootVec, rotation);
    }

    public void OnUpdateStatus(CharacterStatusData cdata, PlayerRelicStatusData rdata)
    {
    }
    #endregion
    #region �G�ʒm�֘A

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

    #endregion
    #region �����b�N�ʒm�֘A

    /// <summary>
    /// �����b�N�����ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="relicID"></param>
    /// <param name="pos"></param>
    public void OnDropRelic(Dictionary<string, DropRelicData> relicDatas)
    {
        OnDropedRelic(relicDatas);
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
    public void OnBootGimmick(int gimmickId)
    {
        OnBootedGimmick(gimmickId);
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
    public void OnGetItem(Guid conId, string itemID)
    {
        OnGetItemSyn(conId, itemID);
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
    public void OnAdanceNextStage(bool isAdvance, STAGE_TYPE stageType)
    {
        OnAdanceNextStageSyn(isAdvance,stageType);
    }

    /// <summary>
    /// �X�e�[�W�i�s�ʒm
    /// Author;�ؓc�W��
    /// </summary>
    public void OnAdvancedStage()
    {
        OnAdvancedStageSyn();
    }

    #endregion

    /// <summary>
    /// �����J�n
    /// Aughtor:�ؓc�W��
    /// </summary>
    public void OnSameStart()
    {
        OnSameStartSyn();
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

    #region ���N�G�X�g�֘A
    #region ��������Q�[���J�n�܂�
    /// <summary>
    /// ��������
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <returns></returns>
    public async UniTask JoinedAsync(string roomName, int userId)
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
    /// �ގ��̓���
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <returns></returns>
    public async UniTask LeavedAsync()
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
    public async UniTask ReadyAsync()
    {
        await roomHub.ReadyAsync();
    }
    #endregion
    #region �Q�[����
    #region �v���C���[�֘A
    /// <summary>
    /// �v���C���[�̍X�V����
    /// </summary>
    /// <param name="playerData"></param>
    /// <returns></returns>
    public async UniTask UpdatePlayerAsync(PlayerData playerData)
    {
        await roomHub.UpdatePlayerAsync(playerData);
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
    /// �v���C���[�_�E������
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="playerID"></param>
    /// <returns></returns>
    public async UniTask PlayerDeadAsync()
    {
        await roomHub.PlayerDeadAsync();
    }

    /// <summary>
    /// �[���N��
    /// Author:Nishiura
    /// </summary>
    /// <param name="termID">�[�����ID</param>
    /// <returns></returns>
    public async UniTask BootTerminalAsync(int termID)
    {
        await roomHub.BootTerminalAsync(termID); 
    }

    /// <summary>
    /// �[����������
    /// Author:Nishiura
    /// </summary>
    /// <param name="termID">�[�����ID</param>
    /// <param name="result">�[������</param>
    /// <returns></returns>
     public async UniTask TerminalsResultAsync(int termID, bool result)
    {
        await roomHub.TerminalsResultAsync(termID, result);
    }

    /// <summary>
    /// �A�C�e���l��
    /// Author:Nishiura
    /// </summary>
    /// <param name="itemType">�A�C�e���̎��</param>
    /// <param name="itemID">����ID(������)</param>
    /// <returns></returns>
     public async UniTask GetItemAsync(EnumManager.ITEM_TYPE itemType, string itemID)
    {
        await roomHub.GetItemAsync(itemType, itemID);
    }

    /// <summary>
    /// ���˕��̐���
    /// </summary>
    /// <param name="type">���˕��̎��</param>
    /// <param name="spawnPos">�����ʒu</param>
    /// <param name="shootVec">���˃x�N�g��</param>
    /// <returns></returns>
    public async UniTask ShootBulletAsync(PROJECTILE_TYPE type, List<DEBUFF_TYPE> debuffs, int power, Vector2 spawnPos, Vector2 shootVec, Quaternion rotation)
    {
        await roomHub.ShootBulletAsync(type, debuffs, power, spawnPos, shootVec, rotation);
    }

    #endregion
    #region �G�֘A
    /// <summary>
    /// �G�̐�������
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="enemID"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public async UniTask SpawnEnemyAsync(List<SpawnEnemyData> spawnEnemyDatas)
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
    public async UniTask EnemyHealthAsync(string enemID, float giverAtack, List<DEBUFF_TYPE> debuffList)
    {
        await roomHub.EnemyHealthAsync(enemID, giverAtack, debuffList);
    }

    /// <summary>
    /// �G�̔�_���[�W��������   �v���C���[�ɂ��_���[�W�ȊO
    /// </summary>
    /// <param name="enemID">�G����ID</param>
    /// <param name="dmgAmount">�K�p������_���[�W��</param>
    /// <returns></returns>
    public async UniTask ApplyDamageToEnemyAsync(string enemID, int dmgAmount)
    {
        await roomHub.ApplyDamageToEnemyAsync(enemID, dmgAmount);
    }

    /// <summary>
    /// �X�e�[�^�X�����I��
    /// </summary>
    /// <param name="conID">�ڑ�ID</param>
    /// <param name="upgradeOpt">��������</param>
    /// <returns></returns>
     public async UniTask ChooseUpgrade(EnumManager.STAT_UPGRADE_OPTION upgradeOpt)
    {
        await roomHub.ChooseUpgrade(upgradeOpt);
    }
    #endregion
    #region �����b�N�֘A

    /// <summary>
    /// �����b�N��������
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public async UniTask DropRelicAsync(Stack<Vector2> pos)
    {
        await roomHub.DropRelicAsync(pos);
    }
    #endregion
    #region �Q�[����UI�A�d�l�֘A
    /// <summary>
    /// �M�~�b�N�̋N������
    ///  Aughter:�ؓc�W��
    /// </summary>
    /// <param name="gimID"></param>
    /// <returns></returns>
    public async UniTask BootGimmickAsync(int gimID)
    {
        await roomHub.BootGimmickAsync(gimID);
    }

    /// <summary>
    /// ��Փx�㏸�̓���
    /// </summary>
    /// <returns></returns>
    public async UniTask AscendDifficultyAsync()
    {
        await roomHub.AscendDifficultyAsync();
    }

    /// <summary>
    /// �X�e�[�W�N���A
    /// Author:Nishiura
    /// </summary>
    /// <param name="isAdvance">�X�e�[�W�i�s����</param>
    /// <returns></returns>
    public async UniTask StageClear(bool isAdvance)
    {
        await roomHub.StageClear(isAdvance);
    }

    /// <summary>
    /// �X�e�[�W�i�s�����̓���
    /// </summary>
    /// <returns></returns>
    public async UniTask AdvancedStageAsync()
    {
        await roomHub.AdvancedStageAsync();
    }
    #endregion
    #endregion
    #endregion
}
