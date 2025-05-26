using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    GameManager gameManager;
    GameObject player;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        player = GameObject.Find("PlayerSample");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "ground")
        {// 床についたら
            // 透明化解除
            this.GetComponent<SpriteRenderer>().enabled = true;
            
            // プレイヤーリストにプレイヤーの情報を格納
            this.GetComponent<EnemyController>().enabled = true;
        }
    }
}
