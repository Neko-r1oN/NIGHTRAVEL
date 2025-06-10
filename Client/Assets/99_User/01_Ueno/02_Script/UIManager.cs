using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    SampleChara player;
    EnemyController boss;

    [SerializeField] Slider playerHpBar;       // �v���C���[��HP�o�[
    //[SerializeField] Slider bossHpBar;         // �{�X��HP�o�[
    [SerializeField] Slider expBar;            // �o���l�o�[
    [SerializeField] Text playerSliderText;    // �v���C���[�̍ő�HP�e�L�X�g
    //[SerializeField] Text bossSliderText;      // �{�X�̍ő�HP�e�L�X�g
    [SerializeField] Text levelText;           // ���x���e�L�X�g
    [SerializeField] Text pointText;           // �|�C���g�e�L�X�g
    [SerializeField] GameObject bossStatus;    // �{�X�̃X�e�[�^�X
    [SerializeField] GameObject powerUpWindow; // �X�e�[�^�X�����E�B���h�E
    [SerializeField] GameObject bossWindow;    // �{�X�o��UI

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
