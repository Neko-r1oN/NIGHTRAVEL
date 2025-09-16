//--------------------------------------------------------------
// �G���[�g�o���^�[�~�i�� [ Elite.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class Elite : TerminalBase
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
        terminalType = EnumManager.TERMINAL_TYPE.Elite;
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
