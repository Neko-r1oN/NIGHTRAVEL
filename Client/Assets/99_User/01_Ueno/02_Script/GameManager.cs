//----------------------------------------------------
// ゲームマネージャー(GameManager.cs)
// Author : Souma Ueno
//----------------------------------------------------
using JetBrains.Annotations;
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
    int crashNum = 0; 　　　　　// 撃破数
    int xp;                     // 経験値
    int requiredXp = 100;       // 必要経験値
    int level;                  // レベル
    int num;                    // 生成までのカウント
    public int spawnInterval;   // 生成間隔
    bool isBossDead;            // ボスが死んだかどうか
    bool isGameStart;           // ゲームが開始したかどうか
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

    float elapsedTime;

    Vector3 bossPos;
    #endregion

    #region 各プロパティ
    [Header("各プロパティ")]

    public int SpawnInterval { get { return spawnInterval; } set { spawnInterval = value; } }

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
    }
    #endregion

    /// <summary>
    /// 初期設定
    /// </summary>
    void Start()
    {
        isBossDead = false;
        //Debug.Log(LevelManager.Instance.GameLevel.ToString());
        UIManager.Instance.ShowUIAndFadeOut();

        isGameStart = true;
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
        else if(Input.GetKeyDown(KeyCode.S))
        {
            Time.timeScale = 20;
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

        //if (spawnCnt < maxSpawnCnt && !isBossDead)
        //{// スポーン回数が限界に達しているか
        //    elapsedTime += Time.deltaTime;
        //    if (elapsedTime > spawnInterval)
        //    {
        //        elapsedTime = 0;

        //        if (!isSpawnBoss)
        //        {
        //            if (spawnCnt < maxSpawnCnt / 2)
        //            {// 敵が100体いない場合
        //                SpawnManager.Instance.GenerateEnemy(Random.Range(3, 7));
        //            }
        //            else
        //            {// いる場合
        //                SpawnManager.Instance.GenerateEnemy(1);
        //            }

        //            Debug.Log(spawnCnt);
        //        }
        //    }
        //}
    }

    /// <summary>
    /// シーン遷移
    /// </summary>
    private void ChengScene()
    {// シーン遷移
        SceneManager.LoadScene("Result ueno");

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

        SpawnManager.Instance.SpawnCnt--;

        if (enemy.IsBoss)
        {
            DeathBoss();
        }
    }

    [ContextMenu("DeathBoss")]
    private void DeathBoss()
    {
        RelicManager.Instance.GenerateRelic(SpawnManager.Instance.Boss.transform.position);

        // ボスフラグを変更
        //bossFlag = false;
        // 死んだ判定にする
        isBossDead = true;

        Invoke(nameof(ChengScene), 15f);
    }

    private void OnDrawGizmos()
    {
        //if (player != null)
        //{
        //    Gizmos.DrawWireCube(player.transform.position, new Vector3(distMinSpawnPos * 2, yRadius * 2));
        //    Gizmos.color = Color.yellow;
        //    Gizmos.DrawWireCube(player.transform.position, new Vector3(xRadius * 2, yRadius * 2));
        //}
    }

    [ContextMenu("DecreaseGeneratInterval")]
    /// <summary>
    /// 時間経過毎にスポーン間隔を早める処理
    /// </summary>
    private void DecreaseGeneratInterval()
    {
        spawnInterval -= 2;
    }
}