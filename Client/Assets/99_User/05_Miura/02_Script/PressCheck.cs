//=========================================
// エレベーターで挟まる接触判定オブジェクトのスクリプト
// Aouther:y-miura
// Date:2025/09/25
//=========================================

using UnityEngine;

public class PressCheck : MonoBehaviour
{
    Press press;
    PlayerBase playerBase;
    public bool isToutch = false;

    public bool ObjectPressCheck() { return isToutch; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    /// <summary>
    /// 天井、地面に触れた時の処理
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {//プレイヤーが触れたら
            isToutch = true; //触れたことにする

            playerBase = collision.gameObject.GetComponent<PlayerBase>();   // つぶされ対象からPlayerBaseを取得

            if (!isToutch) return;  // 接地していない場合、処理しない

            // プレイヤーの最大HP20%相当のダメージに設定
            int damage = Mathf.FloorToInt(playerBase.MaxHP * 0.2f);
            playerBase.ApplyDamage(damage);
            playerBase.MoveCheckPoint();    // つぶれたプレイヤーをチェックポイントへ戻す

        }
        if (collision.gameObject.CompareTag("Enemy"))
        {//敵が触れたら
            isToutch = true; //触れたことにする
        }
    }

    /// <summary>
    /// 天井/床から離れたときの処理
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {//プレイヤーが離れたら
            isToutch = false;
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {//敵が離れたら
            isToutch = true;
        }
    }
}
