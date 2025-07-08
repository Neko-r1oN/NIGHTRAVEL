//====================
//プレスマシンのスクリプト
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

            //Sequenceのインスタンスを作成
            var sequence = DOTween.Sequence();

            //Appendで動作を追加していく
            sequence.Append(this.transform.DOMoveY(-addPow, 1))
                     .AppendInterval(1)
                     .Append(this.transform.DOMoveY(pullPow, 2));

            //Playで実行
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

        //        //Sequenceのインスタンスを作成
        //        var sequence = DOTween.Sequence();

        //        //Appendで動作を追加していく
        //        sequence.Append(this.transform.DOMoveY(-addPow, 1))
        //                 .AppendInterval(1)
        //                 .Append(this.transform.DOMoveY(pullPow, 2));

        //        //Playで実行
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
