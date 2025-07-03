//===================
// 地雷スクリプト
// Author:Nishiura
// Date:2025/07/02
//===================
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Mine : MonoBehaviour
{
    [SerializeField] GameObject boomEffect; // 爆発エフェクトプレハブ

    PlayerBase playerBase;
    EnemyBase enemyBase;

    Vector2 pos;

    private void Start()
    {
        // このゲームオブジェクトのポジションを取得
        pos = this.gameObject.transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            Instantiate(boomEffect);    // 爆発エフェクトを生成
            Destroy(this.gameObject);   // 自身を破壊
            Debug.Log("Boomed Mine");
        }else if (collision.transform.tag == "Enemy")
        {
            Destroy(this.gameObject);
            Debug.Log("Boomed Mine");
        }
    }
}
