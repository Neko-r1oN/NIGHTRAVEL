//----------------------------------------------------
// UI�Ǘ��N���X
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

    #region �eUI
    [Foldout("�L�����o�X")]
    [SerializeField] GameObject canvas;             // �L�����o�X
                                                    
    [Foldout("�v���C���[�X�e�[�^�X�֘A")]          
    [SerializeField] Slider playerHpBar;            // �v���C���[��HP�o�[
    [Foldout("�v���C���[�X�e�[�^�X�֘A")]           
    [SerializeField] Slider expBar;                 // �o���l�o�[
                                                    
    [Foldout("�e�L�X�g")]                           
    [SerializeField] Text playerSliderText;         // �v���C���[�̍ő�HP�e�L�X�g
    [Foldout("�e�L�X�g")]                           
    [SerializeField] Text bossSliderText;           // �{�X�̍ő�HP�e�L�X�g
    [Foldout("�e�L�X�g")]                           
    [SerializeField] Text levelText;                // ���x���e�L�X�g
    [Foldout("�e�L�X�g")]                           
    [SerializeField] Text pointText;                // �|�C���g�e�L�X�g
    [SerializeField] List<Image> relicImages;       
    [Foldout("�e�L�X�g")]                           
    [SerializeField] List<Text> relicCntText;       // �����b�N�������Ă鐔��\������e�L�X�g
    //[Foldout("UI(�e�L�X�g)")]                     
    //[SerializeField] List<Text>  statusText;        // �X�e�[�^�X�A�b�v�����e�L�X�g
    [Foldout("�e�L�X�g")]                           
    [SerializeField] Text levelUpStock;             // ���x���A�b�v�X�g�b�N�e�L�X�g
    [Foldout("�e�L�X�g")]
    [SerializeField] Text levelUpText;              // �����\�e�L�X�g
    [Foldout("�e�L�X�g")]                           
    [SerializeField] Text clashNumText;             // ���j���e�L�X�g
    [Foldout("�e�L�X�g")]                           
    [SerializeField] Text tmText;                   // �N���A�����e�L�X�g
    [Foldout("�e�L�X�g")]                           
    [SerializeField] GameObject playerDmgText;      // �v���C���[�_���[�W�\�L
    [Foldout("�e�L�X�g")]                           
    [SerializeField] GameObject otherDmgText;       // ���̑��_���[�W�\�L
    [Foldout("�e�L�X�g")]                           
    [SerializeField] Text relicName;                // ���̑��_���[�W�\�L
    [Foldout("�e�L�X�g")]                           
    [SerializeField] Text diffText;                 // ��Փx�e�L�X�g
                                                    
                                                    
    [Foldout("�t�F�[�h�A�E�g")]                     
    [SerializeField] Canvas parentCanvas;           // �e�L�X�g���\�������L�����o�X�����蓖�ĂĂ�������
    [Foldout("�t�F�[�h�A�E�g")]                     
    [SerializeField] float fadeDuration = 2f;       // �e�L�X�g���t�F�[�h�A�E�g�ɂ����鎞�ԁi�b�j
    [Foldout("�t�F�[�h�A�E�g")]                     
    [SerializeField] float displayDuration = 1f;    // �e�L�X�g�����S�ɕ\������鎞�ԁi�t�F�[�h�J�n�܂Łj

    [Foldout("�{�X�X�e�[�^�X�֘A")]
    [SerializeField] Slider bossHpBar;              // �{�X��HP�o�[
    [Foldout("�{�X�X�e�[�^�X�֘A")]                 
    [SerializeField] GameObject bossStatus;         // �{�X�̃X�e�[�^�X
                                                    
    [Foldout("�E�B���h�E�֌W")]                     
    [SerializeField] GameObject statusUpWindow;     // �X�e�[�^�X�����E�B���h�E
    [Foldout("�E�B���h�E�֌W")]                     
    [SerializeField] float windowTime;              // �E�B���h�E���\�������b��

    [Foldout("�o�i�[�֌W")]
    [SerializeField] GameObject bossWindow;         // �{�X�o��UI
    [Foldout("�o�i�[�֌W")]                         
    [SerializeField] GameObject termsBanner;        // �N���A�����o�i�[
    [Foldout("�o�i�[�֌W")]                         
    [SerializeField] GameObject relicBanner;        // �擾���������b�N�o�i�[

    [SerializeField] List<GameObject> playerStatus; // �����ȊO�̃v���C���[�̃X�e�[�^�X

    [SerializeField] Image relicImg;                // �����b�N�̃C���[�W

    #endregion

    int windowCnt = 0;   // �E�B���h�E���\���ł���J�E���g(��x�����g��)
    int lastLevel = 0;   // ���x���A�b�v�O�̃��x��
    int statusStock = 0; // ���x���A�b�v�X�g�b�N��
    bool isStatusWindow; // �X�e�[�^�X�E�B���h�E���J���邩�ǂ���
    bool isHold;         // �X�e�[�^�X�E�B���h�E���b�N�p
    string colorCode;    // �J���[�R�[�h
    Color color;
    private Renderer[] childRenderers; // �q�I�u�W�F�N�g��Renderer�𕡐��Ή�
    private Text[] childTexts; // �q�I�u�W�F�N�g�̕W��UI.Text�𕡐��Ή�
    // �eRenderer��Text�̏����F��ۑ����邽�߂̃��X�g
    // �t�F�[�h�C�����Ɍ��̕s�����ȏ�Ԃɖ߂����߂ɕK�v
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

        clashNumText.text = "����:0/" + GameManager.Instance.KnockTermsNum;

        tmText.text = "�N���A�����F5���Ԑ����c�� or �G"
            + GameManager.Instance.KnockTermsNum + "�̓|��";

        level = LevelManager.Instance;

        diffText.text = level.LevelName[level.GameLevel].ToString();
        colorCode = "#ffb6c1";

        if(ColorUtility.TryParseHtmlString(colorCode,out color))
        {
            diffText.color = color;
        }
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

                boss = GameManager.Instance.Boss.GetComponent<EnemyBase>();
                // �{�X�X�e�[�^�XUI
                bossHpBar.maxValue = boss.HP;
            }
            
            bossHpBar.value = boss.HP;
            bossSliderText.text = "" + bossHpBar.maxValue;

            if (boss.HP <= 0)
            {// �{�X��HP�\�����}�C�i�X�ɂȂ�Ȃ��悤�ɂ���
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
                    isRelicGet = true;
                    ShowUIAndFadeOut();
                    GetRelicBanner(relicSprite);
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
    /// �X�e�[�^�X�̋���
    /// </summary>
    public void UpPlayerStatus(int statusID)
    {
        //�X�e�[�^�X�ύX

        if (statusStock <= 0)
        {// �����X�g�b�N��0�̏ꍇ
            CloseStatusWindow();
            isStatusWindow = false;
        }
    }

    [ContextMenu("UpStatusChange")]
    /// <summary>
    /// �X�e�[�^�X�ύX���ڂ�ύX
    /// </summary>
    public void UpStatusChange()
    {
        player.ApplyStatusModifierByRate(0.15f, STATUS_TYPE.Power);
    }

    /// <summary>
    /// �X�e�[�^�X�����E�B���h�E���b�N
    /// </summary>
    public void HoldStatusWindow()
    {
        if (!isHold)
        {// �z�[���h��ԂłȂ��Ƃ�
            // �z�[���h�ɂ���
            isHold = true;
        }
        else
        {// �z�[���h��Ԃ̂Ƃ�
            // �z�[���h����
            isHold = false;
        }
    }

    /// <summary>
    /// �X�e�[�^�X�E�B���h�E����
    /// </summary>
    public void CloseStatusWindow()
    {
        if (!isHold)
        {// �z�[���h��ԂłȂ��Ƃ�
            // �E�B���h�E�����
            statusUpWindow.SetActive(false);
        }
    }

    public void CountTermsText(int crashNum)
    {
        clashNumText.text = "����:" + crashNum + "/" + GameManager.Instance.KnockTermsNum;
    }

    /// <summary>
    /// �����o�i�[����莞�ԕ\�����A���̌�t�F�[�h�A�E�g
    /// </summary>
    public void ShowUIAndFadeOut()
    {
        StopAllCoroutines(); // ���Ɏ��s���̃R���[�`�����~
        StartCoroutine(FadeSequence());
    }

    /// <summary>
    /// �t�F�[�h�A�E�g����
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
            termsBanner.SetActive(false); // ���S�ɓ����ɂȂ�����UI�p�l�����\���ɂ���
        }
    }
    

    /// <summary>
    /// Standard�V�F�[�_�[�œ����x���������߂̐ݒ�w���p�[���\�b�h
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
    /// �_���[�W�\�L����
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
    /// �擾���������b�N���o�i�[�ŕ\��
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
    /// �����b�N�C���[�W�����ɖ߂�
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

        // text�̐F�ύX
        switch (level.GameLevel)
        {// Baby��start�Őݒ�ς݂̂��ߏȂ�

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
