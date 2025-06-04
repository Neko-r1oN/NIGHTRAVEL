using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerDirector : MonoBehaviour
{
    public float gameTimer = 300;
    public float minute = 5;
    public float second;

    [SerializeField] Text timer;
    [SerializeField] Text bossText;

    private void Start()
    {
        second = minute * 60;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.BossFlag == false)
        {
            second -= Time.deltaTime;
            var span = new TimeSpan(0,0,(int)second);
            timer.text = span.ToString(@"mm\:ss");
        }

        if (minute <= 0 && GameManager.Instance.BossFlag == false)
        {// �Q�[���^�C�}�[��0�ȉ��ɂȂ�����&�{�X���o�����ĂȂ�������
            bossText.text = "BOSS";
            bossText.gameObject.SetActive(true);

            timer.gameObject.SetActive(false);
            // �{�X�o��
            GameManager.Instance.BossFlag = true;
        }
    }
}
