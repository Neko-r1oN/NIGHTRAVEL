using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    SampleChara player;
    EnemyController boss;

    [SerializeField] Slider playerHpBar;       // プレイヤーのHPバー
    [SerializeField] Slider bossHpBar;         // ボスのHPバー
    [SerializeField] Slider expBar;            // 経験値バー
    [SerializeField] Text playerSliderText;    // プレイヤーの最大HPテキスト
    [SerializeField] Text bossSliderText;      // ボスの最大HPテキスト
    [SerializeField] Text levelText;           // レベルテキスト
    [SerializeField] Text pointText;           // ポイントテキスト
    [SerializeField] GameObject bossStatus;    // ボスのステータス
    [SerializeField] GameObject powerUpWindow; // ステータス強化ウィンドウ
    [SerializeField] GameObject bossWindow;    // ボス出現UI
    [SerializeField] float windowTime;         // ウィンドウが表示される秒数
    [SerializeField] List<Image> relicImages;

    int windowCnt = 0; // ウィンドウが表示できるカウント(一度だけ使う)

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameManager.Instance.Player.GetComponent<SampleChara>();
        
        playerHpBar.maxValue = player.MaxHP;
        playerSliderText.text = "" + playerHpBar.maxValue;
        expBar.maxValue = player.NextLvExp;
        levelText.text = "" + player.NowExp;
        expBar.value = player.NowExp;

        bossStatus.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // プレイヤーHPUI
        playerHpBar.maxValue = player.MaxHP;
        playerHpBar.value = player.HP;
        playerSliderText.text = player.HP + "/" + playerHpBar.maxValue;

        // 経験値・レベルUI
        expBar.maxValue = player.NextLvExp;
        levelText.text = "LV." + player.NowLv;
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
                    boss = GameManager.Instance.Boss.GetComponent<EnemyController>();
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

    public void DisplayRelic(Sprite relicSprite)
    {
        foreach(Image image in relicImages)
        {
            if (image.sprite == null)
            {
                image.color = new Color(1.0f,1.0f, 1.0f, 1.0f);
                image.sprite = relicSprite;
                break;
            }
        }
    }
}
