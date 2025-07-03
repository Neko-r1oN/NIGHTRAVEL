//----------------------------------------------------
// �Q�[���}�l�[�W���[(GameManager.cs)
// Author : Souma Ueno
//----------------------------------------------------
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

    [SerializeField] GameObject player;      // �v���C���[�̏��

    float elapsedTime;

    Vector3 bossPos;
    #endregion

    #region �e�v���p�e�B
    [Header("�e�v���p�e�B")]
    public bool BossFlag { get { return bossFlag; } set { bossFlag = value; } }

    public GameObject Player { get { return player; } }

    public GameObject Boss {  get { return boss; } }

    public int SpawnInterval { get { return spawnInterval; } set { spawnInterval = value; } }

    public bool IsSpawnBoss { get { return isSpawnBoss; } }

    public int KnockTermsNum { get { return knockTermsNum; } }

    public int SpawnCnt { get { return spawnCnt; } set { spawnCnt = value; } }

    private static GameManager instance;

    //public bool IsBossDead { get { return bossFlag; } set { isBossDead = value; } } 
    #endregion

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

    /// <summary>
    /// �����ݒ�
    /// </summary>
    void Start()
    {
        isBossDead = false;

       // UIManager.Instance.CountClashText(crashNum);
    }

    /// <summary>
    /// �X�V����
    /// </summary>
    void Update()
    {
        if (!isSpawnBoss && bossFlag)
        {
            // �{�X�̐����͈͂̔���
            var spawnPostions = SpawnManager.Instance.CreateEnemySpawnPosition(minCameraPos.position, maxCameraPos.position);

            EnemyBase bossEnemy = bossPrefab.GetComponent<EnemyBase>();

            Vector3? spawnPos = SpawnManager.Instance.GenerateEnemySpawnPosition(spawnPostions.minRange, spawnPostions.maxRange,bossEnemy);

            if (spawnPos != null)
            {// �Ԃ�l��null����Ȃ��Ƃ�
                boss = Instantiate(bossPrefab, (Vector3)spawnPos, Quaternion.identity);
                boss.GetComponent<EnemyBase>().IsBoss = true;

                boss.GetComponent<EnemyBase>().Players.Add(player);
                boss.GetComponent<EnemyBase>().SetNearTarget();
            }

            isSpawnBoss = true;

            bossFlag = false;
        }

        if (isBossDead)
        {// �{�X��|����(��)
            // �x��ČĂяo��
            Invoke(nameof(ChengScene), 15f);
        }

        if (spawnCnt < maxSpawnCnt  && !isBossDead)
        {// �X�|�[���񐔂����E�ɒB���Ă��邩
            elapsedTime += Time.deltaTime;
            if (elapsedTime > spawnInterval)
            {
                elapsedTime = 0;

                if (spawnCnt < maxSpawnCnt / 2)
                {// �G��100�̂��Ȃ��ꍇ
                    for (int i = 0; i < 5; i++)
                    {// �����̓G�𐶐�
                        // �G��������
                        SpawnManager.Instance.GenerateEnemy();
                    }
                }
                else
                {// ����ꍇ
                    SpawnManager.Instance.GenerateEnemy();
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

        UIManager.Instance.CountClashText(crashNum);

        //Debug.Log("�|�������F" + crashNum);

        spawnCnt--;

        if (enemy.IsBoss)
        {
            DeathBoss();
        }
        else if (crashNum >= knockTermsNum)
        {// ���j����15�ȏ�ɂȂ�����(��)

            DeathBoss();

            bossFlag = true;
            Debug.Log("�|�������F" + crashNum + "�{�X");
        }
    }

    [ContextMenu("DeathBoss")]
    public void DeathBoss()
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
    public void DecreaseGeneratInterval()
    {
        spawnInterval -= 2;
    }
}
