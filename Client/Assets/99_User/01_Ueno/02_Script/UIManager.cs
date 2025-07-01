using DG.Tweening;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    PlayerBase player;
    EnemyBase boss;

    #region 各UI
    [SerializeField] Slider playerHpBar;        // プレイヤーのHPバー
    [SerializeField] Slider bossHpBar;          // ボスのHPバー
    [SerializeField] Slider expBar;             // 経験値バー
    [SerializeField] Text playerSliderText;     // プレイヤーの最大HPテキスト
    [SerializeField] Text bossSliderText;       // ボスの最大HPテキスト
    [SerializeField] Text levelText;            // レベルテキスト
    [SerializeField] Text pointText;            // ポイントテキスト
    [SerializeField] GameObject bossStatus;     // ボスのステータス
    [SerializeField] GameObject statusUpWindow; // ステータス強化ウィンドウ
    [SerializeField] GameObject bossWindow;     // ボス出現UI
    [SerializeField] float windowTime;          // ウィンドウが表示される秒数
    [SerializeField] List<Image> relicImages;
    [SerializeField] List<Text> relicCntText;  // レリックを持ってる数を表示するテキスト
    //[SerializeField] List<Text>  statusText;    // ステータスアップ説明テキスト
    [SerializeField] Text levelUpStock;         // レベルアップストックテキスト
    [SerializeField] Text levelUpText;          // 強化可能テキスト
    [SerializeField] Text clashNumText;         // 撃破数テキスト
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float fadeDuration;
    #endregion

    int windowCnt = 0;   // ウィンドウが表示できるカウント(一度だけ使う)
    int lastLevel = 0;   // レベルアップ前のレベル
    int statusStock = 0; // レベルアップストック数
    bool isStatusWindow; // ステータスウィンドウが開けるかどうか
    bool isHold;         // ステータスウィンドウロック用



    private static UIManager instance;

    public static UIManager Instance
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
            // インスタンスが複数存在しないように、既に存在していたら自身を消去する
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初期設定
    /// </summary>
    void Start()
    {
        player = GameManager.Instance.Player.GetComponent<PlayerBase>();

        playerHpBar.maxValue = player.MaxHP;
        playerSliderText.text = "" + playerHpBar.maxValue;
        expBar.maxValue = player.NextLvExp;
        levelText.text = "" + player.NowExp;
        expBar.value = player.NowExp;
        lastLevel = player.NowLv;
        levelText.text = "LV." + player.NowLv;

        for (int i = 0; i < relicCntText.Count; i++)
        {
            relicCntText[i].enabled = false;
        }

        statusUpWindow.SetActive(false);

        isHold = false;
        isStatusWindow = false;

        levelUpText.enabled = false;
        bossStatus.SetActive(false);
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    void Update()
    {
        // プレイヤーHPUI
        playerHpBar.maxValue = player.MaxHP;
        playerHpBar.value = player.HP;
        playerSliderText.text = player.HP + "/" + playerHpBar.maxValue;

        // 経験値・レベルUI
        expBar.maxValue = player.NextLvExp;
        levelText.text = "LV." + player.NowLv;
        if (player.NowLv > lastLevel)
        {
            isStatusWindow = true;
            statusStock += player.NowLv - lastLevel;
            levelUpStock.text = "残り強化数：" + statusStock;
            levelUpText.enabled = true;
            lastLevel = player.NowLv;
        }
        expBar.value = player.NowExp;

        if (GameManager.Instance.IsSpawnBoss)
        {// ボスがスポーンした
            if (windowCnt <= 0)
            {// ウィンドウが一回も出ていないとき
                windowTime -= Time.deltaTime;

                if (windowTime <= 0)
                {
                    windowCnt++;
                    bossWindow.SetActive(false);
                }
                else
                {
                    bossWindow.SetActive(true);
                }

                if (boss == null)
                {// ボスがnullのとき
                    boss = GameManager.Instance.Boss.GetComponent<EnemyBase>();
                    // ボスステータスUI
                    bossHpBar.maxValue = boss.HP;
                    bossHpBar.value = boss.HP;
                    bossSliderText.text = "" + bossHpBar.maxValue;
                }
            }

            bossHpBar.value = boss.HP;

            if (boss.HP <= 0)
            {// ボスのHP表示がマイナスにならないようにする
                bossHpBar.value = 0;
            }

            bossSliderText.text = bossHpBar.value + "/" + bossHpBar.maxValue;

            bossStatus.SetActive(true);
        }
    }

    /// <summary>
    /// 取得したレリックを表示する関数
    /// </summary>
    /// <param name="relicSprite"></param>
    public void DisplayRelic(Sprite relicSprite)
    {
        foreach (Image image in relicImages)
        {
            if (image != relicSprite)
            {
                if (image.sprite == null)
                {
                    image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    image.sprite = relicSprite;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// レリック入れ替えの際に持っているレリックUIをリセットする関数
    /// </summary>
    public void ClearRelic()
    {
        foreach (Image image in relicImages)
        {
            if (image.sprite != null)
            {
                image.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                image.sprite = null;
            }
            else
            {
                break;
            }
        }
    }

    public void totalRelics(Sprite relicSprite, int num)
    {
        int count = 0;
        foreach (Image image in relicImages)
        {
            if (image.sprite == relicSprite)
            {
                if (image.sprite != null)
                {
                    if (num > 1)
                    {
                        relicCntText[count].enabled = true;
                        relicCntText[count].text = "×" + num;
                    }
                }
            }
            count++;
        }
    }

    public void OpenStatusWindow()
    {
        if (isStatusWindow)
        {
            statusUpWindow.SetActive(true);
        }
    }

    /// <summary>
    /// ステータスの強化
    /// </summary>
    public void UpPlayerStatus(int statusID)
    {
        //player.ChangeStatus();

        if (statusStock <= 0)
        {
            CloseStatusWindow();
            isStatusWindow = false;
        }
    }

    /// <summary>
    /// ステータス変更項目を変更
    /// </summary>
    public void UpStatusChange()
    {

    }

    /// <summary>
    /// ステータス強化ウィンドウロック
    /// </summary>
    public void HoldStatusWindow()
    {
        if (!isHold)
        {
            isHold = true;
        }
        else
        {
            isHold = false;
        }
    }

    /// <summary>
    /// ステータスウィンドウ閉じる
    /// </summary>
    public void CloseStatusWindow()
    {
        if (!isHold)
        {
            statusUpWindow.SetActive(false);
        }
    }

    public void CountClashText(int clashNum)
    {
        Text text = Instantiate(
            clashNumText,
            GameManager.Instance.transform.position + Vector3.up,
            Quaternion.identity);

        StartCoroutine(Fade(1, 0));

        text.text = "" + clashNum + "/" + GameManager.Instance.KnockTermsNum;
    }

    IEnumerator Fade(float start, float end)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }
        canvasGroup.alpha = end;
    }
}
