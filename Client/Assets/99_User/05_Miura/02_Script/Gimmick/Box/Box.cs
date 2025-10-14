//====================
// 足場とする箱のスクリプト
// Aouther:y-miura
// Date:2025/07/01
//====================

using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using static Rewired.Glyphs.GlyphSet;

public class Box : ObjectBase
{
    [SerializeField] GameObject BoxPrefab;  //箱プレハブ取得
    [SerializeField] GameObject BoxFragment;　//破片エフェクトを取得
    PlayerBase playerBase;
    EnemyBase enemyBase;

    // 破壊判定
    bool isBroken = false;

    /// <summary>
    /// 箱を消す処理
    /// </summary>
    /// <param name="collision">触れたオブジェクト</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //触れたもののタグが「Abyss」だったら
        if(collision.CompareTag("Gimmick/Abyss"))
        {//ステージ外に出たら
            //箱を消す
            Destroy(this.gameObject);
        }

        // プレイヤーがつぶしエリアに入った場合
        if (collision.transform.tag == "Player" && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            playerBase = collision.gameObject.GetComponent<PlayerBase>();   // つぶされ対象からPlayerBaseを取得

            // プレイヤーの最大HP20%相当のダメージに設定
            int damage = Mathf.FloorToInt(playerBase.MaxHP * 0.2f);
            playerBase.ApplyDamage(damage);
        }

        // 敵がつぶしエリアに入った場合
        if (collision.transform.tag == "Enemy")
        {
            enemyBase = collision.gameObject.GetComponent<EnemyBase>();   // つぶされ対象からEnemyBaseを取得

            // 敵に大量のダメージを与えて、実質即死にする
            int damage = 9999;

            if ((RoomModel.Instance && RoomModel.Instance.IsMaster) || !RoomModel.Instance)
            {
                enemyBase.ApplyDamageRequest(damage, null, false, false);
            }
        }
    }

    protected override void ApplyDamage()
    {

    }

    override public void DestroyFragment(Transform obj)
    {

    }

    /// <summary>
    /// 壊れるオブジェクトの破片をフェードする関数
    /// </summary>
    /// <param name="fragment">破片</param>
    override public void FadeFragment(Transform fragment)
    {

    }

    /// <summary>
    /// ボックス破壊処理
    /// </summary>
    public override void TurnOnPower()
    {
        isBroken = true; // 破壊済みとする
    }
}
