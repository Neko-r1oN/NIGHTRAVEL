using System;
using UnityEngine;
using UnityEngine.UI;

public class TimerDirector : MonoBehaviour
{
    public float gameTimer = 90;

    GameManager gameManager;

    [SerializeField] Text text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameObject.Find("GameManager")
            .GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.BossFlag == false)
        {
            // ���쎞�ԍX�V����
            gameTimer -= Time.deltaTime;
            text.text = "" + Math.Floor(gameTimer);
        }
        
        if (gameTimer <= 0 && gameManager.BossFlag == false)
        {// �Q�[���^�C�}�[��0�ȉ��ɂȂ�����&�{�X���o�����ĂȂ�������
            // �^�C�}�[�Œ�
            gameTimer = 0;
            text.text = "Boss";
            // �{�X�o��
            gameManager.BossFlag = true;

            Debug.Log("�{�X�o��");
        }
    }
}
