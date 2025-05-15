using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
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

    [SerializeField] GameObject boss;
    [SerializeField] GameObject enemy;
    [SerializeField] Transform randRespawnA;
    [SerializeField] Transform randRespawnB;

    GameObject player;

    public bool BossFlag { get { return bossFlag; } set { bossFlag = value; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // �{�X���\��
        boss.SetActive(false);
        // �v���C���[�̃I�u�W�F�N�g�������Ď擾
        player = GameObject.Find("DrawCharacter");
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Return))
        {
            // ���j�����Z
            crushNum++;
            Debug.Log(crushNum);
            if(crushNum >= 15)
            {// ���j����15�ȏ�ɂȂ�����(��)
                bossFlag = true;
                //square.SetActive(true);
                crushNum = 0;
            }
        }*/

        if (crushNum >= 5 && bossFlag)
        {// �{�X��|����(��)
            bossFlag = false;
            //boss.SetActive(false);

            // �x��ČĂяo��
            Invoke(nameof(ChengScene), 1.5f);
        }

        if (spawnCnt < maxSpawnCnt)
        {// �X�|�[���񐔂����E�ɒB���Ă��邩
            num++;

            if (num % createCnt == 0)
            {
                //Debug.Log("�o�Ă���");
                num = 0;

                // �X�e�[�W������K���Ȉʒu���擾
                float x = Random.Range(randRespawnA.position.x, randRespawnB.position.x);
                float y = Random.Range(randRespawnA.position.y, randRespawnB.position.y);
                float z = Random.Range(randRespawnA.position.z, randRespawnB.position.z);
                // �����_���Ȉʒu�𐶐�
                spawnPos = new Vector3(x, y, z);
                
                // �v���C���[�̈ʒu�ƃ����_�������̈ʒu�Ƃ̋���
                float distanceOfPlayer =
                    Vector3.Distance(player.transform.position, spawnPos);

                if (distanceOfPlayer >= 18)
                {// ������20����Ă�����
                    Debug.Log("����:" + Math.Floor(distanceOfPlayer));
                    spawnCnt++;
                    Debug.Log(spawnCnt);
                    // ����
                    Instantiate(enemy, new Vector3(x, y, z), enemy.transform.rotation);
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

    /// <summary>
    ///  �G���j
    /// </summary>
    public void CrushEnemy()
    {
        crushNum++;
        spawnCnt--;
        AddXp();

        //Debug.Log(crushNum);
        /*if (crushNum >= 15)
        {// ���j����15�ȏ�ɂȂ�����(��)
            bossFlag = true;
            boss.SetActive(true);
            Debug.Log("�{�X�łĂ���");
            //crushNum = 0;
        }*/
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
            //Debug.Log(requiredXp);
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
        //Debug.Log("���x���A�b�v:" + level);
    }

    /*public void RespawnEnemy()
    {
        Instantiate(enemy);
    }*/
}
