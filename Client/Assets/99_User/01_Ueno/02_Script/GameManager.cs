//----------------------------------------------------
// �Q�[���}�l�[�W���[(GameManager.cs)
// Author : Souma Ueno
//----------------------------------------------------
using JetBrains.Annotations;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Grpc.Core.Metadata;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    #region �����ݒ�
    [Header("�����ݒ�")]
    int crashNum = 0; �@�@�@�@�@// ���j��
    int xp;                     // �o���l
    int requiredXp = 100;       // �K�v�o���l
    int level;                  // ���x��
    bool isBossDead;            // �{�X�����񂾂��ǂ���
    bool isGameStart;           // �Q�[�����J�n�������ǂ���
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

    float elapsedTime;

    Vector3 bossPos;
    #endregion

    #region �e�v���p�e�B
    [Header("�e�v���p�e�B")]

    public bool IsBossDead { get { return isBossDead; } }

    public int KnockTermsNum { get { return knockTermsNum; } }

    public bool IsGameStart {  get { return isGameStart; } set { isGameStart = value; } }

    private static GameManager instance;

    //public bool IsBossDead { get { return bossFlag; } set { isBossDead = value; } } 
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
    }
    #endregion

    /// <summary>
    /// �����ݒ�
    /// </summary>
    void Start()
    {
        isBossDead = false;
        //Debug.Log(LevelManager.Instance.GameLevel.ToString());
        UIManager.Instance.ShowUIAndFadeOut();

        isGameStart = true;
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
        else if(Input.GetKeyDown(KeyCode.S))
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
    }

    /// <summary>
    /// �V�[���J��
    /// </summary>
    private void ChengScene()
    {// �V�[���J��
        SceneManager.LoadScene("Result ueno");

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

        var result = CharacterManager.Instance.GetEnemiesBySpawnType(EnumManager.SPAWN_ENEMY_TYPE.ByManager);

        if (enemy.IsBoss)
        {
            DeathBoss();
        }
    }

    [ContextMenu("DeathBoss")]
    private void DeathBoss()
    {
        RelicManager.Instance.GenerateRelic(SpawnManager.Instance.Boss.transform.position);

        // ���񂾔���ɂ���
        isBossDead = true;

        Invoke(nameof(ChengScene), 15f);
    }
}