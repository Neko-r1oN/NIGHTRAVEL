using System;
using UnityEngine;
using UnityEngine.UI;

public class TimerDirector : MonoBehaviour
{
    public float gameTimer = 90;

    [SerializeField] Text text;

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.BossFlag == false)
        {
            // 操作時間更新処理
            gameTimer -= Time.deltaTime;
            text.text = "" + Math.Floor(gameTimer);
        }
        
        if (gameTimer <= 0 && GameManager.Instance.BossFlag == false)
        {// ゲームタイマーが0以下になったら&ボスが出現してなかったら
            // タイマー固定
            gameTimer = 0;
            text.text = "Boss";
            // ボス出現
            GameManager.Instance.BossFlag = true;
        }
    }
}
