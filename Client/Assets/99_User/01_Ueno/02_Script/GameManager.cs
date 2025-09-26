//----------------------------------------------------
// �Q�[���}�l�[�W���[(GameManager.cs)
// Author : Souma Ueno
//----------------------------------------------------
using JetBrains.Annotations;
using KanKikuchi.AudioManager;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Grpc.Core.Metadata;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    #region �����ݒ�
    [Header("�����ݒ�")]
    int crashNum = 0; �@�@�@�@�@// ���j
    int xp;                     // �o���l
    int requiredXp = 100;       // �K�v�o���l
    int level;                  // ���x��
    bool isBossDead;            // �{�X�����񂾂��ǂ���
    bool isGameStart;           // �Q�[�����J�n�������ǂ���
    GameObject bossTerminal;    // �{�X�[��
    [SerializeField] STAGE_TYPE nextStage;    // ���݂̃X�e�[�W
    #endregion

    #region ���̑�
    [Header("���̑�")]
    [SerializeField] Transform minCameraPos;   // �J�����͈͂̍ŏ��l
    [SerializeField] Transform maxCameraPos;   // �J�����͈͂̍ő�l
    [SerializeField] float xRadius;            // �����͈͂�x���a
    [SerializeField] float yRadius;            // �����͈͂�y���a
    [SerializeField] float distMinSpawnPos;    // �������Ȃ��͈�
    [SerializeField] int knockTermsNum;        // �G�l�~�[�̌��j������
    [SerializeField] List<GameObject> portals; // �J�ڗp�|�[�^��

    float elapsedTime;

    Vector3 bossPos;
    #endregion

    #region �e�v���p�e�B
    [Header("�e�v���p�e�B")]
    public int CrashNum { get { return crashNum; } }

    public bool IsBossDead { get { return isBossDead; } }

    public int KnockTermsNum { get { return knockTermsNum; } }

    public bool IsGameStart {  get { return isGameStart; } set { isGameStart = value; } }

    public STAGE_TYPE NextStage { get { return nextStage; } }

    private static GameManager instance;
    #endregion

    #region �^�[�~�i���֘A

    // (MAX�̒l��Rand�ŗp���邽�߁A���+1�̐�)
    private const int MIN_TERMINAL_NUM = 3;
    private const int MAX_TERMINAL_NUM = 7;
    private const int MIN_TERMINAL_ID = 1;
    private const int MAX_TERMINAL_ID = 6;

    #endregion

    SceneLoader loader;

    #region Instance
    [Header("Instance")]
    public static GameManager Instance
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

        isGameStart = false;
    }
    #endregion

    ResultData resultData;
    public ResultData ResultData { get { return resultData; } }

    /// <summary>
    /// �����ݒ�
    /// </summary>
    async void Start()
    {
        if (GameObject.Find("BossTerminal") != null) // �X�e�[�W��1�̃��j�[�N�Ȓ[���ׁ̈A���O�Ŏ擾
            bossTerminal = GameObject.Find("BossTerminal");

        foreach (var player in CharacterManager.Instance.PlayerObjs)
        {
            player.Value.gameObject.GetComponent<PlayerBase>().CanMove = false;
            player.Value.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }

        isBossDead = false;
        UIManager.Instance.ShowUIAndFadeOut();

        if (!RoomModel.Instance) StartCoroutine(DelayedCallCoroutine());
        else
        {
            RoomModel.Instance.OnSameStartSyn += this.StartGame;
            RoomModel.Instance.OnAdanceNextStageSyn += this.OnAdanceNextStageSyn;
            RoomModel.Instance.OnGameEndSyn += this.OnGameEnd;

            //�J�ڊ����̃��N�G�X�g (TerminalManager�ɂČĂяo��)
            //await RoomModel.Instance.AdvancedStageAsync();  
        }

        
    }

    private void OnDisable()
    {
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnSameStartSyn -= this.StartGame;
        RoomModel.Instance.OnAdanceNextStageSyn -= this.OnAdanceNextStageSyn;
        RoomModel.Instance.OnGameEndSyn -= this.OnGameEnd;
    }

    /// <summary>
    /// �X�V����
    /// </summary>
    void Update()
    {
        // �|�[�Y����(��)
        if(Input.GetKeyDown(KeyCode.P))
        {
            Time.timeScale = 0;
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            Time.timeScale = 1;
        }
        else if(Input.GetKeyDown(KeyCode.L))
        {// 20�{��(�f�o�b�N�p)
#if UNITY_EDITOR
            Time.timeScale = 20;
#endif
        }

        //Esc�������ꂽ��
        if (Input.GetKey(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UIManager.Instance.DisplayEndGameWindow();
#else
    UIManager.Instance.DisplayEndGameWindow();
#endif
        }

        // �I�t���C���p
        if (!RoomModel.Instance && !CharacterManager.Instance.PlayerObjSelf && isGameStart)
        {
            CangeResult();
        }
    }

    /// <summary>
    /// �|�[�^���̕\��
    /// </summary>
    public void DisplayPortal()
    {
        foreach (var item in portals)
        {
            item.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// �V�[���J�ڒʒm
    /// </summary>
    /// <param name="type"></param>
    void OnAdanceNextStageSyn(STAGE_TYPE type)
    {
        ChengScene(type);
    }

    /// <summary>
    /// �V�[���J��
    /// </summary>
    public void ChengScene(STAGE_TYPE type)
    {
        switch (type)
        {
            case STAGE_TYPE.Rust:
                Initiate.DoneFading();
                Initiate.Fade("4_Stage_01", Color.black, 0.5f);
                break;
            case STAGE_TYPE.Industry:
                Initiate.DoneFading();
                Initiate.Fade("5_Stage_02", Color.black, 0.5f);
                break;
            case STAGE_TYPE.Town:
                Initiate.DoneFading();
                Initiate.Fade("6_Stage_03", Color.black, 0.5f);
                break;
        }
        isGameStart = false;
    }

    /// <summary>
    /// �Q�[���I���ʒm
    /// </summary>
    void OnGameEnd(ResultData resultData)
    {
        UIManager.Instance.OnDeadPlayer();
        this.resultData = resultData;
        CangeResult();
    }

    /// <summary>
    /// ���U���g�̃V�[����ǂݍ���
    /// </summary>
    public void CangeResult()
    {   
        Initiate.DoneFading();
        SceneManager.LoadScene("ResultScene", LoadSceneMode.Additive);
        //Initiate.Fade("ResultScene", Color.black, 0.5f);
        //SceneManager.UnloadSceneAsync("UIScene");
        UIManager.Instance.HideCanvas();

        isGameStart = false;
    }

    [ContextMenu("CrushEnemy")]
    /// <summary>
    ///  �G���j
    /// </summary>
    public void CrushEnemy(EnemyBase enemy)
    {
        SpawnManager.Instance.CrashNum++;

        UIManager.Instance.CountTermsText(SpawnManager.Instance.CrashNum);

        //var result = CharacterManager.Instance.GetEnemiesBySpawnType(EnumManager.SPAWN_ENEMY_TYPE.ByManager);

        if (enemy.IsBoss)
        {
            DeathBoss();
        }
    }

    [ContextMenu("DeathBoss")]
    /// <summary>
    /// �{�X�����񂾏���
    /// </summary>
    private void DeathBoss()
    {
        //RelicManager.Instance.GenerateRelic(SpawnManager.Instance.Boss.transform.position);

        //RelicManager.Instance.GenerateRelic(bossTerminal);

        //RelicManager.Instance.GenerateRelicTest();

        // ���񂾔���ɂ���
        isBossDead = true;

        UIManager.Instance.HideBossUI();

        for(int i = 0; i < portals.Count; i++)
        {
            portals[i].SetActive(true);
        }

        //Invoke(nameof(ChengScene), 15f);
    }

    /// <summary>
    /// �Q�[���J�n����
    /// </summary>
    /// <param name="list"></param>
    public void StartGame(List<TerminalData> list)
    {
        //// �[�������X�e�[�W�ɔ��f
        if (list != null)
            TerminalManager.Instance.SetTerminal(list);

        isGameStart = true;
        Debug.Log("�����J�n�I�I");
        foreach(var player in CharacterManager.Instance.PlayerObjs)
        {
            player.Value.gameObject.GetComponent<PlayerBase>().CanMove = true;
            player.Value.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private IEnumerator DelayedCallCoroutine()
    {
        yield return new WaitForSeconds(0f);

        StartGame(LotteryTerminal());
    }

    /// <summary>
    /// �[���f�[�^���I����
    /// Autho:Nakamoto
    /// </summary>
    /// <returns></returns>
    private List<TerminalData> LotteryTerminal()
    {
        // ID1,2�͌Œ�Őݒ�
        List<TerminalData> terminals = new List<TerminalData>()
            {
                new TerminalData(){ ID = 1, Type = TERMINAL_TYPE.Boss, State = TERMINAL_STATE.Inactive},
                new TerminalData(){ ID = 2, Type = TERMINAL_TYPE.Speed, State = TERMINAL_STATE.Inactive},
            };

        // 3�ȍ~�͒��I
        int terminalCount = Random.Range(MIN_TERMINAL_NUM, MAX_TERMINAL_NUM);   // 3�`6�̒[���𒊑I

        for (int i = 3; i <= terminalCount; i++)
        {
            int termID = 0;

            while (termID == 0 || termID == 2 || termID == 6)
            {   // Speed��Boss�͌Œ�Őݒ肵�Ă��邽�߁A���I���珜�O
                termID = Random.Range(MIN_TERMINAL_ID, MAX_TERMINAL_ID);
            }

            terminals.Add(new TerminalData() { ID = i, Type = (TERMINAL_TYPE)termID, State = TERMINAL_STATE.Inactive });
        }

        return terminals;
    }
}