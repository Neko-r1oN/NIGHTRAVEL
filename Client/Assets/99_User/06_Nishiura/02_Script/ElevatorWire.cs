//===================
// エレベーターワイヤースクリプト
// Author:Nishiura
// Date:2025/07/08
//===================
using DG.Tweening;
using UnityEngine;

public class ElevatorWire : ObjectBase
{
    // エレベーターのプレハブ
    [SerializeField] GameObject Elevator;

    protected override void ApplyDamage()
    {
        Destroy(this.gameObject);   // ワイヤーを破壊
        Elevator.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        Elevator.GetComponent<Elevator>().isBroken = true;
        Elevator.transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = false;
        Elevator.GetComponent<Rigidbody2D>().AddForce(new Vector2(0.0f, -100f));  // 力を加える

        Debug.Log(Elevator.GetComponent<Rigidbody2D>().bodyType);
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

    public override void TurnOnPower()
    {
        throw new System.NotImplementedException();
    }
}
