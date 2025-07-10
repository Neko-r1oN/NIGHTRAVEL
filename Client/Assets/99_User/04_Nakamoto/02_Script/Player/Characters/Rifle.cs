using System.Collections;
using UnityEngine;
using static StatusEffectController;
using static Sword;

public class Rifle : PlayerBase
{
    //--------------------------
    // �t�B�[���h

    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum GS_ANIM_ID
    {
        Attack = 10,
        Skill
    }

    private bool isChanging = false;    // �ό`���t���O
    private bool isRailgun = false;     // �e�ό`�t���O
    private bool isCooldown = false;    // �U���̃N�[���_�E���t���O

    /// <summary>
    /// �X�V����
    /// </summary>
    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Attack1"))
        {   // �ʏ�U��
            int id = animator.GetInteger("animation_id");

            if (isBlink || isSkill || isCooldown || id == 3 || m_IsZipline) return;

            if (canAttack)
            {
                if(isRailgun)
                {
                    canAttack = false;
                    isRailgun = false;
                    animator.SetInteger("animation_id", (int)GS_ANIM_ID.Attack);
                }
                else
                {
                    canAttack = false;
                    animator.SetInteger("animation_id", (int)GS_ANIM_ID.Attack);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.V) || Input.GetButtonDown("Attack2"))
        {   // �X�L��
            isSkill = true;
            animator.SetInteger("animation_id", (int)GS_ANIM_ID.Skill);
        }

        base.Update();

        //-----------------------------
        // �f�o�b�O�p

        if (Input.GetKeyDown(KeyCode.L))
        {
            GetComponent<StatusEffectController>().ApplyStatusEffect(EFFECT_TYPE.Burn);
        }

        //Esc�������ꂽ��
        if (Input.GetKey(KeyCode.Escape))
        {

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;//�Q�[���v���C�I��
#else
    Application.Quit();//�Q�[���v���C�I��
#endif
        }
    }

    /// <summary>
    /// �ړ�����
    /// </summary>
    /// <param name="move">�ړ���</param>
    /// <param name="jump">�W�����v����</param>
    /// <param name="blink">�_�b�V������</param>
    protected override void Move(float move, bool jump, bool blink)
    {
        if (isSkill)
        {   // �e�ό`���͓����Ȃ��悤��
            m_Rigidbody2D.linearVelocity = Vector3.zero;
            return;
        }

        base.Move(move, jump, blink);

        // �_�b�V�����̏ꍇ
        if (isBlinking)
        {   // �N�[���_�E���ɓ���܂ŉ���
            m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * m_BlinkForce, 0);
        }

        // �e�ό`���̈ړ�����
        if (isRailgun)
        {
            var targetVelocity = new Vector2(move, m_Rigidbody2D.linearVelocity.y);
            m_Rigidbody2D.linearVelocity = targetVelocity;
        }
    }

    /// <summary>
    /// �_���[�W��^���鏈��
    /// </summary>
    public override void DoDashDamage()
    {
        power = Mathf.Abs(power);
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, k_AttackRadius);

        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Enemy")
            {
                if (collidersEnemies[i].transform.position.x - transform.position.x < 0)
                {
                    power = -power;
                }
                //++ GetComponent��Enemy�X�N���v�g���擾���AApplyDamage���Ăяo���悤�ɕύX
                //++ �j��ł���I�u�W�F�����ۂɂ̓I�u�W�F�̋��ʔ�_���֐����ĂԂ悤�ɂ���

                collidersEnemies[i].gameObject.GetComponent<EnemyBase>().ApplyDamage(power, playerPos);
                cam.GetComponent<CameraFollow>().ShakeCamera();
            }
            else if (collidersEnemies[i].gameObject.tag == "Object")
            {
                collidersEnemies[i].gameObject.GetComponent<ObjectBase>().ApplyDamage();
            }
        }
    }

    /// <summary>
    /// �U���I����
    /// </summary>
    public void AttackEnd()
    {
        canAttack = true;

        // Idle�ɖ߂�
        animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
    }

    /// <summary>
    /// �X�L�����o�I����
    /// </summary>
    public void SkillEnd()
    {
        isSkill = false;
        isRailgun = true;

        // Idle�ɖ߂�
        animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
    }
}
