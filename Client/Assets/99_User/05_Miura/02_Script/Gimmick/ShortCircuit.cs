using UnityEngine;
using UnityEngine.UIElements;

public class ShortCircuit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    PlayerBase playerBase;
    EnemyBase enemyBase;

    [SerializeField] AudioSource shortCircuitSE;

    Vector2 pos = Vector2.zero;

    /// <summary>
    /// プレイヤーのダメージ量計算処理
    /// </summary>
    void HitPlayerDamage()
    {
        //プレイヤーの最大HPを代入
        int maxHP = playerBase.MaxHP;
        int HP=playerBase.HP;

        //ダメージ量は最大HPの5%
        int damage = Mathf.FloorToInt(maxHP * 0.05f);

        //damageが0より小さいか0だったら
        if(damage <= 0)
        {//damegeを1にする
            damage = 1;
            playerBase.ApplyDamage(damage);
        }

        //もしプレイヤーのHPが0を下回ったか0だったら
        if(HP<=0)
        {
            shortCircuitSE.Stop();
        }

        //PlayerBaseのApplyDamgeを呼び出す
        playerBase.ApplyDamage(damage);
    }

    /// <summary>
    /// 敵のダメージ量計算処理
    /// </summary>
    void HitEnemyDamage()
    {
        //敵の最大HPを代入
        int maxLife = enemyBase.MaxHP;
        int HP=enemyBase.HP;

        //ダメージ量は最大HPの5%
        int damage = Mathf.FloorToInt(maxLife * 0.05f);

        //damageが0より小さいか0だったら
        if (damage <= 0)
        {//damegeを1にする
            damage = 1;
            enemyBase.ApplyDamageRequest(damage);
        }

        //もし敵のHPが0を下回ったか0だったら
        if (HP <= 0)
        {
            shortCircuitSE.Stop();
        }

        //EnemyBaseのApplyDamageを呼び出す
        enemyBase.ApplyDamageRequest(damage);
    }

    /// <summary>
    /// 漏電フィールドでダメージを与える処理
    /// </summary>
    /// <param name="collision">触れたオブジェクト</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {//「Player」タグが付いたオブジェクトが触れたら
            playerBase = collision.gameObject.GetComponent<PlayerBase>();

            InvokeRepeating("HitPlayerDamage", 0.1f,0.5f);
            shortCircuitSE.Play();
        }
        if (collision.gameObject.CompareTag("Enemy") && (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster))
        {//「Enemy」タグが付いたオブジェクトが触れたら
            enemyBase = collision.gameObject.GetComponent<EnemyBase>();

            InvokeRepeating("HitEnemyDamage", 0.1f, 0.5f);
            shortCircuitSE.Play();
        }
    }

    /// <summary>
    /// 漏電フィールドを離れた時の処理
    /// </summary>
    /// <param name="collision">触れたオブジェクト</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CancelInvoke();
            shortCircuitSE.Stop();
        }
        if(collision.gameObject.CompareTag("Enemy"))
        {
            CancelInvoke();
            shortCircuitSE.Stop();
        }
    }
}
