//===================
//ジャンプ台のスクリプト
//Author:y-miura
//===================

using UnityEngine;

public class JumpStep : MonoBehaviour
{
    public float addPow;

    private void Start()
    {

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, addPow));
        }
    }
}
