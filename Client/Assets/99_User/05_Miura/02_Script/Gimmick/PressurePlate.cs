//=================
//感圧板のスクリプト
//Aouther:y-miura
//Date:20025/06/18
//=================

using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] GameObject gameObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //仮でオブジェクトを設置して、最初は非表示状態にする
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// プレイヤーまたは敵が感圧版を踏んだ場合の処理
    /// </summary>
    /// <param name="collision">触れたオブジェクト</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player")||collision.gameObject.CompareTag("Enemy"))
        {
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// プレイヤーまたは敵が感圧版から離れた場合の処理
    /// </summary>
    /// <param name="collision">触れたオブジェクト</param>
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            gameObject.SetActive(false);
        }
    }
}
