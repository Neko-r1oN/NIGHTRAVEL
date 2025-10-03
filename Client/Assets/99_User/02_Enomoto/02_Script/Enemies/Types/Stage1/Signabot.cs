//**************************************************
//  [�G] �V�O�i�{�b�g�̃N���X
//  Author:r-enomoto
//**************************************************
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Signabot : EnemyBase
{
    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Attack,
        Hit,
        Dead,
    }

    /// <summary>
    /// �Ǘ�����R���[�`���̎��
    /// </summary>
    public enum COROUTINE
    {
        AttackCooldown,
        MeleeAttack
    }

    #region �`�F�b�N�֘A
    // �ߋ����U���͈̔�
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Transform meleeAttackCheck;
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField] float meleeAttackRange = 0.9f;

    // �ǁE�n�ʃ`�F�b�N
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Transform wallCheck;
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Vector2 wallCheckRadius = new Vector2(0, 1.5f);
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Transform groundCheck;
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Vector2 groundCheckRadius = new Vector2(0.5f, 0.2f);

    // �����`�F�b�N
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Transform fallCheck;
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Vector2 fallCheckRange;
    #endregion

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
    }

    protected override void FixedUpdate()
    {
        if (isSpawn || isStun || isInvincible || hp <= 0 || !sightChecker) return;
        SetAnimId((int)ANIM_ID.Attack);
        DecideBehavior();
    }


    /// <summary>
    /// �s���p�^�[�����s����
    /// </summary>
    protected override void DecideBehavior()
    {
        if (!isAttacking) return;

        if (m_rb2d.linearVelocityY < 0 && !IsGround())
        {
            AirMovement();
        }
        else if (canPatrol)
        {
            Patorol();
        }
    }

    #region �U�������֘A

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] �ߐڍU������
    /// </summary>

    public override void OnAttackAnimEvent()
    {
        isAttacking = true;

        // ���s���Ă��Ȃ���΁A�ߐڍU���̃R���[�`�����J�n
        string cooldownKey = COROUTINE.MeleeAttack.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(MeleeAttack());
            managedCoroutines.Add(cooldownKey, coroutine);
        }
    }

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] �U���̔��菈�����I��
    /// </summary>
    public override void OnEndAttackAnimEvent()
    {
        isAttacking = false;
        RemoveAndStopCoroutineByKey(COROUTINE.MeleeAttack.ToString());

        // ���s���Ă��Ȃ���΁A�N�[���_�E���̃R���[�`�����J�n
        string cooldownKey = COROUTINE.AttackCooldown.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(AttackCooldown(attackCoolTime, () => {
                RemoveAndStopCoroutineByKey(cooldownKey);
            }));
            managedCoroutines.Add(cooldownKey, coroutine);
        }
    }

    /// <summary>
    /// �ߐڍU���̔�����J��Ԃ�
    /// </summary>
    /// <returns></returns>
    IEnumerator MeleeAttack()
    {
        while (true)
        {
            // ���g���G���[�g�̂̏ꍇ�A�t�^�����Ԉُ�̎�ނ��擾����
            DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();

            Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
            for (int i = 0; i < collidersEnemies.Length; i++)
            {
                if (collidersEnemies[i].gameObject.tag == "Player")
                {
                    collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position, KB_POW.Medium, applyEffect);
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// �U�����̃N�[���_�E������
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time, Action onFinished)
    {
        yield return new WaitForSeconds(time);
        isAttacking = false;
        Idle();
        onFinished?.Invoke();
    }

    #endregion

    #region �ړ������֘A

    /// <summary>
    /// ���񂷂鏈��
    /// </summary>
    protected override void Patorol()
    {
        if (IsFall() || IsWall()) Flip();
        Vector2 speedVec = new Vector2(TransformUtils.GetFacingDirection(transform) * moveSpeed, m_rb2d.linearVelocity.y);
        m_rb2d.linearVelocity = speedVec;
    }

    /// <summary>
    /// �󒆏�Ԃł̈ړ�����
    /// </summary>
    void AirMovement()
    {
        // ���݌����Ă�������Ɉړ�����
        Vector2 speedVec = new Vector2(TransformUtils.GetFacingDirection(transform) * moveSpeed, m_rb2d.linearVelocity.y);
        m_rb2d.linearVelocity = speedVec;
    }

    #endregion

    #region �q�b�g�����֘A

    /// <summary>
    /// �_���[�W���󂯂��Ƃ��̏���
    /// </summary>
    protected override void OnHit()
    {
        base.OnHit();
        SetAnimId((int)ANIM_ID.Hit);
    }

    /// <summary>
    /// ���S����Ƃ��ɌĂ΂�鏈��
    /// </summary>
    /// <returns></returns>
    protected override void OnDead()
    {
        SetAnimId((int)ANIM_ID.Dead);
    }

    #endregion

    #region �e�N�X�`���E�A�j���[�V�����֘A

    /// <summary>
    /// �X�|�[���A�j�����[�V�����J�n��
    /// </summary>
    public override void OnSpawnAnimEvent()
    {
        base.OnSpawnAnimEvent();
        SetAnimId((int)ANIM_ID.Spawn);
    }

    /// <summary>
    /// �X�|�[���A�j���[�V�������I�������Ƃ�
    /// </summary>
    public override void OnEndSpawnAnimEvent()
    {
        base.OnEndSpawnAnimEvent();
        ApplyStun(0.5f, false);
    }

    /// <summary>
    /// �w�肵���A�j���[�V����ID���q�b�g�A�j���[�V�������ǂ���
    /// </summary>
    /// <param name="animationId"></param>
    /// <returns></returns>
    public override bool IsHitAnimIdFrom(int animationId)
    {
        return animationId == (int)ANIM_ID.Hit;
    }

    /// <summary>
    /// �A�j���[�V�����ݒ菈��
    /// </summary>
    /// <param name="id"></param>
    public override void SetAnimId(int id)
    {
        if (animator == null) return;
        animator.SetInteger("animation_id", id);

        switch (id)
        {
            case (int)ANIM_ID.Hit:
                animator.Play("Hit_Signabot", 0, 0);
                break;
            default:
                break;
        }
    }
    #endregion

    #region ���A���^�C�������֘A

    /// <summary>
    /// �}�X�^�N���C�A���g�؂�ւ����ɏ�Ԃ����Z�b�g����
    /// </summary>
    public override void ResetAllStates()
    {
        base.ResetAllStates();

        if (target == null)
        {
            target = sightChecker.GetTargetInSight();
        }
    }

    #endregion

    #region �`�F�b�N�����֘A

    /// <summary>
    /// �ǂ����邩�ǂ���
    /// </summary>
    /// <returns></returns>
    bool IsWall()
    {
        return Physics2D.OverlapBox(wallCheck.position, wallCheckRadius, 0f, terrainLayerMask);
    }

    /// <summary>
    /// ���������ǂ���
    /// </summary>
    /// <returns></returns>
    bool IsFall()
    {
        return !Physics2D.OverlapBox(fallCheck.position, fallCheckRange, 0, terrainLayerMask);
    }

    /// <summary>
    /// �n�ʔ���
    /// </summary>
    /// <returns></returns>
    bool IsGround()
    {
        // �����ɂQ�̎n�_�ƏI�_���쐬����
        Vector3 leftStartPosition = groundCheck.transform.position + Vector3.left * groundCheckRadius.x / 2;
        Vector3 rightStartPosition = groundCheck.transform.position + Vector3.right * groundCheckRadius.x / 2;
        Vector3 endPosition = groundCheck.transform.position + Vector3.down * groundCheckRadius.y;

        return Physics2D.Linecast(leftStartPosition, endPosition, terrainLayerMask)
            || Physics2D.Linecast(rightStartPosition, endPosition, terrainLayerMask);
    }

    /// <summary>
    /// ���o�͈͂̕`�揈��
    /// </summary>
    protected override void DrawDetectionGizmos()
    {
        // �U���J�n����
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackDist);

        // �U���͈�
        if (meleeAttackCheck)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleeAttackCheck.transform.position, meleeAttackRange);
        }

        // �ǂ̔���
        if (wallCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(wallCheck.transform.position, wallCheckRadius);
        }

        // �n�ʔ���
        if (groundCheck)
        {
            Vector3 leftStartPosition = groundCheck.transform.position + Vector3.left * groundCheckRadius.x / 2;
            Vector3 rightStartPosition = groundCheck.transform.position + Vector3.right * groundCheckRadius.x / 2;
            Vector3 endPosition = groundCheck.transform.position + Vector3.down * groundCheckRadius.y;
            Gizmos.DrawLine(leftStartPosition, endPosition);
            Gizmos.DrawLine(rightStartPosition, endPosition);
        }

        // �����`�F�b�N
        if (fallCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(fallCheck.position, fallCheckRange);
        }
    }

    #endregion

}
