//--------------------------------------------------------------
// �{�X�o���^�[�~�i�� [ Jumble.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using DG.Tweening;
using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class Boss : TerminalBase
{
    /// <summary>
    /// ��������
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // �[���̎�ʂ�ݒ�
        terminalType = EnumManager.TERMINAL_TYPE.Boss;
    }

    /// <summary>
    /// �N������
    /// </summary>
    public override void BootTerminal()
    {
        if (!GameManager.Instance.IsCanSpawnBoss)
        {
            usingText.text = "CAN NOT USE";
            return;
        }

        isUsed = true; // �[���g�p���ɂ���
        usingText.text = "IN USE";

        if (RoomModel.Instance)
            TerminalManager.Instance.TerminalDatas[terminalID].State = EnumManager.TERMINAL_STATE.Success;

        // �}�X�^�N���C�A���g�ȊO�͏��������Ȃ�
        if(!RoomModel.Instance)
        {
            SpawnManager.Instance.SpawnBoss();
        }
        else if (RoomModel.Instance && !RoomModel.Instance.IsMaster) return;

        // �{�X��������
        SpawnManager.Instance.SpawnBoss();
    }
}
