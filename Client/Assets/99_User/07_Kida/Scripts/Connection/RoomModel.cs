////////////////////////////////////////////////////////////////
///
/// RoomHubへの接続を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

#region using一覧
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
    private GrpcChannel channel;  //サーバーURL
    private IRoomHub roomHub;     //roomHubの関数を呼び出す時に使う

    //マスタークライアントかどうか
    public bool IsMaster { get; set; }

    //接続ID
    public Guid ConnectionId { get; private set; }

    // 現在の参加者情報
    public Dictionary<Guid, JoinedUser> joinedUserList { get; private set; } = new Dictionary<Guid, JoinedUser>();

    #region 通知定義一覧

    #region システム

    //ユーザー接続通知
    public Action<JoinedUser> OnJoinedUser { get; set; }

    //ユーザー退室通知
    public Action<JoinedUser> OnLeavedUser { get; set; }

    //準備完了通知
    public Action<Guid> OnReadySyn { get; set; }

    //ゲーム開始通知
    public Action OnStartedGame { get; set; }

    //同時開始通知
    public Action OnSameStartSyn { get; set; }

    //難易度上昇通知
    public Action<int> OnAscendDifficultySyn { get; set; }

    //次ステージ進行通知
    public Action<bool, STAGE_TYPE> OnAdanceNextStageSyn { get; set; }

    //レベルアップ通知
    public Action<int, int, Dictionary<Guid, CharacterStatusData>, List<EnumManager.STAT_UPGRADE_OPTION>> OnLevelUpSyn { get; set; }

    //ステージ進行通知
    public Action OnAdvancedStageSyn { get; set; }

    //ゲーム終了通知
    public Action<ResultData> OnGameEndSyn { get; set; }

    #endregion

    #region プレイヤー・マスタクライアント

    //マスタークライアント譲渡
    public Action<JoinedUser> OnMasteredClient { get; set; }

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

    #endregion

    #region 敵

    //敵の出現通知
    public Action<List<SpawnEnemyData>> OnSpawndEnemy { get; set; }

    //敵体力増減通知
    public Action<EnemyDamegeData> OnEnemyHealthSyn { get; set; }


    #endregion

    #region アイテム

    //レリックの生成通知
    public Action<Dictionary<string, DropRelicData>> OnDropedRelic { get; set; }

    //アイテム獲得通知
    public Action<Guid, string> OnGetItemSyn { get; set; }

    #endregion

    #region ギミック

    //ギミックの起動通知
    public Action<int> OnBootedGimmick { get; set; }


    #endregion

    #region 端末

    //端末起動通知
    public Action<int> OnBootedTerminal { get; set; }

    //端末結果通知
    public Action<int, bool> OnTerminalsResultSyn { get; set; }

    #endregion

    #region 発射物

    // 発射物の生成通知
    public Action<PROJECTILE_TYPE, List<DEBUFF_TYPE>, int, Vector2, Vector2, Quaternion> OnShootedBullet { get; set; }

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
    }

    #region 通知の処理
    #region 入室・退室・準備完了通知
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

    /// <summary>
    /// 退室通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="user"></param>
    public void OnLeave(JoinedUser joinedUser)
    {
        if (joinedUserList.ContainsKey(joinedUser.ConnectionId))
            joinedUserList.Remove(joinedUser.ConnectionId);
        OnLeavedUser(joinedUser);
    }

    /// <summary>
    /// 準備完了通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="conID"></param>
    public void OnReady(Guid conID)
    {
        OnReadySyn(conID);
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
    public void OnLevelUp(int level, int nowExp, Dictionary<Guid, CharacterStatusData> characterStatusDataList, List<EnumManager.STAT_UPGRADE_OPTION> statusOptionList)
    {
       // OnLevelUpSyn(level,nowExp,characterStatusDataList,statusOptionList);
    }

    /// <summary>
    /// 発射物の生成通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnShootBullet(PROJECTILE_TYPE type, List<DEBUFF_TYPE> debuffs, int power, Vector2 spawnPos, Vector2 shootVec, Quaternion rotation)
    {
        OnShootedBullet(type, debuffs, power, spawnPos, shootVec, rotation);
    }

    public void OnUpdateStatus(CharacterStatusData cdata, PlayerRelicStatusData rdata)
    {
    }
    #endregion
    #region 敵通知関連

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
    public void OnBootGimmick(int gimmickId)
    {
        OnBootedGimmick(gimmickId);
    }

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
    public void OnTerminalsResult(int termID, bool result)
    {
        OnTerminalsResultSyn(termID, result);
    }

    /// <summary>
    /// アイテム獲得通知
    /// Author:木田晃輔
    /// </summary>
    public void OnGetItem(Guid conId, string itemID)
    {
        OnGetItemSyn(conId, itemID);
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
    public void OnAdanceNextStage(bool isAdvance, STAGE_TYPE stageType)
    {
        OnAdanceNextStageSyn(isAdvance,stageType);
    }

    /// <summary>
    /// ステージ進行通知
    /// Author;木田晃輔
    /// </summary>
    public void OnAdvancedStage()
    {
        OnAdvancedStageSyn();
    }

    #endregion

    /// <summary>
    /// 同時開始
    /// Aughtor:木田晃輔
    /// </summary>
    public void OnSameStart()
    {
        OnSameStartSyn();
    }

    /// <summary>
    /// ゲーム終了通知
    /// </summary>
    /// <param name="result"></param>
    public void OnGameEnd(ResultData result)
    {
        OnGameEndSyn(result);
    }
    #endregion

    #region リクエスト関連
    #region 入室からゲーム開始まで
    /// <summary>
    /// 入室同期
    /// Aughter:木田晃輔
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
        await roomHub.LeavedAsync();
        this.IsMaster = false;
        //自分をリストから消す
        joinedUserList.Clear();
    }

    /// <summary>
    /// 準備完了同期
    /// </summary>
    /// <returns></returns>
    public async UniTask ReadyAsync()
    {
        await roomHub.ReadyAsync();
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
    public async UniTask PlayerDeadAsync()
    {
        await roomHub.PlayerDeadAsync();
    }

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
    /// Author:Nishiura
    /// </summary>
    /// <param name="termID">端末種別ID</param>
    /// <param name="result">端末結果</param>
    /// <returns></returns>
     public async UniTask TerminalsResultAsync(int termID, bool result)
    {
        await roomHub.TerminalsResultAsync(termID, result);
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
    public async UniTask ShootBulletAsync(PROJECTILE_TYPE type, List<DEBUFF_TYPE> debuffs, int power, Vector2 spawnPos, Vector2 shootVec, Quaternion rotation)
    {
        await roomHub.ShootBulletAsync(type, debuffs, power, spawnPos, shootVec, rotation);
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
    /// ステータス強化選択
    /// </summary>
    /// <param name="conID">接続ID</param>
    /// <param name="upgradeOpt">強化項目</param>
    /// <returns></returns>
     public async UniTask ChooseUpgrade(EnumManager.STAT_UPGRADE_OPTION upgradeOpt)
    {
        await roomHub.ChooseUpgrade(upgradeOpt);
    }
    #endregion
    #region レリック関連

    /// <summary>
    /// レリック生成同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public async UniTask DropRelicAsync(Stack<Vector2> pos)
    {
        await roomHub.DropRelicAsync(pos);
    }
    #endregion
    #region ゲーム内UI、仕様関連
    /// <summary>
    /// ギミックの起動同期
    ///  Aughter:木田晃輔
    /// </summary>
    /// <param name="gimID"></param>
    /// <returns></returns>
    public async UniTask BootGimmickAsync(int gimID)
    {
        await roomHub.BootGimmickAsync(gimID);
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
    #endregion
    #endregion
    #endregion
}
