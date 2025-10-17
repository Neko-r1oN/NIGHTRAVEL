using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TimerDirector : MonoBehaviour
{
    #region 初期設定
    [Header("初期設定")]
    [SerializeField] float minute;
    float second;
    static float elapsedTime = 0f; // 経過時間
    float initMinute;       // 初期設定(分)

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

    // Update is called once per frame
    async void Update()
    {
        if (GameManager.Instance.IsGameStart)
        {

            elapsedTime += Time.deltaTime; // 毎フレーム時間を加算

            if (elapsedTime > 150)
            {
                // ゲームレベルアップリクエスト送信

                ResetTimer();

                // ゲームレベルアップ
                if (RoomModel.Instance)
                {
                    if (RoomModel.Instance.IsMaster) await RoomModel.Instance.AscendDifficultyAsync();
                    return;
                }
                else
                {
                    LevelManager.Instance.UpGameLevel();
                }

            }
        }
    }

    /// <summary>
    /// タイマーをリセット
    /// </summary>
    public void ResetTimer()
    {
        elapsedTime = 0f;
    }
}
