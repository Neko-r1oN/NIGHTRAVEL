using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerDirector : MonoBehaviour
{
    #region �����ݒ�
    [Header("�����ݒ�")]
    [SerializeField] float minute;
    float second;
    float elapsedTime = 0f; // �o�ߎ���
    float halfMinute;

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

            elapsedTime += Time.deltaTime; // ���t���[�����Ԃ����Z
            TimeSpan timeSpan = GetCurrentMinutesAndSeconds();

            float currentTime = (float)timeSpan.TotalSeconds;

            if (currentTime >= (minute * 60) / 2 && minute > 0)
            {
                halfMinute = minute / 2;
                LevelManager.Instance.UpGameLevel();
                ResetTimer();
            }
            else if(currentTime >= (second) / 2)
            {
                halfMinute = minute / 2;
                LevelManager.Instance.UpGameLevel();
                ResetTimer();
            }
        }

        if (minute <= 0 && second <= 0 && GameManager.Instance.BossFlag == false)
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

    /// <summary>
    /// ���݂̕��ƕb���擾���܂��B
    /// </summary>
    /// <returns>���݂̎��ԁi���ƕb�j��TimeSpan�ŕԂ��܂��B</returns>
    public TimeSpan GetCurrentMinutesAndSeconds()
    {
        return TimeSpan.FromSeconds(elapsedTime);
    }

    /// <summary>
    /// �^�C�}�[�����Z�b�g���܂��B
    /// </summary>
    public void ResetTimer()
    {
        elapsedTime = 0f;
    }
}
