//--------------------------------------------------------------
// 弾用処理 [ Bullet.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //--------------------------
    // フィールド

    private float timer;                    // 累計生存時間
    private const float DEATH_TIME = 10f;   // 破棄時間
    private PlayerBase player;              // 発射キャラの情報

    /// <summary>
    /// 弾の速さ
    /// </summary>
    public float Speed {  get; set; }

    //--------------------------
    // メソッド

    /// <summary>
    /// 更新処理
    /// </summary>
    private void Update()
    {
        timer += Time.deltaTime;

        if(timer >= DEATH_TIME) Destroy(gameObject);
    }

    /// <summary>
    /// プレイヤー情報の取得
    /// </summary>
    /// <param name="player"></param>
    public void SetPlayer(PlayerBase player)
    {
        this.player = player;
    }

    /// <summary>
    /// 弾の当たり判定
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            collision.gameObject.GetComponent<EnemyBase>().ApplyDamage(player.Power, player.gameObject);
        }
        else if (collision.gameObject.tag == "Object")
        {
            collision.gameObject.GetComponent<ObjectBase>().ApplyDamage();
        }else if(collision.gameObject.tag == "Default" || collision.gameObject.tag == "ground")
        {
            Destroy(gameObject);
        }
    }
}
