//====================
//プレスマシンのスクリプト
//Aouther:y-miura
//Date:2025/06/20
//====================

using DG.Tweening;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class PressMachine : ObjectBase
{
    [SerializeField] GameObject machineFragment;
    PlayerBase player;
    bool isBroken = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.gameObject.transform.DOMoveY(0, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    public override void ApplyDamage()
    {
        if (isBroken == true)
        {
            return;
        }

        isBroken = true;
        player = GameObject.FindWithTag("Player").GetComponent<PlayerBase>();

        GameObject fragment; //破片のオブジェクト
        fragment = Instantiate(machineFragment, new Vector2(this.transform.position.x, this.transform.position.y - 1.5f), this.transform.rotation); //破片オブジェクトを生成(position.xはドアの位置、yはドアより少し下の位置)

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

        Destroy(this.gameObject);//ドアを壊す
        DestroyFragment(fragment.transform);
    }

    public override void FadeFragment(Transform fragment)
    {
        if (fragment == null)
        {
            return;
        }

        fragment.GetComponent<Renderer>().material.DOFade(0, 6);
    }

    public async void DestroyFragment(Transform fragment)
    {
        await Task.Delay(6000);
        Destroy(fragment.gameObject);
    }
}
