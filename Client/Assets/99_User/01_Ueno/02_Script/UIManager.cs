using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    SampleChara player;

    [SerializeField] Slider hpBar;    // HP�o�[
    [SerializeField] Slider expBar;   // �o���l�o�[
    [SerializeField] Text sliderText; // �ő�HP�e�L�X�g
    [SerializeField] Text levelText;  // ���x���e�L�X�g
    [SerializeField] Text pointText;  // �|�C���g�e�L�X�g

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameManager.Instance.Player.GetComponent<SampleChara>();
        
        hpBar.maxValue = player.life;
        sliderText.text = "" + hpBar.maxValue;
        expBar.maxValue = player.nextLvExp;
        levelText.text = "" + player.nowLv;
        expBar.value = player.nowExp;
    }

    // Update is called once per frame
    void Update()
    {
        hpBar.value = player.life;
        sliderText.text = player.life + "/" + hpBar.maxValue;

        expBar.maxValue = player.nextLvExp;
        levelText.text = "LV." + player.nowLv;
        expBar.value = player.nowExp;
    }
}
