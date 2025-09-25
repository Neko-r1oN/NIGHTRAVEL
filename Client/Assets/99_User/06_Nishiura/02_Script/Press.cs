//===================
// プレススクリプト
// Author:Nishiura
// Date:2025/07/07
//===================
using UnityEngine;

public class Press : MonoBehaviour
{
    PlayerBase playerBase;

    [SerializeField] PressCheck pressUP;
    [SerializeField] PressCheck pressDown;

    private void OnTriggerStay2D(Collider2D collision)
    {
        bool isPressUP = pressUP.gameObject.GetComponent<PressCheck>().isToutch;
        bool isPressDown = pressDown.gameObject.GetComponent<PressCheck>().isToutch;

        // プレイヤーがつぶしエリアに入った場合
        if (collision.gameObject.CompareTag("Player") && isPressUP == true || isPressDown==true)
        {
            playerBase = collision.gameObject.GetComponent<PlayerBase>();   // つぶされ対象からPlayerBaseを取得                         

            // プレイヤーの最大HP70%相当のダメージに設定
            int damage = Mathf.FloorToInt(playerBase.MaxHP * 0.7f);
            playerBase.ApplyDamage(damage);
            playerBase.MoveCheckPoint();    // つぶれたプレイヤーをチェックポイントへ戻す

            Debug.Log("You Pancaked");
        }
    }
}
