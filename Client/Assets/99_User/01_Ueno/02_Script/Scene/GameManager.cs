//----------------------------------------------------
// �Q�[���}�l�[�W���[(GameManager.cs)
// Author : Souma Ueno
//----------------------------------------------------
using System;
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
    int crushNum; �@�@�@�@�@// ���j��
    bool bossFlag = false;  // �{�X���o�����ǂ���
    int xp;                 // �o���l
    int requiredXp = 100;   // �K�v�o���l
    int level;              // ���x��
    int num;                // �����܂ł̃J�E���g
    public int createCnt;   // �����Ԋu
    int spawnCnt;           // �X�|�[����
    public int maxSpawnCnt; // �}�b�N�X�X�|�[����
    bool isBossDead;
    bool isSpawnBoss;
    #endregion

    #region ���̑�
    [Header("���̑�")]
    public List<GameObject> enemyList;       // �G�l�~�[���X�g
    [SerializeField] GameObject boss;        // �{�X
    [SerializeField] Transform randRespawnA; // ���X�|�[���͈�A
    [SerializeField] Transform randRespawnB; // ���X�|�[���͈�B
    [SerializeField] Transform minCameraPos;
    [SerializeField] Transform maxCameraPos;
    [SerializeField] float xRadius;
    [SerializeField] float yRadius;
    [SerializeField] float distMinSpawnPos;

    [SerializeField] GameObject player;      // �v���C���[�̏��
    GameObject enemy;                        // �G�l�~�[�̏��

    public GameObject Enemy {  get { return enemy; } }

    public bool BossFlag { get { return bossFlag; } set { bossFlag = value; } }

    //public bool IsBossDead { get { return bossFlag; } set { isBossDead = value; } } 
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // �{�X���\��
        //boss.SetActive(false);
        // �v���C���[�̃I�u�W�F�N�g�������Ď擾
        player = GameObject.Find("PlayerSample");
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

            Vector3? spawnPos = GenerateEnemySpawnPosition(spawnPostions.minRange,spawnPostions.maxRange);


            if (spawnPos != null)
            {// �Ԃ�l��null����Ȃ��Ƃ�
                Instantiate(boss, (Vector3)spawnPos, Quaternion.identity);
            }

            isSpawnBoss = true;

            bossFlag = false;
        }

        if (isBossDead)
        {// �{�X��|����(��)
            //bossFlag = false;
            //boss.SetActive(false);

            // �x��ČĂяo��
            Invoke(nameof(ChengScene), 1.5f);
        }

        if (spawnCnt < maxSpawnCnt)
        {// �X�|�[���񐔂����E�ɒB���Ă��邩
            num++;
            if (num % createCnt == 0)
            {
                num = 0;

                Vector2 minPlayer =
                    new Vector2(player.transform.position.x - xRadius, player.transform.position.y - yRadius);

                Vector2 maxPlayer =
                    new Vector2(player.transform.position.x + xRadius, player.transform.position.y + yRadius);

                // �����_���Ȉʒu�𐶐�
                var spawnPostions = CreateEnemySpawnPosition(minPlayer, maxPlayer);

                // �����_���Ȉʒu�𐶐�
                //Vector3 spawnPos = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY));

                Vector3 ? spawnPos = GenerateEnemySpawnPosition(spawnPostions.minRange,spawnPostions.maxRange);

                if (spawnPos != null)
                {
                    spawnCnt++;
                    int listNum = Random.Range(0, enemyList.Count);

                    // ����
                    enemy = Instantiate(enemyList[listNum], (Vector3)spawnPos, Quaternion.identity);

                    enemy.GetComponent<EnemyController>().Players.Add(player);

                    if (enemy.GetComponent<Rigidbody2D>().gravityScale != 0)
                    {
                        enemy.GetComponent<EnemyController>().enabled = false;

                        // ������
                        enemy.GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }
        }
        else
        {
            Debug.Log("�������E");
        }
    }

    /// <summary>
    /// �V�[���ύX
    /// </summary>
    private void ChengScene()
    {// �V�[���ύX
        SceneManager.LoadScene("Result ueno");
    }

    [ContextMenu("CrushEnemy")]
    /// <summary>
    ///  �G���j
    /// </summary>
    public void CrushEnemy()
    {
        crushNum++;

        spawnCnt--;
        AddXp();

        EnemyController enemy = GetComponent<EnemyController>();

        //Debug.Log(crushNum);
        if(enemy.name == "boss")
        {
            DeathBoss();
        }
        else if (crushNum >= 15)
        {// ���j����15�ȏ�ɂȂ�����(��)

            bossFlag = true;

            //boss.SetActive(true);
            //Debug.Log("�{�X�łĂ���");
            //crushNum = 0;
        }
    }

    /// <summary>
    /// �o���l���Z
    /// </summary>
    public void AddXp()
    {
        xp += 100;
        if (xp >= requiredXp)
        {// �K�v�o���l���𒴂�����

            // �K�v�o���l���𑝂₷
            requiredXp += xp;
            Debug.Log(requiredXp);
            // ���x���A�b�v�֐���
            UpLevel();
        }
    }

    /// <summary>
    /// ���x���A�b�v
    /// </summary>
    public void UpLevel()
    {
        level++;
        Debug.Log("���x���A�b�v:" + level);
    }

    [ContextMenu("DeathBoss")]
    public void DeathBoss()
    {
        // �{�X�t���O��ύX
        bossFlag = false;
        // ���񂾔���ɂ���
        isBossDead = true;
    }

    private void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.DrawWireCube(player.transform.position, new Vector3(distMinSpawnPos * 2,yRadius * 2));
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(player.transform.position, new Vector3(xRadius * 2, yRadius * 2));
        }
    }

    private (Vector3 minRange,Vector3 maxRange) CreateEnemySpawnPosition(Vector3 minPoint,Vector3 maxPoint)
    {
        Vector3 minRange = minPoint, maxRange = maxPoint;
        if (minPoint.y < randRespawnA.position.y)
        {
            minRange.y = randRespawnA.position.y;
        }
        else
        {
            minRange.y = minPoint.y;
        }

        if (minPoint.x < randRespawnA.position.x)
        {
            minRange.x = randRespawnA.position.x;
        }
        else
        {
            minRange.x = minPoint.x;
        }

        if (maxPoint.y > randRespawnB.position.y)
        {
            maxRange.y = randRespawnB.position.y;
        }
        else
        {
            maxRange.y = maxPoint.y;
        }

        if (maxPoint.x > randRespawnB.position.x)
        {
            maxRange.x = randRespawnB.position.x;
        }
        else
        {
            minRange.x = minPoint.x;
        }

        return (minRange, maxRange);
    }

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
}
