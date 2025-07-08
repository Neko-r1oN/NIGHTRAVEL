//===================
// �G���x�[�^�[�X�N���v�g
// Author:Nishiura
// Date:2025/07/04
//===================
using DG.Tweening;
using UnityEngine;

public class Elevator : GimmickBase
{
    // �d������
    public bool isPowerd;
    // �㏸����
    bool isRised = false;
    // �㏸�l
    public float risePow;
    // ���~�l
    public float descentPow;
    // ���~���x�l
    public int moveSpeed;
    // ���쒆����ϐ�
    private bool isMoving;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isMoving == true || isPowerd == false) return;  // �d��off�܂��̓G���x�[�^�[���쒆�̏ꍇ�������Ȃ�

        // �v���C���[���G���x�[�^�[���ɓ������ꍇ
        if (collision.transform.tag == "Player")
        {
            isMoving = true;    // ���쒆�ɂ���
            Invoke("MoveElevator", 1f);  //���~�J�n
            Debug.Log("You Entered Elevator");
        }
    }

    private void MoveElevator()
    {
        Invoke("MovingCheck", 4f);  //����`�F�b�N
        if (!isRised)
        {   //�㏸�ς݂łȂ��ꍇ
            this.transform.DOMoveY((this.gameObject.transform.position.y + risePow), moveSpeed);    //�㏸����
        }
        else
        {   //�㏸�ς݂̏ꍇ
            this.transform.DOMoveY((this.gameObject.transform.position.y - descentPow), moveSpeed); //���~����
        }
    }

    /// <summary>
    /// ����m�F�֐�
    /// </summary>
    private void MovingCheck()
    {
        if (!isRised) 
        {   // �㏸���Ă��Ȃ��Ɣ��f���ꂽ�ꍇ
            isRised=true;       // �㏸�ς݂Ƃ���
        }
        else
        {   // �㏸���Ă���Ɣ��f���ꂽ�ꍇ
            isRised = false;    // ���~�ς݂Ƃ���
        }

        isMoving = false;   // ���슮���ɂ���
    }

    /// <summary>
    /// �d���I���֐�
    /// </summary>
    public override void TurnOnPower()
    {
        isPowerd = true;
    }

    /// <summary>
    /// ���~�{�^������
    /// </summary>
    public void MoveButton(bool type)
    {
        if (isMoving == true || isPowerd == false) return;  // �d��off�܂��̓G���x�[�^�[���쒆�̏ꍇ�������Ȃ�
        if (type == false)
        {
            Invoke("MovingCheck", 4f);  //����`�F�b�N

            if (isRised)
            {   //�㏸�ς݂̏ꍇ
                isMoving = true;    // ���쒆�ɂ���
                this.transform.DOMoveY((this.gameObject.transform.position.y - descentPow), moveSpeed); //���~����
            }
        }
        else
        {
            Invoke("MovingCheck", 4f);  //����`�F�b�N
            if (!isRised)
            {   //�㏸�ς݂łȂ��ꍇ
                isMoving = true;    // ���쒆�ɂ���
                this.transform.DOMoveY((this.gameObject.transform.position.y + risePow), moveSpeed);    //�㏸����
            }
        }
    }
}
