//----------------------------------------------------
// 敵生成クラス
// Author : Souma Ueno
//----------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] Transform randRespawnA;             // リスポーン範囲A
    [SerializeField] Transform randRespawnB;             // リスポーン範囲B
    [SerializeField] float distMinSpawnPos;              // 生成しない範囲
    [SerializeField] GameObject player;                  // プレイヤーの情報
    [SerializeField] List<GameObject> plyers;
    [SerializeField] List<GameObject> enemyList;         // エネミーリスト
    [SerializeField] List<GameObject> terminalSpawnList; // 端末から生成された敵のリスト
    [SerializeField] float xRadius;                      // 生成範囲のx半径
    [SerializeField] float yRadius;                      // 生成範囲のy半径
    
    GameObject enemy;

    public Transform RandRespawnA { get { return randRespawnA; } }
    public Transform RandRespawnB { get { return randRespawnB; } }
    public List<GameObject> TerminalSpawnList {  get { return terminalSpawnList; } }

    private static SpawnManager instance;

    public static SpawnManager Instance
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
    /// 敵のスポーン可能範囲判定処理
    /// </summary>
    /// <param name="minPoint"></param>
    /// <param name="maxPoint"></param>
    /// <returns></returns>
    public (Vector3 minRange, Vector3 maxRange) CreateEnemySpawnPosition(Vector3 minPoint, Vector3 maxPoint)
    {
        Vector3 minRange = minPoint, maxRange = maxPoint;
        if (minPoint.y < randRespawnA.position.y)
        {
            minRange.y = randRespawnA.position.y;
        }

        if (minPoint.x < randRespawnA.position.x)
        {
            minRange.x = randRespawnA.position.x;
        }

        if (maxPoint.y > randRespawnB.position.y)
        {
            maxRange.y = randRespawnB.position.y;
        }

        if (maxPoint.x > randRespawnB.position.x)
        {
            maxRange.x = randRespawnB.position.x;
        }

        return (minRange, maxRange);
    }

    /// <summary>
    /// 敵生成の位置決定処理
    /// </summary>
    /// <param name="minRange"></param>
    /// <param name="maxRange"></param>
    /// <returns></returns>
    public Vector3? GenerateEnemySpawnPosition(Vector3 minRange, Vector3 maxRange, EnemyBase enemyBase)
    {
        // 試行回数
        int loopMax = 100;

        for (int i = 0; i < loopMax; i++)
        {
            Vector3 spawnPos = new Vector3
                 (Random.Range(minRange.x, maxRange.x), Random.Range(minRange.y, maxRange.y));

            Vector3 distToPlayer =
                player.transform.position - spawnPos;

            if (Mathf.Abs(distToPlayer.x) > distMinSpawnPos
                && Mathf.Abs(distToPlayer.y) > distMinSpawnPos)
            {
                Vector2? pos = IsGroundCheck(spawnPos);
                if (pos != null)
                {
                    LayerMask mask = LayerMask.GetMask("Default");

                    Vector2 result = (Vector2)pos;

                    result.y += enemyBase.SpawnGroundOffset;

                    if (!Physics2D.OverlapCircle(new Vector2(result.x, result.y + 1), 0.8f, mask))
                    {
                        return result;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 敵生成処理
    /// </summary>
    public void GenerateEnemy(int num)
    {
        for (int i = 0; i < num; i++)
        {
            int listNum = Random.Range(0, enemyList.Count);

            EnemyBase enemyBase = enemyList[listNum].GetComponent<EnemyBase>();

            Vector2 minPlayer =
                        new Vector2(player.transform.position.x - xRadius,
                        player.transform.position.y - yRadius);

            Vector2 maxPlayer =
                new Vector2(player.transform.position.x + xRadius,
                player.transform.position.y + yRadius);

            // ランダムな位置を生成
            var spawnPostions = CreateEnemySpawnPosition(minPlayer, maxPlayer);

            Vector3? spawnPos = GenerateEnemySpawnPosition(spawnPostions.minRange, spawnPostions.maxRange, enemyBase);

            if (spawnPos != null)
            {
                GameManager.Instance.SpawnCnt++;

                // 生成
                enemy = Instantiate(enemyList[listNum], (Vector3)spawnPos, Quaternion.identity);

                int number = Random.Range(0, 100);

                if(number < 50)
                {
                    enemy.GetComponent<EnemyBase>().PromoteToElite((EnemyElite.ELITE_TYPE)Random.Range(1,4));
                }

                enemy.GetComponent<EnemyBase>().Players.Add(player);
                enemy.GetComponent<EnemyBase>().SetNearTarget();
            }
        }
    }

    /// <summary>
    /// 端末操作時の敵生成処理
    /// </summary>
    public void TerminalGenerateEnemy(int num)
    {
        int enemyCnt = 0;

        while (enemyCnt < num)
        {
            int listNum = Random.Range(0, enemyList.Count);

            EnemyBase enemyBase = enemyList[listNum].GetComponent<EnemyBase>();

            Vector2 minPlayer =
                        new Vector2(player.transform.position.x - xRadius,
                        player.transform.position.y - yRadius);

            Vector2 maxPlayer =
                new Vector2(player.transform.position.x + xRadius,
                player.transform.position.y + yRadius);

            // ランダムな位置を生成
            var spawnPostions = CreateEnemySpawnPosition(minPlayer, maxPlayer);

            Vector3? spawnPos = GenerateEnemySpawnPosition(spawnPostions.minRange, spawnPostions.maxRange, enemyBase);

            if (spawnPos != null)
            {
                GameManager.Instance.SpawnCnt++;

                // 生成
                enemy = Instantiate(enemyList[listNum], (Vector3)spawnPos, Quaternion.identity);

                enemy.GetComponent<EnemyBase>().Players.Add(player);
                enemy.GetComponent<EnemyBase>().SetNearTarget();

                // 端末から出た敵をリストに追加
                terminalSpawnList.Add(enemy);

                enemyCnt++;
            }
        }
    }

    /// <summary>
    /// 床チェック
    /// </summary>
    /// <param name="rayOrigin"></param>
    /// <returns></returns>
    private Vector2? IsGroundCheck(Vector3 rayOrigin)
    {
        LayerMask mask = LayerMask.GetMask("Default");

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, float.MaxValue, mask);

        Debug.DrawRay((Vector2)rayOrigin, Vector2.down * hit.distance, Color.red);
        if (hit && hit.collider.gameObject.CompareTag("ground"))
        {
            return hit.point;
        }
        else
        {
            return null;
        }
    }
}
