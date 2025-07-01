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
    [SerializeField] GameObject machineFragment;
    PlayerBase player;
    Rigidbody2D rigidbody2d;
    bool isBroken = false;
    public float addPow;
    public float pullPow;
    public bool isPowerd = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MovePress();
    }

    private void Update()
    {

    }

    public async void MovePress()
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

    public override void TurnOnPower()
    {
       isPowerd = false;
    }
}
