//====================
// 足場とする箱のスクリプト
// Aouther:y-miura
// Date:2025/07/01
//====================

using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;

public class Box : ObjectBase
{
    [SerializeField] GameObject BoxPrefab;  //箱プレハブ取得
    [SerializeField] GameObject BoxFragment;　//破片エフェクトを取得

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
        {
            //箱を消す
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// 電源オン処理
    /// </summary>
    public override void TurnOnPower()
    {
        // ダメージ付与
        ApplyDamage(BoxFragment, isBroken, new Vector2(0f, 1.5f));
        isBroken = true; // 破壊済みとする
    }

}
