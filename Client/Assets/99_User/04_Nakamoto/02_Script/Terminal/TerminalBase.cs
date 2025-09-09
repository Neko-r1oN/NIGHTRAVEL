//--------------------------------------------------------------
// �^�[�~�i���e�N���X [ TerminalBase.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using DG.Tweening;
using DG.Tweening.Core.Easing;
using KanKikuchi.AudioManager;
using Shared.Interfaces.StreamingHubs;
using System.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class TerminalBase : MonoBehaviour
{
    //--------------------------------
    // �t�B�[���h

    #region �V�X�e���ݒ�

    // �v���C���[���[���ɐG��Ă��邩�̔���ϐ�
    private bool isPlayerIn = false;

    // �N������
    private bool isUsed = false;

    // �[��ID
    private int terminalID = 0;

    // �[���̎��
    protected EnumManager.TERMINAL_TYPE terminalType;

    // ��������
    [SerializeField] protected int limitTime = 25;

    // �J�E���g�_�E��
    private int currentTime;

    #endregion

    #region �O���Q�Ɨp�v���p�e�B

    /// <summary>
    /// �[��ID
    /// </summary>
    public int TerminalID { get { return terminalID; } set { terminalID = value; } }

    /// <summary>
    /// �[���̎��
    /// </summary>
    public EnumManager.TERMINAL_TYPE TerminalType { get { return terminalType; } }

    #endregion

    #region UI�֘A

    // �^�C�}�[�\���p�e�L�X�g
    [SerializeField] private Text timerText;

    // �[���̃A�C�R��
    [SerializeField] private SpriteRenderer terminalIcon;

    #endregion

    #region �}�l�[�W���[

    private GameManager gameManager;
    private SpawnManager spawnManager;
    private UIManager uiManager;
    private RelicManager relicManager;
    private CharacterManager characterManager;
    private PlayerBase player;

    #endregion

    //--------------------------------
    // ���\�b�h

    #region ���ʏ���

    /// <summary>
    ///  ��������
    /// </summary>
    private void Start()
    {
        gameManager = GameManager.Instance;
        uiManager = UIManager.Instance;
        spawnManager = SpawnManager.Instance;
        relicManager = RelicManager.Instance;
        player = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>();

        currentTime = limitTime;    // �������Ԃ��Z�b�g
    }

    /// <summary>
    /// �X�V����
    /// </summary>
    private void Update()
    {
        // E�L�[���͂��v���C���[���[���ɐG��Ă���ꍇ�����̒[�������g�p�ł���ꍇ�A�[�����N��
        if (Input.GetKeyDown(KeyCode.E) && isUsed == false && isPlayerIn == true)
        {
            Debug.Log("Terminal Booted");
            BootTerminal(); // �[�����N��
        }
    }

    ///--------------------------------
    /// �ڐG����
    private void OnTriggerEnter2D(Collider2D collision)
    {// �v���C���[���[���ɐG�ꂽ�ꍇ
        if (collision.transform.tag == "Player")
        {
            isPlayerIn = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {// �v���C���[���[�����痣�ꂽ�ꍇ
        if (collision.transform.tag == "Player")
        {
            isPlayerIn = false;
        }
    }

    /// <summary>
    /// �J�E���g�_�E������
    /// </summary>
    /// <returns></returns>
    protected IEnumerator Countdown()
    {
        while (currentTime > 0)
        {
            timerText.text = currentTime.ToString();
            yield return new WaitForSeconds(1f);
            currentTime--;
        }

        timerText.text = "0"; // �Ō��0��\��

        // �^�C���A�b�v���̏����������ɋL�q
        TimeUp();
    }

    #endregion

    #region �[�����ɏ�������

    /// <summary>
    /// �[���N������
    /// </summary>
    public virtual void BootTerminal()
    {
        isUsed = true; // �N���ς݂ɂ���
    }

    /// <summary>
    /// ��V�r�o����
    /// </summary>
    public virtual void GiveReward()
    {
        // �����b�N��r�o����
        RelicManager.Instance.GenerateRelic(gameObject);

        // �^�[�~�i����\��
        terminalIcon.DOFade(0, 1.5f);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// ���Ԑ؂�
    /// </summary>
    public virtual void TimeUp()
    {
        // �^�C���A�b�v���̏����������ɋL�q
        Debug.Log("Time Up!");

        // �^�[�~�i����\��
        terminalIcon.DOFade(0, 1.5f);
        gameObject.SetActive(false);
    }

    #endregion
}