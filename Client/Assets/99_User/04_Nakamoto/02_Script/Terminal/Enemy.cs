//--------------------------------------------------------------
// �G�l�~�[�o���^�[�~�i�� [ Enemy.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class Enemy : TerminalBase
{
    //--------------------------------
    // �t�B�[���h

    //--------------------------------
    // ���\�b�h

    /// <summary>
    /// ��������
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // �[���̎�ʂ�ݒ�
        terminalType = EnumManager.TERMINAL_TYPE.Enemy;
    }

    /// <summary>
    /// �N������
    /// </summary>
    public override void BootTerminal()
    {
        isUsed = true; // �[���g�p���ɂ���

        if (RoomModel.Instance)
            TerminalManager.Instance.TerminalDatas[terminalID].State = EnumManager.TERMINAL_STATE.Active;
    }
}
