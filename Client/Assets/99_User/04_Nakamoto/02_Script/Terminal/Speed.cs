//--------------------------------------------------------------
// スピードランターミナル [ Speed.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using KanKikuchi.AudioManager;
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class Speed : TerminalBase
{

    //------------------------
    // フィールド

    // スピード用ゴールポイントオブジェクトのリスト
    [SerializeField] List<GameObject> pointList;

    //------------------------
    // メソッド

    /// <summary>
    /// 初期処理
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // 端末の種別を設定
        terminalType = EnumManager.TERMINAL_TYPE.Speed;
    }

    /// <summary>
    /// 端末起動リクエスト処理
    /// </summary>
    public override async void BootRequest()
    {
        base.BootRequest();

        // カウントダウンする
        InvokeRepeating("CountDown", 1, 1);

        timerText.text = currentTime.ToString();

        foreach (var point in pointList)
        {   // 各ゴールポイントを表示
            point.SetActive(true);
        }
    }

    /// <summary>
    /// 起動処理
    /// </summary>
    public override void BootTerminal()
    {
        isUsed = true; // 端末使用中にする

        if(RoomModel.Instance)
            TerminalManager.Instance.TerminalDatas[terminalID].State = EnumManager.TERMINAL_STATE.Active;
    }

    /// <summary>
    /// 失敗リクエスト
    /// </summary>
    public override void FailureRequest()
    {
        foreach (var point in pointList)
        {   // 各ゴールポイントを非表示
            point.SetActive(false);
        }

        base.FailureRequest();
    }

    /// <summary>
    /// ゴールポイントに触れた際の処理
    /// </summary>
    /// <param name="obj"></param>
    public void HitGoalPoint(GameObject obj)
    {
        if (pointList.Contains(obj))    // 渡されたオブジェクトがリスト内にあった場合
        {
            pointList.Remove(obj);  // それを除去する
            Destroy(obj);   // それを破壊する

            if (pointList.Count <= 0)
            { // リストが空になった場合、報酬を付与する
                CancelInvoke("CountDown");
                GiveRewardRequest();
            }
        }
    }
}
