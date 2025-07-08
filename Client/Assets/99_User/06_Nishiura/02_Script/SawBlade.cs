//===================
// �\�[�u���[�h�X�N���v�g
// Author:Nishiura
// Date:2025/07/02
//===================
using DG.Tweening;
using UnityEngine;

public class SawBlade : GimmickBase
{
    PlayerBase playerBase;

    Vector2 pos;
    // ���͒l
    public float addPower;

    // �d������
    public bool isPowerd;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // ���̃Q�[���I�u�W�F�N�g�̃|�W�V�������擾
        pos = this.gameObject.transform.position;
        // �d���I���̏ꍇ�A�ړ��J�n
        if (isPowerd == true) MoveBlade();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isPowerd == true && collision.transform.tag == "Player")
        {
            playerBase = collision.gameObject.GetComponent<PlayerBase>();
            // �v���C���[�̍ő�HP15%�����̃_���[�W�ɐݒ�
            int damage = Mathf.FloorToInt(playerBase.MaxHP * 0.15f);
            playerBase.ApplyDamage(damage, pos);
            Debug.Log("Hit SawBlade");
        }
    }

    /// <summary>
    /// �ۂ̂��ړ��֐�
    /// </summary>
    private void MoveBlade()
    {
        //Sequence�̃C���X�^���X���쐬
        var sequence = DOTween.Sequence();

        //Append�œ����ǉ����Ă���
        sequence.Append(this.transform.DOMoveX((pos.x - addPower), 1))
                .AppendInterval(0.01f)
                .Append(this.transform.DOMoveX(pos.x , 1));

        //Play�Ŏ��s
        sequence.Play()
                .AppendInterval(0.01f)
                .SetLoops(-1);
    }

    /// <summary>
    /// �d���I���֐�
    /// </summary>
    public override void TurnOnPower(int t)
    {
        isPowerd = true;
        MoveBlade();
    }

    public override void TruggerRequest()
    {
        throw new System.NotImplementedException();
    }
}
