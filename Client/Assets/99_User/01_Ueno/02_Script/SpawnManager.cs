using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private void Start()
    {
        this.GetComponent<SpriteRenderer>().enabled = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "ground")
        {// 床についたら
            // 透明化解除
            this.GetComponent<SpriteRenderer>().enabled = true;
            
            // プレイヤーリストにプレイヤーの情報を格納
            this.GetComponent<EnemyBase>().enabled = true;
        }
    }
}
