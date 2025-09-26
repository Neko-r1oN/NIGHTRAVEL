//----------------------------------------------------
// ゲームマネージャー(GameManager.cs)
// Author : Souma Ueno
//----------------------------------------------------
using JetBrains.Annotations;
using KanKikuchi.AudioManager;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Grpc.Core.Metadata;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    #region 初期設定
    [Header("初期設定")]
    int crashNum = 0; 　　　　　// 撃破
    int xp;                     // 経験値
    int requiredXp = 100;       // 必要経験値
    int level;                  // レベル
    bool isBossDead;            // ボスが死んだかどうか
    bool isGameStart;           // ゲームが開始したかどうか
    GameObject bossTerminal;    // ボス端末
    [SerializeField] STAGE_TYPE nextStage;    // 現在のステージ
    #endregion

    #region その他
    [Header("その他")]
    [SerializeField] Transform minCameraPos;   // カメラ範囲の最小値
    [SerializeField] Transform maxCameraPos;   // カメラ範囲の最大値
    [SerializeField] float xRadius;            // 生成範囲のx半径
    [SerializeField] float yRadius;            // 生成範囲のy半径
    [SerializeField] float distMinSpawnPos;    // 生成しない範囲
    [SerializeField] int knockTermsNum;        // エネミーの撃破数条件
    [SerializeField] List<GameObject> portals; // 遷移用ポータル

    float elapsedTime;

    Vector3 bossPos;
    #endregion

    #region 各プロパティ
    [Header("各プロパティ")]
    public int CrashNum { get { return crashNum; } }

    public bool IsBossDead { get { return isBossDead; } }

    public int KnockTermsNum { get { return knockTermsNum; } }

    public bool IsGameStart {  get { return isGameStart; } set { isGameStart = value; } }

    public STAGE_TYPE NextStage { get { return nextStage; } }

    private static GameManager instance;
    #endregion

    #region ターミナル関連

    // (MAXの値はRandで用いるため、上限+1の数)
    private const int MIN_TERMINAL_NUM = 3;
    private const int MAX_TERMINAL_NUM = 7;
    private const int MIN_TERMINAL_ID = 1;
    private const int MAX_TERMINAL_ID = 6;

    #endregion

    SceneLoader loader;

    #region Instance
    [Header("Instance")]
    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            // インスタンスが複数存在しないように、既に存在していたら自身を消去する
            Destroy(gameObject);
        }

        isGameStart = false;
    }
    #endregion

    ResultData resultData;
    public ResultData ResultData { get { return resultData; } }

    /// <summary>
    /// 初期設定
    /// </summary>
    async void Start()
    {
        if (GameObject.Find("BossTerminal") != null) // ステージに1つのユニークな端末の為、名前で取得
            bossTerminal = GameObject.Find("BossTerminal");

        foreach (var player in CharacterManager.Instance.PlayerObjs)
        {
            player.Value.gameObject.GetComponent<PlayerBase>().CanMove = false;
            player.Value.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }

        isBossDead = false;
        UIManager.Instance.ShowUIAndFadeOut();

        if (!RoomModel.Instance) StartCoroutine(DelayedCallCoroutine());
        else
        {
            RoomModel.Instance.OnSameStartSyn += this.StartGame;
            RoomModel.Instance.OnAdanceNextStageSyn += this.OnAdanceNextStageSyn;
            RoomModel.Instance.OnGameEndSyn += this.OnGameEnd;

            //遷移完了のリクエスト (TerminalManagerにて呼び出し)
            //await RoomModel.Instance.AdvancedStageAsync();  
        }

        
    }

    private void OnDisable()
    {
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnSameStartSyn -= this.StartGame;
        RoomModel.Instance.OnAdanceNextStageSyn -= this.OnAdanceNextStageSyn;
        RoomModel.Instance.OnGameEndSyn -= this.OnGameEnd;
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    void Update()
    {
        // ポーズ処理(仮)
        if(Input.GetKeyDown(KeyCode.P))
        {
            Time.timeScale = 0;
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            Time.timeScale = 1;
        }
        else if(Input.GetKeyDown(KeyCode.L))
        {// 20倍速(デバック用)
#if UNITY_EDITOR
            Time.timeScale = 20;
#endif
        }

        //Escが押された時
        if (Input.GetKey(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UIManager.Instance.DisplayEndGameWindow();
#else
    UIManager.Instance.DisplayEndGameWindow();
#endif
        }

        // オフライン用
        if (!RoomModel.Instance && !CharacterManager.Instance.PlayerObjSelf && isGameStart)
        {
            CangeResult();
        }
    }

    /// <summary>
    /// ポータルの表示
    /// </summary>
    public void DisplayPortal()
    {
        foreach (var item in portals)
        {
            item.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// シーン遷移通知
    /// </summary>
    /// <param name="type"></param>
    void OnAdanceNextStageSyn(STAGE_TYPE type)
    {
        ChengScene(type);
    }

    /// <summary>
    /// シーン遷移
    /// </summary>
    public void ChengScene(STAGE_TYPE type)
    {
        switch (type)
        {
            case STAGE_TYPE.Rust:
                Initiate.DoneFading();
                Initiate.Fade("4_Stage_01", Color.black, 0.5f);
                break;
            case STAGE_TYPE.Industry:
                Initiate.DoneFading();
                Initiate.Fade("5_Stage_02", Color.black, 0.5f);
                break;
            case STAGE_TYPE.Town:
                Initiate.DoneFading();
                Initiate.Fade("6_Stage_03", Color.black, 0.5f);
                break;
        }
        isGameStart = false;
    }

    /// <summary>
    /// ゲーム終了通知
    /// </summary>
    void OnGameEnd(ResultData resultData)
    {
        UIManager.Instance.OnDeadPlayer();
        this.resultData = resultData;
        CangeResult();
    }

    /// <summary>
    /// リザルトのシーンを読み込み
    /// </summary>
    public void CangeResult()
    {   
        Initiate.DoneFading();
        SceneManager.LoadScene("ResultScene", LoadSceneMode.Additive);
        //Initiate.Fade("ResultScene", Color.black, 0.5f);
        //SceneManager.UnloadSceneAsync("UIScene");
        UIManager.Instance.HideCanvas();

        isGameStart = false;
    }

    [ContextMenu("CrushEnemy")]
    /// <summary>
    ///  敵撃破
    /// </summary>
    public void CrushEnemy(EnemyBase enemy)
    {
        SpawnManager.Instance.CrashNum++;

        UIManager.Instance.CountTermsText(SpawnManager.Instance.CrashNum);

        //var result = CharacterManager.Instance.GetEnemiesBySpawnType(EnumManager.SPAWN_ENEMY_TYPE.ByManager);

        if (enemy.IsBoss)
        {
            DeathBoss();
        }
    }

    [ContextMenu("DeathBoss")]
    /// <summary>
    /// ボスが死んだ処理
    /// </summary>
    private void DeathBoss()
    {
        //RelicManager.Instance.GenerateRelic(SpawnManager.Instance.Boss.transform.position);

        //RelicManager.Instance.GenerateRelic(bossTerminal);

        //RelicManager.Instance.GenerateRelicTest();

        // 死んだ判定にする
        isBossDead = true;

        UIManager.Instance.HideBossUI();

        for(int i = 0; i < portals.Count; i++)
        {
            portals[i].SetActive(true);
        }

        //Invoke(nameof(ChengScene), 15f);
    }

    /// <summary>
    /// ゲーム開始処理
    /// </summary>
    /// <param name="list"></param>
    public void StartGame(List<TerminalData> list)
    {
        //// 端末情報をステージに反映
        if (list != null)
            TerminalManager.Instance.SetTerminal(list);

        isGameStart = true;
        Debug.Log("同時開始！！");
        foreach(var player in CharacterManager.Instance.PlayerObjs)
        {
            player.Value.gameObject.GetComponent<PlayerBase>().CanMove = true;
            player.Value.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private IEnumerator DelayedCallCoroutine()
    {
        yield return new WaitForSeconds(0f);

        StartGame(LotteryTerminal());
    }

    /// <summary>
    /// 端末データ抽選処理
    /// Autho:Nakamoto
    /// </summary>
    /// <returns></returns>
    private List<TerminalData> LotteryTerminal()
    {
        // ID1,2は固定で設定
        List<TerminalData> terminals = new List<TerminalData>()
            {
                new TerminalData(){ ID = 1, Type = TERMINAL_TYPE.Boss, State = TERMINAL_STATE.Inactive},
                new TerminalData(){ ID = 2, Type = TERMINAL_TYPE.Speed, State = TERMINAL_STATE.Inactive},
            };

        // 3以降は抽選
        int terminalCount = Random.Range(MIN_TERMINAL_NUM, MAX_TERMINAL_NUM);   // 3～6個の端末を抽選

        for (int i = 3; i <= terminalCount; i++)
        {
            int termID = 0;

            while (termID == 0 || termID == 2 || termID == 6)
            {   // SpeedとBossは固定で設定しているため、抽選から除外
                termID = Random.Range(MIN_TERMINAL_ID, MAX_TERMINAL_ID);
            }

            terminals.Add(new TerminalData() { ID = i, Type = (TERMINAL_TYPE)termID, State = TERMINAL_STATE.Inactive });
        }

        return terminals;
    }
}