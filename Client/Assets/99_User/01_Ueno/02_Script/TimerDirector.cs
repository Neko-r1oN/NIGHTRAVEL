using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerDirector : MonoBehaviour
{
    #region �����ݒ�
    [Header("�����ݒ�")]
    [SerializeField] float minute = 5;
    float second;

    [SerializeField] GameObject timerObj; // �^�C�}�[�e�L�X�g�̐e
    [SerializeField] Text timer;          // �^�C�}�[�e�L�X�g
    #endregion

    private void Start()
    {
        second = minute * 60;
        GameManager.Instance.InvokeRepeating("DecreaseGeneratInterval", 0.1f, 60f);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.BossFlag == false)
        {
            second -= Time.deltaTime;
            var span = new TimeSpan(0,0,(int)second);
            minute = span.Minutes;
            timer.text = span.ToString(@"mm\:ss");
        }

        if (minute <= 0 && GameManager.Instance.BossFlag == false)
        {// �Q�[���^�C�}�[��0�ȉ��ɂȂ�����&�{�X���o�����ĂȂ�������
            timerObj.SetActive(false);
            // �{�X�o��
            GameManager.Instance.BossFlag = true;
        }
        else if(GameManager.Instance.IsSpawnBoss == true)
        {
            timerObj.SetActive(false);
        }
    }
}
