//----------------------------------------------------
// �Q�[���}�l�[�W���[(GameManager.cs)
// Author : Souma Ueno
//----------------------------------------------------
using JetBrains.Annotations;
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
    int crashNum = 0; �@�@�@�@�@    // ���j��
    bool bossFlag = false;      // �{�X���o�����ǂ���
    int xp;                     // �o���l
    int requiredXp = 100;       // �K�v�o���l
    int level;                  // ���x��
    int num;                    // �����܂ł̃J�E���g
    public int spawnInterval;   // �����Ԋu
    int spawnCnt;               // �X�|�[����
    public int maxSpawnCnt;     // �}�b�N�X�X�|�[����
    bool isBossDead;            // �{�X�����񂾂��ǂ���
    bool isSpawnBoss;           // �{�X���������ꂽ���ǂ���
    GameObject boss;            // �{�X

    #endregion

    #region ���̑�
    [Header("���̑�")]
    [SerializeField] GameObject bossPrefab;  // �{�X�v���n�u
    [SerializeField] Transform minCameraPos; // �J�����͈͂̍ŏ��l
    [SerializeField] Transform maxCameraPos; // �J�����͈͂̍ő�l
    [SerializeField] float xRadius;          // �����͈͂�x���a
    [SerializeField] float yRadius;          // �����͈͂�y���a
    [SerializeField] float distMinSpawnPos;  // �������Ȃ��͈�
    [SerializeField] int knockTermsNum;      // �{�X�̃G�l�~�[�̌��j������
    [SerializeField] GameObject player;
    [SerializeField] List<GameObject> players;      // �v���C���[�̏��

    float elapsedTime;

    Vector3 bossPos;
    #endregion

    #region �e�v���p�e�B
    [Header("�e�v���p�e�B")]
    public bool BossFlag { get { return bossFlag; } set { bossFlag = value; } }

    public GameObject Player { get { return player; } }

    public List<GameObject> Players { get { return players; } }

    public GameObject Boss {  get { return boss; } }

    public int SpawnInterval { get { return spawnInterval; } set { spawnInterval = value; } }

    public bool IsSpawnBoss { get { return isSpawnBoss; } }

    public int KnockTermsNum { get { return knockTermsNum; } }

    public int SpawnCnt { get { return spawnCnt; } set { spawnCnt = value; } }

    public int MaxSpawnCnt { get { return maxSpawnCnt; } }

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
        else if(Input.GetKeyDown(KeyCode.S))
        {
            Time.timeScale = 1;
        }

        if (!isSpawnBoss && bossFlag)
        {
            // �{�X�̐����͈͂̔���
            var spawnPostions = SpawnManager.Instance.CreateEnemySpawnPosition(minCameraPos.position, maxCameraPos.position);

            EnemyBase bossEnemy = bossPrefab.GetComponent<EnemyBase>();

            Vector3? spawnPos = SpawnManager.Instance.GenerateEnemySpawnPosition(spawnPostions.minRange, spawnPostions.maxRange,bossEnemy);

            if (spawnPos != null)
            {// �Ԃ�l��null����Ȃ��Ƃ�
                boss = Instantiate(bossPrefab, (Vector3)spawnPos, Quaternion.identity);
 
                //boss.GetComponent<EnemyBase>().SetNearTarget();
            }

            isSpawnBoss = true;

            bossFlag = false;
        }

        if (isBossDead)
        {// �{�X��|����(��)
            // �x��ČĂяo��
            Invoke(nameof(ChengScene), 15f);
        }

        if (spawnCnt < maxSpawnCnt && !isBossDead)
        {// �X�|�[���񐔂����E�ɒB���Ă��邩
            elapsedTime += Time.deltaTime;
            if (elapsedTime > spawnInterval)
            {
                elapsedTime = 0;

                if (!IsSpawnBoss)
                {
                    if (spawnCnt < maxSpawnCnt / 2)
                    {// �G��100�̂��Ȃ��ꍇ
                        SpawnManager.Instance.GenerateEnemy(Random.Range(3, 7));
                    }
                    else
                    {// ����ꍇ
                        SpawnManager.Instance.GenerateEnemy(1);
                    }
                }
            }
        }
    }

    /// <summary>
    /// �V�[���J��
    /// </summary>
    private void ChengScene()
    {// �V�[���J��
        SceneManager.LoadScene("Result ueno");
    }

    [ContextMenu("CrushEnemy")]
    /// <summary>
    ///  �G���j
    /// </summary>
    public void CrushEnemy(EnemyBase enemy)
    {
        crashNum++;

        UIManager.Instance.CountTermsText(crashNum);

        spawnCnt--;

        if (enemy.IsBoss)
        {
            DeathBoss();
        }
        else if (crashNum >= knockTermsNum)
        {
            bossFlag = true;
        }
    }

    [ContextMenu("DeathBoss")]
    private void DeathBoss()
    {
        RelicManager.Instance.GenerateRelic(boss.transform.position);

        // �{�X�t���O��ύX
        bossFlag = false;
        // ���񂾔���ɂ���
        isBossDead = true;
    }

    private void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.DrawWireCube(player.transform.position, new Vector3(distMinSpawnPos * 2, yRadius * 2));
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(player.transform.position, new Vector3(xRadius * 2, yRadius * 2));
        }
    }

    [ContextMenu("DecreaseGeneratInterval")]
    /// <summary>
    /// ���Ԍo�ߖ��ɃX�|�[���Ԋu�𑁂߂鏈��
    /// </summary>
    private void DecreaseGeneratInterval()
    {
        spawnInterval -= 2;
    }
}
