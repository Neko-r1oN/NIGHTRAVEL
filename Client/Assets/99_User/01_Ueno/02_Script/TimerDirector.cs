using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerDirector : MonoBehaviour
{
    [SerializeField] float minute = 5;
    float second;

    [SerializeField] Text timer;
    [SerializeField] Text bossText;

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
        }

        if (minute <= 0 && GameManager.Instance.BossFlag == false)
        {// ゲームタイマーが0以下になったら&ボスが出現してなかったら
            bossText.text = "BOSS";
            bossText.gameObject.SetActive(true);

            timer.gameObject.SetActive(false);
            // ボス出現
            GameManager.Instance.BossFlag = true;
        }
        else if(GameManager.Instance.IsSpawnBoss == true)
        {
            timer.gameObject.SetActive(false);

            bossText.text = "BOSS";
            bossText.gameObject.SetActive(true);
        }
    }
}
