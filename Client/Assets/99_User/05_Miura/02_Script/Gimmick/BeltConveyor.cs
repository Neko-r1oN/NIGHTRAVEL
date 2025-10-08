using UnityEngine;

public class BeltConveyor : MonoBehaviour
{
    //加える力の量の変数
    public float addPow;
    public float addPowEnemy;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        { // 対象がプレイヤーの場合
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(addPow, 0));
        }
       
        if(collision.CompareTag("Enemy"))
        { // 対象が敵の場合
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(addPowEnemy, 0));
        }
       
        if (collision.CompareTag("Object"))
        { // 対象がオブジェクトの場合
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(addPowEnemy, 0));
        }
    }
}
