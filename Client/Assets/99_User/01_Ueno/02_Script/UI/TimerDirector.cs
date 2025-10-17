using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TimerDirector : MonoBehaviour
{
    #region �����ݒ�
    [Header("�����ݒ�")]
    [SerializeField] float minute;
    float second;
    static float elapsedTime = 0f; // �o�ߎ���
    float initMinute;       // �����ݒ�(��)

    public float Second { get { return second; } set { second = value; } }

    #endregion

    private static TimerDirector instance;
    public static TimerDirector Instance
    {
        get
        {
            return instance;
        }
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    async void Update()
    {
        if (GameManager.Instance.IsGameStart)
        {

            elapsedTime += Time.deltaTime; // ���t���[�����Ԃ����Z

            if (elapsedTime > 150)
            {
                // �Q�[�����x���A�b�v���N�G�X�g���M

                ResetTimer();

                // �Q�[�����x���A�b�v
                if (RoomModel.Instance)
                {
                    if (RoomModel.Instance.IsMaster) await RoomModel.Instance.AscendDifficultyAsync();
                    return;
                }
                else
                {
                    LevelManager.Instance.UpGameLevel();
                }

            }
        }
    }

    /// <summary>
    /// �^�C�}�[�����Z�b�g
    /// </summary>
    public void ResetTimer()
    {
        elapsedTime = 0f;
    }
}
