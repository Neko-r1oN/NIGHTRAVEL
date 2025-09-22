//----------------------------------------------------
// UI管理クラス
// Author : Souma Ueno
//----------------------------------------------------
using DG.Tweening;
using Pixeye.Unity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using ColorUtility = UnityEngine.ColorUtility;
using Random = UnityEngine.Random;
using System.Linq;
using System.Xml.Schema;
using Cysharp.Threading.Tasks.Triggers;

public class UIManager : MonoBehaviour
{
    PlayerBase player;
    EnemyBase boss;
    LevelManager level;

    #region 各UI
    [Foldout("キャンバス")]
    [SerializeField] GameObject canvas;              // キャンバス
                                                     
    [Foldout("プレイヤーステータス関連")]            
    [SerializeField] Slider playerHpBar;             // プレイヤーのHPバー
    [Foldout("プレイヤーステータス関連")]            
    [SerializeField] Slider expBar;                  // 経験値バー
                                                     
    [Foldout("テキスト")]                            
    [SerializeField] Text playerSliderText;              // プレイヤーの最大HPテキスト
    [Foldout("テキスト")]                                 
    [SerializeField] Text bossSliderText;                // ボスの最大HPテキスト
    [Foldout("テキスト")]                                 
    [SerializeField] Text levelText;                     // レベルテキスト
    [Foldout("テキスト")]                                 
    [SerializeField] Text pointText;                     // ポイントテキスト
    [Foldout("テキスト")]                                 
    [SerializeField] List<Text> relicCntText;            // レリックを持ってる数を表示するテキスト
    [Foldout("テキスト")]
    [SerializeField] List<Text> statusItemText;          // ステータスアップ説明テキスト
    [Foldout("テキスト")]
    [SerializeField] List<Text> statusExplanationsTexts; // ステータスアップ説明テキスト
    [Foldout("テキスト")]
    [SerializeField] Text levelUpStock;                  // レベルアップストックテキスト
    [Foldout("テキスト")]                                
    [SerializeField] Text levelUpText;                   // 強化可能テキスト
    [Foldout("テキスト")]                                
    [SerializeField] Text clashNumText;                  // 撃破数テキスト
    [Foldout("テキスト")]                                
    [SerializeField] Text tmText;                        // クリア条件テキスト
    [Foldout("テキスト")]                                
    [SerializeField] GameObject playerDmgText;           // プレイヤーダメージ表記
    [Foldout("テキスト")]                                
    [SerializeField] GameObject otherDmgText;            // その他ダメージ表記
    [Foldout("テキスト")]                                
    [SerializeField] Text relicName;                     // レリック名テキスト
    [Foldout("テキスト")]                                
    [SerializeField] Text diffText;                      // 難易度テキスト
    [Foldout("テキスト")]                                
    [SerializeField] Text terminalExplanationText;       // ターミナル説明テキスト
    [Foldout("テキスト")]                                
    [SerializeField] Text spectatingNameText;            // 観戦中プレイヤー名テキスト
    [Foldout("テキスト")]                                
    [SerializeField] GameObject healText;                // その他ダメージ表記

    [Foldout("フェードアウト")]                      
    [SerializeField] Canvas parentCanvas;                // テキストが表示されるキャンバスを割り当ててください
    [Foldout("フェードアウト")]                          
    [SerializeField] float fadeDuration = 2f;            // テキストがフェードアウトにかかる時間（秒）
    [Foldout("フェードアウト")]                          
    [SerializeField] float displayDuration = 1f;         // テキストが完全に表示される時間（フェード開始まで）
                                                         
    [Foldout("ボス関連")]                                
    [SerializeField] Slider bossHpBar;                   // ボスのHPバー
    [Foldout("ボス関連")]                                
    [SerializeField] GameObject bossStatus;              // ボスのステータス
                                                         
    [Foldout("ウィンドウ関係")]                          
    [SerializeField] GameObject statusUpWindow;          // ステータス強化ウィンドウ
    [Foldout("ウィンドウ関係")]                          
    [SerializeField] float windowTime;                   // ウィンドウが表示される秒数
    [Foldout("ウィンドウ関係")]                          
    [SerializeField] GameObject endWindow;               // 終了のウィンドウ
    [Foldout("ウィンドウ関係")]                          
    [SerializeField] GameObject spectatingWindow;        // 観戦ウィンドウ
    [Foldout("ウィンドウ関係")]                          
    [SerializeField] GameObject nextStageWindow;         // ステージ遷移ウィンドウ

    [Foldout("ボタン")]
    [SerializeField] Button resultYesButton;
    [Foldout("ボタン")]
    [SerializeField] Button resultNoButton;
    [Foldout("ボタン")]
    [SerializeField] Button changeGameYesButton;
    [Foldout("ボタン")]
    [SerializeField] Button changeGameNoButton;

    [Foldout("バナー関係")]                              
    [SerializeField] GameObject bossWindow;              // ボス出現UI
    [Foldout("バナー関係")]                              
    [SerializeField] GameObject termsBanner;             // クリア条件バナー
    [Foldout("バナー関係")]                              
    [SerializeField] GameObject relicBanner;             // 取得したレリックバナー

    [Foldout("レリック関連")]
    [SerializeField] List<Image> relicImages;
    [Foldout("レリック関連")]
    [SerializeField] Image relicImg;                     // レリックのイメージ

    [Foldout("その他")]
    [SerializeField] List<GameObject> playerStatus;      // 自分以外のプレイヤーのステータス
    [Foldout("その他")]
    [SerializeField] GameObject terminalExplanationObj;  // ターミナル説明用オブジェクト
    [Foldout("その他")]                                  
    [SerializeField] GameObject statusUpButton;          // ステータスアップボタン

    [Foldout("各環境用UI")]
    [SerializeField] GameObject gamePadUI;               // パッド操作UI
    [Foldout("各環境用UI")]                              
    [SerializeField] GameObject keyBoardUI;              // キーボード操作UI
    [Foldout("各環境用UI")]                              
    [SerializeField] GameObject swordSkillUI;            // 剣士スキルUI
    [Foldout("各環境用UI")]                              
    [SerializeField] GameObject gunnerSkillUI;           // ガンナースキルUI
                                                         
    [Foldout("ACTアイコンUI")]                           
    [SerializeField] Image skillCoolDownImage;           // スキルクールダウン
    [Foldout("ACTアイコンUI")]                           
    [SerializeField] Image blinkCoolDownImage;           // ブリンククールダウン
    [Foldout("ACTアイコンUI")]                           
    [SerializeField] RectTransform[] swordIconObjs;      // 剣アイコンオブジェ一覧
    [Foldout("ACTアイコンUI")]                           
    [SerializeField] Image[] swordIconImages;            // 剣アイコン画像一覧
    [Foldout("ACTアイコンUI")]                           
    [SerializeField] RectTransform[] gunIconObjs;        // 銃アイコンオブジェ一覧
    [Foldout("ACTアイコンUI")]                           
    [SerializeField] Image[] gunIconImages;              // 銃アイコン画像一覧
    [Foldout("ACTアイコンUI")]                           
    [SerializeField] GameObject gunSkillLockObj;         // ブリンククールダウン
                                                         
    private bool isInputGamePad;                         // ゲームパッド入力かどうか

    public bool IsInputGamePad { get { return isInputGamePad; } }

    #endregion

    // 定数
    private const float pushIconScale = 0.98f; // キー押下時のアイコン縮小率
    private const float pushIconColor = 0.8f;  // キー押下時のアイコン色変化率

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

    // ターミナル起動時の定型文
    private Dictionary<TERMINAL_TYPE, string> terminalExplanation = new Dictionary<TERMINAL_TYPE, string>
    {
        {TERMINAL_TYPE.None,""},
        {TERMINAL_TYPE.Enemy,"出現した敵を全て倒せ" },
        {TERMINAL_TYPE.Speed,"出現したゲートを全て通れ" },
        {TERMINAL_TYPE.Deal,"" },
        {TERMINAL_TYPE.Jumble,"" },
        {TERMINAL_TYPE.Elite,"出現したエリート敵を全て倒せ" },
        {TERMINAL_TYPE.Boss,"" }
    };

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
        player = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>();

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

        clashNumText.text = "条件:0/" + SpawnManager.Instance.KnockTermsNum;

        tmText.text = "クリア条件：5分間生き残る or 敵"
            + SpawnManager.Instance.KnockTermsNum + "体倒せ";

        level = LevelManager.Instance;

        diffText.text = level.LevelName[(DIFFICULTY_TYPE)level.GameLevel].ToString();
        colorCode = "#ffc0cb";

        if(ColorUtility.TryParseHtmlString(colorCode,out color))
        {
            diffText.color = color;
        }

        terminalExplanationObj.SetActive(false);
        endWindow.SetActive(false);

        spectatingWindow.SetActive(false);

        foreach (Image relic in relicImages)
        {
            relic.enabled = false;
        }

        // キャラのジョブ毎にUIを変更
        if(player.PlayerType == Player_Type.Sword)
        {
            ChangeSkillUI("Sword");
        }
        else
        {
            ChangeSkillUI("Gunner");
        }
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    void Update()
    {
        // 操作UI変更処理
        InputChangeUI();
        
        if (player == null)
        {
            player = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>();
        }

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
        expBar.value = (float)player.NowExp;

        if (SpawnManager.Instance.IsSpawnBoss)
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
            }

            if (boss != null)
            {
                bossHpBar.value = boss.HP;
                bossSliderText.text = bossHpBar.value + "/" + bossHpBar.maxValue;
            }

            if (player.HP <= 0)
            {
                playerHpBar.value = 0;
                playerSliderText.text = "0";
                DisplaySpectatingPlayer();

                if (CheckAllPlayersDead())
                {
                    GameManager.Instance.CangeResult();
                }
            }
        }
    }

    /// <summary>
    /// ボスUI表示
    /// </summary>
    public void DisplayBossUI()
    {// ボスがスポーンした
        boss = CharacterManager.Instance.GetBossObject();
        // ボスステータスUI
        bossHpBar.maxValue = boss.BaseHP;
        bossHpBar.value = boss.BaseHP;
        bossSliderText.text = "" + bossHpBar.maxValue;

        //if (boss.HP <= 0)
        //{// ボスのHP表示がマイナスにならないようにする
        //    bossHpBar.value = 0;
        //}

        bossSliderText.text = bossHpBar.value + "/" + bossHpBar.maxValue;

        bossStatus.SetActive(true);

        clashNumText.enabled = false;
    }

    /// <summary>
    /// ボスUI非表示
    /// </summary>
    public void HideBossUI()
    {
        bossStatus.SetActive(false);
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
                    image.enabled = true;
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
                
                for(int i= 0; i < relicCntText.Count; i++)
                {
                    relicCntText[i].text = "" + 0;
                    relicCntText[i].enabled = false;
                }
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// 同レリックの所持数更新
    /// </summary>
    /// <param name="relicSprite"></param>
    /// <param name="num"></param>
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

    /// <summary>
    /// ステータス強化ウィンドウ表示
    /// </summary>
    public void OpenStatusWindow()
    {
        if (isStatusWindow)
        {
            statusUpWindow.SetActive(true);

            var pair = LevelManager.Instance.Options.FirstOrDefault();

            int currentIndex = 0;
            foreach(var item in pair.Value)
            {
                statusItemText[currentIndex].text = item.Name;
                statusExplanationsTexts[currentIndex].text = item.Explanation;

                currentIndex++;
            }
        }
    }

    /// <summary>
    /// ステータスの強化
    /// </summary>
    public async void UpPlayerStatus(int buttonId)
    {
        // 選択したステータス強化選択肢を取得して削除
        var values = LevelManager.Instance.Options.FirstOrDefault().Value;
        var key = LevelManager.Instance.Options.FirstOrDefault().Key;
        LevelManager.Instance.Options.Remove(key);

        

        if (!RoomModel.Instance)
        {// オフライン
            //ステータス変更
            UpStatusChange();
        }
        else
        {// オンライン
            await RoomModel.Instance.ChooseUpgrade(key, values[buttonId].TypeId);
        }

        if (LevelManager.Instance.Options.Count == 0)
        {
            CloseStatusWindow();
            isStatusWindow = false;
            levelUpText.enabled = false;
        }
        else
        {
            // ステータス強化テキストの更新
            ChangeUpStatusText();
        }

        // ステータス強化回数の減少
        statusStock--;

        levelUpStock.text = "残り強化数：" + statusStock;

        if (statusStock <= 0)
        {// 強化ストックが0の場合
            CloseStatusWindow();
            isStatusWindow = false;
            levelUpText.enabled = false;
        }
    }

    [ContextMenu("UpStatusChange")]
    /// <summary>
    /// ステータス変更項目を変更
    /// </summary>
    public void UpStatusChange()
    {
        int maxCnt = Enum.GetNames(typeof(STATUS_TYPE)).Length;

        int random = Random.Range(0, maxCnt);

        STATUS_TYPE type = (STATUS_TYPE)Enum.ToObject(typeof(STATUS_TYPE), random);

        Debug.Log(type);

        player.ApplyMaxStatusModifierByRate(0.15f, type);
    }

    public void ChangeUpStatusText()
    {
        var pair = LevelManager.Instance.Options.First();
        var data = pair.Value;

        int currentIndex = 0;
        foreach (var item in data)
        {
            statusItemText[currentIndex].text = item.Name;
            statusExplanationsTexts[currentIndex].text = item.Explanation;

            currentIndex++;
        }   
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

    /// <summary>
    /// ボス条件のテキスト更新
    /// </summary>
    /// <param name="crashNum"></param>
    public void CountTermsText(int crashNum)
    {
        clashNumText.text = "条件:" + crashNum + "/" + SpawnManager.Instance.KnockTermsNum;
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
        else if (SpawnManager.Instance.IsSpawnBoss)
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
    /// 回復表記処理
    /// </summary>
    public void PopHealUI(int healVol, Vector3 popPosition)
    {
        GameObject ui = Instantiate(healText);

        ui.GetComponent<Text>().text = healVol.ToString();

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

    /// <summary>
    /// ゲーム内レベルアップ
    /// </summary>
    public void UpGameLevelText()
    {
        if(level.GameLevel > level.LevelHellId)
        {
            return;
        }
             
        diffText.text = level.LevelName[(DIFFICULTY_TYPE)level.GameLevel].ToString();

        // textの色変更
        switch (level.GameLevel)
        {// Babyはstartで設定済みのため省く

            case (int)DIFFICULTY_TYPE.Easy:
                diffText.text = level.LevelName[(DIFFICULTY_TYPE)level.GameLevel].ToString();
                colorCode = "#00ffff";

                if (ColorUtility.TryParseHtmlString(colorCode, out color))
                {
                    diffText.color = color;
                }

                return;

            case (int)DIFFICULTY_TYPE.Normal:

                diffText.text = level.LevelName[(DIFFICULTY_TYPE)level.GameLevel].ToString();
                colorCode = "#66cdaa";

                if (ColorUtility.TryParseHtmlString(colorCode, out color))
                {
                    diffText.color = color;
                }

                return;

            case (int)DIFFICULTY_TYPE.Hard:

                diffText.text = level.LevelName[(DIFFICULTY_TYPE)level.GameLevel].ToString();

                diffText.color = Color.red;

                return;

            case (int)DIFFICULTY_TYPE.VeryHard:

                diffText.text = level.LevelName[(DIFFICULTY_TYPE)level.GameLevel].ToString();
                colorCode = "#b22222";

                if (ColorUtility.TryParseHtmlString(colorCode, out color))
                {
                    diffText.color = color;
                }

                return;

            case (int)DIFFICULTY_TYPE.Hell:

                diffText.text = level.LevelName[(DIFFICULTY_TYPE)level.GameLevel].ToString();
                colorCode = "#ff00ff";

                if (ColorUtility.TryParseHtmlString(colorCode, out color))
                {
                    diffText.color = color;
                }

                return;
        }
    }

    /// <summary>
    /// ターミナルの説明文表示・変更処理
    /// </summary>
    /// <param name="type"></param>
    public void DisplayTerminalExplanation(TERMINAL_TYPE type)
    {
        terminalExplanationObj.SetActive(true);

        terminalExplanationText.text = terminalExplanation[type].ToString();
    }

    /// <summary>
    /// ターミナル説明文非表示・テキストを戻す
    /// </summary>
    public void DisplayTimeInstructions()
    {
        terminalExplanationObj.SetActive(false);
        TimerDirector.Instance.TimerObj.transform.GetChild(0).GetComponent<Text>().text = " 敵衛システム復旧まで";
    }

    /// <summary>
    /// 観戦画面用UIの更新
    /// </summary>
    public void DisplaySpectatingPlayer()
    {
        spectatingWindow.SetActive(true);
        spectatingNameText.text = "player2";

        //statusUpButton.SetActive(false);
        levelUpText.enabled = false;

        foreach (Image relic in relicImages)
        {
            relic.enabled = false;
        }
    }

    /// <summary>
    /// 全プレイヤーが死亡したか確認する処理
    /// </summary>
    /// <returns></returns>
    private bool CheckAllPlayersDead()
    {
        if(CharacterManager.Instance.PlayerObjs == null)
        {
            return false;
        }

        foreach(var player in CharacterManager.Instance.PlayerObjs.Values)
        {
            if(player && player.GetComponent<PlayerBase>().HP > 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// クールダウン表示
    /// </summary>
    /// <param name="skillFlag">true:skill false;blink</param>
    /// <param name="coolTime">クールタイム</param>
    public void DisplayCoolDown(bool skillFlag,float coolTime)
    {
        if(skillFlag)
        {
            skillCoolDownImage.fillAmount = 1f;
            skillCoolDownImage.DOFillAmount(0f, coolTime);
        }
        else
        {
            blinkCoolDownImage.fillAmount = 1f;
            blinkCoolDownImage.DOFillAmount(0f, coolTime);
        }
    }

    /// <summary>
    /// 操作UI変化処理
    /// </summary>
    /// <param name="keyBoardFlag"></param>
    private void ChangeOperationUI(string mode)
    {
        if (mode == "Keyboard")
        {
            isInputGamePad = false;
            gamePadUI.SetActive(false);
            keyBoardUI.SetActive(true);
        }
        else if (mode == "Gamepad")
        {
            isInputGamePad = true;
            gamePadUI.SetActive(true);
            keyBoardUI.SetActive(false);
        }
    }

    /// <summary>
    /// キャラ毎のスキルUI変化
    /// </summary>
    /// <param name="job"></param>
    private void ChangeSkillUI(string job)
    {
        if (job == "Sword")
        {
            swordSkillUI.SetActive(true);
            gunnerSkillUI.SetActive(false);
        }
        else if (job == "Gunner")
        {
            swordSkillUI.SetActive(false);
            gunnerSkillUI.SetActive(true);
        }
    }

    /// <summary>
    /// 入力時のUI変更
    /// </summary>
    private void InputChangeUI()
    {
        // ガンナースキルロック処理
        if (!player.GetGrounded() && player.PlayerType == Player_Type.Gunner)
        {
            gunSkillLockObj.SetActive(true);
        }
        else
        {
            gunSkillLockObj.SetActive(false);
        }

        // キーボード or ゲームパッドの入力でUI変化
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            ChangeOperationUI("Keyboard");
        }

        if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Blink") || Input.GetButtonDown("Attack1") || Input.GetButtonDown("Attack2"))
        {
            ChangeOperationUI("Gamepad");
        }

        // キー入力 or ボタン入力でUIリアクション

        // 通常攻撃
        if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Attack1"))
        {
            if (player.PlayerType == Player_Type.Sword)
            {   // 剣士アイコン
                swordIconObjs[0].localScale = new Vector3(pushIconScale, pushIconScale, pushIconScale);
                swordIconImages[0].color = new Color(pushIconColor, pushIconColor, pushIconColor, 1.0f);
            }
            else
            {   // ガンナーアイコン
                gunIconObjs[0].localScale = new Vector3(pushIconScale, pushIconScale, pushIconScale);
                gunIconImages[0].color = new Color(pushIconColor, pushIconColor, pushIconColor, 1.0f);
            }
        }
        else if (Input.GetMouseButtonUp(0) || Input.GetButtonUp("Attack1"))
        {
            if (player.PlayerType == Player_Type.Sword)
            {
                swordIconObjs[0].localScale = new Vector3(1.0f, 1.0f, 1.0f);
                swordIconImages[0].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
            else
            {
                gunIconObjs[0].localScale = new Vector3(1.0f, 1.0f, 1.0f);
                gunIconImages[0].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
        }
        // スキル攻撃
        if (Input.GetMouseButtonDown(1) || Input.GetButtonDown("Attack2"))
        {
            if (player.PlayerType == Player_Type.Sword)
            {   // 剣士アイコン
                swordIconObjs[1].localScale = new Vector3(pushIconScale, pushIconScale, pushIconScale);
                swordIconImages[1].color = new Color(pushIconColor, pushIconColor, pushIconColor, 1.0f);
            }
            else
            {   // ガンナーアイコン
                gunIconObjs[1].localScale = new Vector3(pushIconScale, pushIconScale, pushIconScale);
                gunIconImages[1].color = new Color(pushIconColor, pushIconColor, pushIconColor, 1.0f);
            }
        }
        else if (Input.GetMouseButtonUp(1) || Input.GetButtonUp("Attack2"))
        {
            if (player.PlayerType == Player_Type.Sword)
            {
                swordIconObjs[1].localScale = new Vector3(1.0f, 1.0f, 1.0f);
                swordIconImages[1].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
            else
            {
                gunIconObjs[1].localScale = new Vector3(1.0f, 1.0f, 1.0f);
                gunIconImages[1].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
        }
        // ブリンク
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("Blink"))
        {
            if (player.PlayerType == Player_Type.Sword)
            {   // 剣士アイコン
                swordIconObjs[2].localScale = new Vector3(pushIconScale, pushIconScale, pushIconScale);
                swordIconImages[2].color = new Color(pushIconColor, pushIconColor, pushIconColor, 1.0f);
            }
            else
            {   // ガンナーアイコン
                gunIconObjs[2].localScale = new Vector3(pushIconScale, pushIconScale, pushIconScale);
                gunIconImages[2].color = new Color(pushIconColor, pushIconColor, pushIconColor, 1.0f);
            }
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetButtonUp("Blink"))
        {
            if (player.PlayerType == Player_Type.Sword)
            {
                swordIconObjs[2].localScale = new Vector3(1.0f, 1.0f, 1.0f);
                swordIconImages[2].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
            else
            {
                gunIconObjs[2].localScale = new Vector3(1.0f, 1.0f, 1.0f);
                gunIconImages[2].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
        }
        // ジャンプ
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump"))
        {
            if (player.PlayerType == Player_Type.Sword)
            {   // 剣士アイコン
                swordIconObjs[3].localScale = new Vector3(pushIconScale, pushIconScale, pushIconScale);
                swordIconImages[3].color = new Color(pushIconColor, pushIconColor, pushIconColor, 1.0f);
            }
            else
            {   // ガンナーアイコン
                gunIconObjs[3].localScale = new Vector3(pushIconScale, pushIconScale, pushIconScale);
                gunIconImages[3].color = new Color(pushIconColor, pushIconColor, pushIconColor, 1.0f);
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Jump"))
        {
            if (player.PlayerType == Player_Type.Sword)
            {
                swordIconObjs[3].localScale = new Vector3(1.0f, 1.0f, 1.0f);
                swordIconImages[3].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
            else
            {
                gunIconObjs[3].localScale = new Vector3(1.0f, 1.0f, 1.0f);
                gunIconImages[3].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
        }
    }

    /// <summary>
    /// 次の画面に移動するか確認するウィンドウの表示
    /// </summary>
    public void DisplayNextStageWindow()
    {
        nextStageWindow.SetActive(true);
    }

    /// <summary>
    /// ゲーム終了確認ウィンドウ表示
    /// </summary>
    public void DisplayEndGameWindow()
    {
        endWindow.SetActive(true);
    }

    /// <summary>
    /// ゲーム終了ボタン
    /// </summary>
    /// <param name="id"></param>
    public async void EndGameButtonPush(int id)
    {
        switch (id)
        {
            case 0:

                if (!RoomModel.Instance) GameManager.Instance.CangeResult();
                else await RoomModel.Instance.StageClear(false);

                changeGameYesButton.interactable = false;
                changeGameNoButton.interactable = false;

                break;
            case 1:
                endWindow.SetActive(false);

                break;
        }
    }

    /// <summary>
    /// 次ステージ移動のボタン処理
    /// </summary>
    /// <param name="id"></param>
    public async void NextGameButtonPush(int id)
    {
        switch (id)
        {
            case 0:
                if (!RoomModel.Instance)
                {
                    
                    GameManager.Instance.ChengScene(GameManager.Instance.NextStage);
                }
                else await RoomModel.Instance.StageClear(true);

                changeGameYesButton.interactable = false;
                changeGameNoButton.interactable = false;

                break;
            case 1:
                nextStageWindow.SetActive(false);

                break;
        }
    }
}
