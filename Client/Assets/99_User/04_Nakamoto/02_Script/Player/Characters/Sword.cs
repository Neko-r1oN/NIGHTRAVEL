//--------------------------------------------------------------
// ���m�L���� [ Sword.cs ]
// Author�FKenta Nakamoto
// ���p�Fhttps://assetstore.unity.com/packages/2d/characters/metroidvania-controller-166731
//--------------------------------------------------------------
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;
using Pixeye.Unity;
using System;
using HardLight2DUtil;
using static StatusEffectController;

public class Sword : PlayerBase
{
    //--------------------------
    // �t�B�[���h

    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum S_ANIM_ID
    {
        Attack1 = 10,
        Attack2,
        Attack3,
        Skill
    }

    private bool isCombo = false;   // �R���{�\�t���O
    private bool cantAtk = false;   // �U���\�t���O

    private float plDirection = 0;  // �v���C���[�̌���

    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private float skillForth = 45f;       // �X�L���̈ړ���

    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private float skillTime = 0.5f;        // �X�L�����ʎ���

    [Foldout("�L�����ʃX�e�[�^�X")]
    [SerializeField] private float skillCoolDown = 5.0f;    // �X�L���̃N�[���_�E��

    [Foldout("�X�L���G�t�F�N�g")]
    [SerializeField] private GameObject skillEffect1;   // �L�����ɔ�������G�t�F�N�g

    [Foldout("�X�L���G�t�F�N�g")]
    [SerializeField] private GameObject skillEffect2;   // ����ɔ�������G�t�F�N�g

    [Foldout("�X�L���G�t�F�N�g")]
    [SerializeField] private GameObject skillEffect3;   // �ǉ��Ŕ���������G�t�F�N�g

    //--------------------------
    // ���\�b�h

    /// <summary>
    /// �X�V����
    /// </summary>
    private void Update()
    {
        //Debug.Log("�U���F" + nowAttack + " �R���{�F" + isCombo);

        // �L�����̈ړ�
        horizontalMove = Input.GetAxisRaw("Horizontal") * moveSpeed;
        verticalMove = Input.GetAxisRaw("Vertical") * moveSpeed;
        Ladder();

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetButtonDown("Jump"))
        {   // �W�����v������
            if (animator.GetInteger("animation_id") != (int)ANIM_ID.Blink)
                isJump = true;
        }

        if (Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("Blink"))
        {   // �u�����N������
            if(nowAttack)
            {
                isBlink = true;
                gameObject.layer = 21;
            }
        }

        if (Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Attack1"))
        {   // �ʏ�U��
            if (isBlink || cantAtk) return;

            if (nowAttack && !isCombo)
            {   // �U��1�i��
                nowAttack = false;
                animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack1);
            }
            else if (isCombo)
            {   // �U��2,3�i��
                isCombo = false;
                int id = animator.GetInteger("animation_id");

                if (id == (int)S_ANIM_ID.Attack1)
                {
                    animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack2);
                }
                if (id == (int)S_ANIM_ID.Attack2)
                {
                    animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack3);
                    StartCoroutine(LastComboAttack());
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.V) || Input.GetButtonDown("Attack2"))
        {   // �U��2
            if (canSkill && nowAttack)
            {
                gameObject.layer = 21;
                animator.SetInteger("animation_id", (int)S_ANIM_ID.Skill);
                canSkill = false;
                plDirection = transform.localScale.x;
                StartCoroutine(SkillCoolDown());
            }
        }

        //-----------------------------
        // �f�o�b�O�p

        if (Input.GetKeyDown(KeyCode.L))
        {
            GetComponent<StatusEffectController>().ApplyStatusEffect(EFFECT_TYPE.Burn);
        }
    }

    protected override void FixedUpdate()
    {
        Debug.Log(canSkill + "�F" + isSkill);

        base.FixedUpdate();

        if (isSkill)
        {
            // �N�[���_�E���ɓ���܂ŉ���
            m_Rigidbody2D.linearVelocity = new Vector2(plDirection * skillForth, 0);
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
    public void HitAttack()
    {
        isCombo = true;
    }

    /// <summary>
    /// �U���I����
    /// </summary>
    public void AttackEnd()
    {
        if (!isCombo) return;

        // �t���O�̏�����
        nowAttack = true;
        isCombo = false;

        // �ړ����x�ɉ����ăA�j���[�V��������
        if (Mathf.Abs(horizontalMove) >= 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Run);

        if (Mathf.Abs(horizontalMove) < 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
    }

    /// <summary>
    /// �ŏI�R���{����
    /// </summary>
    IEnumerator LastComboAttack()
    {
        cantAtk = true;

        // �R���{�I�����ҋ@  
        yield return new WaitForSeconds(1.5f);
        cantAtk = false;
    }

    IEnumerator SkillCoolDown()
    {
        isSkill = true;

        // Effect�Đ�
        skillEffect1.SetActive(true);
        skillEffect2.SetActive(true);
        skillEffect3.SetActive(true);

        yield return new WaitForSeconds(skillTime);
        isSkill = false;
        gameObject.layer = 20;

        // Effect��\��
        skillEffect1.SetActive(false);
        skillEffect2.SetActive(false);
        skillEffect3.SetActive(false);

        // �ړ����x�ɉ����ăA�j���[�V��������
        if (Mathf.Abs(horizontalMove) >= 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Run);

        if (Mathf.Abs(horizontalMove) < 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Idle);

        yield return new WaitForSeconds(skillCoolDown);
        canSkill = true;
    }

    [ContextMenu("�V���b�N")]
    public void ShockTest()
    {
        this.GetComponent<StatusEffectController>().ApplyStatusEffect(StatusEffectController.EFFECT_TYPE.Shock);
    }
}
