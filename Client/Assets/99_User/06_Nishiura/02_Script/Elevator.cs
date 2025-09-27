//===================
// �G���x�[�^�[�X�N���v�g
// Author:Nishiura
// Date:2025/07/04
//===================
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
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

    [SerializeField] AudioClip moveSE;
    [SerializeField] AudioClip arrivedSE;

    AudioSource audioSource;

    // �G���x�[�^�[�̖ڕW�n�_
    Vector2 riseEndPos;
    Vector2 descentEndPos;

    // ���C���[�̖ڕW�n�_
    Vector2 riseEndWirePos;
    Vector2 descentEndWirePos;

    private void Start()
    {
        // ���~�ς݂̏�Ԃ���n�܂�̂�z�肵���ڕW�n�_��ݒ�
        riseEndPos = transform.position + Vector3.up * risePow;
        descentEndPos = transform.position;
        riseEndWirePos = wire.transform.position + Vector3.up * risePow;
        descentEndWirePos = wire.transform.position;

        audioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        if (isBroken)
        {
            tweener.Kill();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // �G���x�[�^�[�ɏ���Ă���L�����N�^�[���q�I�u�W�F�N�g�ɐݒ�
        var obj = collision.transform.gameObject;
        if (obj.tag == "Player" && obj == CharacterManager.Instance.PlayerObjSelf
            || collision.transform.tag == "Enemy")
        {
            collision.transform.SetParent(transform);
        }

        if (isBroken == true || isMoving == true || isPowerd == false) return;  // �d��off�܂��̓G���x�[�^�[���쒆�̏ꍇ�������Ȃ�
        // �v���C���[���G���x�[�^�[���ɓ������ꍇ
        if (collision.transform.tag == "Player" && collision.gameObject == CharacterManager.Instance.PlayerObjSelf)
        {
            isMoving = true;    // ���쒆�ɂ���
            Invoke("InvokeTurnOnPowerRequest", 0.5f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // �G���x�[�^�[�ɏ���Ă����L�����N�^�[���q�I�u�W�F�N�g����O��
        if (collision.transform.tag == "Player" || collision.transform.tag == "Enemy")
        {
            var obj = collision.transform.gameObject;
            if (obj.tag == "Player" && obj != CharacterManager.Instance.PlayerObjSelf) return;
            if (collision.transform.parent == transform) collision.transform.parent = null;
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
            //arrivalSE.Play();
            audioSource.PlayOneShot(arrivedSE);
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
    /// �㏸����
    /// </summary>
    void MoveUp()
    {
        if (isRised || (Vector2)transform.position == riseEndPos) return;
        Invoke("MovingCheck", moveSpeed);  //����`�F�b�N
        isRised = true;       // �㏸�ς݂Ƃ���
        if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            tweener = this.transform.DOMove(riseEndPos, moveSpeed);
            if (wire) wire.transform.DOMove(riseEndWirePos, moveSpeed);
        }
        //�ړ�SE���Đ�����
        movementSE.Play();
    }

    /// <summary>
    /// ���~����
    /// </summary>
    void MoveDown()
    {
        if (!isRised || (Vector2)transform.position == descentEndPos) return;
        Invoke("MovingCheck", moveSpeed);  //����`�F�b�N
        isRised = false;    // ���~�ς݂Ƃ���
        isMoving = true;    // ���쒆�ɂ���

        if (!RoomModel.Instance || RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            tweener = this.transform.DOMove(descentEndPos, moveSpeed);
            if (wire) wire.transform.DOMove(descentEndWirePos, moveSpeed);
        }
        //�ړ�SE���Đ�����
        movementSE.Play();
    }

    /// <summary>
    /// �d���I���֐�
    /// </summary>
    public override void TurnOnPower()
    {
        if (!isRised) MoveUp();
        else MoveDown();
    }

    /// <summary>
    /// ���~�{�^������
    /// </summary>
    public void MoveButton(bool type)
    {
        if (isBroken == true || isMoving == true || isPowerd == false) return;  // �d��off�܂��̓G���x�[�^�[���쒆�̏ꍇ�������Ȃ�

        if (type == false) MoveDown();
        else MoveUp();
    }

    /// <summary>
    /// �}�X�^�؂�ւ����̍ċN������
    /// </summary>
    public override void Reactivate()
    {
        CancelInvoke("MovingCheck");

        if (isRised)
        {
            transform.position = riseEndPos;
            if (wire) wire.transform.position = riseEndWirePos;
        }
        else
        {
            transform.position = descentEndPos;
            if (wire) wire.transform.position = descentEndWirePos;
        }
        isMoving = false;
    }
}