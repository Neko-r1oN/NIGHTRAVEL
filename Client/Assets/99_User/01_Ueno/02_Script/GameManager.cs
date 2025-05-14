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
    //Vector3 spawnPos;

    [SerializeField] GameObject boss;
    [SerializeField] GameObject enemy;
    [SerializeField] Transform randRespawnA;
    [SerializeField] Transform randRespawnB;

    GameObject player;

    public bool BossFlag { get { return bossFlag; } set { bossFlag = value; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boss.SetActive(false);
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

        /*float distanceOfPlayer =
            Vector3.Distance(player.transform.position, spawnPos);

        Debug.Log("����:" + Math.Floor(distanceOfPlayer));*/

        num++;
        //Debug.Log(num);

        if (num % createCnt == 0)
        {
            Debug.Log("�o�Ă���");
            num = 0;

            float x = Random.Range(randRespawnA.position.x, randRespawnB.position.x);
            float y = Random.Range(randRespawnA.position.y, randRespawnB.position.y);
            float z = Random.Range(randRespawnA.position.z, randRespawnB.position.z);

            if (spawnCnt <= maxSpawnCnt)
            {
                spawnCnt++;
                Debug.Log(spawnCnt);
                Instantiate(enemy, new Vector3(x, y, z), enemy.transform.rotation);
            }
        }
    }

    private void ChengScene()
    {// �V�[���ύX
        SceneManager.LoadScene("Result ueno");
    }

    public void CrushEnemy()
    {
        crushNum++;
        AddXp();

        Debug.Log(crushNum);
        if (crushNum >= 4)
        {// ���j����15�ȏ�ɂȂ�����(��)
            bossFlag = true;
            boss.SetActive(true);
            Debug.Log("�{�X�łĂ���");
            //crushNum = 0;
        }
    }

    public void AddXp()
    {
        xp += 100;
        if (xp >= requiredXp)
        {
            requiredXp += xp;
            Debug.Log(requiredXp);
            UpLevel();
        }
    }

    public void UpLevel()
    {
        level++;
        Debug.Log("���x���A�b�v:" + level);
    }

    /*public void RespawnEnemy()
    {
        Instantiate(enemy);
    }*/
}
