using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    int crushNum;
    bool bossFlag = false;
    int xp;
    int level;

    [SerializeField] GameObject boss;

    public bool BossFlag { get { return bossFlag; } set { bossFlag = value; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boss.SetActive(false);
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

        if(crushNum >= 5 && bossFlag)
        {// ボスを倒した(仮)
            bossFlag = false;
            //boss.SetActive(false);

            // 遅れて呼び出し
            Invoke(nameof(ChengScene), 1.5f);
        }
    }

    private void ChengScene()
    {// シーン変更
        SceneManager.LoadScene("Result ueno");
    }

    public void CrushEnemy()
    {
        crushNum++;
        xp += 100;
        if (xp >= 100)
        {
            UpLevel();
        }

        Debug.Log(crushNum);
        if (crushNum >= 4)
        {// 撃破数が15以上になったら(仮)
            bossFlag = true;
            boss.SetActive(true);
            Debug.Log("ボスでてきた");
            //crushNum = 0;
        }
    }

    public void UpLevel()
    {
        level++;
        Debug.Log("レベルアップ:" + level);
    }
}
