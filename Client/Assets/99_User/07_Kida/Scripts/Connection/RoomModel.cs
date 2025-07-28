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
    private GrpcChannel channel;  //サーバーURL
    private IRoomHub roomHub;     //roomHubの関数を呼び出す時に使う

    //マスタークライアントかどうか
    public bool IsMaster { get; set; }

    //接続ID
    public Guid ConnectionId { get; private set; }

    #region 通知定義一覧

    public Dictionary<Guid, JoinedUser> joinedUserList { get; private set; } = new Dictionary<Guid, JoinedUser>();

    //ユーザー接続通知
    public Action<JoinedUser> OnJoinedUser { get; set; }

    //ユーザー退室通知
    public Action<JoinedUser> OnLeavedUser { get; set; }

    //準備完了通知
    public Action<Guid> OnReadySyn {  get; set; }

    //ゲーム開始通知
    public Action OnStartedGame { get; set; }

    //プレイヤー位置回転通知
    public Action<PlayerData> OnMovePlayerSyn { get; set; }

    //マスタークライアントの変更通知
    public Action OnChangedMasterClient {  get; set; }

    //マスタークライアントの更新通知
    public Action<MasterClientData> OnUpdateMasterClientSyn { get; set; }

    //脱出通知
    public Action<JoinedUser> OnEscapeCharacter { get; set; }

    //敵の出現通知
    public Action<EnemyData, Vector3> OnSpawndEnemy { get; set; }

    //敵の移動通知
    public Action<int , Vector2 , Quaternion , EnemyAnimState> OnMoveEnemySyn { get; set; }

    //敵の撃破通知
    public Action<string> OnExcusionedEnemy { get; set; }

    //マスタークライアント譲渡
    public Action<JoinedUser> OnMasteredClient { get; set; }

    //オブジェクトの移動回転通知
    public Action<string, Vector3, Quaternion> OnMovedObject { get; set; }

    //レリックの生成通知
    public Action<int, Vector2> OnSpawnedRelic {  get; set; }

    //レリックの取得通知
    public Action<int , string> OnGotRelic {  get; set; }

    //ギミックの起動通知
    public Action<GimmickData> OnBootedGimmick { get; set; }

    //難易度上昇通知
    public Action<int> OnAscendDifficultySyn {  get; set; }

    //次ステージ進行通知
    public Action<int> OnAdanceNextStageSyn { get; set; }

    //プレイヤー体力増減通知
    public Action<int,float> OnPlayerHealthSyn {  get; set; }

    //敵体力増減通知
    public Action<int,float> OnEnemyHealthSyn { get; set; }

    //敵死亡通知
    public Action<int> OnKilledEnemySyn {  get; set; }

    //経験値通知
    public Action<int> OnEXPSyn {  get; set; }

    //レベルアップ通知
    public Action OnLevelUpSyn { get; set; }

    //プレイヤーダウン通知
    public Action<int> OnPlayerDeadSyn {  get; set; }

    //ダメージ表記通知
    public Action<int> OnDamaged {  get; set; }
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

    private void OnDisable()
    {
        Debug.Log("死にました");
    }

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

    #region 同期の処理
    #region 入室・退室・準備完了同期
    /// <summary>
    /// 入室同期
    /// Aughter:木田晃輔
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
                Debug.Log("モデル：" + RoomModel.Instance.ConnectionId);
            }
        }
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
    /// 退室の同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <returns></returns>
    public async UniTask LeaveAsync()
    {
        await roomHub.LeavedAsync();
        //自分をリストから消す
        joinedUserList.Clear();
    }

    /// <summary>
    /// 準備完了同期
    /// </summary>
    /// <returns></returns>
    public async Task ReadyAsync()
    {
        await roomHub.ReadyAsync();
    }
    #endregion
    #region プレイヤー同期関連
    /// <summary>
    /// プレイヤーの移動同期
    /// Aughter:木田晃輔
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
    /// プレイヤーの体力増減同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="playerHP"></param>
    /// <returns></returns>
    public async Task PlayerHealthAsync(int playerID, float playerHP)
    {
        await roomHub.PlayerHealthAsync(playerID, playerHP);
    }

    /// <summary>
    /// プレイヤーのレベルアップ同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <returns></returns>
    public async Task LevelUpAsync()
    {
        await roomHub.LevelUpAsync();
    }

    /// <summary>
    /// プレイヤーダウン同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="playerID"></param>
    /// <returns></returns>
    public async Task PlayerDeadAsync(int playerID)
    {
        await roomHub.PlayerDeadAsync(playerID);
    }

    /// <summary>
    /// 経験値同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public async Task EXPAsync(int exp)
    {
        await roomHub.EXPAsync(exp);
    }

    #endregion
    #region 敵同期関連
    /// <summary>
    /// 敵の位置同期
    /// Aughter:木田晃輔
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
    /// 敵の生成同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="enemID"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public async Task SpawnEnemyAsync(List<int> enemID, Vector2 pos)
    {
        await roomHub.SpawnEnemyAsync(enemID, pos);
    }

    /// <summary>
    /// 敵体力増減同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="enemID"></param>
    /// <param name="enemHP"></param>
    /// <returns></returns>
    public async Task EnemyHealthAsync(int enemID, float enemHP)
    {
        await roomHub.EnemyHealthAsync(enemID, enemHP);
    }

    /// <summary>
    /// 敵死亡同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="enemID"></param>
    /// <returns></returns>
    public async Task KilledEnemyAsync(int enemID)
    {
        await roomHub?.KilledEnemyAsync(enemID);
    }
    #endregion
    #region レリック同期関連
    /// <summary>
    /// レリック生成同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public async Task SpawnRelicAsync(Vector2 pos)
    {
        await roomHub.SpawnRelicAsync(pos);
    }

    /// <summary>
    /// レリック取得同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="relicID"></param>
    /// <param name="relicName"></param>
    /// <returns></returns>
    public async Task GetRelicAsync(int relicID, string relicName)
    {
        await roomHub.GetRelicAsync(relicID, relicName);
    }
    #endregion
    #region ゲーム内UI・仕様の同期関連
    /// <summary>
    /// ギミックの起動同期
    ///  Aughter:木田晃輔
    /// </summary>
    /// <param name="gimID"></param>
    /// <returns></returns>
    public async Task BootGimmickAsync(int gimID)
    {
        await roomHub.BootGimmickAsync(gimID);
    }

    /// <summary>
    /// 難易度上昇の同期
    /// </summary>
    /// <param name="difID"></param>
    /// <returns></returns>
    public async Task AscendDifficultyAsync(int difID)
    {
        await roomHub.AscendDifficultyAsync(difID);
    }

    /// <summary>
    /// 次ステージ進行の同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="stageID"></param>
    public async Task AdvanceNextStageAsync(int stageID, bool isBossStage)
    {
        await roomHub.AdvanceNextStageAsync(stageID, isBossStage);
    }

    /// <summary>
    /// ダメージ表記同期
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="dmg"></param>
    /// <returns></returns>
    public async Task DamageAsync(int dmg)
    {
        await roomHub.DamageAsync(dmg);
    }

    #endregion
    #endregion

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
    public void OnMovePlayer(PlayerData playerData)
    {
        OnMovePlayerSyn(playerData);
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
    /// プレイヤーの体力増減通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="playerHP"></param>
    public void OnPlayerHealth(int playerID, float playerHP)
    {
        OnPlayerHealthSyn(playerID, playerHP);
    }

    /// <summary>
    /// プレイヤーダウン通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="playerID"></param>
    public void OnPlayerDead(int playerID)
    {
        OnPlayerDeadSyn(playerID);
    }


    /// <summary>
    /// プレイヤーの経験値通知
    /// Author:Nishiura
    /// </summary>
    /// <param name="exp">経験値</param>
    public void OnEXP(int exp)
    {
        OnEXPSyn(exp);
    }

    /// <summary>
    /// プレイヤーのレベルアップ通知
    /// Aughter:木田晃輔
    /// </summary>
    public void OnLevelUp()
    {
        OnLevelUpSyn();
    }
    #endregion
    #region 敵通知関連
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
    /// 敵の生成通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="enemData"></param>
    /// <param name="pos"></param>
    public void OnSpawnEnemy(EnemyData enemData, Vector2 pos)
    {
        OnSpawndEnemy(enemData, pos);
    }

    /// <summary>
    /// 敵体力増減通知
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
        OnKilledEnemySyn(enemID);
    }
    #endregion
    #region レリック通知関連
    /// <summary>
    /// レリック生成通知
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
    public void OnBootGimmick(GimmickData gimmickData)
    {
        OnBootedGimmick(gimmickData);
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
    public void OnAdanceNextStage(int stageID)
    {
        OnAdanceNextStageSyn(stageID);
    }

    /// <summary>
    /// ダメージ表記通知
    /// Aughter:木田晃輔
    /// </summary>
    /// <param name="dmg"></param>
    public void OnDamage(int dmg)
    {
        OnDamaged(dmg);
    }
    #endregion
    #endregion
}
