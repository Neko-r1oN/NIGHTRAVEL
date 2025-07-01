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

    #region �eUI
    [SerializeField] Slider playerHpBar;        // �v���C���[��HP�o�[
    [SerializeField] Slider bossHpBar;          // �{�X��HP�o�[
    [SerializeField] Slider expBar;             // �o���l�o�[
    [SerializeField] Text playerSliderText;     // �v���C���[�̍ő�HP�e�L�X�g
    [SerializeField] Text bossSliderText;       // �{�X�̍ő�HP�e�L�X�g
    [SerializeField] Text levelText;            // ���x���e�L�X�g
    [SerializeField] Text pointText;            // �|�C���g�e�L�X�g
    [SerializeField] GameObject bossStatus;     // �{�X�̃X�e�[�^�X
    [SerializeField] GameObject statusUpWindow; // �X�e�[�^�X�����E�B���h�E
    [SerializeField] GameObject bossWindow;     // �{�X�o��UI
    [SerializeField] float windowTime;          // �E�B���h�E���\�������b��
    [SerializeField] List<Image> relicImages;
    [SerializeField] List<Text> relicCntText;  // �����b�N�������Ă鐔��\������e�L�X�g
    //[SerializeField] List<Text>  statusText;    // �X�e�[�^�X�A�b�v�����e�L�X�g
    [SerializeField] Text levelUpStock;         // ���x���A�b�v�X�g�b�N�e�L�X�g
    [SerializeField] Text levelUpText;          // �����\�e�L�X�g
    [SerializeField] Text clashNumText;         // ���j���e�L�X�g
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float fadeDuration;
    #endregion

    int windowCnt = 0;   // �E�B���h�E���\���ł���J�E���g(��x�����g��)
    int lastLevel = 0;   // ���x���A�b�v�O�̃��x��
    int statusStock = 0; // ���x���A�b�v�X�g�b�N��
    bool isStatusWindow; // �X�e�[�^�X�E�B���h�E���J���邩�ǂ���
    bool isHold;         // �X�e�[�^�X�E�B���h�E���b�N�p



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
            // �C���X�^���X���������݂��Ȃ��悤�ɁA���ɑ��݂��Ă����玩�g����������
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// �����ݒ�
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
    /// �X�V����
    /// </summary>
    void Update()
    {
        // �v���C���[HPUI
        playerHpBar.maxValue = player.MaxHP;
        playerHpBar.value = player.HP;
        playerSliderText.text = player.HP + "/" + playerHpBar.maxValue;

        // �o���l�E���x��UI
        expBar.maxValue = player.NextLvExp;
        levelText.text = "LV." + player.NowLv;
        if (player.NowLv > lastLevel)
        {
            isStatusWindow = true;
            statusStock += player.NowLv - lastLevel;
            levelUpStock.text = "�c�苭�����F" + statusStock;
            levelUpText.enabled = true;
            lastLevel = player.NowLv;
        }
        expBar.value = player.NowExp;

        if (GameManager.Instance.IsSpawnBoss)
        {// �{�X���X�|�[������
            if (windowCnt <= 0)
            {// �E�B���h�E�������o�Ă��Ȃ��Ƃ�
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
                {// �{�X��null�̂Ƃ�
                    boss = GameManager.Instance.Boss.GetComponent<EnemyBase>();
                    // �{�X�X�e�[�^�XUI
                    bossHpBar.maxValue = boss.HP;
                    bossHpBar.value = boss.HP;
                    bossSliderText.text = "" + bossHpBar.maxValue;
                }
            }

            bossHpBar.value = boss.HP;

            if (boss.HP <= 0)
            {// �{�X��HP�\�����}�C�i�X�ɂȂ�Ȃ��悤�ɂ���
                bossHpBar.value = 0;
            }

            bossSliderText.text = bossHpBar.value + "/" + bossHpBar.maxValue;

            bossStatus.SetActive(true);
        }
    }

    /// <summary>
    /// �擾���������b�N��\������֐�
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
    /// �����b�N����ւ��̍ۂɎ����Ă��郌���b�NUI�����Z�b�g����֐�
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
                        relicCntText[count].text = "�~" + num;
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
    /// �X�e�[�^�X�̋���
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
    /// �X�e�[�^�X�ύX���ڂ�ύX
    /// </summary>
    public void UpStatusChange()
    {

    }

    /// <summary>
    /// �X�e�[�^�X�����E�B���h�E���b�N
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
    /// �X�e�[�^�X�E�B���h�E����
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
