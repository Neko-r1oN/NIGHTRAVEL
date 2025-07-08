//===================
// エレベーターワイヤースクリプト
// Author:Nishiura
// Date:2025/07/08
//===================
using UnityEngine;

public class ElevatorWire : ObjectBase
{
    // エレベーターのプレハブ
    [SerializeField] GameObject Elevator;

    public override void ApplyDamage()
    {
        Destroy(this.gameObject);   // ワイヤーを破壊
        Elevator.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

        Debug.Log(Elevator.GetComponent<Rigidbody2D>().bodyType);
    }

    public override void FadeFragment(Transform fragment)
    {
        throw new System.NotImplementedException();
    }
    public override void TruggerRequest()
    {
        throw new System.NotImplementedException();
    }
    public override void TurnOnPower(int triggerID)
    {
        throw new System.NotImplementedException();
    }
}
