//===================
//ジャンプ台のスクリプト
//Author:y-miura
//===================

using UnityEngine;

public class JumpStep : MonoBehaviour
{
    //加える力の量の変数
    public float addPow;

    /// <summary>
    /// 触れたオブジェクトに力を加える処理
    /// </summary>
    /// <param name="collision">触れたオブジェクト</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            //ぶつかったオブジェクトに、addPow分の力を加える
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, addPow));
        }
    }
}
