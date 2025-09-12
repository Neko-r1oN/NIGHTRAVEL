//--------------------------------------------------------------
// ����^�[�~�i�� [ Deal.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using Shared.Interfaces.StreamingHubs;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Deal : TerminalBase
{
    //--------------------------------
    // �t�B�[���h

    /// <summary>
    /// �_���[�W����
    /// </summary>
    private const float DEAL_RATE = 0.5f;

    //--------------------------------
    // ���\�b�h

    /// <summary>
    /// ��������
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // �[���̎�ʂ�ݒ�
        terminalType = EnumManager.TERMINAL_TYPE.Deal;
    }

    /// <summary>
    /// �N�����N�G�X�g
    /// </summary>
    public override async void BootRequest()
    {
        base.BootRequest();

        DealDamage();
    }

    /// <summary>
    /// �N������
    /// </summary>
    public override void BootTerminal()
    {
        isUsed = true; // �[���g�p���ɂ���
        TerminalManager.Instance.TerminalDatas[terminalID].State = EnumManager.TERMINAL_STATE.Success;

        // �����b�N�������N�G�X�g
        GiveRewardRequest();
    }

    /// <summary>
    /// ����_���[�W
    /// </summary>
    private void DealDamage()
    {
        // �ő�̗͊����̃_���[�W���󂯂�
        int damage = (int)(CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>().MaxHP * DEAL_RATE);
        damage = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>().hp - damage <= 0 ? 1 : damage;

        CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>().hp -= damage;
        UIManager.Instance.PopDamageUI(damage, CharacterManager.Instance.PlayerObjSelf.transform.position, true);
    }
}
