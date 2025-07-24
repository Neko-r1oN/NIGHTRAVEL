//----------------------------------------------------
// UI管理クラス
// Author : Souma Ueno
//----------------------------------------------------
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static CharacterBase;
using ColorUtility = UnityEngine.ColorUtility;

public class UIManager : MonoBehaviour
{
    PlayerBase player;
    EnemyBase boss;
    LevelManager level;

    #region 各UI
    [Foldout("キャンバス")]
    [SerializeField] GameObject canvas;             // キャンバス
                                                    
    [Foldout("プレイヤーステータス関連")]          
    [SerializeField] Slider playerHpBar;            // プレイヤーのHPバー
    [Foldout("プレイヤーステータス関連")]           
    [SerializeField] Slider expBar;                 // 経験値バー
                                                    
    [Foldout("テキスト")]                           
    [SerializeField] Text playerSliderText;         // プレイヤーの最大HPテキスト
    [Foldout("テキスト")]                           
    [SerializeField] Text bossSliderText;           // ボスの最大HPテキスト
    [Foldout("テキスト")]                           
    [SerializeField] Text levelText;                // レベルテキスト
    [Foldout("テキスト")]                           
    [SerializeField] Text pointText;                // ポイントテキスト
    [SerializeField] List<Image> relicImages;       
    [Foldout("テキスト")]                           
    [SerializeField] List<Text> relicCntText;       // レリックを持ってる数を表示するテキスト
    //[Foldout("UI(テキスト)")]                     
    //[SerializeField] List<Text>  statusText;        // ステータスアップ説明テキスト
    [Foldout("テキスト")]                           
    [SerializeField] Text levelUpStock;             // レベルアップストックテキスト
    [Foldout("テキスト")]
    [SerializeField] Text levelUpText;              // 強化可能テキスト
    [Foldout("テキスト")]                           
    [SerializeField] Text clashNumText;             // 撃破数テキスト
    [Foldout("テキスト")]                           
    [SerializeField] Text tmText;                   // クリア条件テキスト
    [Foldout("テキスト")]                           
    [SerializeField] GameObject playerDmgText;      // プレイヤーダメージ表記
    [Foldout("テキスト")]                           
    [SerializeField] GameObject otherDmgText;       // その他ダメージ表記
    [Foldout("テキスト")]                           
    [SerializeField] Text relicName;                // その他ダメージ表記
    [Foldout("テキスト")]                           
    [SerializeField] Text diffText;                 // 難易度テキスト
                                                    
                                                    
    [Foldout("フェードアウト")]                     
    [SerializeField] Canvas parentCanvas;           // テキストが表示されるキャンバスを割り当ててください
    [Foldout("フェードアウト")]                     
    [SerializeField] float fadeDuration = 2f;       // テキストがフェードアウトにかかる時間（秒）
    [Foldout("フェードアウト")]                     
    [SerializeField] float displayDuration = 1f;    // テキストが完全に表示される時間（フェード開始まで）

    [Foldout("ボスステータス関連")]
    [SerializeField] Slider bossHpBar;              // ボスのHPバー
    [Foldout("ボスステータス関連")]                 
    [SerializeField] GameObject bossStatus;         // ボスのステータス
                                                    
    [Foldout("ウィンドウ関係")]                     
    [SerializeField] GameObject statusUpWindow;     // ステータス強化ウィンドウ
    [Foldout("ウィンドウ関係")]                     
    [SerializeField] float windowTime;              // ウィンドウが表示される秒数

    [Foldout("バナー関係")]
    [SerializeField] GameObject bossWindow;         // ボス出現UI
    [Foldout("バナー関係")]                         
    [SerializeField] GameObject termsBanner;        // クリア条件バナー
    [Foldout("バナー関係")]                         
    [SerializeField] GameObject relicBanner;        // 取得したレリックバナー

    [SerializeField] List<GameObject> playerStatus; // 自分以外のプレイヤーのステータス

    [SerializeField] Image relicImg;                // レリックのイメージ

    #endregion

    int windowCnt = 0;   // ウィンドウが表示できるカウント(一度だけ使う)
    int lastLevel = 0;   // レベルアップ前のレベル
    int statusStock = 0; // レベルアップストック数
    bool isStatusWindow; // ステータスウィンドウが開けるかどうか
    bool isHold;         // ステータスウィンドウロック用
    string colorCode;    // カラーコード
    Color color;
    private Renderer[] childRenderers; // 子オブジェクトのRendererを複数対応
    private Text[] childTexts; // 子オブジェクトの標準UI.Textを複数対応
    // 各RendererとTextの初期色を保存するためのリスト
    // フェードイン時に元の不透明な状態に戻すために必要
    private System.Collections.Generic.List<Color> initialRendererColors = new System.Collections.Generic.List<Color>();
    private System.Collections.Generic.List<Color> initialTextColors = new System.Collections.Generic.List<Color>();

    bool isRelicGet;

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

        //player = GameManager.Instance.Players.GetComponent<PlayerBase>();

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
        isRelicGet = false;

        levelUpText.enabled = false;
        bossStatus.SetActive(false);

        relicBanner.SetActive(false);

        for (int i = 0; i < playerStatus.Count; i++)
        {
            playerStatus[i].SetActive(false);
        }

        clashNumText.text = "条件:0/" + GameManager.Instance.KnockTermsNum;

        tmText.text = "クリア条件：5分間生き残る or 敵"
            + GameManager.Instance.KnockTermsNum + "体倒せ";

        level = LevelManager.Instance;

        diffText.text = level.LevelName[level.GameLevel].ToString();
        colorCode = "#ffb6c1";

        if(ColorUtility.TryParseHtmlString(colorCode,out color))
        {
            diffText.color = color;
        }
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

                boss = GameManager.Instance.Boss.GetComponent<EnemyBase>();
                // ボスステータスUI
                bossHpBar.maxValue = boss.HP;
            }
            
            bossHpBar.value = boss.HP;
            bossSliderText.text = "" + bossHpBar.maxValue;

            if (boss.HP <= 0)
            {// ボスのHP表示がマイナスにならないようにする
                bossHpBar.value = 0;
            }

            bossSliderText.text = bossHpBar.value + "/" + bossHpBar.maxValue;

            bossStatus.SetActive(true);

            clashNumText.enabled = false;
        }

        if(player.HP <= 0)
        {
            playerHpBar.value = 0;
            playerSliderText.text = "0";
            ChangTitleScene();
        }
    }

    private void ChangTitleScene()
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
                    isRelicGet = true;
                    ShowUIAndFadeOut();
                    GetRelicBanner(relicSprite);
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

        GetRelicBanner(relicSprite);
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
        //ステータス変更

        if (statusStock <= 0)
        {// 強化ストックが0の場合
            CloseStatusWindow();
            isStatusWindow = false;
        }
    }

    [ContextMenu("UpStatusChange")]
    /// <summary>
    /// ステータス変更項目を変更
    /// </summary>
    public void UpStatusChange()
    {
        player.ApplyStatusModifierByRate(0.15f, STATUS_TYPE.Power);
    }

    /// <summary>
    /// ステータス強化ウィンドウロック
    /// </summary>
    public void HoldStatusWindow()
    {
        if (!isHold)
        {// ホールド状態でないとき
            // ホールドにする
            isHold = true;
        }
        else
        {// ホールド状態のとき
            // ホールド解除
            isHold = false;
        }
    }

    /// <summary>
    /// ステータスウィンドウ閉じる
    /// </summary>
    public void CloseStatusWindow()
    {
        if (!isHold)
        {// ホールド状態でないとき
            // ウィンドウを閉じる
            statusUpWindow.SetActive(false);
        }
    }

    public void CountTermsText(int crashNum)
    {
        clashNumText.text = "条件:" + crashNum + "/" + GameManager.Instance.KnockTermsNum;
    }

    /// <summary>
    /// 条件バナーを一定時間表示し、その後フェードアウト
    /// </summary>
    public void ShowUIAndFadeOut()
    {
        StopAllCoroutines(); // 既に実行中のコルーチンを停止
        StartCoroutine(FadeSequence());
    }

    /// <summary>
    /// フェードアウト処理
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeSequence()
    {
        if (isRelicGet)
        {
            relicBanner.SetActive(true);
        }
        else if (GameManager.Instance.IsSpawnBoss)
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

        if (bossWindow.activeSelf == true)
        {
            bossWindow.SetActive(false);
        }
        else if (relicBanner.activeSelf == true)
        {
            relicBanner.SetActive(false);
            isRelicGet = false;
            DeleteRelicBunnerImg();
        }
        else
        {
            termsBanner.SetActive(false); // 完全に透明になったらUIパネルを非表示にする
        }
    }
    

    /// <summary>
    /// Standardシェーダーで透明度を扱うための設定ヘルパーメソッド
    /// </summary>
    /// <param name="material"></param>
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

    /// <summary>
    /// ダメージ表記処理
    /// </summary>
    public void PopDamageUI(int dmgVol, Vector3 popPosition, bool isPlayer)
    {
        GameObject ui;

        if (isPlayer) ui = Instantiate(playerDmgText);
        else ui = Instantiate(otherDmgText);

        ui.GetComponent<Text>().text = dmgVol.ToString();

        ui.transform.SetParent(canvas.transform);

        var circlePos = UnityEngine.Random.insideUnitCircle * 1.2f;
        var textPos = popPosition + new Vector3(0, 0.5f, 0) * UnityEngine.Random.Range(0.5f, 1.5f) + new Vector3(circlePos.x, 0, circlePos.y);
        ui.GetComponent<RectTransform>().position = RectTransformUtility.WorldToScreenPoint(Camera.main, textPos);

        ui.SetActive(true);
    }

    /// <summary>
    /// 取得したレリックをバナーで表示
    /// </summary>
    /// <param name="relicImg"></param>
    private void GetRelicBanner(Sprite relicSprite)
    {
        if (relicImg.sprite == null)
        {
            relicImg.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            relicImg.sprite = relicSprite;
        }
    }

    /// <summary>
    /// レリックイメージを元に戻す
    /// </summary>
    private void DeleteRelicBunnerImg()
    {
        if (relicImg.sprite != null)
        {
            relicImg.sprite = null;
        }
    }

    public void UpGameLevelText()
    {
        diffText.text = level.LevelName[level.GameLevel].ToString();

        // textの色変更
        switch (level.GameLevel)
        {// Babyはstartで設定済みのため省く

            case LevelManager.GAME_LEVEL.Easy:
                diffText.text = level.LevelName[level.GameLevel].ToString();
                colorCode = "#00ffff";

                if (ColorUtility.TryParseHtmlString(colorCode, out color))
                {
                    diffText.color = color;
                }

                return;
                
            case LevelManager.GAME_LEVEL.Normal:

                diffText.text = level.LevelName[level.GameLevel].ToString();
                colorCode = "#66cdaa";

                if (ColorUtility.TryParseHtmlString(colorCode, out color))
                {
                    diffText.color = color;
                }

                return;

            case LevelManager.GAME_LEVEL.Hard:

                diffText.text = level.LevelName[level.GameLevel].ToString();
                colorCode = "#dc143c";

                if (ColorUtility.TryParseHtmlString(colorCode, out color))
                {
                    diffText.color = color;
                }

                return;

            case LevelManager.GAME_LEVEL.Berryhard:

                diffText.text = level.LevelName[level.GameLevel].ToString();
                colorCode = "#b22222";

                if (ColorUtility.TryParseHtmlString(colorCode, out color))
                {
                    diffText.color = color;
                }

                return;
        }
    }
}
