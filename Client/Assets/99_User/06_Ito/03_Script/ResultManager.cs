using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Pixeye.Unity;
using Shared.Interfaces.StreamingHubs;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // �e�X�g�f�[�^
        ResultData resultData = new ResultData()
        {
            AliveTime = 200,
            Difficulty = 2,
            PlayerClass = EnumManager.Player_Type.Sword,
            EnemyKillCount = 20,
            TotalGaveDamage = 50,
            TotalReceivedDamage = 20,
            TotalGottenItem = 2,
            TotalActivedTerminal = 3
        };

        DisplayResultData(resultData);

        //DisplayResultData(GameManager.Instance.ResultData);
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
        Initiate.Fade("Title Ueno", Color.black, 1.0f);   // �t�F�[�h����1�b
    }

    /// <summary>
    /// ���U���g��ʂ��烍�r�[�Ɉړ�
    /// </summary>
    public void BackLobbySceneButton()
    {
        Initiate.Fade("PlayStandbyScene", Color.black, 1.0f);   // �t�F�[�h����1�b
    }

    /// <summary>
    /// ���U���g��ʍX�V����
    /// </summary>
    public void DisplayResultData(ResultData resultData)
    {
        jobText.text = resultData.PlayerClass.ToString();                         // �v���C���[�̏��
        levelText.text = resultData.Difficulty.ToString();                        // �Q�[���̓�Փx
        stageNumText.text = "3";                                                  // �U���X�e�[�W��
        arrivalLevelText.text = "�n�[�h";                                         // ���B���x��
        survivalTimeText.text = resultData.AliveTime.ToString();                  // ��������
        totalExterminationText.text = resultData.EnemyKillCount.ToString();       // ��������
        grantDamageText.text = resultData.TotalGaveDamage.ToString();             // ���t�^�_���[�W��
        receiveDamageText.text = resultData.TotalReceivedDamage.ToString();       // ��_���[�W��
        totalEarningsItemText.text = resultData.TotalGottenItem.ToString();       // ���l���A�C�e����
        terminalStartupNumText.text = resultData.TotalActivedTerminal.ToString(); // ���v�[���N����
        totalScore.text = "50000";
    }
}
