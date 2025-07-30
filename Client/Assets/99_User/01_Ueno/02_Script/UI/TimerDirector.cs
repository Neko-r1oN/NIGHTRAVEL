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
    float initMinute;       // �����ݒ�(��)

    [SerializeField] GameObject timerObj; // �^�C�}�[�e�L�X�g�̐e
    [SerializeField] Text timer;          // �^�C�}�[�e�L�X�g
    #endregion

    private void Start()
    {
        second = minute * 60;
        initMinute = minute * 60;
        GameManager.Instance.InvokeRepeating("DecreaseGeneratInterval", 0.1f, 60f);
    }

    // Update is called once per frame
    ////void Update()
    ////{
    ////    if (Terminal.Instance.IsTerminal)
    ////    {
    ////        timerObj.SetActive(false);
    ////    }
    ////    else
    ////    {
    ////        timerObj.SetActive(true);
    ////    }

        if (minute <= 0 && second <= 0 && SpawnManager.Instance.IsSpawnBoss == false)
        {// �Q�[���^�C�}�[��0�ȉ��ɂȂ�����&�{�X���o�����ĂȂ�������
            timerObj.SetActive(false);
            // �{�X�o��
            SpawnManager.Instance.IsSpawnBoss = true;
        }
        else if (SpawnManager.Instance.IsSpawnBoss == true)
        {// �{�X���o��������
            // �^�C�}�[�폜
            timerObj.SetActive(false);
        }
        else if (SpawnManager.Instance.IsSpawnBoss == false)
        {// �{�X���o�����Ă��Ȃ�������
            elapsedTime += Time.deltaTime; // ���t���[�����Ԃ����Z

    ////        // �^�C�}�[(UI)�̍X�V
    ////        UpdateTimerDisplay();

    ////        if(elapsedTime > initMinute)
    ////        {
    ////            // �Q�[�����x���A�b�v
    ////            LevelManager.Instance.UpGameLevel();
    ////            ResetTimer();
    ////        }
    ////    }
    ////}

    /// <summary>
    /// �^�C�}�[�X�V
    /// </summary>
    public void UpdateTimerDisplay()
    {
        // �t���[���̌o�ߎ��ԕ����Z
        second -= Time.deltaTime;
        var span = new TimeSpan(0, 0, (int)second);
        minute = span.Minutes;
        timer.text = span.ToString(@"mm\:ss");
    }

    /// <summary>
    /// �^�C�}�[�����Z�b�g
    /// </summary>
    public void ResetTimer()
    {
        elapsedTime = 0f;
    }
}
