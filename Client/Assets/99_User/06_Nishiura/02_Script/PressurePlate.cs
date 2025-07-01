//==================
// 感圧版スクリプト
// Author:Nishiura
// Date:2025/07/01
//==================
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    // 押下判定変数
    private bool isPushed = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // プレイヤーが触れた場合かつ押された状態でない場合
        if (collision.transform.tag == "Player" && isPushed != true)
        {
            isPushed = true;
            Debug.Log("Plate Activated");
        }
    }
}
