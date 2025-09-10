//----------------------------------------------------
// ゲームマネージャー(GameManager.cs)
// Author : Souma Ueno
//----------------------------------------------------
using JetBrains.Annotations;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
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
    #endregion

    #region その他
    [Header("その他")]
    [SerializeField] GameObject bossPrefab;  // ボスプレハブ
    [SerializeField] Transform minCameraPos; // カメラ範囲の最小値
    [SerializeField] Transform maxCameraPos; // カメラ範囲の最大値
    [SerializeField] float xRadius;          // 生成範囲のx半径
    [SerializeField] float yRadius;          // 生成範囲のy半径
    [SerializeField] float distMinSpawnPos;  // 生成しない範囲
    [SerializeField] int knockTermsNum;      // エネミーの撃破数条件
    [SerializeField] List<GameObject> portals;

    float elapsedTime;

    Vector3 bossPos;
    #endregion

    #region 各プロパティ
    [Header("各プロパティ")]
    public int CrashNum { get { return crashNum; } }

    public bool IsBossDead { get { return isBossDead; } }

    public int KnockTermsNum { get { return knockTermsNum; } }

    public bool IsGameStart {  get { return isGameStart; } set { isGameStart = value; } }

    private static GameManager instance;

    //public bool IsBossDead { get { return bossFlag; } set { isBossDead = value; } } 
    #endregion

    #region

    // ターミナル関連 (MAXの値はRandで用いるため、上限+1の数)
    private const int MIN_TERMINAL_NUM = 3;
    private const int MAX_TERMINAL_NUM = 7;
    private const int MIN_TERMINAL_ID = 1;
    private const int MAX_TERMINAL_ID = 6;

    #endregion

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

        foreach(var player in CharacterManager.Instance.PlayerObjs)
        {
            player.Value.gameObject.GetComponent<PlayerBase>().CanMove = false;
            player.Value.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }
    }
    #endregion

    /// <summary>
    /// 初期設定
    /// </summary>
    async void Start()
    {
        if (GameObject.Find("BossTerminal") != null) // ステージに1つのユニークな端末の為、名前で取得
            bossTerminal = GameObject.Find("BossTerminal");
        
        isBossDead = false;
        //Debug.Log(LevelManager.Instance.GameLevel.ToString());
        UIManager.Instance.ShowUIAndFadeOut();

        if (!RoomModel.Instance) StartGame(LotteryTerminal());
        else
        {
            RoomModel.Instance.OnSameStartSyn += this.StartGame;
            await RoomModel.Instance.AdvancedStageAsync();  //遷移完了のリクエスト
        }
    }

    private void OnDisable()
    {
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnSameStartSyn -= this.StartGame;
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

    public void DisplayPortal()
    {
        foreach (var item in portals)
        {
            item.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// シーン遷移
    /// </summary>
    public void ChengScene()
    {// シーン遷移
        SceneManager.LoadScene("Stage Ueno");

        isGameStart = false;
    }

    public void CangeResult()
    {
        SceneManager.UnloadSceneAsync("UIScene");
        SceneManager.LoadScene("ResultScene", LoadSceneMode.Additive);

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
    private void DeathBoss()
    {
        //RelicManager.Instance.GenerateRelic(SpawnManager.Instance.Boss.transform.position);

        //RelicManager.Instance.GenerateRelic(bossTerminal);

        //RelicManager.Instance.GenerateRelicTest();

        // 死んだ判定にする
        isBossDead = true;

        GameObject portal;

        for(int i = 0; i < portals.Count; i++)
        {
            portal = Instantiate(portals[i], portals[i].transform.position, Quaternion.identity);
        }

        //Invoke(nameof(ChengScene), 15f);
    }

    public void StartGame(List<TerminalData> list)
    {
        // 端末情報をステージに反映
        TerminalManager.Instance.SetTerminal(list);

        isGameStart = true;
        Debug.Log("同時開始！！");
        foreach(var player in CharacterManager.Instance.PlayerObjs)
        {
            player.Value.gameObject.GetComponent<PlayerBase>().CanMove = true;
            player.Value.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }
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
        int terminalCount = Random.Range(MIN_TERMINAL_NUM, MAX_TERMINAL_NUM); // 3～6個の端末を抽選

        for (int i = 3; i <= terminalCount; i++)
        {
            int termID = Random.Range(MIN_TERMINAL_ID, MAX_TERMINAL_ID);

            terminals.Add(new TerminalData() { ID = i, Type = (TERMINAL_TYPE)termID, State = TERMINAL_STATE.Inactive });
        }

        return terminals;
    }
}