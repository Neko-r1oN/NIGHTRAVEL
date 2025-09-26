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

        if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster) Invoke("Request", 0.5f);
    }

    public void Request()
    {
        TurnOnPowerRequest(CharacterManager.Instance.PlayerObjSelf);
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

        if(addPower<=0)
        {
            sparkObj.SetActive(false);
        }
    }

    /// <summary>
    /// �d���I���֐�
    /// </summary>
    public override void TurnOnPower()
    {
        //�ΉԂ��U�炷
        sparkObj.SetActive(true);

        if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster) MoveBlade();

        //�ۂ̂�����]������
        sawBlade.StateRotet(); //SawBlade�N���X��StateRotet�֐����Ăяo��

        if(addPower==0)
        {//addpower��0��0�ȉ���������

            //�ΉԂ��\���ɂ���
            sparkObj.SetActive(false);

            //�ۂ̂����ړ�������
            MoveBlade();
        }
    }

    /// <summary>
    /// �M�~�b�N�ċN������
    /// </summary>
    public override void Reactivate()
    {
        transform.position = pos;
        TurnOnPower();
    }
}
