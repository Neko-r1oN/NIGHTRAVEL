//===================
// �G���x�[�^�[���C���[�X�N���v�g
// Author:Nishiura
// Date:2025/07/08
//===================
using DG.Tweening;
using UnityEngine;

public class ElevatorWire : ObjectBase
{
    // �G���x�[�^�[�̃v���n�u
    [SerializeField] GameObject Elevator;

    protected override void ApplyDamage()
    {
        Destroy(this.gameObject);   // ���C���[��j��
        Elevator.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        Elevator.GetComponent<Elevator>().isBroken = true;
        Elevator.transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = false;
        Elevator.GetComponent<Rigidbody2D>().AddForce(new Vector2(0.0f, -100f));  // �͂�������

        Debug.Log(Elevator.GetComponent<Rigidbody2D>().bodyType);
    }

    override public void DestroyFragment(Transform obj)
    {
    }

    /// <summary>
    /// ����I�u�W�F�N�g�̔j�Ђ��t�F�[�h����֐�
    /// </summary>
    /// <param name="fragment">�j��</param>
    override public void FadeFragment(Transform fragment)
    {
    }

    public override void TurnOnPower()
    {
        throw new System.NotImplementedException();
    }
}
