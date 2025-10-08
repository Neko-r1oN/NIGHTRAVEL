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
    float elapsedTime = 0f; // �o�ߎ���
    float initMinute;       // �����ݒ�(��)


    [SerializeField] GameObject timerObj; // �^�C�}�[�e�L�X�g�̐e
    public GameObject TimerObj { get { return timerObj; } set { timerObj = value; } }
    [SerializeField] Text timer;          // �^�C�}�[�e�L�X�g

    public Text Timer { get { return timer; } set { timer = value; } }

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

    private void Start()
    {
        second = minute * 60;
        initMinute = minute * 60;
        //GameManager.Instance.InvokeRepeating("DecreaseGeneratInterval", 0.1f, 60f);
    }



    // Update is called once per frame
    async void Update()
    {
        if (GameManager.Instance.IsGameStart)
        {
            if (minute <= 0 && second <= 0 && SpawnManager.Instance.IsBossActive == false)
            {// �Q�[���^�C�}�[��0�ȉ��ɂȂ�����&�{�X���o�����ĂȂ�������
                timerObj.SetActive(false);
                // �{�X�o��
                //SpawnManager.Instance.IsBossActive = true;
                SpawnManager.Instance.SpawnBoss();
            }
            else if (SpawnManager.Instance.IsBossActive == true)
            {// �{�X���o��������
             // �^�C�}�[�폜
                timerObj.SetActive(false);
            }
            else if (SpawnManager.Instance.IsBossActive == false)
            {// �{�X���o�����Ă��Ȃ�������
                elapsedTime += Time.deltaTime; // ���t���[�����Ԃ����Z

                // �^�C�}�[(UI)�̍X�V
                UpdateTimerDisplay();

                if (elapsedTime > 60)
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

        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            timerObj.SetActive (false);
        }
    }

    /// <summary>
    /// �^�C�}�[�X�V
    /// </summary>
    public async void UpdateTimerDisplay()
    {
        // �ȍ~�I�t���C���p
        if (RoomModel.Instance) return;

        // �t���[���̌o�ߎ��ԕ����Z
        second -= Time.deltaTime;
        OnUpdateTimer(second);
        
    }

    /// <summary>
    /// �^�C�}�[�e�L�X�g�X�V
    /// </summary>
    /// <param name="time"></param>
    public void OnUpdateTimer(float time)
    {
        second = time;
        var span = new TimeSpan(0, 0, (int)time);
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
