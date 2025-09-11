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
    public bool isMoving;
    // �j�󔻒�ϐ�
    public bool isBroken = false;
    // ���C���[
    [SerializeField] GameObject wire;

    // �㏸�{�^��
    [SerializeField] GameObject riseButton;
    // ���~�{�^��
    [SerializeField] GameObject descButton;

    Tweener tweener;
    private void Update()
    {
        if (isBroken)
        {
            tweener.Kill();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isBroken == true || isMoving == true || isPowerd == false) return;  // �d��off�܂��̓G���x�[�^�[���쒆�̏ꍇ�������Ȃ�

        // �v���C���[���G���x�[�^�[���ɓ������ꍇ
        if (collision.transform.tag == "Player")
        {
            isMoving = true;    // ���쒆�ɂ���
            Invoke("MoveElevator", 1f);  //���~�J�n
        }
    }

    private void MoveElevator()
    {
        Invoke("MovingCheck", 4f);  //����`�F�b�N
        if (!isRised)
        {   //�㏸�ς݂łȂ��ꍇ
            tweener = this.transform.DOMoveY((this.gameObject.transform.position.y + risePow), moveSpeed);    //�㏸����
            if (wire != null) wire.transform.DOMoveY((wire.gameObject.transform.position.y + risePow), moveSpeed);    //���C���[�㏸����
        }
        else
        {   //�㏸�ς݂̏ꍇ
            tweener = this.transform.DOMoveY((this.gameObject.transform.position.y - descentPow), moveSpeed); //���~����
            if (wire != null) wire.transform.DOMoveY((wire.gameObject.transform.position.y - descentPow), moveSpeed); //���C���[�����~����
           
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
        if (isPowerd) return;   // ���łɋN�����Ă���ꍇ�͏������Ȃ�
        isPowerd = true;
    }

    /// <summary>
    /// ���~�{�^������
    /// </summary>
    public void MoveButton(bool type)
    {
        if (isBroken == true || isMoving == true || isPowerd == false) return;  // �d��off�܂��̓G���x�[�^�[���쒆�̏ꍇ�������Ȃ�

        isMoving = true;    // ���쒆�ɂ���

        if (type == false)
        {
            Invoke("MovingCheck", 4f);  //����`�F�b�N

            if (isRised)
            {   //�㏸�ς݂̏ꍇ
                tweener = this.transform.DOMoveY((this.gameObject.transform.position.y - descentPow), moveSpeed); //���~����
                if (wire != null) wire.transform.DOMoveY((wire.gameObject.transform.position.y - descentPow), moveSpeed); //���C���[�����~����
            }
        }
        else
        {
            Invoke("MovingCheck", 4f);  //����`�F�b�N
            if (!isRised)
            {   //�㏸�ς݂łȂ��ꍇ
                tweener = this.transform.DOMoveY((this.gameObject.transform.position.y + risePow), moveSpeed);    //�㏸����
                if (wire != null) wire.transform.DOMoveY((wire.gameObject.transform.position.y + risePow), moveSpeed);    //���C���[�㏸����
            }
        }
    }
}
