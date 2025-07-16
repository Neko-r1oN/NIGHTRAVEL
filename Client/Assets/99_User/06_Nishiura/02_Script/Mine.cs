//===================
// 地雷スクリプト
// Author:Nishiura
// Date:2025/07/02
//===================
using UnityEngine;

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            Instantiate(boomEffect, pos, Quaternion.identity);    // 爆発エフェクトを生成
            Destroy(this.gameObject);   // 自身を破壊
            Debug.Log("Boomed Mine");
        }
        else if (collision.transform.tag == "Enemy")
        {
            Instantiate(boomEffect, pos, Quaternion.identity);    // 爆発エフェクトを生成
            Destroy(this.gameObject);   // 自身を破壊
            Debug.Log("Boomed Mine");
        }
    }
}
