using UnityEngine;

public class BeltConveyor : MonoBehaviour
{
    //加える力の量の変数
    public float addPow;
    public float addPowEnemy;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && collision.gameObject == CharacterManager.Instance.PlayerObjSelf||collision.CompareTag("Object"))
        {
            //ぶつかったオブジェクトに、addPow分の力を加える
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(addPow, 0));
        }
        if(collision.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(addPowEnemy, 0));
        }
    }
}
