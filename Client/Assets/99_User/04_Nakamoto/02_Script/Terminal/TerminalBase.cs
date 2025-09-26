//--------------------------------------------------------------
// ターミナル親クラス [ TerminalBase.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using DG.Tweening;
using DG.Tweening.Core.Easing;
using KanKikuchi.AudioManager;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public abstract class TerminalBase : MonoBehaviour
{
    //--------------------------------
    // フィールド

    #region システム設定

    // プレイヤーが端末に触れているかの判定変数
    private bool isPlayerIn = false;

    // 起動判定
    protected bool isUsed = false;

    // 端末ID
    protected int terminalID = 0;

    // 端末の種別
    protected EnumManager.TERMINAL_TYPE terminalType;

    // カウントダウン
    protected int currentTime;

    // 敵生成数
    protected const int SPAWN_ENEMY_NUM = 5;

    #endregion

    #region 外部参照用プロパティ

    /// <summary>
    /// 端末ID
    /// </summary>
    public int TerminalID { get { return terminalID; } set { terminalID = value; } }

    /// <summary>
    /// 端末の種別
    /// </summary>
    public EnumManager.TERMINAL_TYPE TerminalType { get { return terminalType; } set { terminalType = value; } }

    #endregion

    #region 外部設定

    // タイマー表示用テキスト
    [SerializeField] protected Text timerText;

    // 端末スプライト
    [SerializeField] protected SpriteRenderer terminalSprite;

    // アイコンスプライト
    [SerializeField] protected SpriteRenderer iconSprite;

    // 制限時間
    [SerializeField] protected int limitTime = 40;

    // レリック生成位置
    [SerializeField] protected Transform[] relicSpawnPoints;

    #endregion

    #region マネージャー

    private GameManager gameManager;
    private SpawnManager spawnManager;
    private UIManager uiManager;
    private RelicManager relicManager;
    private CharacterManager characterManager;
    private PlayerBase player;

    #endregion

    //--------------------------------
    // メソッド

    #region 共通処理

    /// <summary>
    ///  初期処理
    /// </summary>
    protected virtual void Start()
    {
        gameManager = GameManager.Instance;
        spawnManager = SpawnManager.Instance;
        uiManager = UIManager.Instance;
        relicManager = RelicManager.Instance;
        characterManager = CharacterManager.Instance;
        player = characterManager.PlayerObjSelf.GetComponent<PlayerBase>();

        currentTime = limitTime;    // 制限時間をセット
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    protected void Update()
    {
        // Eキー入力かつプレイヤーが端末に触れている場合かつその端末が未使用である場合、端末を起動
        if (Input.GetKeyDown(KeyCode.E) && isUsed == false && isPlayerIn == true)
        {
            Debug.Log("Terminal Booted");
            BootRequest(); // 端末を起動
        }
    }

    ///--------------------------------
    /// 接触判定
    private void OnTriggerEnter2D(Collider2D collision)
    {// プレイヤーが端末に触れた場合
        if (collision.transform.tag == "Player")
        {
            isPlayerIn = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {// プレイヤーが端末から離れた場合
        if (collision.transform.tag == "Player")
        {
            isPlayerIn = false;
        }
    }

    /// <summary>
    /// カウントダウン処理
    /// </summary>
    public void CountDown()
    {
        //limitTImeを1ずつ減らす
        currentTime--;

        if (RoomModel.Instance)
            TerminalManager.Instance.TerminalDatas[terminalID - 1].Time = currentTime;

        timerText.text = currentTime.ToString();

        //制限時間が0以下になったら(時間切れ)
        if (currentTime <= 0)
        {
            //limitTimeを0にする
            currentTime = 0;

            //カウントダウンを停止する
            CancelInvoke("CountDown");

            // 失敗リクエスト
            FailureRequest();
        }
    }

    /// <summary>
    /// 通知時タイマー更新
    /// </summary>
    public void OnCountDown(int time)
    {
        timerText.text = time.ToString();
    }

    /// <summary>
    /// 端末起動リクエスト処理
    /// </summary>
    public virtual async void BootRequest()
    {
        if(terminalType == EnumManager.TERMINAL_TYPE.Jumble)
        {
            Debug.Log("レリックがありません");
            if (RelicManager.HaveRelicList.Count == 0) return;
        }

        isUsed = true; // 起動済みにする

        // 起動リクエストをサーバーに送信
        if(RoomModel.Instance)
        {
            await RoomModel.Instance.BootTerminalAsync(terminalID);
        }
        else
        {
            BootTerminal();
        }
    }

    /// <summary>
    /// 報酬排出リクエスト処理
    /// </summary>
    public void GiveRewardRequest()
    {
        Stack<Vector2> posStack = new Stack<Vector2>();

        foreach (var point in relicSpawnPoints)
        {
            posStack.Push(point.position);
        }

        //レリックを排出する
        RelicManager.Instance.DropRelicRequest(posStack, false);
    }

    /// <summary>
    /// 端末成功処理
    /// </summary>
    public async void SuccessTerminal()
    {
        // ターミナル非表示
        terminalSprite.DOFade(0, 2.5f);
        iconSprite.DOFade(0, 2.5f).OnComplete(() => { gameObject.SetActive(false); });

        // マスターはレリック要求
        if(RoomModel.Instance.IsMaster) GiveRewardRequest();
    }

    #endregion

    #region 端末毎に処理実装

    /// <summary>
    /// 起動処理
    /// </summary>
    public abstract void BootTerminal();

    /// <summary>
    /// 失敗リクエスト
    /// </summary>
    public async virtual void FailureRequest()
    {
        // リストの該当端末IDの状態を失敗にする
        if (RoomModel.Instance)
        {
            // サーバーに失敗通知
            await RoomModel.Instance.TerminalFailureAsync(terminalID);
        }
        else
        {
            FailureTerminal();
        }
    }

    /// <summary>
    /// 失敗処理
    /// </summary>
    public virtual void FailureTerminal()
    {
        if (RoomModel.Instance)
            TerminalManager.Instance.TerminalDatas[terminalID].State = EnumManager.TERMINAL_STATE.Failure;

        // ターミナル非表示
        terminalSprite.DOFade(0, 2.5f);
        iconSprite.DOFade(0, 2.5f).OnComplete(() => { gameObject.SetActive(false); });
    }

    #endregion
}