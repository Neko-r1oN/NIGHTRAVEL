//----------------------------------------------------
// ゲームマネージャー(GameManager.cs)
// Author : Souma Ueno
//----------------------------------------------------
using JetBrains.Annotations;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Grpc.Core.Metadata;
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
    [SerializeField] List<GameObject> portal;

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
        bossTerminal = GameObject.Find("BossTerminal");     // ステージに1つのユニークな端末の為、名前で取得
        isBossDead = false;
        //Debug.Log(LevelManager.Instance.GameLevel.ToString());
        UIManager.Instance.ShowUIAndFadeOut();

        if (!RoomModel.Instance) Invoke("StartGame", 0f);
        else
        {
            RoomModel.Instance.OnSameStartSyn += this.StartGame;
            await RoomModel.Instance.AdvancedStageAsync();  //遷移完了のリクエスト
        }

        foreach (GameObject obj in portal)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
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
    }

    public void DisplayPortal()
    {
        foreach (var item in portal)
        {
            item.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// シーン遷移
    /// </summary>
    public void ChengScene()
    {// シーン遷移
        SceneManager.LoadScene("stage");

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

        RelicManager.Instance.GenerateRelic(bossTerminal);

        // 死んだ判定にする
        isBossDead = true;

        foreach(GameObject obj in portal)
        {
            obj.SetActive(true);
        }

        //Invoke(nameof(ChengScene), 15f);
    }

    public void StartGame()
    {
        isGameStart = true;
        Debug.Log("同時開始！！");
        foreach(var player in CharacterManager.Instance.PlayerObjs)
        {
            player.Value.gameObject.GetComponent<PlayerBase>().CanMove = true;
            player.Value.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }
    }
}