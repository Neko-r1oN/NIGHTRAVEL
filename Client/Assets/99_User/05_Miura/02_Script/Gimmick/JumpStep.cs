//===================
//ジャンプ台のスクリプト
//Author:y-miura
//===================

using UnityEngine;

public class JumpStep : MonoBehaviour
{
    PlayerBase sample;

    private void Start()
    {
        sample=GetComponent<PlayerBase>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {

        }
    }
}
