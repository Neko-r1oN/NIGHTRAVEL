using DG.Tweening;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class SealedDoor : ObjectBase
{
    [SerializeField] GameObject DoorFragment;　//破片エフェクトを取得
    PlayerBase player;
    bool isBroken = false;


    /// <summary>
    /// ドアを壊したときの処理
    /// </summary>
    public override void ApplyDamage()
    {
        if (isBroken == true)
        {
            return;
        }

        isBroken = true;
        player = GameObject.FindWithTag("Player").GetComponent<PlayerBase>();

        GameObject fragment; //破片のオブジェクト

        //破片オブジェクトを生成(position.xはドアの位置、yはドアより少し下の位置)
        fragment = Instantiate(DoorFragment, new Vector2(this.transform.position.x, this.transform.position.y - 1.5f), this.transform.rotation);

        for (int i = 0; i < fragment.transform.childCount; i++)
        {//fragmentの子の数だけループ
            if (this.transform.position.x - player.transform.position.x >= 0)
            {//ドアの位置と比べて、プレイヤーが左側にいたら
                fragment.transform.GetChild(i).GetComponent<Rigidbody2D>().AddForce(new Vector2(1000, 200)); //右側に破片を飛ばす
            }
            else
            {//ドアの位置と比べて、プレイヤーが右側にいたら
                fragment.transform.GetChild(i).GetComponent<Rigidbody2D>().AddForce(new Vector2(-1000, -200)); //左側に破片を飛ばす
            }
            FadeFragment(fragment.transform.GetChild(i));
        }

        //ドアを壊す
        Destroy(this.gameObject);

        //破片を消す
        DestroyFragment(fragment.transform);
    }


    /// <summary>
    /// 破片をフェードアウトする処理
    /// </summary>
    /// <param name="fragment">破片プレハブ</param>
    public override void FadeFragment(Transform fragment)
    {
        if (fragment == null)
        {
            return;
        }

        //6秒かけてフェードアウトする
        fragment.GetComponent<Renderer>().material.DOFade(0, 6);
    }

    /// <summary>
    /// 破片を消す処理
    /// </summary>
    /// <param name="fragment">破片プレハブ</param>
    public async void DestroyFragment(Transform fragment)
    {
        await Task.Delay(6000);

        if (fragment == null)
        {
            return;
        }

        Destroy(fragment.gameObject);
    }
}