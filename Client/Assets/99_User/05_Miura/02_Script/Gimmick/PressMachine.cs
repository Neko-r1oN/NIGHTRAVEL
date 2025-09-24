//====================
//�v���X�}�V���̃X�N���v�g
//Aouther:y-miura
//Date:2025/06/20
//====================

using DG.Tweening;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class PressMachine : GimmickBase
{
    PlayerBase player;
    public float addPow;
    public float pullPow;
    public bool isPowerd;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(!isPowerd) return;
        isPowerd = true;

        if(!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
        MovePress();
    }

    void MovePress()
    {
        if (isPowerd == true)
        {
            //Sequence�̃C���X�^���X���쐬
            var sequence = DOTween.Sequence();

            //Append�œ����ǉ����Ă���
            sequence.Append(this.transform.DOMoveY(-addPow, 1))
                     .AppendInterval(1)
                     .Append(this.transform.DOMoveY(pullPow, 2));

            //Play�Ŏ��s
            sequence.Play()
                    .AppendInterval(1)
                    .SetLoops(-1);
        }
    }

    /// <summary>
    /// �M�~�b�N�ċN������
    /// </summary>
    public override void Reactivate()
    {
        DOTween.Clear(this.transform);
        transform.localPosition = Vector3.zero;
        MovePress();
    }
}
