using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    int crushNum;
    bool bossFlag = false;
    int xp;
    int level;

    [SerializeField] GameObject boss;

    public bool BossFlag { get { return bossFlag; } set { bossFlag = value; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boss.SetActive(false);
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

        if(crushNum >= 5 && bossFlag)
        {// �{�X��|����(��)
            bossFlag = false;
            //boss.SetActive(false);

            // �x��ČĂяo��
            Invoke(nameof(ChengScene), 1.5f);
        }
    }

    private void ChengScene()
    {// �V�[���ύX
        SceneManager.LoadScene("Result ueno");
    }

    public void CrushEnemy()
    {
        crushNum++;
        xp += 100;
        if (xp >= 100)
        {
            UpLevel();
        }

        Debug.Log(crushNum);
        if (crushNum >= 4)
        {// ���j����15�ȏ�ɂȂ�����(��)
            bossFlag = true;
            boss.SetActive(true);
            Debug.Log("�{�X�łĂ���");
            //crushNum = 0;
        }
    }

    public void UpLevel()
    {
        level++;
        Debug.Log("���x���A�b�v:" + level);
    }
}
