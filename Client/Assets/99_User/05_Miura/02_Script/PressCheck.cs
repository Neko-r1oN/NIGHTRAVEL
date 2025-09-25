//=========================================
// エレベーターで挟まる接触判定オブジェクトのスクリプト
// Aouther:y-miura
// Date:2025/09/25
//=========================================

using UnityEngine;

public class PressCheck : MonoBehaviour
{
    public bool isToutch = false;

    public bool ObjectPressCheck() { return isToutch; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    /// <summary>
    /// 天井、地面に触れた時の処理
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {//プレイヤーが触れたら
            isToutch = true; //触れたことにする
        }
        if(collision.gameObject.CompareTag("Enemy"))
        {//敵が触れたら
            isToutch = true; //触れたことにする
        }
    }

    /// <summary>
    /// 天井/床から離れたときの処理
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {//プレイヤーが離れたら
            isToutch = false;
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {//敵が離れたら
            isToutch = true;
        }
    }
}
