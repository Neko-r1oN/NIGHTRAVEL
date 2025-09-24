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
        if(!isPowerd) return;
        isPowerd = true;

        if(!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
        MovePress();
    }

    void MovePress()
    {
        if (isPowerd == true)
        {
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
    }

    /// <summary>
    /// ギミック再起動処理
    /// </summary>
    public override void Reactivate()
    {
        DOTween.Clear(this.transform);
        transform.localPosition = Vector3.zero;
        MovePress();
    }
}
