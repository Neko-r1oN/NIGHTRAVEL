//----------------------------------------------------
// 敵生成クラス
// Author : Souma Ueno
//----------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    #region 敵生成条件
    [SerializeField] Vector2 spawnRange;
    [SerializeField] float spawnRangeOffset;
    #endregion

    #region ステージ情報
    [SerializeField] Transform stageMin;             // リスポーン範囲A
    [SerializeField] Transform stageMax;             // リスポーン範囲B
    //[SerializeField] Transform minTerminalRespawn;       // ターミナルリスポーン範囲
    //[SerializeField] Transform maxTerminalRespawn;       // ターミナルリスポーン範囲

    #region 外部参照
    public Transform StageMinPoint { get { return stageMin; } }
    public Transform StageMaxPoint { get { return stageMax; } }
    #endregion

    #endregion

    #region 敵関連
    [SerializeField] List<GameObject> enemyPrefabs;      // エネミーリスト
    [SerializeField] List<GameObject> enemiesByTerminal; // 端末から生成された敵のリスト
    float[] enemyWeights;
    int eliteEnemyCnt;

    #region 外部参照
    public List<GameObject> EnemiesByTerminal { get { return enemiesByTerminal; } }
    #endregion

    #endregion

    #region TMP
    [SerializeField] float distMinSpawnPos;              // 生成しない範囲
    [SerializeField] float xRadius;                      // 生成範囲のx半径
    [SerializeField] float yRadius;                      // 生成範囲のy半径
    #endregion

    [SerializeField] GameObject player;
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

    private void Start()
    {
        player = GameManager.Instance.Player;
        enemyWeights = new float[enemyPrefabs.Count];

        // 割合
        enemyWeights[0] = 24; // ドローン
        enemyWeights[1] = 76; // いぬ
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
        if (minPoint.y < stageMin.position.y)
        {
            minRange.y = stageMin.position.y;
        }

        if (minPoint.x < stageMin.position.x)
        {
            minRange.x = stageMin.position.x;
        }

        if (maxPoint.y > stageMax.position.y)
        {
            maxRange.y = stageMax.position.y;
        }

        if (maxPoint.x > stageMax.position.x)
        {
            maxRange.x = stageMax.position.x;
        }

        return (minRange, maxRange);
    }

    /// <summary>
    /// 敵のスポーン可能範囲判定処理
    /// </summary>
    /// <param name="minPoint"></param>
    /// <param name="maxPoint"></param>
    /// <returns></returns>
    /*public (Vector3 minRange, Vector3 maxRange) CreateEnemyTerminalSpawnPosition(Vector3 minPoint, Vector3 maxPoint)
    {
        Vector3 minRange = minPoint, maxRange = maxPoint;
        if (minPoint.y < minTerminalRespawn.position.y)
        {
            minRange.y = minTerminalRespawn.position.y;
        }

        if (minPoint.x < minTerminalRespawn.position.x)
        {
            minRange.x = minTerminalRespawn.position.x;
        }

        if (maxPoint.y > maxTerminalRespawn.position.y)
        {
            maxRange.y = maxTerminalRespawn.position.y;
        }

        if (maxPoint.x > maxTerminalRespawn.position.x)
        {
            maxRange.x = maxTerminalRespawn.position.x;
        }

        return (minRange, maxRange);
    }*/

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
            int seed = System.DateTime.Now.Millisecond + i;
            Random.InitState(seed);  // Unityの乱数にシードを設定

            // 確率の計算
            int listNum = Choose(enemyWeights);

            EnemyBase enemyBase = enemyPrefabs[listNum].GetComponent<EnemyBase>();

            // ランダムな位置を生成
            //var spawnPostions = CreateEnemySpawnPosition(minPlayer, maxPlayer);

            //Vector3? spawnPos = GenerateEnemySpawnPosition(spawnPostions.minRange, spawnPostions.maxRange, enemyBase);

            //if (spawnPos != null)
            //{
            //    SpawnEnemyRequest(enemyPrefabs[listNum], (Vector3)spawnPos);
            //}
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
            int listNum = Random.Range(0, enemyPrefabs.Count);

            EnemyBase enemyBase = enemyPrefabs[listNum].GetComponent<EnemyBase>();

            Vector2 minPlayer =
                        new Vector2(player.transform.position.x - xRadius,
                        player.transform.position.y - yRadius);

            Vector2 maxPlayer =
                new Vector2(player.transform.position.x + xRadius,
                player.transform.position.y + yRadius);

            // ランダムな位置を生成
            /*var spawnPostions = CreateEnemyTerminalSpawnPosition(minPlayer, maxPlayer);

            Vector3? spawnPos = GenerateEnemySpawnPosition(spawnPostions.minRange, spawnPostions.maxRange, enemyBase);

            if (spawnPos != null)
            {
                SpawnEnemyRequest(enemyPrefabs[listNum], (Vector3)spawnPos);

                // 端末から出た敵をリストに追加
                enemiesByTerminal.Add(enemy);

                enemyCnt++;
            }*/
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

    /// <summary>
    /// 敵の出現確率の抽出
    /// </summary>
    /// <param name="probs"></param>
    /// <returns></returns>
    public int Choose(float[] probs)
    {
        float total = 0;

        //配列の要素を代入して重みの計算
        foreach (float elem in probs)
        {
            total += elem;
        }

        //重みの総数に0から1.0の乱数をかけて抽選を行う
        float randomPoint = Random.value * total;

        //iが配列の最大要素数になるまで繰り返す
        for (int i = 0; i < probs.Length; i++)
        {
            //ランダムポイントが重みより小さいなら
            if (randomPoint < probs[i])
            {
                return i;
            }
            else
            {
                //ランダムポイントが重みより大きいならその値を引いて次の要素へ
                randomPoint -= probs[i];
            }
        }

        //乱数が１の時、配列数の-１＝要素の最後の値をChoose配列に戻している
        return probs.Length - 1;
    }

    /// <summary>
    /// 敵生成リクエスト
    /// </summary>
    /// <param name="spawnEnemy"></param>
    /// <param name="spawnPos"></param>
    public GameObject SpawnEnemyRequest(GameObject spawnEnemy,Vector3 spawnPos, bool canPromoteToElite = true)
    {
        GameManager.Instance.SpawnCnt++;

        // 生成
        GameObject enemy = Instantiate(spawnEnemy, spawnPos, Quaternion.identity);
        if (!canPromoteToElite) return enemy;

        int number = Random.Range(0, 100);

        // エリート敵生成限度を設定
        int onePercentOfMaxEnemies =
            Mathf.FloorToInt(GameManager.Instance.MaxSpawnCnt * 0.3f);

        if (number < 5 * ((int)LevelManager.Instance.GameLevel + 1) 
            && eliteEnemyCnt < onePercentOfMaxEnemies)
        {// 5%(* 現在のゲームレベル)以下で、エリート敵生成限度に達していなかったら
            // エリート敵生成
            enemy.GetComponent<EnemyBase>().PromoteToElite((EnemyElite.ELITE_TYPE)Random.Range(1, 4));
            eliteEnemyCnt++;
        }

        return enemy;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube((Vector2)player.transform.position + Vector2.right * spawnRangeOffset, spawnRange);  // 右
        Gizmos.DrawWireCube((Vector2)player.transform.position + Vector2.left * spawnRangeOffset, spawnRange);   // 左
    }
}
