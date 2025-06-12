using UnityEngine;
using UnityEngine.UIElements;

public class ShortCircuit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    Player player;
    EnemyController enemyController;
    Vector2 pos = Vector2.zero;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void PlayerHitDamage()
    {
        //int maxLife = (int)player.maxHP; //プレイヤーの最大HP

        ////プレイヤーのダメージ計算
        //int damage = Mathf.FloorToInt(maxLife * 0.05f); //ダメージ量をfloatからintに変換
        //player.DealDamage(this.gameObject, damage, Vector2.zero); //トラップごとに対応したダメージ対応処理を呼び出し
    }

    void EnemyHitDamage()
    {
        int maxHP = enemyController.maxHP; //敵の最大HP

        //敵のダメージ計算
        int damage = Mathf.FloorToInt(maxHP * 0.05f);
        enemyController.ApplyDamage(damage); //敵のダメージ対応関数を呼び出し
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {//プレイヤー/敵が漏電フィールドに当たったら
        if (collision.gameObject.CompareTag("Player"))
        {//プレイヤーが漏電フィールドに当たったら
            player = collision.GetComponent<Player>(); //playerに、当たったものを代入
            InvokeRepeating("PlayerHitDamage", 0.1f, 0.5f); //「PlayerHitDamage」を、0.1秒間、0.5秒ごとに繰り返す
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {//敵が漏電フィールドに当たったら
            enemyController = collision.GetComponent<EnemyController>(); //enemyControllerに、当たったものを代入
            int HP = enemyController.HP;

            InvokeRepeating("EnemyHitDamage", 0.1f, 0.5f); //「EnemyHitDamage」を、0.1秒後に、0.5秒ごとに繰り返す
            Debug.Log(HP);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {//プレイヤー/敵が漏電フィールドから離れたら
        if (collision.gameObject.CompareTag("Player"))
        {//プレイヤーが漏電フィールドから離れたら
            player = collision.GetComponent<Player>(); //playerに、当たったものを代入
            CancelInvoke(); //「PlayerHitDmage」のリピートを止める
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {//敵が漏電フィールドから離れたら
            enemyController = collision.GetComponent<EnemyController>(); //enemyControllerに、当たったものを代入
            CancelInvoke(); //「EnemyHitDamage」のリピートを止める
        }
    }
}
