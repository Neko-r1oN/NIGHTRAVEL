//--------------------------------------------------------------
// �G�l�~�[�o���^�[�~�i�� [ EnemyTerminal.cs ]
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
        base.BootTerminal();

        //++ �N�����N�G�X�g���T�[�o�[�ɑ��M
    }
}
