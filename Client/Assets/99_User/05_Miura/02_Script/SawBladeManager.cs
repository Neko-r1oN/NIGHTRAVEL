using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using Shared.Interfaces.StreamingHubs;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class SawBladeManager : GimmickBase
{
    Vector2 pos;

    [SerializeField] GameObject sparkObj;
    [SerializeField] SawBlade sawBlade;
    [SerializeField] AudioSource sawBladeSE;

    // ���͒l
    public float addPower;

    // �ړ����x
    public float moveSpeed;

    // �d������
    public bool isPowerd;

    void Start()
    {
        isPowerd = true;

        // ���̃Q�[���I�u�W�F�N�g�̃|�W�V�������擾
        pos = this.transform.position;

        if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster) TurnOnPower();
    }

    /// <summary>
    /// �ۂ̂��ړ��֐�
    /// </summary>
    private void MoveBlade()
    {
        //Sequence�̃C���X�^���X���쐬
        var sequence = DOTween.Sequence();

        transform.DOMoveX((pos.x - addPower), 1)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo)
                    .OnStepComplete(() =>
                    {
                        // ���[�v1�������ƂɌĂ΂��
                        Vector3 scale = transform.localScale;
                        scale.x *= -1;
                        transform.localScale = scale;
                    });
    }

    /// <summary>
    /// �d���I���֐�
    /// </summary>
    public override void TurnOnPower()
    {
        sawBladeSE.Play();

        //�ΉԂ��U�炷
        sparkObj.SetActive(true);

        //�ۂ̂����ړ�������
        MoveBlade();

        //�ۂ̂�����]������
        sawBlade.StateRotet(); //SawBlade�N���X��StateRotet�֐����Ăяo��
    }

    /// <summary>
    /// �M�~�b�N�N�����N�G�X�g
    /// </summary>
    void RequestActivateGimmick()
    {
        TurnOnPowerRequest(CharacterManager.Instance.PlayerObjSelf);
    }


    /// <summary>
    /// �M�~�b�N�ċN������
    /// </summary>
    public override void Reactivate()
    {
        transform.position = pos;
        TurnOnPower();
    }

    /// <summary>
    /// [���A���^�C�������p] �M�~�b�N�X�V
    /// </summary>
    /// <param name="gimmickData"></param>
    public override void UpdateGimmick(GimmickData gimmickData)
    {
        sawBlade.transform.DOMove(gimmickData.Position, CharacterManager.Instance.UpdateSec).SetEase(Ease.Linear);
    }
}
