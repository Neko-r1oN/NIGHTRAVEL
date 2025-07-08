//===================
// ソーブレードスクリプト
// Author:Nishiura
// Date:2025/07/02
//===================
using DG.Tweening;
using UnityEngine;

public class SawBlade : GimmickBase
{
    PlayerBase playerBase;

    Vector2 pos;
    // 加力値
    public float addPower;

    // 電源判定
    public bool isPowerd;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // このゲームオブジェクトのポジションを取得
        pos = this.gameObject.transform.position;
        // 電源オンの場合、移動開始
        if (isPowerd == true) MoveBlade();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isPowerd == true && collision.transform.tag == "Player")
        {
            playerBase = collision.gameObject.GetComponent<PlayerBase>();
            // プレイヤーの最大HP15%相当のダメージに設定
            int damage = Mathf.FloorToInt(playerBase.MaxHP * 0.15f);
            playerBase.ApplyDamage(damage, pos);
            Debug.Log("Hit SawBlade");
        }
    }

    /// <summary>
    /// 丸のこ移動関数
    /// </summary>
    private void MoveBlade()
    {
        //Sequenceのインスタンスを作成
        var sequence = DOTween.Sequence();

        //Appendで動作を追加していく
        sequence.Append(this.transform.DOMoveX((pos.x - addPower), 1))
                .AppendInterval(0.01f)
                .Append(this.transform.DOMoveX(pos.x , 1));

        //Playで実行
        sequence.Play()
                .AppendInterval(0.01f)
                .SetLoops(-1);
    }

    /// <summary>
    /// 電源オン関数
    /// </summary>
    public override void TurnOnPower(int t)
    {
        isPowerd = true;
        MoveBlade();
    }

    public override void TruggerRequest()
    {
        throw new System.NotImplementedException();
    }
}
