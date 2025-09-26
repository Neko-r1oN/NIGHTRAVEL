//===================
// プレススクリプト
// Author:Nishiura
// Date:2025/07/07
//===================
using UnityEngine;

public class Press : MonoBehaviour
{
    PlayerBase playerBase;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤーがつぶしエリアに入った場合
        if (collision.transform.tag == "Player" && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            playerBase = collision.gameObject.GetComponent<PlayerBase>();   // つぶされ対象からPlayerBaseを取得
            bool isGround =  playerBase.GetGrounded();  // PlayerBaseから接地判定変数を取得

            if (!isGround) return;  // 接地していない場合、処理しない

            // プレイヤーの最大HP20%相当のダメージに設定
            int damage = Mathf.FloorToInt(playerBase.MaxHP * 0.2f);
            playerBase.ApplyDamage(damage);
            playerBase.MoveCheckPoint();    // つぶれたプレイヤーをチェックポイントへ戻す

            Debug.Log("You Pancaked");
        }
    }
}
