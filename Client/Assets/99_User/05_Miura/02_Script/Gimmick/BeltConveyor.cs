using UnityEngine;

public class BeltConveyor : MonoBehaviour
{
    //加える力の量の変数
    public float addPow;
    public float addPowEnemy;
    private void OnTriggerStay2D(Collider2D collision)
    {
        // 対象がプレイヤーの場合
        if(collision.CompareTag("Player") && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            //ぶつかったオブジェクトに、addPow分の力を加える
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(addPow, 0));
        }
        // 対象が敵の場合
        if(collision.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(addPowEnemy, 0));
        }

        if (collision.CompareTag("Object"))
        {
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(addPowEnemy, 0));
        }
    }
}
