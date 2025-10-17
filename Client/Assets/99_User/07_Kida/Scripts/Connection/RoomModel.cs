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
using NIGHTRAVEL.Shared.Interfaces.Services;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using static Shared.Interfaces.StreamingHubs.IRoomHubReceiver;
using static Unity.Cinemachine.CinemachineSplineRoll;
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

    //���݂̃��[�����
    public RoomData[] roomDataList { get; set; }

    #region �ʒm��`�ꗗ

    #region �V�X�e��

    //���[�������ʒm
    public Action<List<string>, List<string>, List<string>> OnSearchedRoom { get; set; }

    //���[�������ʒm
    public Action OnCreatedRoom { get; set; }

    //���[�U�[�ڑ��ʒm
    public Action<JoinedUser> OnJoinedUser { get; set; }

    //�������s�ʒm
    public Action<int> OnFailedJoinSyn { get; set; }

    //���[�U�[�ގ��ʒm
    public Action<JoinedUser> OnLeavedUser { get; set; }

    //�L�����N�^�[�ύX�ʒm
    public Action<Guid, int> OnChangedCharacter { get; set; }

    //���������ʒm
    public Action<Guid> OnReadySyn { get; set; }

    //�Q�[���J�n�ʒm
    public Action OnStartedGame { get; set; }

    //�����J�n�ʒm
    public Action<List<TerminalData>> OnSameStartSyn { get; set; }

    //��Փx�㏸�ʒm
    public Action<int> OnAscendDifficultySyn { get; set; }

    //���X�e�[�W�i�s�ʒm
    public Action<STAGE_TYPE> OnAdanceNextStageSyn { get; set; }

    //���x���A�b�v�ʒm
    public Action<int, int, int, CharacterStatusData, Guid, List<StatusUpgrateOptionData>> OnLevelUpSyn { get; set; }

    //�X�e�[�W�i�s�ʒm
    public Action OnAdvancedStageSyn { get; set; }

    //�Q�[���I���ʒm
    public Action<ResultData> OnGameEndSyn { get; set; }

    #endregion

    #region �v���C���[�E�}�X�^�N���C�A���g

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

    public Action<Guid, bool> OnBeamEffectActived { get; set; }

    #endregion

    #region �G

    //�G�̏o���ʒm
    public Action<List<SpawnEnemyData>> OnSpawndEnemy { get; set; }

    //�G�̗͑����ʒm
    public Action<EnemyDamegeData> OnEnemyHealthSyn { get; set; }

    //�G�폜�ʒm
    public Action<string> OnDeleteEnemySyn { get; set; }


    #endregion

    #region �A�C�e��

    //�����b�N�̐����ʒm
    public Action<Dictionary<string, DropRelicData>> OnDropedRelic { get; set; }

    //�A�C�e���l���ʒm
    public Action<Guid, string, int, int, int> OnGetItemSyn { get; set; }

    #endregion

    #region �M�~�b�N

    //�M�~�b�N�̋N���ʒm
    public Action<string, bool> OnBootedGimmick { get; set; }

    // �I�u�W�F�N�g�����ʒm
    public Action<OBJECT_TYPE, Vector2, string> OnSpawnedObjectSyn { get; set; }

    #endregion

    #region �[��

    // �[���N���ʒm
    public Action<int> OnBootedTerminal { get; set; }

    // �[�������ʒm
    public Action<int> OnTerminalsSuccessed { get; set; }

    // �[�����s�ʒm
    public Action<int> OnTerminalFailured { get; set; }

    // �[���W�����u���K�p�ʒm
    public Action<List<DropRelicData>> OnTerminalJumbled { get; set; }

    #endregion

    #region ���˕�

    // ���˕��̐����ʒm
    public Action<List<ShootBulletData>> OnShootedBullet { get; set; }

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
        instance = null;
    }

    #region �ʒm�̏���

    /// <summary>
    /// �����J�n
    /// Aughtor:�ؓc�W��
    /// </summary>
    public void OnSameStart(List<TerminalData> list)
    {
        OnSameStartSyn(list);
    }

    /// <summary>
    /// �Q�[���I���ʒm
    /// </summary>
    /// <param name="result"></param>
    public async void OnGameEnd(ResultData result)
    {
        OnGameEndSyn(result);
        await roomHub.LeavedAsync(true);
    }

    #region �����E�ގ��E���������ʒm

    /// <summary>
    /// ���[�������ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="userName"></param>
    public void OnSearchRoom(RoomData[] roomDatas)
    {
        List<string> roomNameList = new List<string>();
        List<string> userNameList = new List<string>();
        List<string> passWordList = new List<string>();

        foreach (RoomData roomData in roomDatas)
        {
            roomNameList.Add(roomData.roomName);
            userNameList.Add(roomData.userName);
            passWordList.Add(roomData.passWord);
        }

        OnSearchedRoom(roomNameList, userNameList, passWordList);
    }

    /// <summary>
    /// ���[�������ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <returns></returns>
    public void OnRoom()
    {
        OnCreatedRoom();
    }

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

    public void OnFailedJoin(int errorId)
    {
        OnFailedJoinSyn(errorId);
    }

    /// <summary>
    /// �ގ��ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="user"></param>
    public void OnLeave(Dictionary<Guid, JoinedUser> joinedUser, Guid targetUser)
    {
        int i = 1;
        JoinedUser leaveUser = joinedUser[targetUser];
        joinedUserList = joinedUser;
        joinedUserList.Remove(targetUser);
        foreach (var user in joinedUserList)
        {
            user.Value.JoinOrder = i;
            i++;
        }

        OnLeavedUser(leaveUser);
    }

    /// <summary>
    /// �L�����N�^�[�ύX�ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnChangeCharacter(Guid guid, int characterId)
    {
        joinedUserList[guid].CharacterID = characterId;
        OnChangedCharacter(guid, characterId);
    }

    /// <summary>
    /// ���������ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="conID"></param>
    public void OnReady(JoinedUser joinedUser)
    {
        joinedUserList[joinedUser.ConnectionId] = joinedUser;
        OnReadySyn(joinedUser.ConnectionId);
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
        Debug.Log("���Ȃ����}�X�^�[�N���C�A���g�ɂȂ�܂���");
        IsMaster = true;
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
    public void OnLevelUp(int level, int nowExp, int nextLvExp, CharacterStatusData updatedStatusData, Guid optionsKey, List<StatusUpgrateOptionData> statusOptionList)
    {
        OnLevelUpSyn(level, nowExp, nextLvExp, updatedStatusData, optionsKey, statusOptionList);
    }

    /// <summary>
    /// ���˕��̐����ʒm
    /// Aughter:�ؓc�W��
    /// </summary>
    public void OnShootBullets(params ShootBulletData[] shootBulletDatas)
    {
        OnShootedBullet(shootBulletDatas.ToList());
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
    /// �w�肵���G���폜����ʒm
    /// </summary>
    /// <param name="enemId"></param>
    public void OnDeleteEnemy(string enemId)
    {
        OnDeleteEnemySyn(enemId);
    }

    /// <summary>
    /// �r�[���G�t�F�N�g�̃A�N�e�B�u�ʒm
    /// </summary>
    /// <param name="conID"></param>
    /// <param name="isActive"></param>
    public void OnBeamEffectActive(Guid conID, bool isActive)
    {
        OnBeamEffectActived(conID, isActive);
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

    #region �[���֘A

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
    public void OnTerminalsSuccess(int termID)
    {
        OnTerminalsSuccessed(termID);
    }

    /// <summary>
    /// �[�����s�ʒm
    /// </summary>
    /// <param name="termID"></param>
    public void OnTerminalFailure(int termID)
    {
        OnTerminalFailured(termID);
    }

    /// <summary>
    /// �[���W�����u���ʒm
    /// </summary>
    /// <param name="termID"></param>
    public void OnTerminalJumble(List<DropRelicData> relics)
    {
        OnTerminalJumbled(relics);
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
    public void OnBootGimmick(string uniqueID, bool triggerOnce)
    {
        OnBootedGimmick(uniqueID, triggerOnce);
    }

    /// <summary>
    /// �A�C�e���l���ʒm
    /// Author:�ؓc�W��
    /// </summary>
    public void OnGetItem(Guid conId, string itemID, int nowLevel, int nowExp, int nextLevelExp)
    {
        OnGetItemSyn(conId, itemID, nowLevel, nowExp, nextLevelExp);
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
    public void OnAdanceNextStage(STAGE_TYPE stageType)
    {
        OnAdanceNextStageSyn(stageType);
    }

    /// <summary>
    /// �X�e�[�W�i�s�ʒm
    /// Author;�ؓc�W��
    /// </summary>
    public void OnAdvancedStage()
    {
        OnAdvancedStageSyn();
    }

    public void OnSpawnObject(OBJECT_TYPE type, Vector2 spawnPos, string uniqueId)
    {
        OnSpawnedObjectSyn(type, spawnPos, uniqueId);
    }

    #endregion

    #endregion

    #region ���N�G�X�g�֘A

    #region ��������Q�[���J�n�܂�

    /// <summary>
    /// �����̌���
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <returns></returns>
    public async Task SearchRoomAsync()
    {
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler });
        var client = MagicOnionClient.Create<IRoomService>(channel);

        var roomDatas = await client.GetAllRoom();

        roomDataList = new RoomData[roomDatas.Length];

        for (int i = 0; i < roomDatas.Length; i++)
        {
            roomDataList[i] = new RoomData();
            roomDataList[i].roomName = roomDatas[i].roomName;
            roomDataList[i].userName = roomDatas[i].userName;
            roomDataList[i].passWord = roomDatas[i].password;
            roomDataList[i].isStarted = roomDatas[i].is_started;
        }

        OnSearchRoom(roomDataList);
    }



    /// <summary>
    /// ��������
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <returns></returns>
    public async UniTask JoinedAsync(string roomName, int userId, string userName, string pass,int gameMode)
    {
        //if(MatchingManager.JoinMode != "create")
        //{
        //    var handler = new YetAnotherHttpHandler() { Http2Only = true };
        //    var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler });
        //    var client = MagicOnionClient.Create<IRoomService>(channel);

        //    var roomData = await client.GetRoom(userName);
        //    if (roomData == null)
        //    {
        //        OnFailedJoinSyn(3);
        //        return;
        //    }

        //}

        joinedUserList = await roomHub.JoinedAsync(roomName, userId, userName, pass,gameMode);
        if (joinedUserList == null) return;
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
        await roomHub.LeavedAsync(false);
        this.IsMaster = false;
        //���������X�g�������
        joinedUserList.Clear();
    }

    /// <summary>
    /// �L�����N�^�[�ύX
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <returns></returns>
    public async UniTask ChangeCharacterAsync(int characterId)
    {
        await roomHub.ChangeCharacterAsync(characterId);
    }

    /// <summary>
    /// ������������
    /// </summary>
    /// <returns></returns>
    public async UniTask ReadyAsync(int characterId)
    {
        await roomHub.ReadyAsync(characterId);
    }

    /// <summary>
    /// �X�^�[�g
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="hostName"></param>
    /// <returns></returns>
    public async Task StartRoomAsync(string hostName)
    {
        if (TitleManagerk.GameMode == 0) return;
        var handler = new YetAnotherHttpHandler() { Http2Only = true };
        var channel = GrpcChannel.ForAddress(ServerURL, new GrpcChannelOptions() { HttpHandler = handler });
        var client = MagicOnionClient.Create<IRoomService>(channel);

        await client.StartRoom(hostName);
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
    public async UniTask<PlayerDeathResult> PlayerDeadAsync()
    {
        return await roomHub.PlayerDeadAsync();
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
    public async UniTask ShootBulletAsync(params ShootBulletData[] shootBulletDatas)
    {
        await roomHub.ShootBulletsAsync(shootBulletDatas);
    }

    /// <summary>
    /// �r�[���G�t�F�N�g�̃A�N�e�B�u����
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    public async UniTask BeamEffectActiveAsync(bool isActive)
    {
        await roomHub.BeamEffectActiveAsync(isActive);
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
    /// ���I���̃X�e�[�^�X�����I�����擾����
    /// </summary>
    /// <returns></returns>
    //public async UniTask<Dictionary<Guid, List<StatusUpgrateOptionData>>> GetUpgradeGroupsAsync()
    //{
    //    return await roomHub.GetUpgradeGroupsAsync();
    //}

    /// <summary>
    /// �X�e�[�^�X�����I��
    /// </summary>
    /// <param name="conID">�ڑ�ID</param>
    /// <param name="upgradeOpt">��������</param>
    /// <returns></returns>
    public async UniTask ChooseUpgrade(Guid optionsKey, STAT_UPGRADE_OPTION upgradeOpt)
    {
        await roomHub.ChooseUpgrade(optionsKey, upgradeOpt);
    }

    /// <summary>
    /// �w�肵���G���폜����
    /// </summary>
    /// <param name="enemId"></param>
    /// <returns></returns>
    public async UniTask DeleteEnemyAsync(string enemId)
    {
        await roomHub.DeleteEnemyAsync(enemId);
    }

    #endregion

    #region �����b�N�֘A

    /// <summary>
    /// �����b�N��������
    /// Aughter:�ؓc�W��
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="includeBossRarity">�{�X�p�̃����b�N�����I�Ώۂɂ��邩�ǂ���</param>
    /// <returns></returns>
    public async UniTask DropRelicAsync(Stack<Vector2> pos, bool includeBossRarity)
    {
        await roomHub.DropRelicAsync(pos, includeBossRarity);
    }
    #endregion

    #region �Q�[����UI�A�d�l�֘A
    /// <summary>
    /// �M�~�b�N�̋N������
    ///  Aughter:�ؓc�W��
    /// </summary>
    /// <param name="uniqueID"></param>
    /// <returns></returns>
    public async UniTask BootGimmickAsync(string uniqueID, bool triggerOnce)
    {
        await roomHub.BootGimmickAsync(uniqueID, triggerOnce);
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

    /// <summary>
    /// �I�u�W�F�N�g�������N�G�X�g
    /// </summary>
    /// <returns></returns>
    public async UniTask SpawnObjectAsync(OBJECT_TYPE type, Vector2 spawnPos)
    {
        await roomHub.SpawnObjectAsync(type, spawnPos);
    }

    #endregion

    #region �[���֘A

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
    /// </summary>
    /// <param name="termID"></param>
    /// <returns></returns>
    public async UniTask TerminalSuccessAsync(int termID)
    {
        await roomHub.TerminalSuccessAsync(termID);
    }

    /// <summary>
    /// �[�����s����
    /// </summary>
    /// <param name="termID"></param>
    /// <returns></returns>
    public async UniTask TerminalFailureAsync(int termID)
    {
        await roomHub.TerminalFailureAsync(termID);
    }

    #endregion

    #endregion

    #endregion
}
