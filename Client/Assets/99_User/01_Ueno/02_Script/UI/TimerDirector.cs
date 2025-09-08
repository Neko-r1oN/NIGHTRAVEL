using Shared.Interfaces.StreamingHubs;
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
    float initMinute;       // 初期設定(分)


    [SerializeField] GameObject timerObj; // タイマーテキストの親
    public GameObject TimerObj { get { return timerObj; } set { timerObj = value; } }
    [SerializeField] Text timer;          // タイマーテキスト

    public Text Timer { get { return timer; } set { timer = value; } }

    public float Second { get { return second; } set { second = value; } }

    #endregion

    private static TimerDirector instance;
    public static TimerDirector Instance
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
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        second = minute * 60;
        initMinute = minute * 60;
        //GameManager.Instance.InvokeRepeating("DecreaseGeneratInterval", 0.1f, 60f);

    }



    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.IsGameStart)
        {
            if (Terminal.Instance.IsTerminal && Terminal.Instance.code != (Terminal.TerminalCode)2)
            {
                timerObj.SetActive(false);
            }
            else
            {
                timerObj.SetActive(true);
            }

            if (minute <= 0 && second <= 0 && SpawnManager.Instance.IsSpawnBoss == false)
            {// ゲームタイマーが0以下になったら&ボスが出現してなかったら
                timerObj.SetActive(false);
                // ボス出現
                //SpawnManager.Instance.IsSpawnBoss = true;
                SpawnManager.Instance.SpawnBoss();
            }
            else if (SpawnManager.Instance.IsSpawnBoss == true)
            {// ボスが出現したら
             // タイマー削除
                timerObj.SetActive(false);
            }
            else if (SpawnManager.Instance.IsSpawnBoss == false)
            {// ボスが出現していなかったら
                elapsedTime += Time.deltaTime; // 毎フレーム時間を加算

                if (!Terminal.Instance.IsTerminal)
                {
                    // タイマー(UI)の更新
                    UpdateTimerDisplay();
                }

                if (elapsedTime > 150)
                {
                    // ゲームレベルアップリクエスト送信

                    // ゲームレベルアップ
                    LevelManager.Instance.UpGameLevel();
                    ResetTimer();
                }
            }
        }
    }

    /// <summary>
    /// タイマー更新
    /// </summary>
    public async void UpdateTimerDisplay()
    {
        // 以降オフライン用
        if (RoomModel.Instance) return;

        // フレームの経過時間分減算
        second -= Time.deltaTime;
        OnUpdateTimer(second);
        
    }

    /// <summary>
    /// タイマーテキスト更新
    /// </summary>
    /// <param name="time"></param>
    public void OnUpdateTimer(float time)
    {
        second = time;
        var span = new TimeSpan(0, 0, (int)time);
        minute = span.Minutes;
        timer.text = span.ToString(@"mm\:ss");
    }

    /// <summary>
    /// タイマーをリセット
    /// </summary>
    public void ResetTimer()
    {
        elapsedTime = 0f;
    }
}
