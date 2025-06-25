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
    Rigidbody2D rigidbody2d;
    bool isBroken = false;
    public float addPow;
    public float pullPow;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MovePress();
    }

    private void Update()
    {

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

    public async void MovePress()
    {
        //Sequenceのインスタンスを作成
        var sequence = DOTween.Sequence();

        //Appendで動作を追加していく
         sequence.Append(this.transform.DOMoveY(-addPow, 1))
                 .AppendInterval(1)
                 .Append(this.transform.DOMoveY(pullPow, 2));
                
        //Playで実行
        sequence.Play()
                .AppendInterval(1)
                .SetLoops(-1);
    }
}
