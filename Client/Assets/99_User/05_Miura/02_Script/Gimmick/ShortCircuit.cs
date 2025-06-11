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
        //int maxLife = (int)player.HP; //プレイヤーの最大HP

        ////プレイヤーのダメージ計算
        //int damage = Mathf.FloorToInt(maxLife * 0.05f); //プダメージ量をfloatからintに変換
        //player.DealDamage(this.gameObject, damage, Vector2.zero); //トラップごとに対応したダメージ対応処理を呼び出し
    }

    void EnemyHitDamage()
    {
        int HP = enemyController.maxHP; //敵の最大HP

        //==========
        //ダメージ計算
        //==========
        int damage = Mathf.FloorToInt(HP * 0.05f); //ダメージ量を
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
        {
            enemyController = collision.GetComponent<EnemyController>(); //enemyControllerに、当たったものを代入
            InvokeRepeating("EnemyHitDamage", 0.1f, 0.5f); //「EnemyHitDamage」を、0.1秒間、0.5秒ごとに繰り返す
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
