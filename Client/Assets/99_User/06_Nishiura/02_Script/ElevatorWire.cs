//===================
// �G���x�[�^�[���C���[�X�N���v�g
// Author:Nishiura
// Date:2025/07/08
//===================
using UnityEngine;

public class ElevatorWire : ObjectBase
{
    // �G���x�[�^�[�̃v���n�u
    [SerializeField] GameObject Elevator;

    public override void ApplyDamage()
    {
        Destroy(this.gameObject);   // ���C���[��j��
        Elevator.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        Elevator.GetComponent<Elevator>().isBroken = true;
        Elevator.transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = false;
        Elevator.GetComponent<Rigidbody2D>().AddForce(new Vector2(0.0f, -100f));  // �͂�������

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
