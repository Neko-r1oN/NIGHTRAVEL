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

    #region �eUI
    [Foldout("UI(�v���C���[�X�e�[�^�X�֘A)")]
    [SerializeField] Slider playerHpBar;          // �v���C���[��HP�o�[
    [Foldout("UI(�v���C���[�X�e�[�^�X�֘A)")]
    [SerializeField] Slider expBar;               // �o���l�o�[

    [Foldout("UI(�e�L�X�g)")]
    [SerializeField] Text playerSliderText;       // �v���C���[�̍ő�HP�e�L�X�g
    [Foldout("UI(�e�L�X�g)")]
    [SerializeField] Text bossSliderText;         // �{�X�̍ő�HP�e�L�X�g
    [Foldout("UI(�e�L�X�g)")]
    [SerializeField] Text levelText;              // ���x���e�L�X�g
    [Foldout("UI(�e�L�X�g)")]
    [SerializeField] Text pointText;              // �|�C���g�e�L�X�g
    [SerializeField] List<Image> relicImages;
    [Foldout("UI(�e�L�X�g)")]
    [SerializeField] List<Text> relicCntText;     // �����b�N�������Ă鐔��\������e�L�X�g
    //[Foldout("UI(�e�L�X�g)")]
    //[SerializeField] List<Text>  statusText;      // �X�e�[�^�X�A�b�v�����e�L�X�g
    [Foldout("UI(�e�L�X�g)")]
    [SerializeField] Text levelUpStock;           // ���x���A�b�v�X�g�b�N�e�L�X�g
    [Foldout("UI(�e�L�X�g)")]
    [SerializeField] Text levelUpText;            // �����\�e�L�X�g
    [Foldout("UI(�e�L�X�g)")]
    [SerializeField] Text clashNumText;           // ���j���e�L�X�g
    [Foldout("UI(�e�L�X�g)")]
    [SerializeField] Text tmText;                 // �N���A�����e�L�X�g

    [Foldout("UI(�t�F�[�h�A�E�g)")]
    [SerializeField] Canvas parentCanvas;         // �e�L�X�g���\�������L�����o�X�����蓖�ĂĂ�������
    [Foldout("UI(�t�F�[�h�A�E�g)")]
    [SerializeField] float fadeDuration = 2f;     // �e�L�X�g���t�F�[�h�A�E�g�ɂ����鎞�ԁi�b�j
    [Foldout("UI(�t�F�[�h�A�E�g)")]
    [SerializeField] float displayDuration = 1f;  // �e�L�X�g�����S�ɕ\������鎞�ԁi�t�F�[�h�J�n�܂Łj

    [Foldout("UI(�{�X�X�e�[�^�X�֘A)")]
    [SerializeField] Slider bossHpBar;            // �{�X��HP�o�[
    [Foldout("UI(�{�X�X�e�[�^�X�֘A)")]
    [SerializeField] GameObject bossStatus;       // �{�X�̃X�e�[�^�X

    [Foldout("UI(�E�B���h�E�֌W)")]
    [SerializeField] GameObject statusUpWindow;   // �X�e�[�^�X�����E�B���h�E
    [Foldout("UI(�E�B���h�E�֌W)")]
    [SerializeField] float windowTime;            // �E�B���h�E���\�������b��

    [Foldout("UI(�o�i�[�֌W)")]
    [SerializeField] GameObject bossWindow;       // �{�X�o��UI
    [Foldout("UI(�o�i�[�֌W)")]
    [SerializeField] GameObject termsBanner;      // �N���A�����o�i�[

    #endregion

    int windowCnt = 0;   // �E�B���h�E���\���ł���J�E���g(��x�����g��)
    int lastLevel = 0;   // ���x���A�b�v�O�̃��x��
    int statusStock = 0; // ���x���A�b�v�X�g�b�N��
    bool isStatusWindow; // �X�e�[�^�X�E�B���h�E���J���邩�ǂ���
    bool isHold;         // �X�e�[�^�X�E�B���h�E���b�N�p
    Text clashText;
    private Renderer[] childRenderers; // �q�I�u�W�F�N�g��Renderer�𕡐��Ή�
    private Text[] childTexts; // �q�I�u�W�F�N�g�̕W��UI.Text�𕡐��Ή�
    // �eRenderer��Text�̏����F��ۑ����邽�߂̃��X�g
    // �t�F�[�h�C�����Ɍ��̕s�����ȏ�Ԃɖ߂����߂ɕK�v
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
            // �C���X�^���X���������݂��Ȃ��悤�ɁA���ɑ��݂��Ă����玩�g����������
            Destroy(gameObject);
        }

        // �e�I�u�W�F�N�g�Ƃ��̎q����Renderer�����ׂĎ擾
        childRenderers = termsBanner.GetComponentsInChildren<Renderer>(true);
        // �e�I�u�W�F�N�g�Ƃ��̎q����W��UI.Text�����ׂĎ擾
        childTexts = termsBanner.GetComponentsInChildren<Text>(true);

        // �eRenderer�̃}�e���A���ݒ�Ə����F�̕ۑ�
        foreach (Renderer renderer in childRenderers)
        {
            if (renderer != null)
            {
                SetMaterialFadeMode(renderer.material);
                initialRendererColors.Add(renderer.material.color);
            }
        }

        // �eText�̏����F�̕ۑ�
        foreach (Text text in childTexts)
        {
            if (text != null)
            {
                initialTextColors.Add(text.color);
            }
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

        clashNumText.text = "����:0/" + GameManager.Instance.KnockTermsNum;

        tmText.text = "�N���A�����F5���Ԑ����c�� or �G"
            + GameManager.Instance.KnockTermsNum + "�̓|��";
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

    public void CountTermsText(int crashNum)
    {
        clashNumText.text = "����:" + crashNum + "/" + GameManager.Instance.KnockTermsNum;
    }

    /// <summary>
    /// �����o�i�[����莞�ԕ\�����A���̌�t�F�[�h�A�E�g�����܂��B
    /// �Q�[����Update�͒�~���܂���B
    /// </summary>
    public void ShowUIAndFadeOut()
    {
        StopAllCoroutines(); // ���Ɏ��s���̃R���[�`�����~
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
            termsBanner.SetActive(true); // UI�p�l����\��
        }

        // �eRenderer��Text�̃A���t�@�l�����S�ɕs�����ɐݒ�
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

        // �w�肳�ꂽ�\�����ԑҋ@ (Time.timeScale��1�Ȃ̂�WaitForSeconds��OK)
        yield return new WaitForSeconds(displayDuration);

        // �t�F�[�h�A�E�g����
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime; // Time.timeScale��1�Ȃ̂�deltaTime��OK
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration); // 1����0�֐��`���

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
            yield return null; // 1�t���[���ҋ@
        }

        // �t�F�[�h�A�E�g������̏��� (���S�ɓ����ɂ���)
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
            termsBanner.SetActive(false); // ���S�ɓ����ɂȂ�����UI�p�l�����\���ɂ���
        }
    }

    // Standard�V�F�[�_�[�œ����x���������߂̐ݒ�w���p�[���\�b�h
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
