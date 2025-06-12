using UnityEngine;
using UnityEngine.UIElements;

public class ShortCircuit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //[SerializeField] GameObject ElectronicEffect;
    PlayerBase sample;
    EnemyController enemyController;
    Vector2 pos = Vector2.zero;
    //int count = 0;

    void Start()
    {
        sample=GameObject.Find("PlayerSample").GetComponent<PlayerBase>();
        enemyController=GameObject.FindWithTag("Enemy").GetComponent<EnemyController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void HitDamage()
    {
        //count++;
        //Debug.Log("プレイヤーが漏電フィールドに当たった" + count);

        //SampleChara_CopyのmaxLifeをintに変換
        int maxLife= (int)sample.MaxHP;

        int damage = Mathf.FloorToInt(maxLife * 0.05f);
        sample.DealDamage(this.gameObject,damage);
        //enemyController.ApplyDamage(damage,) //敵が当たった時のダメージ
        Debug.Log(damage);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            InvokeRepeating("HitDamage",0.1f,0.5f);
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //Debug.Log("敵が漏電フィールドに当たった");
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
