using DG.Tweening;
using NUnit.Framework;
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    PlayerBase player;
    EnemyBase boss;

    #region 各UI
    [Foldout("UI(プレイヤーステータス関連)")]
    [SerializeField] Slider playerHpBar;          // プレイヤーのHPバー
    [Foldout("UI(プレイヤーステータス関連)")]
    [SerializeField] Slider expBar;               // 経験値バー

    [Foldout("UI(テキスト)")]
    [SerializeField] Text playerSliderText;       // プレイヤーの最大HPテキスト
    [Foldout("UI(テキスト)")]
    [SerializeField] Text bossSliderText;         // ボスの最大HPテキスト
    [Foldout("UI(テキスト)")]
    [SerializeField] Text levelText;              // レベルテキスト
    [Foldout("UI(テキスト)")]
    [SerializeField] Text pointText;              // ポイントテキスト
    [SerializeField] List<Image> relicImages;
    [Foldout("UI(テキスト)")]
    [SerializeField] List<Text> relicCntText;     // レリックを持ってる数を表示するテキスト
    //[Foldout("UI(テキスト)")]
    //[SerializeField] List<Text>  statusText;      // ステータスアップ説明テキスト
    [Foldout("UI(テキスト)")]
    [SerializeField] Text levelUpStock;           // レベルアップストックテキスト
    [Foldout("UI(テキスト)")]
    [SerializeField] Text levelUpText;            // 強化可能テキスト
    [Foldout("UI(テキスト)")]
    [SerializeField] Text clashNumText;           // 撃破数テキスト
    [Foldout("UI(テキスト)")]
    [SerializeField] Text tmText;                 // クリア条件テキスト

    [Foldout("UI(フェードアウト)")]
    [SerializeField] Canvas parentCanvas;         // テキストが表示されるキャンバスを割り当ててください
    [Foldout("UI(フェードアウト)")]
    [SerializeField] float fadeDuration = 2f;     // テキストがフェードアウトにかかる時間（秒）
    [Foldout("UI(フェードアウト)")]
    [SerializeField] float displayDuration = 1f;  // テキストが完全に表示される時間（フェード開始まで）

    [Foldout("UI(ボスステータス関連)")]
    [SerializeField] Slider bossHpBar;            // ボスのHPバー
    [Foldout("UI(ボスステータス関連)")]
    [SerializeField] GameObject bossStatus;       // ボスのステータス

    [Foldout("UI(ウィンドウ関係)")]
    [SerializeField] GameObject statusUpWindow;   // ステータス強化ウィンドウ
    [Foldout("UI(ウィンドウ関係)")]
    [SerializeField] float windowTime;            // ウィンドウが表示される秒数

    [Foldout("UI(バナー関係)")]
    [SerializeField] GameObject bossWindow;       // ボス出現UI
    [Foldout("UI(バナー関係)")]
    [SerializeField] GameObject termsBanner;      // クリア条件バナー

    #endregion

    int windowCnt = 0;   // ウィンドウが表示できるカウント(一度だけ使う)
    int lastLevel = 0;   // レベルアップ前のレベル
    int statusStock = 0; // レベルアップストック数
    bool isStatusWindow; // ステータスウィンドウが開けるかどうか
    bool isHold;         // ステータスウィンドウロック用
    Text clashText;
    private Renderer[] childRenderers; // 子オブジェクトのRendererを複数対応
    private Text[] childTexts; // 子オブジェクトの標準UI.Textを複数対応
    // 各RendererとTextの初期色を保存するためのリスト
    // フェードイン時に元の不透明な状態に戻すために必要
    private System.Collections.Generic.List<Color> initialRendererColors = new System.Collections.Generic.List<Color>();
    private System.Collections.Generic.List<Color> initialTextColors = new System.Collections.Generic.List<Color>();

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

        // 親オブジェクトとその子からRendererをすべて取得
        childRenderers = termsBanner.GetComponentsInChildren<Renderer>(true);
        // 親オブジェクトとその子から標準UI.Textをすべて取得
        childTexts = termsBanner.GetComponentsInChildren<Text>(true);

        // 各Rendererのマテリアル設定と初期色の保存
        foreach (Renderer renderer in childRenderers)
        {
            if (renderer != null)
            {
                SetMaterialFadeMode(renderer.material);
                initialRendererColors.Add(renderer.material.color);
            }
        }

        // 各Textの初期色の保存
        foreach (Text text in childTexts)
        {
            if (text != null)
            {
                initialTextColors.Add(text.color);
            }
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

        clashNumText.text = "条件:0/" + GameManager.Instance.KnockTermsNum;

        tmText.text = "クリア条件：5分間生き残る or 敵"
            + GameManager.Instance.KnockTermsNum + "体倒せ";
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

            if(GameManager.Instance.IsSpawnBoss)
            {
                clashNumText.enabled = false;
            }
        }
    }

    public void ChangTitleScene()
    {
        SceneManager.LoadScene("Title ueno");
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

    public void CountTermsText(int crashNum)
    {
        clashNumText.text = "条件:" + crashNum + "/" + GameManager.Instance.KnockTermsNum;
    }

    /// <summary>
    /// 条件バナーを一定時間表示し、その後フェードアウトさせます。
    /// ゲームのUpdateは停止しません。
    /// </summary>
    public void ShowUIAndFadeOut()
    {
        StopAllCoroutines(); // 既に実行中のコルーチンを停止
        StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        if (GameManager.Instance.IsSpawnBoss)
        {
            bossWindow.SetActive(true);
        }
        else
        {
            termsBanner.SetActive(true); // UIパネルを表示
        }

        // 各RendererとTextのアルファ値を完全に不透明に設定
        int rendererIndex = 0;
        foreach (Renderer renderer in childRenderers)
        {
            if (renderer != null && rendererIndex < initialRendererColors.Count)
            {
                Color originalColor = initialRendererColors[rendererIndex];
                renderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
                rendererIndex++;
            }
        }

        int textIndex = 0;
        foreach (Text text in childTexts)
        {
            if (text != null && textIndex < initialTextColors.Count)
            {
                Color originalColor = initialTextColors[textIndex];
                text.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
                textIndex++;
            }
        }

        // 指定された表示時間待機 (Time.timeScaleが1なのでWaitForSecondsでOK)
        yield return new WaitForSeconds(displayDuration);

        // フェードアウト処理
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime; // Time.timeScaleが1なのでdeltaTimeでOK
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration); // 1から0へ線形補間

            foreach (Renderer renderer in childRenderers)
            {
                if (renderer != null)
                {
                    Color currentColor = renderer.material.color;
                    currentColor.a = alpha;
                    renderer.material.color = currentColor;
                }
            }
            foreach (Text text in childTexts)
            {
                if (text != null)
                {
                    Color currentColor = text.color;
                    currentColor.a = alpha;
                    text.color = currentColor;
                }
            }
            yield return null; // 1フレーム待機
        }

        // フェードアウト完了後の処理 (完全に透明にする)
        foreach (Renderer renderer in childRenderers)
        {
            if (renderer != null)
            {
                Color currentColor = renderer.material.color;
                currentColor.a = 0f;
                renderer.material.color = currentColor;
            }
        }
        foreach (Text text in childTexts)
        {
            if (text != null)
            {
                Color currentColor = text.color;
                currentColor.a = 0f;
                text.color = currentColor;
            }
        }

        if(bossWindow.activeSelf == true)
        {
            bossWindow.SetActive(false);
        }
        else
        {
            termsBanner.SetActive(false); // 完全に透明になったらUIパネルを非表示にする
        }
    }

    // Standardシェーダーで透明度を扱うための設定ヘルパーメソッド
    private void SetMaterialFadeMode(Material material)
    {
        if (material == null) return;

        material.SetOverrideTag("RenderType", "Fade");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    }
}
