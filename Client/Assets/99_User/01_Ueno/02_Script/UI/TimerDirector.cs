using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerDirector : MonoBehaviour
{
    #region 初期設定
    [Header("初期設定")]
    [SerializeField] float minute;
    float second;
    float elapsedTime = 0f; // 経過時間
    float halfMinute;

    [SerializeField] GameObject timerObj; // タイマーテキストの親
    [SerializeField] Text timer;          // タイマーテキスト
    #endregion

    private void Start()
    {
        second = minute * 60;
        GameManager.Instance.InvokeRepeating("DecreaseGeneratInterval", 0.1f, 60f);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.BossFlag == false)
        {
            second -= Time.deltaTime;
            var span = new TimeSpan(0,0,(int)second);
            minute = span.Minutes;
            timer.text = span.ToString(@"mm\:ss");

            elapsedTime += Time.deltaTime; // 毎フレーム時間を加算
            TimeSpan timeSpan = GetCurrentMinutesAndSeconds();

            float currentTime = (float)timeSpan.TotalSeconds;

            if (currentTime >= (minute * 60) / 2 && minute > 0)
            {
                halfMinute = minute / 2;
                LevelManager.Instance.UpGameLevel();
                ResetTimer();
            }
            else if(currentTime >= (second) / 2)
            {
                halfMinute = minute / 2;
                LevelManager.Instance.UpGameLevel();
                ResetTimer();
            }
        }

        if (minute <= 0 && second <= 0 && GameManager.Instance.BossFlag == false)
        {// ゲームタイマーが0以下になったら&ボスが出現してなかったら
            timerObj.SetActive(false);
            // ボス出現
            GameManager.Instance.BossFlag = true;
        }
        else if(GameManager.Instance.IsSpawnBoss == true)
        {
            timerObj.SetActive(false);
        }
    }

    /// <summary>
    /// 現在の分と秒を取得します。
    /// </summary>
    /// <returns>現在の時間（分と秒）をTimeSpanで返します。</returns>
    public TimeSpan GetCurrentMinutesAndSeconds()
    {
        return TimeSpan.FromSeconds(elapsedTime);
    }

    /// <summary>
    /// タイマーをリセットします。
    /// </summary>
    public void ResetTimer()
    {
        elapsedTime = 0f;
    }
}
