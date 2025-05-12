using System;
using UnityEngine;
using UnityEngine.UI;

public class TimerDirector : MonoBehaviour
{
    public float gameTimer = 90;

    GameManager gameManager;

    [SerializeField] Text text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameObject.Find("GameManager")
            .GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.BossFlag == false)
        {
            // 操作時間更新処理
            gameTimer -= Time.deltaTime;
            text.text = "" + Math.Floor(gameTimer);
        }
        
        if (gameTimer <= 0 && gameManager.BossFlag == false)
        {// ゲームタイマーが0以下になったら&ボスが出現してなかったら
            // タイマー固定
            gameTimer = 0;
            text.text = "Boss";
            // ボス出現
            gameManager.BossFlag = true;

            Debug.Log("ボス出現");
        }
    }
}
