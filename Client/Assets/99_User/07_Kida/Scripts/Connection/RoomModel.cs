////////////////////////////////////////////////////////////////
///
/// RoomHubへの接続を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

#region using一覧
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
    private GrpcChannel channel;  //サーバーURL
    private IRoomHub roomHub;     //roomHubの関数を呼び出す時に使う

    //マスタークライアントかどうか
    public bool IsMaster { get; set; }

    //接続ID
    public Guid ConnectionId { get; private set; }

    // 現在の参加者情報
    public Dictionary<Guid, JoinedUser> joinedUserList { get; private set; } = new Dictionary<Guid, JoinedUser>();

    //現在のルーム情報
    public RoomData[] roomDataList { get; set; }

    #region 通知定義一覧

    #region システム

    //ルーム検索通知
    public Action<List<string>, List<string>, List<string>> OnSearchedRoom { get; set; }

    //ルーム生成通知
    public Action OnCreatedRoom { get; set; }

    //ユーザー接続通知
    public Action<JoinedUser> OnJoinedUser { get; set; }

    //入室失敗通知
    public Action<int> OnFailedJoinSyn { get; set; }

    //ユーザー退室通知
    public Action<JoinedUser> OnLeavedUser { get; set; }

    //キャラクター変更通知
    public Action<Guid, int> OnChangedCharacter { get; set; }

    //準備完了通知
    public Action<Guid> OnReadySyn { get; set; }

    //ゲーム開始通知
    public Action OnStartedGame { get; set; }

    //同時開始通知
    public Action<List<TerminalData>> OnSameStartSyn { get; set; }

    //難易度上昇通知
    public Action<int> OnAscendDifficultySyn { get; set; }

    //次ステージ進行通知
    public Action<STAGE_TYPE> OnAdanceNextStageSyn { get; set; }

    //レベルアップ通知
    public Action<int, int, int, CharacterStatusData, Guid, List<StatusUpgrateOptionData>> OnLevelUpSyn { get; set; }

    //ステージ進行通知
    public Action OnAdvancedStageSyn { get; set; }

    //ゲーム終了通知
    public Action<ResultData> OnGameEndSyn { get; set; }

    #endregion

    #region プレイヤー・マスタクライアント

    //マスタークライアントの変更通知
    public Action OnChangedMasterClient { get; set; }

    //マスタークライアントの更新通知
    public Action<MasterClientData> OnUpdateMasterClientSyn { get; set; }

    //プレイヤー位置回転通知
    public Action<PlayerData> OnUpdatePlayerSyn { get; set; }

    // プレイヤーのステータス更新通知
    public Action<CharacterStatusData, PlayerRelicStatusData> OnUpdateStatusSyn { get; set; }

    //プレイヤーダウン通知
    public Action<Guid> OnPlayerDeadSyn { get; set; }

    public Action<Guid, bool> OnBeamEffectActived { get; set; }

    #endregion

    #region 敵

    //敵の出現通知
    public Action<List<SpawnEnemyData>> OnSpawndEnemy { get; set; }

    //敵体力増減通知
    public Action<EnemyDamegeData> OnEnemyHealthSyn { get; set; }

    //敵削除通知
    public Action<string> OnDeleteEnemySyn { get; set; }


    #endregion

    #region アイテム

    //レリックの生成通知
    public Action<Dictionary<string, DropRelicData>> OnDropedRelic { get; set; }

    //アイテム獲得通知
    public Action<Guid, string, int, int, int> OnGetItemSyn { get; set; }

    #endregion

    #region ギミック

    //ギミックの起動通知
    public Action<string, bool> OnBootedGimmick { get; set; }

    // オブジェクト生成通知
    public Action<OBJECT_TYPE, Vector2, string> OnSpawnedObjectSyn { get; set; }

    #endregion

    #region 端末

    // 端末起動通知
    public Action<int> OnBootedTerminal { get; set; }

    // 端末成功通知
    public Action<int> OnTerminalsSuccessed { get; set; }

    // 端末失敗通知
    public Action<int> OnTerminalFailured { get; set; }

    // 端末ジャンブル適用通知
    public Action<List<DropRelicData>> OnTerminalJumbled { get; set; }

    #endregion

    #region 発射物

    // 発射物の生成通知
    public Action<List<ShootBulletData>> OnShootedBullet { get; set; }

    #endregion

    #endregion

    #region RoomModelインスタンス生成
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
            // インスタンスが複数存在しないように、既に存在していたら自身を消去する
            Destroy(gameObject);
        }
    }

    //public static RoomModel Instance
    //{
    //    get
    //    {
    //        // GETプロパティを呼ばれたときにインスタンスを作成する(初回のみ)
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

    #region MagicOnion接続・切断処理
    /// <summary>
    /// MagicOnion接続処理
    /// Aughter:木田晃輔
    /// </summary>
    /// <returns></returns>
    public async UniTask ConnectAsync()
    {
        var channel = GrpcChannelx.ForAddress(ServerURL);
        roomHub = await StreamingHubClient.ConnectAsync<IRoomHub, IRoomHubReceiver>(channel, this);
    }

    /// <summary>
    /// MagicOnion切断処理
    /// Aughter:木田晃輔
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
    /// 破棄処理
    /// Aughter:木田晃輔
    /// </summary>
    async void OnDestroy()
    {
        DisconnectAsync();
        instance = null;
    }

    #region 通知の処理

    /// <summary>
    /// 同時開始
    /// Aughtor:木田晃輔
    /// </summary>
    public void OnSameStart(List<TerminalData> list)
    {
        OnSameStartSyn(list);
    }

    /// <summary>
    /// ゲーム終了通知
    /// </summary>
    /// <param name="result"></param>
    public async void OnGameEnd(ResultData result)
    {
        OnGameEndSyn(result);
        await roomHub.LeavedAsync(true);
    }

    #region 入室・退室・準備完了通知

    /// <summary>
    /// ルーム検索通知
    /// Aughter:木田晃輔
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
    /// ルーム生成通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <returns></returns>
    public void OnRoom()
    {
        OnCreatedRoom();
    }

    /// <summary>
    /// 入室通知(IRoomHubReceiverインターフェイスの実装)
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="joinedUser"></param>
    public void Onjoin(JoinedUser joinedUser)
    {
        //OnJoinedUser?.Invoke(joinedUser);

        if (!joinedUserList.ContainsKey(joinedUser.ConnectionId))
            joinedUserList.Add(joinedUser.ConnectionId, joinedUser);

        //入室通知
        OnJoinedUser(joinedUser);

    }

    public void OnFailedJoin(int errorId)
    {
        OnFailedJoinSyn(errorId);
    }

    /// <summary>
    /// 退室通知
    /// Aughter:木田晃輔
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
    /// キャラクター変更通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnChangeCharacter(Guid guid, int characterId)
    {
        joinedUserList[guid].CharacterID = characterId;
        OnChangedCharacter(guid, characterId);
    }

    /// <summary>
    /// 準備完了通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="conID"></param>
    public void OnReady(JoinedUser joinedUser)
    {
        joinedUserList[joinedUser.ConnectionId] = joinedUser;
        OnReadySyn(joinedUser.ConnectionId);
    }
    #endregion

    #region プレイヤー通知関連
    /// <summary>
    /// プレイヤーの移動通知
    /// Aughter:木田晃輔
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
    /// マスタークライアントの変更通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnChangeMasterClient()
    {
        OnChangedMasterClient();
        Debug.Log("あなたがマスタークライアントになりました");
        IsMaster = true;
    }

    /// <summary>
    /// マスタークライアントの更新通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="masterClientData"></param>
    public void OnUpdateMasterClient(MasterClientData masterClientData)
    {
        OnUpdateMasterClientSyn(masterClientData);
    }

    /// <summary>
    /// プレイヤーのステータス更新通知
    /// </summary>
    public void OnUpdateStatus(CharacterStatusData characterStatus, PlayerRelicStatusData prsData)
    {
        OnUpdateStatusSyn(characterStatus, prsData);
    }

    /// <summary>
    /// プレイヤーダウン通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="playerID"></param>
    public void OnPlayerDead(Guid playerID)
    {
        OnPlayerDeadSyn(playerID);
    }

    /// <summary>
    /// プレイヤーのレベルアップ通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnLevelUp(int level, int nowExp, int nextLvExp, CharacterStatusData updatedStatusData, Guid optionsKey, List<StatusUpgrateOptionData> statusOptionList)
    {
        OnLevelUpSyn(level, nowExp, nextLvExp, updatedStatusData, optionsKey, statusOptionList);
    }

    /// <summary>
    /// 発射物の生成通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnShootBullets(params ShootBulletData[] shootBulletDatas)
    {
        OnShootedBullet(shootBulletDatas.ToList());
    }

    /// <summary>
    /// 敵の生成通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="enemyData"></param>
    /// <param name="pos"></param>
    public void OnSpawnEnemy(List<SpawnEnemyData> spawnEnemyDatas)
    {
        OnSpawndEnemy(spawnEnemyDatas);
    }

    /// <summary>
    /// 敵体力増減通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnEnemyHealth(EnemyDamegeData enemyDamegeData)
    {
        OnEnemyHealthSyn(enemyDamegeData);
    }

    /// <summary>
    /// 指定した敵を削除する通知
    /// </summary>
    /// <param name="enemId"></param>
    public void OnDeleteEnemy(string enemId)
    {
        OnDeleteEnemySyn(enemId);
    }

    /// <summary>
    /// ビームエフェクトのアクティブ通知
    /// </summary>
    /// <param name="conID"></param>
    /// <param name="isActive"></param>
    public void OnBeamEffectActive(Guid conID, bool isActive)
    {
        OnBeamEffectActived(conID, isActive);
    }

    #endregion

    #region レリック通知関連

    /// <summary>
    /// レリック生成通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="relicID"></param>
    /// <param name="pos"></param>
    public void OnDropRelic(Dictionary<string, DropRelicData> relicDatas)
    {
        OnDropedRelic(relicDatas);
    }

    #endregion

    #region 端末関連

    /// <summary>
    /// 端末起動通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="termID"></param>
    public void OnBootTerminal(int termID)
    {
        OnBootedTerminal(termID);
    }

    /// <summary>
    /// 端末結果通知
    /// Author:木田晃輔
    /// </summary>
    /// <param name="termID"></param>
    /// <param name="result"></param>
    public void OnTerminalsSuccess(int termID)
    {
        OnTerminalsSuccessed(termID);
    }

    /// <summary>
    /// 端末失敗通知
    /// </summary>
    /// <param name="termID"></param>
    public void OnTerminalFailure(int termID)
    {
        OnTerminalFailured(termID);
    }

    /// <summary>
    /// 端末ジャンブル通知
    /// </summary>
    /// <param name="termID"></param>
    public void OnTerminalJumble(List<DropRelicData> relics)
    {
        OnTerminalJumbled(relics);
    }

    #endregion

    #region ゲーム内UI・仕様の同期関連
    /// <summary>
    /// ゲーム開始通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnStartGame()
    {
        OnStartedGame();
    }

    /// <summary>
    /// ギミックの起動通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="gimmickData"></param>
    public void OnBootGimmick(string uniqueID, bool triggerOnce)
    {
        OnBootedGimmick(uniqueID, triggerOnce);
    }

    /// <summary>
    /// アイテム獲得通知
    /// Author:木田晃輔
    /// </summary>
    public void OnGetItem(Guid conId, string itemID, int nowLevel, int nowExp, int nextLevelExp)
    {
        OnGetItemSyn(conId, itemID, nowLevel, nowExp, nextLevelExp);
    }

    /// <summary>
    /// 難易度上昇の通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="difID"></param>
    public void OnAscendDifficulty(int difID)
    {
        OnAscendDifficultySyn(difID);
    }

    /// <summary>
    /// 次ステージ進行の通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="stageID"></param>
    public void OnAdanceNextStage(STAGE_TYPE stageType)
    {
        OnAdanceNextStageSyn(stageType);
    }

    /// <summary>
    /// ステージ進行通知
    /// Author;木田晃輔
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

    #region リクエスト関連

    #region 入室からゲーム開始まで

    /// <summary>
    /// 部屋の検索
    /// Aughter:木田晃輔
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
    /// 入室同期
    /// Aughter:木田晃輔
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
                Debug.Log("モデル：" + RoomModel.Instance.ConnectionId);
            }
        }
    }

    /// <summary>
    /// 退室の同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <returns></returns>
    public async UniTask LeavedAsync()
    {
        await roomHub.LeavedAsync(false);
        this.IsMaster = false;
        //自分をリストから消す
        joinedUserList.Clear();
    }

    /// <summary>
    /// キャラクター変更
    /// Aughter:木田晃輔
    /// </summary>
    /// <returns></returns>
    public async UniTask ChangeCharacterAsync(int characterId)
    {
        await roomHub.ChangeCharacterAsync(characterId);
    }

    /// <summary>
    /// 準備完了同期
    /// </summary>
    /// <returns></returns>
    public async UniTask ReadyAsync(int characterId)
    {
        await roomHub.ReadyAsync(characterId);
    }

    /// <summary>
    /// スタート
    /// Aughter:木田晃輔
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

    #region ゲーム内

    #region プレイヤー関連
    /// <summary>
    /// プレイヤーの更新同期
    /// </summary>
    /// <param name="playerData"></param>
    /// <returns></returns>
    public async UniTask UpdatePlayerAsync(PlayerData playerData)
    {
        await roomHub.UpdatePlayerAsync(playerData);
    }

    /// <summary>
    /// マスタークライアントの更新同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="masterClient"></param>
    /// <returns></returns>
    public async UniTask UpdateMasterClientAsync(MasterClientData masterClient)
    {
        await roomHub.UpdateMasterClientAsync(masterClient);
    }

    /// <summary>
    /// プレイヤーダウン同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="playerID"></param>
    /// <returns></returns>
    public async UniTask<PlayerDeathResult> PlayerDeadAsync()
    {
        return await roomHub.PlayerDeadAsync();
    }

    /// <summary>
    /// アイテム獲得
    /// Author:Nishiura
    /// </summary>
    /// <param name="itemType">アイテムの種類</param>
    /// <param name="itemID">識別ID(文字列)</param>
    /// <returns></returns>
    public async UniTask GetItemAsync(EnumManager.ITEM_TYPE itemType, string itemID)
    {
        await roomHub.GetItemAsync(itemType, itemID);
    }

    /// <summary>
    /// 発射物の生成
    /// </summary>
    /// <param name="type">発射物の種類</param>
    /// <param name="spawnPos">生成位置</param>
    /// <param name="shootVec">発射ベクトル</param>
    /// <returns></returns>
    public async UniTask ShootBulletAsync(params ShootBulletData[] shootBulletDatas)
    {
        await roomHub.ShootBulletsAsync(shootBulletDatas);
    }

    /// <summary>
    /// ビームエフェクトのアクティブ制御
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    public async UniTask BeamEffectActiveAsync(bool isActive)
    {
        await roomHub.BeamEffectActiveAsync(isActive);
    }

    #endregion

    #region 敵関連
    /// <summary>
    /// 敵の生成同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="enemID"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public async UniTask SpawnEnemyAsync(List<SpawnEnemyData> spawnEnemyDatas)
    {
        await roomHub.SpawnEnemyAsync(spawnEnemyDatas);
    }

    /// <summary>
    /// 敵体力増減同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="enemID"></param>
    /// <param name="enemHP"></param>
    /// <returns></returns>
    public async UniTask EnemyHealthAsync(string enemID, float giverAtack, List<DEBUFF_TYPE> debuffList)
    {
        await roomHub.EnemyHealthAsync(enemID, giverAtack, debuffList);
    }

    /// <summary>
    /// 敵の被ダメージ同期処理   プレイヤーによるダメージ以外
    /// </summary>
    /// <param name="enemID">敵識別ID</param>
    /// <param name="dmgAmount">適用させるダメージ量</param>
    /// <returns></returns>
    public async UniTask ApplyDamageToEnemyAsync(string enemID, int dmgAmount)
    {
        await roomHub.ApplyDamageToEnemyAsync(enemID, dmgAmount);
    }

    /// <summary>
    /// 未選択のステータス強化選択肢取得する
    /// </summary>
    /// <returns></returns>
    //public async UniTask<Dictionary<Guid, List<StatusUpgrateOptionData>>> GetUpgradeGroupsAsync()
    //{
    //    return await roomHub.GetUpgradeGroupsAsync();
    //}

    /// <summary>
    /// ステータス強化選択
    /// </summary>
    /// <param name="conID">接続ID</param>
    /// <param name="upgradeOpt">強化項目</param>
    /// <returns></returns>
    public async UniTask ChooseUpgrade(Guid optionsKey, STAT_UPGRADE_OPTION upgradeOpt)
    {
        await roomHub.ChooseUpgrade(optionsKey, upgradeOpt);
    }

    /// <summary>
    /// 指定した敵を削除する
    /// </summary>
    /// <param name="enemId"></param>
    /// <returns></returns>
    public async UniTask DeleteEnemyAsync(string enemId)
    {
        await roomHub.DeleteEnemyAsync(enemId);
    }

    #endregion

    #region レリック関連

    /// <summary>
    /// レリック生成同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="includeBossRarity">ボス用のレリックも抽選対象にするかどうか</param>
    /// <returns></returns>
    public async UniTask DropRelicAsync(Stack<Vector2> pos, bool includeBossRarity)
    {
        await roomHub.DropRelicAsync(pos, includeBossRarity);
    }
    #endregion

    #region ゲーム内UI、仕様関連
    /// <summary>
    /// ギミックの起動同期
    ///  Aughter:木田晃輔
    /// </summary>
    /// <param name="uniqueID"></param>
    /// <returns></returns>
    public async UniTask BootGimmickAsync(string uniqueID, bool triggerOnce)
    {
        await roomHub.BootGimmickAsync(uniqueID, triggerOnce);
    }

    /// <summary>
    /// 難易度上昇の同期
    /// </summary>
    /// <returns></returns>
    public async UniTask AscendDifficultyAsync()
    {
        await roomHub.AscendDifficultyAsync();
    }

    /// <summary>
    /// ステージクリア
    /// Author:Nishiura
    /// </summary>
    /// <param name="isAdvance">ステージ進行判定</param>
    /// <returns></returns>
    public async UniTask StageClear(bool isAdvance)
    {
        await roomHub.StageClear(isAdvance);
    }

    /// <summary>
    /// ステージ進行完了の同期
    /// </summary>
    /// <returns></returns>
    public async UniTask AdvancedStageAsync()
    {
        await roomHub.AdvancedStageAsync();
    }

    /// <summary>
    /// オブジェクト生成リクエスト
    /// </summary>
    /// <returns></returns>
    public async UniTask SpawnObjectAsync(OBJECT_TYPE type, Vector2 spawnPos)
    {
        await roomHub.SpawnObjectAsync(type, spawnPos);
    }

    #endregion

    #region 端末関連

    /// <summary>
    /// 端末起動
    /// Author:Nishiura
    /// </summary>
    /// <param name="termID">端末種別ID</param>
    /// <returns></returns>
    public async UniTask BootTerminalAsync(int termID)
    {
        await roomHub.BootTerminalAsync(termID);
    }

    /// <summary>
    /// 端末成功処理
    /// </summary>
    /// <param name="termID"></param>
    /// <returns></returns>
    public async UniTask TerminalSuccessAsync(int termID)
    {
        await roomHub.TerminalSuccessAsync(termID);
    }

    /// <summary>
    /// 端末失敗処理
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
