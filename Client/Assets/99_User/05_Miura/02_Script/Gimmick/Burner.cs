using UnityEngine;

public class Burner : MonoBehaviour
{
    PlayerBase player;
    EnemyBase enemy;

    private void Start()
    {

    }

    /// <summary>
    /// 触れたオブジェクトに炎上効果を付与する処理
    /// </summary>
    /// <param name="collision">触れたオブジェクト</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            player = GetComponent<PlayerBase>();

            Debug.Log("プレイヤーに炎上状態を付与");
        }

        if(collision.CompareTag("Enemy"))
        {
            enemy = GetComponent<EnemyBase>();

            Debug.Log("敵に炎上状態を付与");
        }
    }
}
