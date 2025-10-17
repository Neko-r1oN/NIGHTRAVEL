//----------------------------------------------------
// �Q�[���}�l�[�W���[(GameManager.cs)
// Author : Souma Ueno
//----------------------------------------------------
using JetBrains.Annotations;
using KanKikuchi.AudioManager;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using NUnit.Framework;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Grpc.Core.Metadata;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;
using Unity.Cinemachine;
using Unity.Mathematics;

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
    bool isCanSpawnBoss;        // �{�X���X�|�[���\���ǂ���
    public bool IsCanSpawnBoss { get { return isCanSpawnBoss; } } 
    GameObject bossTerminal;    // �{�X�[��
    [SerializeField] STAGE_TYPE nextStage;    // ���݂̃X�e�[�W
    [SerializeField] SceneConducter conducter;    // ���݂̃X�e�[�W
    #endregion

    #region ���̑�
    [Header("���̑�")]
    [SerializeField] float xRadius;            // �����͈͂�x���a
    [SerializeField] float yRadius;            // �����͈͂�y���a
    [SerializeField] float distMinSpawnPos;    // �������Ȃ��͈�
    [SerializeField] int knockTermsNum;        // �G�l�~�[�̌��j������
    [SerializeField] List<GameObject> portals; // �J�ڗp�|�[�^��
    [SerializeField] AudioResource bossBGM;      // �{�XBGM
    [SerializeField] AudioResource normalBGM;

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

    [SerializeField]
    ColorChanger colorChanger;  // �{�X���j���̉��o�p

    [SerializeField] CinemachineBasicMultiChannelPerlin VirtualCamera;
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

        if (!RoomModel.Instance) return;

        //�v���C���[�҂�
        if (SceneManager.GetActiveScene().name != "Tutorial")
            conducter.TakeYourPlayer();

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
        isCanSpawnBoss = SpawnManager.Instance.CrashNum >= SpawnManager.Instance.KnockTermsNum;
        UIManager.Instance.ShowUIAndFadeOut();

        if (!RoomModel.Instance) StartCoroutine(DelayedCallCoroutine());
        else
        {
            RoomModel.Instance.OnSameStartSyn += this.StartGame;
            RoomModel.Instance.OnAdanceNextStageSyn += this.OnAdanceNextStageSyn;
            RoomModel.Instance.OnGameEndSyn += this.OnGameEnd;
        }

        PlayStageBGM();
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
        if (Input.GetKeyDown(KeyCode.LeftAlt) && Input.GetKey(KeyCode.RightAlt))
        {
            CharacterManager.Instance.ApplyCheatEffect();
            SpawnManager.Instance.KnockTermsNum = 0;
        }
        if(Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.RightControl))
        {
            CharacterManager.Instance.RemoveCheatEffect();
            SpawnManager.Instance.KnockTermsNum = 50;
        }

#if UNITY_EDITOR
        // �|�[�Y����(��)
        if (Input.GetKeyDown(KeyCode.P))
        {
            Time.timeScale = 0;
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            Time.timeScale = 1;
        }
        else if(Input.GetKeyDown(KeyCode.L))
        {// 20�{��(�f�o�b�N�p)
            Time.timeScale = 20;
        }
#endif

//        //Esc�������ꂽ��
//        if (Input.GetKey(KeyCode.Escape))
//        {
//#if UNITY_EDITOR
//            UIManager.Instance.DisplayEndGameWindow();
//#else
//    UIManager.Instance.DisplayEndGameWindow();
//#endif
//        }

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
        CharacterManager.Instance.UpdateSelfSelfPlayerStatusData(); // �J�ڂ���O�̃v���C���[�̃X�e�[�^�X��ێ�
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
            case STAGE_TYPE.FinalBoss:
                Initiate.DoneFading();
                Initiate.Fade("7_Stage_04", Color.black, 0.5f);
                break;
        }
        isGameStart = false;
    }

    /// <summary>
    /// �Q�[���I���ʒm
    /// </summary>
    void OnGameEnd(ResultData resultData)
    {
        //UIManager.Instance.OnDeadPlayer();
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

        if(SpawnManager.Instance.CrashNum >= SpawnManager.Instance.KnockTermsNum) isCanSpawnBoss = true;

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
        StartCoroutine(colorChanger.ColorChange());     // �{�X���j���o�Đ�

        // ���񂾔���ɂ���
        isBossDead = true;
        BGMManager.Instance.Stop();

        switch (nextStage)
        {
            ///���X�{�X
            case STAGE_TYPE.Rust:  
               
                SEManager.Instance.Play(
                audioPath: SEPath.LAST_DEATH, //�Đ��������I�[�f�B�I�̃p�X
                volumeRate: 0.5f,                //���ʂ̔{��
                delay: 0.5f,                //�Đ������܂ł̒x������
                pitch: 0.8f,                //�s�b�`
                isLoop: false,             //���[�v�Đ����邩
                callback: null              //�Đ��I����̏���
                );

                break;

            ///���
            case STAGE_TYPE.Industry:

                SEManager.Instance.Play(
               audioPath: SEPath.MOB_DEATH, //�Đ��������I�[�f�B�I�̃p�X
               volumeRate: 1.0f,                //���ʂ̔{��
               delay: 0.0f,                //�Đ������܂ł̒x������
               pitch: 1.0f,                //�s�b�`
               isLoop: false,             //���[�v�Đ����邩
               callback: null              //�Đ��I����̏���
               );

                break;

            ///���
            case STAGE_TYPE.Town:

                SEManager.Instance.Play(
                audioPath: SEPath.WORM_DEATH, //�Đ��������I�[�f�B�I�̃p�X
                volumeRate: 1.0f,                //���ʂ̔{��
                delay: 0.0f,                //�Đ������܂ł̒x������
                pitch: 1.0f,                //�s�b�`
                isLoop: false,             //���[�v�Đ����邩
                callback: null              //�Đ��I����̏���
                );

                SEManager.Instance.Play(
               audioPath: SEPath.IRON_HIT, //�Đ��������I�[�f�B�I�̃p�X
               volumeRate: 1.0f,                //���ʂ̔{��
               delay: 0.0f,                //�Đ������܂ł̒x������
               pitch: 1.0f,                //�s�b�`
               isLoop: false,             //���[�v�Đ����邩
               callback: null              //�Đ��I����̏���
               );
                break;

            ///�O��
            case STAGE_TYPE.FinalBoss:

                SEManager.Instance.Play(
                audioPath: SEPath.IRON_HIT, //�Đ��������I�[�f�B�I�̃p�X
                volumeRate: 1.0f,                //���ʂ̔{��
                delay: 0.0f,                //�Đ������܂ł̒x������
                pitch: 1.0f,                //�s�b�`
                isLoop: false,             //���[�v�Đ����邩
                callback: null              //�Đ��I����̏���
                );

                SEManager.Instance.Play(
               audioPath: SEPath.MOB_DEATH, //�Đ��������I�[�f�B�I�̃p�X
               volumeRate: 1.0f,                //���ʂ̔{��
               delay: 0.0f,                //�Đ������܂ł̒x������
               pitch: 1.0f,                //�s�b�`
               isLoop: false,             //���[�v�Đ����邩
               callback: null              //�Đ��I����̏���
               );
                break;

               
        }
        ShakeCamera(1.5f);

        SEManager.Instance.Play(
                   audioPath: SEPath.KAMINARI, //�Đ��������I�[�f�B�I�̃p�X
                   volumeRate: 300,                //���ʂ̔{��
                   delay: 0,                //�Đ������܂ł̒x������
                   pitch: 1,                //�s�b�`
                   isLoop: false,             //���[�v�Đ����邩
                   callback: null              //�Đ��I����̏���
                   );

        SEManager.Instance.Play(
                  audioPath: SEPath.GEKIHA, //�Đ��������I�[�f�B�I�̃p�X
                  volumeRate: 0.6f,                //���ʂ̔{��
                  delay: 0,                //�Đ������܂ł̒x������
                  pitch: 1,                //�s�b�`
                  isLoop: false,             //���[�v�Đ����邩
                  callback: null              //�Đ��I����̏���
                  );


        // �{�X���j���Ƀ����b�N���h���b�v
        TerminalManager.Instance.OnTerminalsSuccessed(1);

        UIManager.Instance.HideBossUI();

        for (int i = 0; i < portals.Count; i++)
        {
            portals[i].SetActive(true);
        }
    }

    /// <summary>
    /// �Q�[���J�n����
    /// </summary>
    /// <param name="list"></param>
    public void StartGame(List<TerminalData> list)
    {
        // �[�������X�e�[�W�ɔ��f
        if (list != null && SceneManager.GetActiveScene().name != "Tutorial")
            TerminalManager.Instance.SetTerminal(list);

        isGameStart = true;
        Debug.Log("�����J�n�I�I");
        foreach(var player in CharacterManager.Instance.PlayerObjs)
        {
            player.Value.gameObject.GetComponent<PlayerBase>().CanMove = true;
            player.Value.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }

        //�v���C���[�҂�����
        if (SceneManager.GetActiveScene().name != "Tutorial")
            conducter.SameStartPlayers();

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
    public void PlayStageBGM()
    {
        switch (nextStage)
        {
            case STAGE_TYPE.Rust:
                BGMManager.Instance.Play(
                audioPath: BGMPath.STAGE4,           //�Đ��������I�[�f�B�I�̃p�X
                volumeRate: 0.6f,                      //���ʂ̔{��
                delay: 0,                           //�Đ������܂ł̒x������
                pitch: 1,                           //�s�b�`
                isLoop: true,                       //���[�v�Đ����邩
                allowsDuplicate: false              //����BGM�Əd�����čĐ������邩
                );
                break;
            case STAGE_TYPE.Industry:
                BGMManager.Instance.Play(
                audioPath: BGMPath.STAGE1,           //�Đ��������I�[�f�B�I�̃p�X
                volumeRate: 0.6f,                      //���ʂ̔{��
                delay: 0,                           //�Đ������܂ł̒x������
                pitch: 1,                           //�s�b�`
                isLoop: true,                       //���[�v�Đ����邩
                allowsDuplicate: false              //����BGM�Əd�����čĐ������邩
                );
                break;
            case STAGE_TYPE.Town:
                BGMManager.Instance.Play(
                audioPath: BGMPath.STAGE2,           //�Đ��������I�[�f�B�I�̃p�X
                volumeRate: 0.6f,                      //���ʂ̔{��
                delay: 0,                           //�Đ������܂ł̒x������
                pitch: 1,                           //�s�b�`
                isLoop: true,                       //���[�v�Đ����邩
                allowsDuplicate: false              //����BGM�Əd�����čĐ������邩
                );
                break;
            case STAGE_TYPE.FinalBoss:
                BGMManager.Instance.Play(
                audioPath: BGMPath.STAGE3,           //�Đ��������I�[�f�B�I�̃p�X
                volumeRate: 0.6f,                      //���ʂ̔{��
                delay: 0,                           //�Đ������܂ł̒x������
                pitch: 1,                           //�s�b�`
                isLoop: true,                       //���[�v�Đ����邩
                allowsDuplicate: false              //����BGM�Əd�����čĐ������邩
                );

                break;
        }


    }


    public void PlayBossBGM()
    {
        switch (nextStage)
        {
            case STAGE_TYPE.Rust:
                BGMManager.Instance.Play(
                audioPath: BGMPath.BOSS4,           //�Đ��������I�[�f�B�I�̃p�X
                volumeRate: 1,                      //���ʂ̔{��
                delay: 0,                           //�Đ������܂ł̒x������
                pitch: 1,                           //�s�b�`
                isLoop: true,                       //���[�v�Đ����邩
                allowsDuplicate: false              //����BGM�Əd�����čĐ������邩
                );
                break;
            case STAGE_TYPE.Industry:
                BGMManager.Instance.Play(
                audioPath: BGMPath.BOSS1,           //�Đ��������I�[�f�B�I�̃p�X
                volumeRate: 1,                      //���ʂ̔{��
                delay: 0,                           //�Đ������܂ł̒x������
                pitch: 1,                           //�s�b�`
                isLoop: true,                       //���[�v�Đ����邩
                allowsDuplicate: false              //����BGM�Əd�����čĐ������邩
                );
                break;
            case STAGE_TYPE.Town:
                BGMManager.Instance.Play(
                audioPath: BGMPath.BOSS2,           //�Đ��������I�[�f�B�I�̃p�X
                volumeRate: 1,                      //���ʂ̔{��
                delay: 0,                           //�Đ������܂ł̒x������
                pitch: 1,                           //�s�b�`
                isLoop: true,                       //���[�v�Đ����邩
                allowsDuplicate: false              //����BGM�Əd�����čĐ������邩
                );
                break;
            case STAGE_TYPE.FinalBoss:
                BGMManager.Instance.Play(
                audioPath: BGMPath.BOSS3,           //�Đ��������I�[�f�B�I�̃p�X
                volumeRate: 1,                      //���ʂ̔{��
                delay: 0,                           //�Đ������܂ł̒x������
                pitch: 1,                           //�s�b�`
                isLoop: true,                       //���[�v�Đ����邩
                allowsDuplicate: false              //����BGM�Əd�����čĐ������邩
                );
                break;
        }

        ShakeCamera(2.5f);

    }

    public void ShakeCamera(float diffuseTime)
    {
        VirtualCamera.enabled = true;
        Invoke("DiffuseShake", diffuseTime);
    }

    
    private void DiffuseShake()
    {
        VirtualCamera.enabled = false;
    }
}