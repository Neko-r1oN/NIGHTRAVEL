//--------------------------------------------------------------
// ターミナル親クラス [ TerminalBase.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using DG.Tweening;
using DG.Tweening.Core.Easing;
using KanKikuchi.AudioManager;
using Shared.Interfaces.StreamingHubs;
using System.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class TerminalBase : MonoBehaviour
{
    //--------------------------------
    // フィールド

    #region システム設定

    // プレイヤーが端末に触れているかの判定変数
    private bool isPlayerIn = false;

    // 起動判定
    private bool isUsed = false;

    // 端末ID
    private int terminalID = 0;

    // 端末の種別
    protected EnumManager.TERMINAL_TYPE terminalType;

    // 制限時間
    [SerializeField] protected int limitTime = 25;

    // カウントダウン
    private int currentTime;

    #endregion

    #region 外部参照用プロパティ

    /// <summary>
    /// 端末ID
    /// </summary>
    public int TerminalID { get { return terminalID; } set { terminalID = value; } }

    /// <summary>
    /// 端末の種別
    /// </summary>
    public EnumManager.TERMINAL_TYPE TerminalType { get { return terminalType; } }

    #endregion

    #region UI関連

    // タイマー表示用テキスト
    [SerializeField] private Text timerText;

    // 端末のアイコン
    [SerializeField] private SpriteRenderer terminalIcon;

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
    private void Start()
    {
        gameManager = GameManager.Instance;
        uiManager = UIManager.Instance;
        spawnManager = SpawnManager.Instance;
        relicManager = RelicManager.Instance;
        player = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>();

        currentTime = limitTime;    // 制限時間をセット
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    private void Update()
    {
        // Eキー入力かつプレイヤーが端末に触れている場合かつその端末が未使用である場合、端末を起動
        if (Input.GetKeyDown(KeyCode.E) && isUsed == false && isPlayerIn == true)
        {
            Debug.Log("Terminal Booted");
            BootTerminal(); // 端末を起動
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
    /// <returns></returns>
    protected IEnumerator Countdown()
    {
        while (currentTime > 0)
        {
            timerText.text = currentTime.ToString();
            yield return new WaitForSeconds(1f);
            currentTime--;
        }

        timerText.text = "0"; // 最後に0を表示

        // タイムアップ時の処理をここに記述
        TimeUp();
    }

    #endregion

    #region 端末毎に処理実装

    /// <summary>
    /// 端末起動処理
    /// </summary>
    public virtual void BootTerminal()
    {
        isUsed = true; // 起動済みにする
    }

    /// <summary>
    /// 報酬排出処理
    /// </summary>
    public virtual void GiveReward()
    {
        // レリックを排出する
        RelicManager.Instance.GenerateRelic(gameObject);

        // ターミナル非表示
        terminalIcon.DOFade(0, 1.5f);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 時間切れ
    /// </summary>
    public virtual void TimeUp()
    {
        // タイムアップ時の処理をここに記述
        Debug.Log("Time Up!");

        // ターミナル非表示
        terminalIcon.DOFade(0, 1.5f);
        gameObject.SetActive(false);
    }

    #endregion
}