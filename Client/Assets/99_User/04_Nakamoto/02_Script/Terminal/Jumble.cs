//--------------------------------------------------------------
// �����b�N�W�����u���^�[�~�i�� [ Jumble.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using DG.Tweening;
using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class Jumble : TerminalBase
{
    /// <summary>
    /// ��������
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // �[���̎�ʂ�ݒ�
        terminalType = EnumManager.TERMINAL_TYPE.Jumble;
    }

    /// <summary>
    /// �N������
    /// </summary>
    public override void BootTerminal()
    {
        isUsed = true; // �[���g�p���ɂ���
        usingText.text = "IN USE";

        if (RoomModel.Instance)
            TerminalManager.Instance.TerminalDatas[terminalID - 1].State = EnumManager.TERMINAL_STATE.Success;

        // ��������
        if(RoomModel.Instance.IsMaster) SuccessRequest();

        // �^�[�~�i����\��
        terminalSprite.DOFade(0, 2.5f);
        iconSprite.DOFade(0, 2.5f).OnComplete(() => { gameObject.SetActive(false); });
    }
}
