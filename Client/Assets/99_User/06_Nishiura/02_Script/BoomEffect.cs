//===================
// 爆発エフェクトスクリプト
// Author:Nishiura
// Date:2025/07/03
//===================
using UnityEngine;

public class BoomEffect : MonoBehaviour
{
    PlayerBase playerBase;
    EnemyBase enemyBase;

    Vector2 pos;

    private void Start()
    {
        // このゲームオブジェクトのポジションを取得
        pos = this.gameObject.transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        { 
            Invoke("DeleteThis", 0.6f);
            playerBase = collision.gameObject.GetComponent<PlayerBase>();
            // プレイヤーの最大HP30%相当のダメージに設定
            int damage = Mathf.FloorToInt(playerBase.MaxHP * 0.3f);
            playerBase.ApplyDamage(damage, pos);


        }
        else if(collision.transform.tag =="Enemy")
        {
            enemyBase = collision.gameObject.GetComponent<EnemyBase>();

            // 敵の最大HP30%相当のダメージに設定
            int damage = Mathf.FloorToInt(enemyBase.MaxHP * 0.3f);
            enemyBase.ApplyDamageRequest(damage);

            Invoke("DeleteThis", 0.6f);
        }
    }

    /// <summary>
    /// エフェクト消去処理
    /// </summary>
    private void DeleteThis()
    {
        Destroy(this.gameObject);
    }
}
