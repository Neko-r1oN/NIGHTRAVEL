using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    GameManager gameManager;
    GameObject player;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        player = GameObject.Find("DrawCharacter");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "ground")
        {// 床についたら
            // 透明化解除　
            gameManager.Enemy.GetComponent<SpriteRenderer>().enabled = true;
            // プレイヤーリストにプレイヤーの情報を格納
            gameManager.Enemy.GetComponent<EnemyController>().Players.Add(player);
        }
    }
}
