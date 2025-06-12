using UnityEngine;
using UnityEngine.UIElements;

public class ShortCircuit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    PlayerBase playerBase;
    EnemyBase enemyBase;
    Vector2 pos = Vector2.zero;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void HitDamage()
    {
        //SampleChara_CopyのmaxLifeをintに変換
        int maxLife= (int)playerBase.MaxHP;

        int damage = Mathf.FloorToInt(maxLife * 0.05f);
        playerBase.DealDamage(this.gameObject,damage);
        Debug.Log(damage);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerBase = GameObject.FindWithTag("Player").GetComponent<PlayerBase>();
            InvokeRepeating("HitDamage",0.1f,0.5f);
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //Debug.Log("敵が漏電フィールドに当たった");
            enemyBase = GameObject.FindWithTag("Enemy").GetComponent<EnemyBase>();
            InvokeRepeating("HitDamage", 0.1f, 0.5f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CancelInvoke();
        }
        if(collision.gameObject.CompareTag("Enemy"))
        {
            CancelInvoke();
        }
    }
}
