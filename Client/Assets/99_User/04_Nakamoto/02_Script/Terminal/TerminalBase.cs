//--------------------------------------------------------------
// �^�[�~�i���e�N���X [ TerminalBase.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using DG.Tweening;
using DG.Tweening.Core.Easing;
using KanKikuchi.AudioManager;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public abstract class TerminalBase : MonoBehaviour
{
    //--------------------------------
    // �t�B�[���h

    #region �V�X�e���ݒ�

    // �v���C���[���[���ɐG��Ă��邩�̔���ϐ�
    private bool isPlayerIn = false;

    // �N������
    protected bool isUsed = false;

    // �[��ID
    protected int terminalID = 0;

    // �[���̎��
    protected EnumManager.TERMINAL_TYPE terminalType;

    // �J�E���g�_�E��
    protected int currentTime;

    // �G������
    protected const int SPAWN_ENEMY_NUM = 5;

    #endregion

    #region �O���Q�Ɨp�v���p�e�B

    /// <summary>
    /// �[��ID
    /// </summary>
    public int TerminalID { get { return terminalID; } set { terminalID = value; } }

    /// <summary>
    /// �[���̎��
    /// </summary>
    public EnumManager.TERMINAL_TYPE TerminalType { get { return terminalType; } set { terminalType = value; } }

    #endregion

    #region �O���ݒ�

    // �^�C�}�[�\���p�e�L�X�g
    [SerializeField] protected Text timerText;

    // �[���X�v���C�g
    [SerializeField] protected SpriteRenderer terminalSprite;

    // �A�C�R���X�v���C�g
    [SerializeField] protected SpriteRenderer iconSprite;

    // ��������
    [SerializeField] protected int limitTime = 40;

    // �����b�N�����ʒu
    [SerializeField] protected Transform[] relicSpawnPoints;

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
    protected virtual void Start()
    {
        gameManager = GameManager.Instance;
        spawnManager = SpawnManager.Instance;
        uiManager = UIManager.Instance;
        relicManager = RelicManager.Instance;
        characterManager = CharacterManager.Instance;
        player = characterManager.PlayerObjSelf.GetComponent<PlayerBase>();

        currentTime = limitTime;    // �������Ԃ��Z�b�g
    }

    /// <summary>
    /// �X�V����
    /// </summary>
    protected void Update()
    {
        // E�L�[���͂��v���C���[���[���ɐG��Ă���ꍇ�����̒[�������g�p�ł���ꍇ�A�[�����N��
        if (Input.GetKeyDown(KeyCode.E) && isUsed == false && isPlayerIn == true)
        {
            Debug.Log("Terminal Booted");
            BootRequest(); // �[�����N��
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
    public void CountDown()
    {
        //limitTIme��1�����炷
        currentTime--;

        if (RoomModel.Instance)
            TerminalManager.Instance.TerminalDatas[terminalID - 1].Time = currentTime;

        timerText.text = currentTime.ToString();

        //�������Ԃ�0�ȉ��ɂȂ�����(���Ԑ؂�)
        if (currentTime <= 0)
        {
            //limitTime��0�ɂ���
            currentTime = 0;

            //�J�E���g�_�E�����~����
            CancelInvoke("CountDown");

            // ���s���N�G�X�g
            FailureRequest();
        }
    }

    /// <summary>
    /// �ʒm���^�C�}�[�X�V
    /// </summary>
    public void OnCountDown(int time)
    {
        timerText.text = time.ToString();
    }

    /// <summary>
    /// �[���N�����N�G�X�g����
    /// </summary>
    public virtual async void BootRequest()
    {
        if(terminalType == EnumManager.TERMINAL_TYPE.Jumble)
        {
            Debug.Log("�����b�N������܂���");
            if (RelicManager.HaveRelicList.Count == 0) return;
        }

        isUsed = true; // �N���ς݂ɂ���

        // �N�����N�G�X�g���T�[�o�[�ɑ��M
        if(RoomModel.Instance)
        {
            await RoomModel.Instance.BootTerminalAsync(terminalID);
        }
        else
        {
            BootTerminal();
        }
    }

    /// <summary>
    /// ��V�r�o���N�G�X�g����
    /// </summary>
    public void GiveRewardRequest()
    {
        Stack<Vector2> posStack = new Stack<Vector2>();

        foreach (var point in relicSpawnPoints)
        {
            posStack.Push(point.position);
        }

        //�����b�N��r�o����
        RelicManager.Instance.DropRelicRequest(posStack, false);
    }

    /// <summary>
    /// �[����������
    /// </summary>
    public async void SuccessTerminal()
    {
        // �^�[�~�i����\��
        terminalSprite.DOFade(0, 2.5f);
        iconSprite.DOFade(0, 2.5f).OnComplete(() => { gameObject.SetActive(false); });

        // �}�X�^�[�̓����b�N�v��
        if(RoomModel.Instance.IsMaster) GiveRewardRequest();
    }

    #endregion

    #region �[�����ɏ�������

    /// <summary>
    /// �N������
    /// </summary>
    public abstract void BootTerminal();

    /// <summary>
    /// ���s���N�G�X�g
    /// </summary>
    public async virtual void FailureRequest()
    {
        // ���X�g�̊Y���[��ID�̏�Ԃ����s�ɂ���
        if (RoomModel.Instance)
        {
            // �T�[�o�[�Ɏ��s�ʒm
            await RoomModel.Instance.TerminalFailureAsync(terminalID);
        }
        else
        {
            FailureTerminal();
        }
    }

    /// <summary>
    /// ���s����
    /// </summary>
    public virtual void FailureTerminal()
    {
        if (RoomModel.Instance)
            TerminalManager.Instance.TerminalDatas[terminalID].State = EnumManager.TERMINAL_STATE.Failure;

        // �^�[�~�i����\��
        terminalSprite.DOFade(0, 2.5f);
        iconSprite.DOFade(0, 2.5f).OnComplete(() => { gameObject.SetActive(false); });
    }

    #endregion
}