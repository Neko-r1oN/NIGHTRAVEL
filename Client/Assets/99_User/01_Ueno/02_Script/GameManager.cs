//----------------------------------------------------
// �Q�[���}�l�[�W���[(GameManager.cs)
// Author : Souma Ueno
//----------------------------------------------------
using JetBrains.Annotations;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
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
    #endregion

    #region ���̑�
    [Header("���̑�")]
    [SerializeField] GameObject bossPrefab;  // �{�X�v���n�u
    [SerializeField] Transform minCameraPos; // �J�����͈͂̍ŏ��l
    [SerializeField] Transform maxCameraPos; // �J�����͈͂̍ő�l
    [SerializeField] float xRadius;          // �����͈͂�x���a
    [SerializeField] float yRadius;          // �����͈͂�y���a
    [SerializeField] float distMinSpawnPos;  // �������Ȃ��͈�
    [SerializeField] int knockTermsNum;      // �G�l�~�[�̌��j������
    [SerializeField] List<GameObject> portals;

    float elapsedTime;

    Vector3 bossPos;
    #endregion

    #region �e�v���p�e�B
    [Header("�e�v���p�e�B")]
    public int CrashNum { get { return crashNum; } }

    public bool IsBossDead { get { return isBossDead; } }

    public int KnockTermsNum { get { return knockTermsNum; } }

    public bool IsGameStart {  get { return isGameStart; } set { isGameStart = value; } }

    private static GameManager instance;

    //public bool IsBossDead { get { return bossFlag; } set { isBossDead = value; } } 
    #endregion

    #region

    // �^�[�~�i���֘A (MAX�̒l��Rand�ŗp���邽�߁A���+1�̐�)
    private const int MIN_TERMINAL_NUM = 3;
    private const int MAX_TERMINAL_NUM = 7;
    private const int MIN_TERMINAL_ID = 1;
    private const int MAX_TERMINAL_ID = 6;

    #endregion

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

        foreach(var player in CharacterManager.Instance.PlayerObjs)
        {
            player.Value.gameObject.GetComponent<PlayerBase>().CanMove = false;
            player.Value.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }
    }
    #endregion

    /// <summary>
    /// �����ݒ�
    /// </summary>
    async void Start()
    {
        if (GameObject.Find("BossTerminal") != null) // �X�e�[�W��1�̃��j�[�N�Ȓ[���ׁ̈A���O�Ŏ擾
            bossTerminal = GameObject.Find("BossTerminal");
        
        isBossDead = false;
        //Debug.Log(LevelManager.Instance.GameLevel.ToString());
        UIManager.Instance.ShowUIAndFadeOut();

        if (!RoomModel.Instance) StartGame(LotteryTerminal());
        else
        {
            RoomModel.Instance.OnSameStartSyn += this.StartGame;
            await RoomModel.Instance.AdvancedStageAsync();  //�J�ڊ����̃��N�G�X�g
        }
    }

    private void OnDisable()
    {
        if (!RoomModel.Instance) return;
        RoomModel.Instance.OnSameStartSyn -= this.StartGame;
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

    public void DisplayPortal()
    {
        foreach (var item in portals)
        {
            item.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// �V�[���J��
    /// </summary>
    public void ChengScene()
    {// �V�[���J��
        SceneManager.LoadScene("Stage Ueno");

        isGameStart = false;
    }

    public void CangeResult()
    {
        SceneManager.UnloadSceneAsync("UIScene");
        SceneManager.LoadScene("ResultScene", LoadSceneMode.Additive);

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
    private void DeathBoss()
    {
        //RelicManager.Instance.GenerateRelic(SpawnManager.Instance.Boss.transform.position);

        //RelicManager.Instance.GenerateRelic(bossTerminal);

        //RelicManager.Instance.GenerateRelicTest();

        // ���񂾔���ɂ���
        isBossDead = true;

        GameObject portal;

        for(int i = 0; i < portals.Count; i++)
        {
            portal = Instantiate(portals[i], portals[i].transform.position, Quaternion.identity);
        }

        //Invoke(nameof(ChengScene), 15f);
    }

    public void StartGame(List<TerminalData> list)
    {
        // �[�������X�e�[�W�ɔ��f
        TerminalManager.Instance.SetTerminal(list);

        isGameStart = true;
        Debug.Log("�����J�n�I�I");
        foreach(var player in CharacterManager.Instance.PlayerObjs)
        {
            player.Value.gameObject.GetComponent<PlayerBase>().CanMove = true;
            player.Value.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }
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
        int terminalCount = Random.Range(MIN_TERMINAL_NUM, MAX_TERMINAL_NUM); // 3�`6�̒[���𒊑I

        for (int i = 3; i <= terminalCount; i++)
        {
            int termID = Random.Range(MIN_TERMINAL_ID, MAX_TERMINAL_ID);

            terminals.Add(new TerminalData() { ID = i, Type = (TERMINAL_TYPE)termID, State = TERMINAL_STATE.Inactive });
        }

        return terminals;
    }
}