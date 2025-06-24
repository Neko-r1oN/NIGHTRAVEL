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
using static Grpc.Core.Metadata;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    #region �����ݒ�
    [Header("�����ݒ�")]
    int crushNum; �@�@�@�@�@    // ���j��
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
    public List<GameObject> enemyList;       // �G�l�~�[���X�g
    [SerializeField] GameObject bossPrefab;  // �{�X�v���n�u
    [SerializeField] Transform randRespawnA; // ���X�|�[���͈�A
    [SerializeField] Transform randRespawnB; // ���X�|�[���͈�B
    [SerializeField] Transform minCameraPos; // �J�����͈͂̍ŏ��l
    [SerializeField] Transform maxCameraPos; // �J�����͈͂̍ő�l
    [SerializeField] float xRadius;          // �����͈͂�x���a
    [SerializeField] float yRadius;          // �����͈͂�y���a
    [SerializeField] float distMinSpawnPos;  // �������Ȃ��͈�

    [SerializeField] GameObject player;      // �v���C���[�̏��
    GameObject enemy;                        // �G�l�~�[�̏��

    float elapsedTime;

    Vector3 bossPos;
    #endregion

    #region �e�v���p�e�B
    [Header("�e�v���p�e�B")]
    public GameObject Enemy { get { return enemy; } }

    public bool BossFlag { get { return bossFlag; } set { bossFlag = value; } }

    public GameObject Player { get { return player; } }

    public GameObject Boss {  get { return boss; } }

    public int SpawnInterval { get { return spawnInterval; } set { spawnInterval = value; } }

    public bool IsSpawnBoss { get { return isSpawnBoss; } }

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
    }

    /// <summary>
    /// �X�V����
    /// </summary>
    void Update()
    {
        if (!isSpawnBoss && bossFlag)
        {
            // �{�X�̐����͈͂̔���
            var spawnPostions = CreateEnemySpawnPosition(minCameraPos.position, maxCameraPos.position);

            Vector3? spawnPos = GenerateEnemySpawnPosition(spawnPostions.minRange, spawnPostions.maxRange);

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
            Invoke(nameof(ChengScene), 1.5f);
        }

        if (spawnCnt < maxSpawnCnt  && !isBossDead)
        {// �X�|�[���񐔂����E�ɒB���Ă��邩
            elapsedTime += Time.deltaTime;
            if (elapsedTime > spawnInterval)
            {
                elapsedTime = 0;

                if (spawnCnt < 100)
                {// �G��100�̂��Ȃ��ꍇ
                    for (int i = 0; i < 5; i++)
                    {// �����̓G�𐶐�
                        // �G��������
                        GenerateEnemy();
                    }
                }
                else
                {// ����ꍇ
                    GenerateEnemy();
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
        crushNum++;

        Debug.Log("�|�������F" + crushNum);

        spawnCnt--;

        if (enemy.IsBoss)
        {
            DeathBoss();
        }
        else if (crushNum >= 15)
        {// ���j����15�ȏ�ɂȂ�����(��)

            bossFlag = true;
            Debug.Log("�|�������F" + crushNum + "�{�X");
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

    /// <summary>
    /// �G�̃X�|�[���\�͈͔��菈��
    /// </summary>
    /// <param name="minPoint"></param>
    /// <param name="maxPoint"></param>
    /// <returns></returns>
    private (Vector3 minRange, Vector3 maxRange) CreateEnemySpawnPosition(Vector3 minPoint, Vector3 maxPoint)
    {
        Vector3 minRange = minPoint, maxRange = maxPoint;
        if (minPoint.y < randRespawnA.position.y)
        {
            minRange.y = randRespawnA.position.y;
        }

        if (minPoint.x < randRespawnA.position.x)
        {
            minRange.x = randRespawnA.position.x;
        }

        if (maxPoint.y > randRespawnB.position.y)
        {
            maxRange.y = randRespawnB.position.y;
        }

        if (maxPoint.x > randRespawnB.position.x)
        {
            maxRange.x = randRespawnB.position.x;
        }

        return (minRange, maxRange);
    }

    /// <summary>
    /// �G�����̈ʒu���菈��
    /// </summary>
    /// <param name="minRange"></param>
    /// <param name="maxRange"></param>
    /// <returns></returns>
    private Vector3? GenerateEnemySpawnPosition(Vector3 minRange, Vector3 maxRange)
    {
        // ���s��
        int loopMax = 10;

        for (int i = 0; i < loopMax; i++)
        {
            Vector3 spawnPos = new Vector3
                 (Random.Range(minRange.x, maxRange.x), Random.Range(minRange.y, maxRange.y));

            Vector3 distToPlayer =
                player.transform.position - spawnPos;

            if (Mathf.Abs(distToPlayer.x) > distMinSpawnPos
                && Mathf.Abs(distToPlayer.y) > distMinSpawnPos)
            {
                return spawnPos;
            }
        }

        return null;
    }

    /// <summary>
    /// ���Ԍo�ߖ��ɃX�|�[���Ԋu�𑁂߂鏈��
    /// </summary>
    public void DecreaseGeneratInterval()
    {
        spawnInterval -= 1;
    }

    /// <summary>
    /// �G��������
    /// </summary>
    public void GenerateEnemy()
    {
        Vector2 minPlayer =
                    new Vector2(player.transform.position.x - xRadius, player.transform.position.y - yRadius);

        Vector2 maxPlayer =
            new Vector2(player.transform.position.x + xRadius, player.transform.position.y + yRadius);

        // �����_���Ȉʒu�𐶐�
        var spawnPostions = CreateEnemySpawnPosition(minPlayer, maxPlayer);

        Vector3? spawnPos = GenerateEnemySpawnPosition(spawnPostions.minRange, spawnPostions.maxRange);

        if (spawnPos != null)
        {
            spawnCnt++;

            int listNum = Random.Range(0, enemyList.Count);

            // ����
            enemy = Instantiate(enemyList[listNum], (Vector3)spawnPos, Quaternion.identity);

            enemy.GetComponent<EnemyBase>().Players.Add(player);
            enemy.GetComponent<EnemyBase>().SetNearTarget();

            if (enemy.GetComponent<Rigidbody2D>().gravityScale != 0)
            {
                enemy.GetComponent<EnemyBase>().enabled = false;

                // ������
                enemy.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }
}
