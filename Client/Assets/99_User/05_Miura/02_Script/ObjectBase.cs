//========================
//壊れるオブジェクトの親クラス
//Author:y-Miura
//date:2025/03/17
//========================

using DG.Tweening;
using Rewired;
using UnityEngine;
using UnityEngine.UIElements;

abstract public class ObjectBase : GimmickBase
{
    /// <summary>
    /// 壊れるオブジェクトのダメージ関数
    /// </summary>
    virtual protected void ApplyDamage(GameObject fragmentObj, bool isBroken, Vector2 pos)
    {
        if (isBroken == true) return;

        PlayerBase player = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>();
        //破片オブジェクトを生成(position.xは箱の位置、yは箱より少し上の位置)
        GameObject fragment = Instantiate(fragmentObj, new Vector2(this.transform.position.x, this.transform.position.y + pos.y), this.transform.rotation);

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
    }

    virtual protected void DestroyFragment(Transform obj)
    {
        if (obj == null) return;
        Destroy(obj.gameObject);
    }

    /// <summary>
    /// 壊れるオブジェクトの破片をフェードする関数
    /// </summary>
    /// <param name="fragment">破片</param>
    virtual protected void FadeFragment(Transform fragment)
    {
        if (fragment == null) return;

        // 6秒かけて破片をフェードアウトさせ、その後破壊する
        fragment.GetComponent<Renderer>().material.DOFade(0, 6).OnComplete(() =>
        { DestroyFragment(fragment.transform); });
    }
}
