//----------------------------------------------------
// ゲームマネージャー(GameManager.cs)
// Author : Souma Ueno
//----------------------------------------------------
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Grpc.Core.Metadata;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    #region 初期設定
    [Header("初期設定")]
    int crushNum; 　　　　　// 撃破数
    bool bossFlag = false;  // ボスが出たかどうか
    int xp;                 // 経験値
    int requiredXp = 100;   // 必要経験値
    int level;              // レベル
    int num;                // 生成までのカウント
    public int createCnt;   // 生成間隔
    int spawnCnt;           // スポーン回数
    public int maxSpawnCnt; // マックススポーン回数
    bool isBossDead;
    bool isSpawnBoss;
    #endregion

    #region その他
    [Header("その他")]
    public List<GameObject> enemyList;       // エネミーリスト
    [SerializeField] GameObject boss;        // ボス
    [SerializeField] Transform randRespawnA; // リスポーン範囲A
    [SerializeField] Transform randRespawnB; // リスポーン範囲B
    [SerializeField] Transform minCameraPos;
    [SerializeField] Transform maxCameraPos;
    [SerializeField] float xRadius;
    [SerializeField] float yRadius;
    [SerializeField] float distMinSpawnPos;

    [SerializeField] GameObject player;      // プレイヤーの情報
    GameObject enemy;                        // エネミーの情報

    public GameObject Enemy {  get { return enemy; } }

    public bool BossFlag { get { return bossFlag; } set { bossFlag = value; } }

    //public bool IsBossDead { get { return bossFlag; } set { isBossDead = value; } } 
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ボスを非表示
        //boss.SetActive(false);
        // プレイヤーのオブジェクト検索して取得
        player = GameObject.Find("PlayerSample");
        isBossDead = false;
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    void Update()
    {
        if (!isSpawnBoss && bossFlag)
        {
            // ボスの生成範囲の判定
            var spawnPostions = CreateEnemySpawnPosition(minCameraPos.position, maxCameraPos.position);

            Vector3? spawnPos = GenerateEnemySpawnPosition(spawnPostions.minRange,spawnPostions.maxRange);


            if (spawnPos != null)
            {// 返り値がnullじゃないとき
                Instantiate(boss, (Vector3)spawnPos, Quaternion.identity);
            }

            isSpawnBoss = true;

            bossFlag = false;
        }

        if (isBossDead)
        {// ボスを倒した(仮)
            //bossFlag = false;
            //boss.SetActive(false);

            // 遅れて呼び出し
            Invoke(nameof(ChengScene), 1.5f);
        }

        if (spawnCnt < maxSpawnCnt)
        {// スポーン回数が限界に達しているか
            num++;
            if (num % createCnt == 0)
            {
                num = 0;

                Vector2 minPlayer =
                    new Vector2(player.transform.position.x - xRadius, player.transform.position.y - yRadius);

                Vector2 maxPlayer =
                    new Vector2(player.transform.position.x + xRadius, player.transform.position.y + yRadius);

                // ランダムな位置を生成
                var spawnPostions = CreateEnemySpawnPosition(minPlayer, maxPlayer);

                // ランダムな位置を生成
                //Vector3 spawnPos = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY));

                Vector3 ? spawnPos = GenerateEnemySpawnPosition(spawnPostions.minRange,spawnPostions.maxRange);

                if (spawnPos != null)
                {
                    spawnCnt++;
                    int listNum = Random.Range(0, enemyList.Count);

                    // 生成
                    enemy = Instantiate(enemyList[listNum], (Vector3)spawnPos, Quaternion.identity);

                    enemy.GetComponent<EnemyController>().Players.Add(player);

                    if (enemy.GetComponent<Rigidbody2D>().gravityScale != 0)
                    {
                        enemy.GetComponent<EnemyController>().enabled = false;

                        // 透明化
                        enemy.GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }
        }
        else
        {
            Debug.Log("生成限界");
        }
    }

    /// <summary>
    /// シーン変更
    /// </summary>
    private void ChengScene()
    {// シーン変更
        SceneManager.LoadScene("Result ueno");
    }

    [ContextMenu("CrushEnemy")]
    /// <summary>
    ///  敵撃破
    /// </summary>
    public void CrushEnemy()
    {
        crushNum++;

        spawnCnt--;
        AddXp();

        EnemyController enemy = GetComponent<EnemyController>();

        //Debug.Log(crushNum);
        if(enemy.name == "boss")
        {
            DeathBoss();
        }
        else if (crushNum >= 15)
        {// 撃破数が15以上になったら(仮)

            bossFlag = true;

            //boss.SetActive(true);
            //Debug.Log("ボスでてきた");
            //crushNum = 0;
        }
    }

    /// <summary>
    /// 経験値加算
    /// </summary>
    public void AddXp()
    {
        xp += 100;
        if (xp >= requiredXp)
        {// 必要経験値数を超えたら

            // 必要経験値数を増やす
            requiredXp += xp;
            Debug.Log(requiredXp);
            // レベルアップ関数を
            UpLevel();
        }
    }

    /// <summary>
    /// レベルアップ
    /// </summary>
    public void UpLevel()
    {
        level++;
        Debug.Log("レベルアップ:" + level);
    }

    [ContextMenu("DeathBoss")]
    public void DeathBoss()
    {
        // ボスフラグを変更
        bossFlag = false;
        // 死んだ判定にする
        isBossDead = true;
    }

    private void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.DrawWireCube(player.transform.position, new Vector3(distMinSpawnPos * 2,yRadius * 2));
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(player.transform.position, new Vector3(xRadius * 2, yRadius * 2));
        }
    }

    private (Vector3 minRange,Vector3 maxRange) CreateEnemySpawnPosition(Vector3 minPoint,Vector3 maxPoint)
    {
        Vector3 minRange = minPoint, maxRange = maxPoint;
        if (minPoint.y < randRespawnA.position.y)
        {
            minRange.y = randRespawnA.position.y;
        }
        else
        {
            minRange.y = minPoint.y;
        }

        if (minPoint.x < randRespawnA.position.x)
        {
            minRange.x = randRespawnA.position.x;
        }
        else
        {
            minRange.x = minPoint.x;
        }

        if (maxPoint.y > randRespawnB.position.y)
        {
            maxRange.y = randRespawnB.position.y;
        }
        else
        {
            maxRange.y = maxPoint.y;
        }

        if (maxPoint.x > randRespawnB.position.x)
        {
            maxRange.x = randRespawnB.position.x;
        }
        else
        {
            minRange.x = minPoint.x;
        }

        return (minRange, maxRange);
    }

    private Vector3? GenerateEnemySpawnPosition(Vector3 minRange, Vector3 maxRange)
    {
        // 試行回数
        int loopMax = 10;

        for (int i = 0; i < loopMax; i++)
        {
            Vector3 spawnPos = new Vector3
                 (Random.Range(minRange.x, maxRange.x), Random.Range(minRange.y, maxRange.y));

            Vector3 distToPlayer =
                player.transform.position - spawnPos;

            if (Mathf.Abs(distToPlayer.x) > distMinSpawnPos
                && Mathf.Abs(distToPlayer.y) > distMinSpawnPos)
            {
                return spawnPos;
            }
        }

        return null;
    }
}
