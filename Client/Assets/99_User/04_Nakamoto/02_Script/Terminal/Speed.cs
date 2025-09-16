//--------------------------------------------------------------
// �X�s�[�h�����^�[�~�i�� [ Speed.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using KanKikuchi.AudioManager;
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class Speed : TerminalBase
{

    //------------------------
    // �t�B�[���h

    // �X�s�[�h�p�S�[���|�C���g�I�u�W�F�N�g�̃��X�g
    [SerializeField] List<GameObject> pointList;

    //------------------------
    // ���\�b�h

    /// <summary>
    /// ��������
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // �[���̎�ʂ�ݒ�
        terminalType = EnumManager.TERMINAL_TYPE.Speed;
    }

    /// <summary>
    /// �[���N�����N�G�X�g����
    /// </summary>
    public override async void BootRequest()
    {
        base.BootRequest();

        // �J�E���g�_�E������
        InvokeRepeating("CountDown", 1, 1);

        timerText.text = currentTime.ToString();

        foreach (var point in pointList)
        {   // �e�S�[���|�C���g��\��
            point.SetActive(true);
        }
    }

    /// <summary>
    /// �N������
    /// </summary>
    public override void BootTerminal()
    {
        isUsed = true; // �[���g�p���ɂ���

        if(RoomModel.Instance)
            TerminalManager.Instance.TerminalDatas[terminalID].State = EnumManager.TERMINAL_STATE.Active;
    }

    /// <summary>
    /// ���s���N�G�X�g
    /// </summary>
    public override void FailureRequest()
    {
        foreach (var point in pointList)
        {   // �e�S�[���|�C���g���\��
            point.SetActive(false);
        }

        base.FailureRequest();
    }

    /// <summary>
    /// �S�[���|�C���g�ɐG�ꂽ�ۂ̏���
    /// </summary>
    /// <param name="obj"></param>
    public void HitGoalPoint(GameObject obj)
    {
        if (pointList.Contains(obj))    // �n���ꂽ�I�u�W�F�N�g�����X�g���ɂ������ꍇ
        {
            pointList.Remove(obj);  // �������������
            Destroy(obj);   // �����j�󂷂�

            if (pointList.Count <= 0)
            { // ���X�g����ɂȂ����ꍇ�A��V��t�^����
                CancelInvoke("CountDown");
                GiveRewardRequest();
            }
        }
    }
}
