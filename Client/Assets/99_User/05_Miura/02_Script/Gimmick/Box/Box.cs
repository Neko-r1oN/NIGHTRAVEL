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

    protected override void ApplyDamage()
    {
        if (isBroken == true) return;

        PlayerBase player = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>();
        //破片オブジェクトを生成(position.xは箱の位置、yは箱より少し上の位置)
        GameObject fragment = Instantiate(BoxFragment, new Vector2(this.transform.position.x, this.transform.position.y + 1.5f), this.transform.rotation);

        for (int i = 0; i < fragment.transform.childCount; i++)
        {//fragmentの子の数だけループ
            if (this.transform.position.x - player.transform.position.x >= 0)
            {//箱の位置と比べて、プレイヤーが左側にいたら
                fragment.transform.GetChild(i).GetComponent<Rigidbody2D>().AddForce(new Vector2(1000, 200)); //右側に破片を飛ばす
            }
            else
            {//箱の位置と比べて、プレイヤーが右側にいたら
                fragment.transform.GetChild(i).GetComponent<Rigidbody2D>().AddForce(new Vector2(-1000, -200)); //左側に破片を飛ばす
            }
            FadeFragment(fragment.transform.GetChild(i));
        }

        //箱を壊す
        Destroy(this.gameObject);
        GetComponent<AudioSource>().Play();
    }

    override public void DestroyFragment(Transform obj)
    {
        if (obj == null) return;
        Destroy(obj.gameObject);
    }

    /// <summary>
    /// 壊れるオブジェクトの破片をフェードする関数
    /// </summary>
    /// <param name="fragment">破片</param>
    override public void FadeFragment(Transform fragment)
    {
        if (fragment == null) return;

        // 6秒かけて破片をフェードアウトさせ、その後破壊する
        fragment.GetComponent<Renderer>().material.DOFade(0, 6).OnComplete(() =>
        { DestroyFragment(fragment.transform);});
    }

    /// <summary>
    /// ボックス破壊処理
    /// </summary>
    public override void TurnOnPower()
    {
        // ダメージ付与
        ApplyDamage();
        isBroken = true; // 破壊済みとする
    }

}
