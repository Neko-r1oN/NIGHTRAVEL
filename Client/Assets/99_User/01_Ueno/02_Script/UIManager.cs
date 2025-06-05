using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    SampleChara player;

    [SerializeField] Slider hpBar;    // HPバー
    [SerializeField] Slider expBar;   // 経験値バー
    [SerializeField] Text sliderText; // 最大HPテキスト
    [SerializeField] Text levelText;  // レベルテキスト
    [SerializeField] Text pointText;  // ポイントテキスト

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameManager.Instance.Player.GetComponent<SampleChara>();
        
        hpBar.maxValue = player.HP;
        sliderText.text = "" + hpBar.maxValue;
        expBar.maxValue = player.NextLvExp;
        levelText.text = "" + player.NowExp;
        expBar.value = player.NowExp;
    }

    // Update is called once per frame
    void Update()
    {
        hpBar.value = player.HP;
        sliderText.text = player.HP + "/" + hpBar.maxValue;

        expBar.maxValue = player.NextLvExp;
        levelText.text = "LV." + player.NowLv;
        expBar.value = player.NowExp;
    }
}
