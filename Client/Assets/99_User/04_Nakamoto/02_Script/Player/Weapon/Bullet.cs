//--------------------------------------------------------------
// 弾用処理 [ Bullet.cs ]
// Author：Kenta Nakamoto
//--------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

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
    /// プレイヤー情報取得
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
            // ボスかどうか判定
            if (collision.gameObject.GetComponent<EnemyBase>().IsBoss)
            {
                // 二回攻撃の抽選
                if (player.LotteryRelic(RELIC_TYPE.Rugrouter))
                {
                    collision.GetComponent<EnemyBase>().ApplyDamageRequest(player.Power, player.gameObject, true, true, player.LotteryDebuff());
                    collision.GetComponent<EnemyBase>().ApplyDamageRequest(player.Power / 2, player.gameObject, true, true, player.LotteryDebuff());
                }
                else
                {
                    collision.GetComponent<EnemyBase>().ApplyDamageRequest(player.Power, player.gameObject, true, true, player.LotteryDebuff());
                }
            }
            else
            {
                // ボスの場合はボスエリアに入っているか確認
                if (player.IsBossArea)
                {
                    // 二回攻撃の抽選
                    if (player.LotteryRelic(RELIC_TYPE.Rugrouter))
                    {
                        collision.GetComponent<EnemyBase>().ApplyDamageRequest(player.Power, player.gameObject, true, true, player.LotteryDebuff());
                        collision.GetComponent<EnemyBase>().ApplyDamageRequest(player.Power / 2, player.gameObject, true, true, player.LotteryDebuff());
                    }
                    else
                    {
                        collision.GetComponent<EnemyBase>().ApplyDamageRequest(player.Power, player.gameObject, true, true, player.LotteryDebuff());
                    }
                }
            }
        }
        else if (collision.gameObject.tag == "Object")
        {
            collision.gameObject.GetComponent<ObjectBase>().ApplyDamage();
        }
        else if(collision.gameObject.layer == LayerMask.NameToLayer("Gimmick") || collision.gameObject.tag == "ground")
        {
            Destroy(gameObject);
        }
    }


}
