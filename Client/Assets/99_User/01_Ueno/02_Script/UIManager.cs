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
    [SerializeField] float windowTime;   // ウィンドウが表示される秒数

    int windowCnt = 0; // ウィンドウが表示できるカウント(一度だけ使う)

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
        playerHpBar.value = player.HP;
        playerSliderText.text = player.HP + "/" + playerHpBar.maxValue;

        expBar.maxValue = player.NextLvExp;
        levelText.text = "LV." + player.NowLv;
        expBar.value = player.NowExp;

        if (GameManager.Instance.IsSpawnBoss)
        {
            if (windowCnt <= 0)
            {
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
                {
                    boss = GameManager.Instance.Boss.GetComponent<EnemyController>();

                    bossHpBar.maxValue = boss.HP;
                    bossHpBar.value = boss.HP;
                    bossSliderText.text = "" + bossHpBar.maxValue;
                }
            }

            bossHpBar.value = boss.HP;
            if (boss.HP <= 0)
            {
                bossSliderText.text = "0/" + bossHpBar.maxValue;
            }
            else
            {
                bossSliderText.text = boss.HP + "/" + bossHpBar.maxValue;
            }

            bossStatus.SetActive(true);
        }
    }
}
