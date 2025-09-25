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
    //�@�G���x�[�^�[�ړ�SE
    [SerializeField] AudioSource movementSE;
    //�@�G���x�[�^�[����SE
    [SerializeField] AudioSource arrivalSE;
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
        if (collision.transform.tag == "Player" && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            isMoving = true;    // ���쒆�ɂ���
            Invoke("InvokeTurnOnPowerRequest", 0.5f);
        }
    }
    void InvokeTurnOnPowerRequest()
    {
        if (isBroken == true || isPowerd == false) return;
        TurnOnPowerRequest(CharacterManager.Instance.PlayerObjSelf);
    }

    /// <summary>
    /// ����m�F�֐�
    /// </summary>
    private void MovingCheck()
    {
        if (!isRised)
        {   // �㏸���Ă��Ȃ��Ɣ��f���ꂽ�ꍇ
            //����SE���Đ�����
            arrivalSE.Play();
            //�ړ�SE�̍Đ����~����
            movementSE.Stop();
        }
        else
        {   // �㏸���Ă���Ɣ��f���ꂽ�ꍇ
            //����SE���Đ�����
            arrivalSE.Play();
            //�ړ�SE�̍Đ����~����
            movementSE.Stop();
        }
        isMoving = false;   // ���슮���ɂ���
    }
    /// <summary>
    /// �d���I���֐�
    /// </summary>
    public override void TurnOnPower()
    {
        Invoke("MovingCheck", 4f);  //����`�F�b�N
        if (!isRised)
        {   //�㏸�ς݂łȂ��ꍇ
            isRised = true;       // �㏸�ς݂Ƃ���
            if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
            {
                tweener = this.transform.DOMoveY((this.gameObject.transform.position.y + risePow), moveSpeed);    //�㏸����
                if (wire != null) wire.transform.DOMoveY((wire.gameObject.transform.position.y + risePow), moveSpeed);    //���C���[�㏸����
            }
            //�ړ�SE���Đ�����
            movementSE.Play();
        }
        else
        {   //�㏸�ς݂̏ꍇ
            isRised = false;    // ���~�ς݂Ƃ���
            if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
            {
                tweener = this.transform.DOMoveY((this.gameObject.transform.position.y - descentPow), moveSpeed); //���~����
                if (wire != null) wire.transform.DOMoveY((wire.gameObject.transform.position.y - descentPow), moveSpeed); //���C���[�����~����
            }
            //�ړ�SE���Đ�����
            movementSE.Play();
        }
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
                isRised = false;    // ���~�ς݂Ƃ���
                if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
                {
                    tweener = this.transform.DOMoveY((this.gameObject.transform.position.y - descentPow), moveSpeed); //���~����
                    if (wire != null) wire.transform.DOMoveY((wire.gameObject.transform.position.y - descentPow), moveSpeed); //���C���[�����~����
                }
                //�ړ�SE���Đ�����
                movementSE.Play();
            }
        }
        else
        {
            Invoke("MovingCheck", 4f);  //����`�F�b�N
            if (!isRised)
            {   //�㏸�ς݂łȂ��ꍇ
                isRised = true;       // �㏸�ς݂Ƃ���
                if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
                {
                    tweener = this.transform.DOMoveY((this.gameObject.transform.position.y + risePow), moveSpeed);    //�㏸����
                    if (wire != null) wire.transform.DOMoveY((wire.gameObject.transform.position.y + risePow), moveSpeed);    //���C���[�㏸����
                }
                //�ړ�SE���Đ�����
                movementSE.Play();
            }
        }
    }
}