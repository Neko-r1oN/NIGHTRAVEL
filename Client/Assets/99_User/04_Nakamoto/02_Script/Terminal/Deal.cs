//--------------------------------------------------------------
// ����^�[�~�i�� [ Deal.cs ]
// Author�FKenta Nakamoto
//--------------------------------------------------------------
using DG.Tweening;
using Shared.Interfaces.StreamingHubs;
using UnityEngine;

public class Deal : TerminalBase
{
    //--------------------------------
    // �t�B�[���h

    /// <summary>
    /// �_���[�W����
    /// </summary>
    private const float DEAL_RATE = 0.3f;

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
        int damage = (int)((float)CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>().MaxHP * DEAL_RATE);
        if (CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>().hp - damage <= 0) return;
        DealDamage(damage);

        base.BootRequest();
    }

    /// <summary>
    /// �N������
    /// </summary>
    public override void BootTerminal()
    {
        isUsed = true; // �[���g�p���ɂ���
        usingText.text = "IN USE";

        if (RoomModel.Instance)
            TerminalManager.Instance.TerminalDatas[terminalID-1].State = EnumManager.TERMINAL_STATE.Success;

        // �����b�N�������N�G�X�g
        SuccessRequest();

        // �^�[�~�i����\��
        terminalSprite.DOFade(0, 2.5f);
        iconSprite.DOFade(0, 2.5f).OnComplete(() => { gameObject.SetActive(false); });
    }

    /// <summary>
    /// ����_���[�W
    /// </summary>
    private void DealDamage(int damage)
    {
        // �ő�̗͊����̃_���[�W���󂯂�
        CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>().ApplyDamage(damage);
        UIManager.Instance.PopDamageUI(damage, CharacterManager.Instance.PlayerObjSelf.transform.position, true);
    }
}
