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

    void HitPlayerDamage()
    {
        int maxHP = playerBase.MaxHP;

        int damage = Mathf.FloorToInt(maxHP * 0.05f);
        playerBase.ApplyDamage(damage);
        Debug.Log(playerBase.HP);
    }
    void HitEnemyDamage()
    {
        int maxLife = enemyBase.MaxHP;

        int damage = Mathf.FloorToInt(maxLife * 0.05f);
        enemyBase.ApplyDamage(damage);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerBase = collision.gameObject.GetComponent<PlayerBase>();
            InvokeRepeating("HitPlayerDamage", 0.1f,0.5f);
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //Debug.Log("敵が漏電フィールドに当たった");
            enemyBase = collision.gameObject.GetComponent<EnemyBase>();
            InvokeRepeating("HitEnemyDamage", 0.1f, 0.5f);
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
