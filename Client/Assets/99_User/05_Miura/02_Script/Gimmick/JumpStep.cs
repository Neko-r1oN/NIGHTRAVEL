//===================
//ジャンプ台のスクリプト
//Author:y-miura
//===================

using UnityEngine;

public class JumpStep : MonoBehaviour
{
    Player player;

    private void Start()
    {
        player=GetComponent<Player>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {

        }
    }
}
