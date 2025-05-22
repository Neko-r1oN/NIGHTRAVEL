//----------------------------------------------------
// ゲームマネージャー(GameManager.cs)
// Author : Souma Ueno
//----------------------------------------------------
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    #endregion

    #region その他
    [Header("その他")]
    public List<GameObject> enemyList;       // エネミーリスト
    [SerializeField] GameObject boss;        // ボス
    [SerializeField] Transform randRespawnA; // リスポーン範囲A
    [SerializeField] Transform randRespawnB; // リスポーン範囲B

    GameObject player;                       // プレイヤーの情報
    GameObject enemy;                        // エネミーの情報

    public GameObject Enemy {  get { return enemy; } }

    public bool BossFlag { get { return bossFlag; } set { bossFlag = value; } }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ボスを非表示
        boss.SetActive(false);
        // プレイヤーのオブジェクト検索して取得
        player = GameObject.Find("Player");
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    void Update()
    {
        if (crushNum >= 16 && bossFlag)
        {// ボスを倒した(仮)
            bossFlag = false;
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

                // ステージ内から適当な位置を取得
                float x = Random.Range(randRespawnA.position.x, randRespawnB.position.x);
                float y = Random.Range(randRespawnA.position.y, randRespawnB.position.y);
                float z = Random.Range(randRespawnA.position.z, randRespawnB.position.z);
                // ランダムな位置を生成
                spawnPos = new Vector3(x, y, z);

                // プレイヤーの位置とランダム生成の位置との距離
                float distanceOfPlayer =
                    Vector3.Distance(player.transform.position, spawnPos);

                if (distanceOfPlayer >= 8 && distanceOfPlayer < 13)
                {// 距離が10離れていたら
                    spawnCnt++;

                    int listNum = Random.Range(0, 2);

                    // 生成
                    enemy = Instantiate(enemyList[listNum], new Vector3(x, y, z), Quaternion.identity);

                    // 透明化
                    enemy.GetComponent<SpriteRenderer>().enabled = false;
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
            bossFlag = true;
            boss.SetActive(true);
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
}
