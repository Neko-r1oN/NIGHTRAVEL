using System;
using UnityEngine;
using UnityEngine.UI;

public class TimerDirector : MonoBehaviour
{
    public float gameTimer = 90;

    [SerializeField] Text text;

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.BossFlag == false)
        {
            // ���쎞�ԍX�V����
            gameTimer -= Time.deltaTime;
            text.text = "" + Math.Floor(gameTimer);
        }
        
        if (gameTimer <= 0 && GameManager.Instance.BossFlag == false)
        {// �Q�[���^�C�}�[��0�ȉ��ɂȂ�����&�{�X���o�����ĂȂ�������
            // �^�C�}�[�Œ�
            gameTimer = 0;
            text.text = "Boss";
            // �{�X�o��
            GameManager.Instance.BossFlag = true;
        }
    }
}
