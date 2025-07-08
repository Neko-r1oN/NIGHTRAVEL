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
        if(isPowerd==false)
        {
            return;
        }

        MovePress();
        Debug.Log(isPowerd);
    }

    private void Update()
    {

    }

    public void MovePress()
    {
        if (isPowerd == true)
        {
            Debug.Log(isPowerd);

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
        if (isPowerd == false)
        {
            Debug.Log(isPowerd);
            return;
        }

        //switch (isPowerd)
        //{
        //    case true:
        //        Debug.Log(isPowerd);

        //        //Sequence�̃C���X�^���X���쐬
        //        var sequence = DOTween.Sequence();

        //        //Append�œ����ǉ����Ă���
        //        sequence.Append(this.transform.DOMoveY(-addPow, 1))
        //                 .AppendInterval(1)
        //                 .Append(this.transform.DOMoveY(pullPow, 2));

        //        //Play�Ŏ��s
        //        sequence.Play()
        //                .AppendInterval(1)
        //                .SetLoops(-1);
        //        break;

        //    case false:
        //        break;

        //    default:
        //}
    }

    public override void TurnOnPower(int t)
    {
        isPowerd = true;
    }

    public override void TruggerRequest()
    {
        throw new System.NotImplementedException();
    }
}
