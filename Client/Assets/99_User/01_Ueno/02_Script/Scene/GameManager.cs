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
    Vector3 spawnPos;       // ランダムで生成する位置
    [SerializeField] int bossCount;
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
    [SerializeField] Transform xRadius;
    [SerializeField] Transform yRadius;

    GameObject player;                       // プレイヤーの情報
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
            for (int i = 0; i < bossCount; i++)
            {
                float minX, maxX;
                float minY, maxY;

                if (minCameraPos.position.y < randRespawnA.position.y)
                {
                    minY = randRespawnA.position.y;
                }
                else
                {
                    minY = minCameraPos.position.y;
                }

                if (maxCameraPos.position.y > randRespawnB.position.y)
                {
                    maxY = randRespawnB.position.y;
                }
                else
                {
                    maxY = maxCameraPos.position.y;
                }

                if (minCameraPos.position.x < randRespawnA.position.x)
                {
                    minX = randRespawnA.position.x;
                }
                else
                {
                    minX = minCameraPos.position.x;
                }

                if (maxCameraPos.position.x > randRespawnB.position.x)
                {
                    maxX = randRespawnB.position.x;
                }
                else
                {
                    maxX = maxCameraPos.position.x;
                }

                // ステージ内から適当な位置を取得
                float x = Random.Range(minX, maxX);
                float y = Random.Range(minY, maxY);

                // ランダムな位置を生成
                spawnPos = new Vector3(x, y);

                Instantiate(boss, new Vector3(x, y), Quaternion.identity);
            }

            isSpawnBoss = true;

            bossFlag = false;
        }

        if (isBossDead)
        {// ボスを倒した(仮)
            //bossFlag = false;
            boss.SetActive(false);

            // 遅れて呼び出し
            Invoke(nameof(ChengScene), 1.5f);
        }

        if (spawnCnt < maxSpawnCnt)
        {// スポーン回数が限界に達しているか
            num++;
            if (num % createCnt == 0)
            {
                num = 0;

                // ステージ内から適当な位置を取得
                float x = Random.Range(randRespawnA.position.x, randRespawnB.position.x);
                float y = Random.Range(randRespawnA.position.y, randRespawnB.position.y);
                float z = Random.Range(randRespawnA.position.z, randRespawnB.position.z);
                // ランダムな位置を生成
                spawnPos = new Vector3(x, y, z);

                float minX,minY,maxX,maxY;





                // プレイヤーの位置とランダム生成の位置との距離
                float distanceOfPlayer =
                    Vector3.Distance(player.transform.position, spawnPos);

                if (distanceOfPlayer >= 8 && distanceOfPlayer < 13)
                {// 距離が10離れていたら
                    spawnCnt++;

                    int listNum = Random.Range(0, enemyList.Count);

                    // 生成
                    enemy = Instantiate(enemyList[listNum], new Vector3(x, y, z), Quaternion.identity);
                    
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

        Debug.Log(crushNum);
        if (crushNum >= 15)
        {// 撃破数が15以上になったら(仮)

            BossFlag = true;

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
        // ボスのカウントを減らす
        bossCount--;

        // 呼び出されたときボスカウントが0以下なら
        if(bossCount <= 0)
        {
            // ボスフラグを変更
            bossFlag = false;
            // 死んだ判定にする
            isBossDead = true;
        }

        Debug.Log("死んだよん");
    }


}
