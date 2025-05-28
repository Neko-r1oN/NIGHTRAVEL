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
    Vector3 spawnPos;       // �����_���Ő�������ʒu
    [SerializeField] int bossCount;
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
    [SerializeField] Transform xRadius;
    [SerializeField] Transform yRadius;

    GameObject player;                       // �v���C���[�̏��
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
            for (int i = 0; i < bossCount; i++)
            {
                float minX, maxX;
                float minY, maxY;

                if (minCameraPos.position.y < randRespawnA.position.y)
                {
                    minY = randRespawnA.position.y;
                }
                else
                {
                    minY = minCameraPos.position.y;
                }

                if (maxCameraPos.position.y > randRespawnB.position.y)
                {
                    maxY = randRespawnB.position.y;
                }
                else
                {
                    maxY = maxCameraPos.position.y;
                }

                if (minCameraPos.position.x < randRespawnA.position.x)
                {
                    minX = randRespawnA.position.x;
                }
                else
                {
                    minX = minCameraPos.position.x;
                }

                if (maxCameraPos.position.x > randRespawnB.position.x)
                {
                    maxX = randRespawnB.position.x;
                }
                else
                {
                    maxX = maxCameraPos.position.x;
                }

                // �X�e�[�W������K���Ȉʒu���擾
                float x = Random.Range(minX, maxX);
                float y = Random.Range(minY, maxY);

                // �����_���Ȉʒu�𐶐�
                spawnPos = new Vector3(x, y);

                Instantiate(boss, new Vector3(x, y), Quaternion.identity);
            }

            isSpawnBoss = true;

            bossFlag = false;
        }

        if (isBossDead)
        {// �{�X��|����(��)
            //bossFlag = false;
            boss.SetActive(false);

            // �x��ČĂяo��
            Invoke(nameof(ChengScene), 1.5f);
        }

        if (spawnCnt < maxSpawnCnt)
        {// �X�|�[���񐔂����E�ɒB���Ă��邩
            num++;
            if (num % createCnt == 0)
            {
                num = 0;

                // �X�e�[�W������K���Ȉʒu���擾
                float x = Random.Range(randRespawnA.position.x, randRespawnB.position.x);
                float y = Random.Range(randRespawnA.position.y, randRespawnB.position.y);
                float z = Random.Range(randRespawnA.position.z, randRespawnB.position.z);
                // �����_���Ȉʒu�𐶐�
                spawnPos = new Vector3(x, y, z);

                float minX,minY,maxX,maxY;





                // �v���C���[�̈ʒu�ƃ����_�������̈ʒu�Ƃ̋���
                float distanceOfPlayer =
                    Vector3.Distance(player.transform.position, spawnPos);

                if (distanceOfPlayer >= 8 && distanceOfPlayer < 13)
                {// ������10����Ă�����
                    spawnCnt++;

                    int listNum = Random.Range(0, enemyList.Count);

                    // ����
                    enemy = Instantiate(enemyList[listNum], new Vector3(x, y, z), Quaternion.identity);
                    
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

        Debug.Log(crushNum);
        if (crushNum >= 15)
        {// ���j����15�ȏ�ɂȂ�����(��)

            BossFlag = true;

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
        // �{�X�̃J�E���g�����炷
        bossCount--;

        // �Ăяo���ꂽ�Ƃ��{�X�J�E���g��0�ȉ��Ȃ�
        if(bossCount <= 0)
        {
            // �{�X�t���O��ύX
            bossFlag = false;
            // ���񂾔���ɂ���
            isBossDead = true;
        }

        Debug.Log("���񂾂��");
    }


}
