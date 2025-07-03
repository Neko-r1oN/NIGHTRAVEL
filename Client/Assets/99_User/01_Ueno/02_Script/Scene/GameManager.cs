//----------------------------------------------------
// ゲームマネージャー(GameManager.cs)
// Author : Souma Ueno
//----------------------------------------------------
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
    int crashNum = 0; 　　　　　    // 撃破数
    bool bossFlag = false;      // ボスが出たかどうか
    int xp;                     // 経験値
    int requiredXp = 100;       // 必要経験値
    int level;                  // レベル
    int num;                    // 生成までのカウント
    public int spawnInterval;   // 生成間隔
    int spawnCnt;               // スポーン回数
    public int maxSpawnCnt;     // マックススポーン回数
    bool isBossDead;            // ボスが死んだかどうか
    bool isSpawnBoss;           // ボスが生成されたかどうか
    GameObject boss;            // ボス

    #endregion

    #region その他
    [Header("その他")]
    [SerializeField] GameObject bossPrefab;  // ボスプレハブ
    [SerializeField] Transform minCameraPos; // カメラ範囲の最小値
    [SerializeField] Transform maxCameraPos; // カメラ範囲の最大値
    [SerializeField] float xRadius;          // 生成範囲のx半径
    [SerializeField] float yRadius;          // 生成範囲のy半径
    [SerializeField] float distMinSpawnPos;  // 生成しない範囲
    [SerializeField] int knockTermsNum;      // ボスのエネミーの撃破数条件

    [SerializeField] GameObject player;      // プレイヤーの情報

    float elapsedTime;

    Vector3 bossPos;
    #endregion

    #region 各プロパティ
    [Header("各プロパティ")]
    public bool BossFlag { get { return bossFlag; } set { bossFlag = value; } }

    public GameObject Player { get { return player; } }

    public GameObject Boss {  get { return boss; } }

    public int SpawnInterval { get { return spawnInterval; } set { spawnInterval = value; } }

    public bool IsSpawnBoss { get { return isSpawnBoss; } }

    public int KnockTermsNum { get { return knockTermsNum; } }

    public int SpawnCnt { get { return spawnCnt; } set { spawnCnt = value; } }

    private static GameManager instance;

    //public bool IsBossDead { get { return bossFlag; } set { isBossDead = value; } } 
    #endregion

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

    /// <summary>
    /// 初期設定
    /// </summary>
    void Start()
    {
        isBossDead = false;

       // UIManager.Instance.CountClashText(crashNum);
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    void Update()
    {
        if (!isSpawnBoss && bossFlag)
        {
            // ボスの生成範囲の判定
            var spawnPostions = SpawnManager.Instance.CreateEnemySpawnPosition(minCameraPos.position, maxCameraPos.position);

            EnemyBase bossEnemy = bossPrefab.GetComponent<EnemyBase>();

            Vector3? spawnPos = SpawnManager.Instance.GenerateEnemySpawnPosition(spawnPostions.minRange, spawnPostions.maxRange,bossEnemy);

            if (spawnPos != null)
            {// 返り値がnullじゃないとき
                boss = Instantiate(bossPrefab, (Vector3)spawnPos, Quaternion.identity);
                boss.GetComponent<EnemyBase>().IsBoss = true;

                boss.GetComponent<EnemyBase>().Players.Add(player);
                boss.GetComponent<EnemyBase>().SetNearTarget();
            }

            isSpawnBoss = true;

            bossFlag = false;
        }

        if (isBossDead)
        {// ボスを倒した(仮)
            // 遅れて呼び出し
            Invoke(nameof(ChengScene), 15f);
        }

        if (spawnCnt < maxSpawnCnt  && !isBossDead)
        {// スポーン回数が限界に達しているか
            elapsedTime += Time.deltaTime;
            if (elapsedTime > spawnInterval)
            {
                elapsedTime = 0;

                if (spawnCnt < maxSpawnCnt / 2)
                {// 敵が100体いない場合
                    for (int i = 0; i < 5; i++)
                    {// 複数体敵を生成
                        // 敵生成処理
                        SpawnManager.Instance.GenerateEnemy();
                    }
                }
                else
                {// いる場合
                    SpawnManager.Instance.GenerateEnemy();
                }
            }
        }
    }

    /// <summary>
    /// シーン遷移
    /// </summary>
    private void ChengScene()
    {// シーン遷移
        SceneManager.LoadScene("Result ueno");
    }

    [ContextMenu("CrushEnemy")]
    /// <summary>
    ///  敵撃破
    /// </summary>
    public void CrushEnemy(EnemyBase enemy)
    {
        crashNum++;

        UIManager.Instance.CountClashText(crashNum);

        //Debug.Log("倒した数：" + crashNum);

        spawnCnt--;

        if (enemy.IsBoss)
        {
            DeathBoss();
        }
        else if (crashNum >= knockTermsNum)
        {// 撃破数が15以上になったら(仮)

            DeathBoss();

            bossFlag = true;
            Debug.Log("倒した数：" + crashNum + "ボス");
        }
    }

    [ContextMenu("DeathBoss")]
    public void DeathBoss()
    {
        RelicManager.Instance.GenerateRelic(boss.transform.position);

        // ボスフラグを変更
        bossFlag = false;
        // 死んだ判定にする
        isBossDead = true;
    }

    private void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.DrawWireCube(player.transform.position, new Vector3(distMinSpawnPos * 2, yRadius * 2));
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(player.transform.position, new Vector3(xRadius * 2, yRadius * 2));
        }
    }

    [ContextMenu("DecreaseGeneratInterval")]
    /// <summary>
    /// 時間経過毎にスポーン間隔を早める処理
    /// </summary>
    public void DecreaseGeneratInterval()
    {
        spawnInterval -= 2;
    }
}
