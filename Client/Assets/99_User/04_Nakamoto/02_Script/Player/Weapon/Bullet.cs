//--------------------------------------------------------------
// 弾用処理 [ Bullet.cs ]
// Author：Kenta Nakamoto
// 引用：https://assetstore.unity.com/packages/2d/characters/metroidvania-controller-166731
//--------------------------------------------------------------
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //--------------------------
    // フィールド

    private float timer;                    // 累計生存時間
    private bool orbitFlag;                 // 追尾フラグ
    private PlayerBase player;              // 発射キャラの情報
    [SerializeField] private float trackingStart;            // 追尾開始時間

    /// <summary>
    /// 弾の速さ
    /// </summary>
    public float Speed {  get; set; }

    /// <summary>
    /// 加速係数
    /// </summary>
    public float AcceleCoefficient { get; set; }

    //--------------------------
    // メソッド

    /// <summary>
    /// 更新処理
    /// </summary>
    private void Update()
    {
        timer += Time.deltaTime;

        if(trackingStart <= timer && !orbitFlag)
        {   // 軌道変化
            orbitFlag = true;

            var target = player.FetchNearObjectWithTag("Enemy");

            // 敵に向けて加速
            if(target != null)
            {
                var vec = target.transform.position - transform.position;
                vec = vec.normalized;
                gameObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, vec);
                gameObject.GetComponent<Rigidbody2D>().linearVelocity = (vec * Speed) * AcceleCoefficient;
            }
        }

        if(timer >= 10f) Destroy(gameObject);
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
            collision.gameObject.GetComponent<EnemyBase>().ApplyDamage(player.Power, player.transform);
            Destroy(gameObject);
        }else if (collision.gameObject.tag == "Object")
        {
            collision.gameObject.GetComponent<ObjectBase>().ApplyDamage();
        }
    }
}
