//===================
// ソーブレードスクリプト
// Author:Nishiura
// Date:2025/07/02
//===================
using DG.Tweening;
using UnityEngine;

public class SawBlade : MonoBehaviour
{
    PlayerBase playerBase;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            playerBase = collision.gameObject.GetComponent<PlayerBase>();
            // プレイヤーの最大HP15%相当のダメージに設定
            int damage = Mathf.FloorToInt(playerBase.MaxHP * 0.15f);
            playerBase.ApplyDamage(damage, this.gameObject.transform.position);
            Debug.Log("Hit SawBlade");
        }
    }

    private void MoveBlade()
    {
        //Sequenceのインスタンスを作成
        var sequence = DOTween.Sequence();

        //Appendで動作を追加していく
        sequence.Append(this.transform.DOMoveY(-1, 1))
                .AppendInterval(1)
                .Append(this.transform.DOMoveY(1, 2));

        //Playで実行
        sequence.Play()
                .AppendInterval(1)
                .SetLoops(-1);
    }
}
