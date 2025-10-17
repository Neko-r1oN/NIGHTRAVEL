using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Pixeye.Unity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class ResultManager : MonoBehaviour
{
    [Foldout("�e�L�X�g")]
    [SerializeField] Text jobText;                  // �E��
    [Foldout("�e�L�X�g")]                           
    [SerializeField] Text levelText;                // �Q�[�����x��
    [Foldout("�e�L�X�g")]                           
    [SerializeField] Text stageNumText;             // �U���X�e�[�W
    [Foldout("�e�L�X�g")]
    [SerializeField] Text arrivalLevelText;         // ���B���x��
    [Foldout("�e�L�X�g")]                           
    [SerializeField] Text survivalTimeText;         // ��������
    [Foldout("�e�L�X�g")]
    [SerializeField] Text totalExterminationText;   // ������
    [Foldout("�e�L�X�g")]                           
    [SerializeField] Text grantDamageText;          // ���t�^�_���[�W
    [Foldout("�e�L�X�g")]                           
    [SerializeField] Text receiveDamageText;        // ��_���[�W
    [Foldout("�e�L�X�g")]                           
    [SerializeField] Text totalEarningsItemText;    // ���l���A�C�e��
    [Foldout("�e�L�X�g")]
    [SerializeField] Text terminalStartupNumText;   // �[���N����
    [Foldout("�e�L�X�g")]
    [SerializeField] Text totalScore;               // ���X�R�A

    [SerializeField] GameObject ItemImage;
    [SerializeField] Image imagePrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //// �e�X�g�f�[�^
        //if (!RoomModel.Instance)
        //{
        //    ResultData resultData = new ResultData()
        //    {
        //        PlayerClass = EnumManager.Player_Type.Sword,
        //        GottenRelicList = new List<EnumManager.RELIC_TYPE>() {
        //        EnumManager.RELIC_TYPE.Firewall,
        //        EnumManager.RELIC_TYPE.Firewall,
        //        EnumManager.RELIC_TYPE.MoveSpeedTip,
        //        EnumManager.RELIC_TYPE.Firewall,
        //        EnumManager.RELIC_TYPE.CoolingFan},

        //        TotalClearStageCount = 3,
        //        DifficultyLevel = 2,
        //        AliveTime = new TimeSpan(0, 0, 600),
        //        EnemyKillCount = 20,
        //        TotalGaveDamage = 50,
        //        TotalGottenItem = 2,
        //        TotalActivedTerminal = 3,
        //        TotalScore = 30000
        //    };

        //    DisplayResultData(resultData);
        //}
        DisplayResultData(GameManager.Instance.ResultData);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// ���U���g��ʂ���^�C�g���Ɉړ�
    /// </summary>
    public void BackTitleButton()
    {
        //SceneManager.LoadScene("TitleScene");
        Initiate.DoneFading();
        Initiate.Fade("1_TitleScene", Color.black, 1.0f);   // �t�F�[�h����1�b
    }

    /// <summary>
    /// ���U���g��ʂ��烍�r�[�Ɉړ�
    /// </summary>
    public void BackLobbySceneButton()
    {
        Initiate.DoneFading();
        Initiate.Fade("PlayStandbyScene", Color.black, 1.0f);   // �t�F�[�h����1�b
    }

    /// <summary>
    /// ���U���g��ʍX�V����
    /// </summary>
    public void DisplayResultData(ResultData resultData)
    {
        jobText.text = resultData.PlayerClass.ToString();   // �v���C���[�̏��
        //levelText.text = resultData.DifficultyLevel.ToString();  

        // �Q�[���̓�Փx
        if (resultData.DifficultyLevel <= 5)
        {
            levelText.text =
                LevelManager.Instance.LevelName[(DIFFICULTY_TYPE)resultData.DifficultyLevel].ToString();
        }
        else
        {
            levelText.text = LevelManager.Instance.LevelName[DIFFICULTY_TYPE.Hell].ToString();
        }

        List<EnumManager.RELIC_TYPE> relics = new List<EnumManager.RELIC_TYPE>();

        foreach (var item in resultData.GottenRelicList)
        {
            if (!relics.Contains(item))
            {
                GameObject ChildObj =
            Instantiate(imagePrefab.gameObject, Vector3.zero, Quaternion.identity, ItemImage.transform);

                ChildObj.transform.localScale = ChildObj.transform.localScale;

                ChildObj.GetComponent<Image>().sprite =
                RelicManager.Instance.RelicSprites[(int)item - 1];
                relics.Add(item);
            }
        }

        stageNumText.text = resultData.TotalClearStageCount.ToString();           // �U���X�e�[�W��
        arrivalLevelText.text
            = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>().NowLv.ToString();
        survivalTimeText.text = resultData.AliveTime.ToString(@"mm\:ss");         // ��������
        totalExterminationText.text = resultData.EnemyKillCount.ToString();       // ��������
        grantDamageText.text = resultData.TotalGaveDamage.ToString();             // ���t�^�_���[�W��
        totalEarningsItemText.text = resultData.TotalGottenItem.ToString();       // ���l���A�C�e����
        terminalStartupNumText.text = resultData.TotalActivedTerminal.ToString(); // ���v�[���N����
        totalScore.text = resultData.TotalScore.ToString();
    }
}
