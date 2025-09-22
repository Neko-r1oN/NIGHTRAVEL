//----------------------------------------------------
// UI�Ǘ��N���X
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

    #region �eUI
    [Foldout("�L�����o�X")]
    [SerializeField] GameObject canvas;              // �L�����o�X
                                                     
    [Foldout("�v���C���[�X�e�[�^�X�֘A")]            
    [SerializeField] Slider playerHpBar;             // �v���C���[��HP�o�[
    [Foldout("�v���C���[�X�e�[�^�X�֘A")]            
    [SerializeField] Slider expBar;                  // �o���l�o�[
                                                     
    [Foldout("�e�L�X�g")]                            
    [SerializeField] Text playerSliderText;              // �v���C���[�̍ő�HP�e�L�X�g
    [Foldout("�e�L�X�g")]                                 
    [SerializeField] Text bossSliderText;                // �{�X�̍ő�HP�e�L�X�g
    [Foldout("�e�L�X�g")]                                 
    [SerializeField] Text levelText;                     // ���x���e�L�X�g
    [Foldout("�e�L�X�g")]                                 
    [SerializeField] Text pointText;                     // �|�C���g�e�L�X�g
    [Foldout("�e�L�X�g")]                                 
    [SerializeField] List<Text> relicCntText;            // �����b�N�������Ă鐔��\������e�L�X�g
    [Foldout("�e�L�X�g")]
    [SerializeField] List<Text> statusItemText;          // �X�e�[�^�X�A�b�v�����e�L�X�g
    [Foldout("�e�L�X�g")]
    [SerializeField] List<Text> statusExplanationsTexts; // �X�e�[�^�X�A�b�v�����e�L�X�g
    [Foldout("�e�L�X�g")]
    [SerializeField] Text levelUpStock;                  // ���x���A�b�v�X�g�b�N�e�L�X�g
    [Foldout("�e�L�X�g")]                                
    [SerializeField] Text levelUpText;                   // �����\�e�L�X�g
    [Foldout("�e�L�X�g")]                                
    [SerializeField] Text clashNumText;                  // ���j���e�L�X�g
    [Foldout("�e�L�X�g")]                                
    [SerializeField] Text tmText;                        // �N���A�����e�L�X�g
    [Foldout("�e�L�X�g")]                                
    [SerializeField] GameObject playerDmgText;           // �v���C���[�_���[�W�\�L
    [Foldout("�e�L�X�g")]                                
    [SerializeField] GameObject otherDmgText;            // ���̑��_���[�W�\�L
    [Foldout("�e�L�X�g")]                                
    [SerializeField] Text relicName;                     // �����b�N���e�L�X�g
    [Foldout("�e�L�X�g")]                                
    [SerializeField] Text diffText;                      // ��Փx�e�L�X�g
    [Foldout("�e�L�X�g")]                                
    [SerializeField] Text terminalExplanationText;       // �^�[�~�i�������e�L�X�g
    [Foldout("�e�L�X�g")]                                
    [SerializeField] Text spectatingNameText;            // �ϐ풆�v���C���[���e�L�X�g
    [Foldout("�e�L�X�g")]                                
    [SerializeField] GameObject healText;                // ���̑��_���[�W�\�L

    [Foldout("�t�F�[�h�A�E�g")]                      
    [SerializeField] Canvas parentCanvas;                // �e�L�X�g���\�������L�����o�X�����蓖�ĂĂ�������
    [Foldout("�t�F�[�h�A�E�g")]                          
    [SerializeField] float fadeDuration = 2f;            // �e�L�X�g���t�F�[�h�A�E�g�ɂ����鎞�ԁi�b�j
    [Foldout("�t�F�[�h�A�E�g")]                          
    [SerializeField] float displayDuration = 1f;         // �e�L�X�g�����S�ɕ\������鎞�ԁi�t�F�[�h�J�n�܂Łj
                                                         
    [Foldout("�{�X�֘A")]                                
    [SerializeField] Slider bossHpBar;                   // �{�X��HP�o�[
    [Foldout("�{�X�֘A")]                                
    [SerializeField] GameObject bossStatus;              // �{�X�̃X�e�[�^�X
                                                         
    [Foldout("�E�B���h�E�֌W")]                          
    [SerializeField] GameObject statusUpWindow;          // �X�e�[�^�X�����E�B���h�E
    [Foldout("�E�B���h�E�֌W")]                          
    [SerializeField] float windowTime;                   // �E�B���h�E���\�������b��
    [Foldout("�E�B���h�E�֌W")]                          
    [SerializeField] GameObject endWindow;               // �I���̃E�B���h�E
    [Foldout("�E�B���h�E�֌W")]                          
    [SerializeField] GameObject spectatingWindow;        // �ϐ�E�B���h�E
    [Foldout("�E�B���h�E�֌W")]                          
    [SerializeField] GameObject nextStageWindow;         // �X�e�[�W�J�ڃE�B���h�E

    [Foldout("�{�^��")]
    [SerializeField] Button resultYesButton;
    [Foldout("�{�^��")]
    [SerializeField] Button resultNoButton;
    [Foldout("�{�^��")]
    [SerializeField] Button changeGameYesButton;
    [Foldout("�{�^��")]
    [SerializeField] Button changeGameNoButton;

    [Foldout("�o�i�[�֌W")]                              
    [SerializeField] GameObject bossWindow;              // �{�X�o��UI
    [Foldout("�o�i�[�֌W")]                              
    [SerializeField] GameObject termsBanner;             // �N���A�����o�i�[
    [Foldout("�o�i�[�֌W")]                              
    [SerializeField] GameObject relicBanner;             // �擾���������b�N�o�i�[

    [Foldout("�����b�N�֘A")]
    [SerializeField] List<Image> relicImages;
    [Foldout("�����b�N�֘A")]
    [SerializeField] Image relicImg;                     // �����b�N�̃C���[�W

    [Foldout("���̑�")]
    [SerializeField] List<GameObject> playerStatus;      // �����ȊO�̃v���C���[�̃X�e�[�^�X
    [Foldout("���̑�")]
    [SerializeField] GameObject terminalExplanationObj;  // �^�[�~�i�������p�I�u�W�F�N�g
    [Foldout("���̑�")]                                  
    [SerializeField] GameObject statusUpButton;          // �X�e�[�^�X�A�b�v�{�^��

    [Foldout("�e���pUI")]
    [SerializeField] GameObject gamePadUI;               // �p�b�h����UI
    [Foldout("�e���pUI")]                              
    [SerializeField] GameObject keyBoardUI;              // �L�[�{�[�h����UI
    [Foldout("�e���pUI")]                              
    [SerializeField] GameObject swordSkillUI;            // ���m�X�L��UI
    [Foldout("�e���pUI")]                              
    [SerializeField] GameObject gunnerSkillUI;           // �K���i�[�X�L��UI
                                                         
    [Foldout("ACT�A�C�R��UI")]                           
    [SerializeField] Image skillCoolDownImage;           // �X�L���N�[���_�E��
    [Foldout("ACT�A�C�R��UI")]                           
    [SerializeField] Image blinkCoolDownImage;           // �u�����N�N�[���_�E��
    [Foldout("ACT�A�C�R��UI")]                           
    [SerializeField] RectTransform[] swordIconObjs;      // ���A�C�R���I�u�W�F�ꗗ
    [Foldout("ACT�A�C�R��UI")]                           
    [SerializeField] Image[] swordIconImages;            // ���A�C�R���摜�ꗗ
    [Foldout("ACT�A�C�R��UI")]                           
    [SerializeField] RectTransform[] gunIconObjs;        // �e�A�C�R���I�u�W�F�ꗗ
    [Foldout("ACT�A�C�R��UI")]                           
    [SerializeField] Image[] gunIconImages;              // �e�A�C�R���摜�ꗗ
    [Foldout("ACT�A�C�R��UI")]                           
    [SerializeField] GameObject gunSkillLockObj;         // �u�����N�N�[���_�E��
                                                         
    private bool isInputGamePad;                         // �Q�[���p�b�h���͂��ǂ���

    public bool IsInputGamePad { get { return isInputGamePad; } }

    #endregion

    // �萔
    private const float pushIconScale = 0.98f; // �L�[�������̃A�C�R���k����
    private const float pushIconColor = 0.8f;  // �L�[�������̃A�C�R���F�ω���

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

    // �^�[�~�i���N�����̒�^��
    private Dictionary<TERMINAL_TYPE, string> terminalExplanation = new Dictionary<TERMINAL_TYPE, string>
    {
        {TERMINAL_TYPE.None,""},
        {TERMINAL_TYPE.Enemy,"�o�������G��S�ē|��" },
        {TERMINAL_TYPE.Speed,"�o�������Q�[�g��S�Ēʂ�" },
        {TERMINAL_TYPE.Deal,"" },
        {TERMINAL_TYPE.Jumble,"" },
        {TERMINAL_TYPE.Elite,"�o�������G���[�g�G��S�ē|��" },
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

        clashNumText.text = "����:0/" + SpawnManager.Instance.KnockTermsNum;

        tmText.text = "�N���A�����F5���Ԑ����c�� or �G"
            + SpawnManager.Instance.KnockTermsNum + "�̓|��";

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

        // �L�����̃W���u����UI��ύX
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
    /// �X�V����
    /// </summary>
    void Update()
    {
        // ����UI�ύX����
        InputChangeUI();
        
        if (player == null)
        {
            player = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>();
        }

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
        expBar.value = (float)player.NowExp;

        if (SpawnManager.Instance.IsSpawnBoss)
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
    /// �{�XUI�\��
    /// </summary>
    public void DisplayBossUI()
    {// �{�X���X�|�[������
        boss = CharacterManager.Instance.GetBossObject();
        // �{�X�X�e�[�^�XUI
        bossHpBar.maxValue = boss.BaseHP;
        bossHpBar.value = boss.BaseHP;
        bossSliderText.text = "" + bossHpBar.maxValue;

        //if (boss.HP <= 0)
        //{// �{�X��HP�\�����}�C�i�X�ɂȂ�Ȃ��悤�ɂ���
        //    bossHpBar.value = 0;
        //}

        bossSliderText.text = bossHpBar.value + "/" + bossHpBar.maxValue;

        bossStatus.SetActive(true);

        clashNumText.enabled = false;
    }

    /// <summary>
    /// �{�XUI��\��
    /// </summary>
    public void HideBossUI()
    {
        bossStatus.SetActive(false);
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
    /// �������b�N�̏������X�V
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
                        relicCntText[count].text = "�~" + num;
                    }
                }
            }
            count++;
        }

        GetRelicBanner(relicSprite);
    }

    /// <summary>
    /// �X�e�[�^�X�����E�B���h�E�\��
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
    /// �X�e�[�^�X�̋���
    /// </summary>
    public async void UpPlayerStatus(int buttonId)
    {
        // �I�������X�e�[�^�X�����I�������擾���č폜
        var values = LevelManager.Instance.Options.FirstOrDefault().Value;
        var key = LevelManager.Instance.Options.FirstOrDefault().Key;
        LevelManager.Instance.Options.Remove(key);

        

        if (!RoomModel.Instance)
        {// �I�t���C��
            //�X�e�[�^�X�ύX
            UpStatusChange();
        }
        else
        {// �I�����C��
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
            // �X�e�[�^�X�����e�L�X�g�̍X�V
            ChangeUpStatusText();
        }

        // �X�e�[�^�X�����񐔂̌���
        statusStock--;

        levelUpStock.text = "�c�苭�����F" + statusStock;

        if (statusStock <= 0)
        {// �����X�g�b�N��0�̏ꍇ
            CloseStatusWindow();
            isStatusWindow = false;
            levelUpText.enabled = false;
        }
    }

    [ContextMenu("UpStatusChange")]
    /// <summary>
    /// �X�e�[�^�X�ύX���ڂ�ύX
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

    /// <summary>
    /// �{�X�����̃e�L�X�g�X�V
    /// </summary>
    /// <param name="crashNum"></param>
    public void CountTermsText(int crashNum)
    {
        clashNumText.text = "����:" + crashNum + "/" + SpawnManager.Instance.KnockTermsNum;
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
        else if (SpawnManager.Instance.IsSpawnBoss)
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
    /// �񕜕\�L����
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

    /// <summary>
    /// �Q�[�������x���A�b�v
    /// </summary>
    public void UpGameLevelText()
    {
        if(level.GameLevel > level.LevelHellId)
        {
            return;
        }
             
        diffText.text = level.LevelName[(DIFFICULTY_TYPE)level.GameLevel].ToString();

        // text�̐F�ύX
        switch (level.GameLevel)
        {// Baby��start�Őݒ�ς݂̂��ߏȂ�

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
    /// �^�[�~�i���̐������\���E�ύX����
    /// </summary>
    /// <param name="type"></param>
    public void DisplayTerminalExplanation(TERMINAL_TYPE type)
    {
        terminalExplanationObj.SetActive(true);

        terminalExplanationText.text = terminalExplanation[type].ToString();
    }

    /// <summary>
    /// �^�[�~�i����������\���E�e�L�X�g��߂�
    /// </summary>
    public void DisplayTimeInstructions()
    {
        terminalExplanationObj.SetActive(false);
        TimerDirector.Instance.TimerObj.transform.GetChild(0).GetComponent<Text>().text = " �G�q�V�X�e�������܂�";
    }

    /// <summary>
    /// �ϐ��ʗpUI�̍X�V
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
    /// �S�v���C���[�����S�������m�F���鏈��
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
    /// �N�[���_�E���\��
    /// </summary>
    /// <param name="skillFlag">true:skill false;blink</param>
    /// <param name="coolTime">�N�[���^�C��</param>
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
    /// ����UI�ω�����
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
    /// �L�������̃X�L��UI�ω�
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
    /// ���͎���UI�ύX
    /// </summary>
    private void InputChangeUI()
    {
        // �K���i�[�X�L�����b�N����
        if (!player.GetGrounded() && player.PlayerType == Player_Type.Gunner)
        {
            gunSkillLockObj.SetActive(true);
        }
        else
        {
            gunSkillLockObj.SetActive(false);
        }

        // �L�[�{�[�h or �Q�[���p�b�h�̓��͂�UI�ω�
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            ChangeOperationUI("Keyboard");
        }

        if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Blink") || Input.GetButtonDown("Attack1") || Input.GetButtonDown("Attack2"))
        {
            ChangeOperationUI("Gamepad");
        }

        // �L�[���� or �{�^�����͂�UI���A�N�V����

        // �ʏ�U��
        if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Attack1"))
        {
            if (player.PlayerType == Player_Type.Sword)
            {   // ���m�A�C�R��
                swordIconObjs[0].localScale = new Vector3(pushIconScale, pushIconScale, pushIconScale);
                swordIconImages[0].color = new Color(pushIconColor, pushIconColor, pushIconColor, 1.0f);
            }
            else
            {   // �K���i�[�A�C�R��
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
        // �X�L���U��
        if (Input.GetMouseButtonDown(1) || Input.GetButtonDown("Attack2"))
        {
            if (player.PlayerType == Player_Type.Sword)
            {   // ���m�A�C�R��
                swordIconObjs[1].localScale = new Vector3(pushIconScale, pushIconScale, pushIconScale);
                swordIconImages[1].color = new Color(pushIconColor, pushIconColor, pushIconColor, 1.0f);
            }
            else
            {   // �K���i�[�A�C�R��
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
        // �u�����N
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("Blink"))
        {
            if (player.PlayerType == Player_Type.Sword)
            {   // ���m�A�C�R��
                swordIconObjs[2].localScale = new Vector3(pushIconScale, pushIconScale, pushIconScale);
                swordIconImages[2].color = new Color(pushIconColor, pushIconColor, pushIconColor, 1.0f);
            }
            else
            {   // �K���i�[�A�C�R��
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
        // �W�����v
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump"))
        {
            if (player.PlayerType == Player_Type.Sword)
            {   // ���m�A�C�R��
                swordIconObjs[3].localScale = new Vector3(pushIconScale, pushIconScale, pushIconScale);
                swordIconImages[3].color = new Color(pushIconColor, pushIconColor, pushIconColor, 1.0f);
            }
            else
            {   // �K���i�[�A�C�R��
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
    /// ���̉�ʂɈړ����邩�m�F����E�B���h�E�̕\��
    /// </summary>
    public void DisplayNextStageWindow()
    {
        nextStageWindow.SetActive(true);
    }

    /// <summary>
    /// �Q�[���I���m�F�E�B���h�E�\��
    /// </summary>
    public void DisplayEndGameWindow()
    {
        endWindow.SetActive(true);
    }

    /// <summary>
    /// �Q�[���I���{�^��
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
    /// ���X�e�[�W�ړ��̃{�^������
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
