//====================
//足場とする箱のスクリプト
//Aouther:y-miura
//Date:2025/07/01
//====================

using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;

public class Box : ObjectBase
{
    [SerializeField] GameObject BoxPrefab;  //箱プレハブ取得
    [SerializeField] GameObject BoxFragment;　//破片エフェクトを取得
    PlayerBase player;

    bool isBroken = false;

    void Start()
    {
        //SpawnBox関数を繰り返し呼び出して、箱を繰り返し生成する
        InvokeRepeating("SpawnBox", 5.5f, 10);
    }

    /// <summary>
    /// 箱を生成する処理
    /// </summary>
    public void SpawnBox()
    {
        GameObject boxObj = BoxPrefab;

        //箱を生成する
        Instantiate(boxObj, new Vector2(28.0f, 27.0f), Quaternion.identity);
    }

    /// <summary>
    /// 箱を消す処理
    /// </summary>
    /// <param name="collision">触れたオブジェクト</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //触れたもののタグが「Abyss」だったら
        if(collision.CompareTag("Abyss"))
        {
            GameObject boxPrefab = BoxPrefab;

            //箱を消す
            Destroy(boxPrefab);
        }
    }

    /// <summary>
    /// 箱を壊したときの処理
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

        //破片オブジェクトを生成(position.xは箱の位置、yは箱より少し上の位置)
        fragment = Instantiate(BoxFragment, new Vector2(this.transform.position.x, this.transform.position.y + 1.5f), this.transform.rotation);

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

        //破片を消す
        DestroyFragment(fragment.transform);
    }

    /// <summary>
    /// 破片をフェードアウトさせる処理
    /// </summary>
    /// <param name="fragment">破片プレハブ</param>
    public override void FadeFragment(Transform fragment)
    {
        if (fragment == null)
        {
            //何もしない
            return;
        }

        //6秒かけて破片をフェードアウトさせる
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
