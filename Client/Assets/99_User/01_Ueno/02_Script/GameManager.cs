using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    int crushNum; 　　　　　// 撃破数
    bool bossFlag = false;  // ボスが出たかどうか
    int xp;                 // 経験値
    int requiredXp = 100;   // 必要経験値
    int level;              // レベル
    int num;                // 生成までのカウント
    public int createCnt;   // 生成間隔
    int spawnCnt;           // スポーン回数
    public int maxSpawnCnt; // マックススポーン回数
    //Vector3 spawnPos;

    [SerializeField] GameObject boss;
    [SerializeField] GameObject enemy;
    [SerializeField] Transform randRespawnA;
    [SerializeField] Transform randRespawnB;

    GameObject player;

    public bool BossFlag { get { return bossFlag; } set { bossFlag = value; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boss.SetActive(false);
        player = GameObject.Find("DrawCharacter");
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Return))
        {
            // 撃破数加算
            crushNum++;
            Debug.Log(crushNum);
            if(crushNum >= 15)
            {// 撃破数が15以上になったら(仮)
                bossFlag = true;
                //square.SetActive(true);
                crushNum = 0;
            }
        }*/

        if (crushNum >= 5 && bossFlag)
        {// ボスを倒した(仮)
            bossFlag = false;
            //boss.SetActive(false);

            // 遅れて呼び出し
            Invoke(nameof(ChengScene), 1.5f);
        }

        /*float distanceOfPlayer =
            Vector3.Distance(player.transform.position, spawnPos);

        Debug.Log("距離:" + Math.Floor(distanceOfPlayer));*/

        num++;
        //Debug.Log(num);

        if (num % createCnt == 0)
        {
            Debug.Log("出てきた");
            num = 0;

            float x = Random.Range(randRespawnA.position.x, randRespawnB.position.x);
            float y = Random.Range(randRespawnA.position.y, randRespawnB.position.y);
            float z = Random.Range(randRespawnA.position.z, randRespawnB.position.z);

            if (spawnCnt <= maxSpawnCnt)
            {
                spawnCnt++;
                Debug.Log(spawnCnt);
                Instantiate(enemy, new Vector3(x, y, z), enemy.transform.rotation);
            }
        }
    }

    private void ChengScene()
    {// シーン変更
        SceneManager.LoadScene("Result ueno");
    }

    public void CrushEnemy()
    {
        crushNum++;
        AddXp();

        Debug.Log(crushNum);
        if (crushNum >= 4)
        {// 撃破数が15以上になったら(仮)
            bossFlag = true;
            boss.SetActive(true);
            Debug.Log("ボスでてきた");
            //crushNum = 0;
        }
    }

    public void AddXp()
    {
        xp += 100;
        if (xp >= requiredXp)
        {
            requiredXp += xp;
            Debug.Log(requiredXp);
            UpLevel();
        }
    }

    public void UpLevel()
    {
        level++;
        Debug.Log("レベルアップ:" + level);
    }

    /*public void RespawnEnemy()
    {
        Instantiate(enemy);
    }*/
}
