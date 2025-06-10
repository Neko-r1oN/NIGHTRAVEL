using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    SampleChara player;
    EnemyController boss;

    [SerializeField] Slider playerHpBar;       // プレイヤーのHPバー
    //[SerializeField] Slider bossHpBar;         // ボスのHPバー
    [SerializeField] Slider expBar;            // 経験値バー
    [SerializeField] Text playerSliderText;    // プレイヤーの最大HPテキスト
    //[SerializeField] Text bossSliderText;      // ボスの最大HPテキスト
    [SerializeField] Text levelText;           // レベルテキスト
    [SerializeField] Text pointText;           // ポイントテキスト
    [SerializeField] GameObject bossStatus;    // ボスのステータス
    [SerializeField] GameObject powerUpWindow; // ステータス強化ウィンドウ
    [SerializeField] GameObject bossWindow;    // ボス出現UI

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameManager.Instance.Player.GetComponent<SampleChara>();
        boss = GameManager.Instance.Boss.GetComponent<EnemyController>();
        
        playerHpBar.maxValue = player.Life;
        playerSliderText.text = "" + playerHpBar.maxValue;
        expBar.maxValue = player.NextLvExp;
        levelText.text = "" + player.NowExp;
        expBar.value = player.NowExp;

        bossStatus.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        playerHpBar.value = player.Life;
        playerSliderText.text = player.Life + "/" + playerHpBar.maxValue;

        expBar.maxValue = player.NextLvExp;
        levelText.text = "LV." + player.NowLv;
        expBar.value = player.NowExp;

        if (GameManager.Instance.IsSpawnBoss)
        {
            

            bossStatus.SetActive(true);
        }
    }
}
