//**************************************************
//  [�G] �J���J�X�̃N���X
//  Author:r-enomoto
//**************************************************
using DG.Tweening;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Carcass : EnemyBase
{
    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        RotationAttack,
        RotationAttack_Rotate,
        RotationAttack_Finish,
        MeleeAttack,
        Run,
        Hit,
        Dead,
    }

    /// <summary>
    /// �s���p�^�[��
    /// </summary>
    public enum DECIDE_TYPE
    {
        Waiting = 1,
        RotationAttack,
        MeleeAttack,
        Tracking,
        AirMovement
    }

    /// <summary>
    /// �Ǘ�����R���[�`���̎��
    /// </summary>
    public enum COROUTINE
    {
        RotationAttack,
        MeleeAttack,
        AttackCooldown,
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

    #region �v���C���[�֘A
    List<GameObject> hitPlayers = new List<GameObject>();   // �U�����󂯂��v���C���[�̃��X�g
    readonly float disToTargetMin = 0.25f;
    #endregion

    #region �I���W�i��
    [Foldout("�I���W�i��")]
    [Tooltip("��]�U���̍ۂ�power�ɂ�����{��")]
    float rotationPowerRate = 1.5f;

    bool isRotationAttacking;
    #endregion

    protected override void Start()
    {
        isRotationAttacking = false;
        isAttacking = false;
        base.Start();
    }

    /// <summary>
    /// �s���p�^�[�����s����
    /// </summary>
    protected override void DecideBehavior()
    {
        if (canChaseTarget && !IsGround())
        {
            AirMovement();
        }
        else if (canAttack && !sightChecker.IsObstructed() && disToTarget <= attackDist
            || canAttack && !sightChecker.IsObstructed() && disToTarget <= attackDist * 3)
        {
            if (disToTarget <= attackDist)
            {
                Attack(DECIDE_TYPE.MeleeAttack);
            }
            else if (disToTarget <= attackDist * 3)
            {
                Attack(DECIDE_TYPE.RotationAttack);
            }
        }
        else if (moveSpeed > 0 && canChaseTarget && Mathf.Abs(disToTargetX) > disToTargetMin)
        {
            if (canChaseTarget && IsWall() && !canJump)
            {
                Idle();
            }
            else if (canChaseTarget && target)
            {
                Tracking();
            }
        }
        else Idle();
    }

    /// <summary>
    /// �A�C�h������
    /// </summary>
    protected override void Idle()
    {
        SetAnimId((int)ANIM_ID.Idle);
        m_rb2d.linearVelocity = new Vector2(0f, m_rb2d.linearVelocity.y);
    }

    #region �U�������֘A

    /// <summary>
    /// ��]�U�����̍U������p
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isRotationAttacking && collision.gameObject.tag == "Player" && !hitPlayers.Contains(collision.gameObject))
        {
            // ���g���G���[�g�̂̏ꍇ�A�t�^�����Ԉُ�̎�ނ��擾����
            DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();

            hitPlayers.Add(collision.gameObject);
            collision.gameObject.GetComponent<PlayerBase>().ApplyDamage((int)(power * rotationPowerRate), transform.position, KB_POW.Medium, applyEffect);

        }
    }

    /// <summary>
    /// �U������
    /// </summary>
    public void Attack(DECIDE_TYPE attackType)
    {
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;

        switch (attackType)
        {
            case DECIDE_TYPE.RotationAttack:
                SetAnimId((int)ANIM_ID.RotationAttack);
                break;
            case DECIDE_TYPE.MeleeAttack:
                SetAnimId((int)ANIM_ID.MeleeAttack);
                break;
        }
    }

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] ��]�U������
    /// </summary>

    public override void OnAttackAnimEvent()
    {
        if (isStun)
        {
            isAttacking = false;
            return;
        }

        hitPlayers.Clear();

        SetAnimId((int)ANIM_ID.RotationAttack_Rotate);
        m_rb2d.linearVelocity = Vector3.zero;
        m_rb2d.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);

        // ���s���Ă��Ȃ���΁A��]�U���̃R���[�`�����J�n
        string cooldownKey = COROUTINE.RotationAttack.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(RotationAttackCoroutine(() =>
            {
                RemoveAndStopCoroutineByKey(cooldownKey);
            }));
            managedCoroutines.Add(cooldownKey, coroutine);
        }
    }

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] �ߐڍU������
    /// </summary>

    public override void OnAttackAnim2Event()
    {
        // �O�ɔ�яo��
        Vector2 jumpVec = new Vector2(moveSpeed * 1.5f * TransformUtils.GetFacingDirection(transform), jumpPower / 3);
        m_rb2d.linearVelocity = jumpVec;
        hitPlayers.Clear();

        // ���s���Ă��Ȃ���΁A�ߐڍU���̃R���[�`�����J�n
        string cooldownKey = COROUTINE.MeleeAttack.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(MeleeAttack());
            managedCoroutines.Add(cooldownKey, coroutine);
        }
    }

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] �ߐڍU���̔��菈�����I��
    /// </summary>
    public override void OnEndAttackAnim2Event()
    {
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
    /// ��]�U���̃R���[�`��
    /// </summary>
    /// <returns></returns>
    IEnumerator RotationAttackCoroutine(Action onFinished)
    {
        bool isAttacked = false;
        float startAttackSec = 2f;
        float currentSec = 0;

        while (true)
        {
            if (isStun)
            {
                m_rb2d.bodyType = RigidbodyType2D.Dynamic;
                isAttacking = false;
                yield break;
            }

            if(target == null)
            {
                var nearPlayer = GetNearPlayer(transform.position);
                if (nearPlayer != null) target = nearPlayer;
            }
            if (target)
            {
                // �^�[�Q�b�g�̂������������
                LookAtTarget();
            }

            currentSec += Time.deltaTime;

            if (currentSec >= startAttackSec && !isAttacked)
            {
                isRotationAttacking = true;
                isAttacked = true;
                Vector3 targetPos = target ? target.transform.position : this.transform.position + Vector3.down;

                // �ړI�n�Ɍ������ēːi
                m_rb2d.bodyType = RigidbodyType2D.Dynamic;
                m_rb2d.linearVelocity = Vector3.zero;
                m_rb2d.AddForce((targetPos - transform.position).normalized * jumpPower * 1.5f, ForceMode2D.Impulse);
            }
            else if (m_rb2d.linearVelocity.y <= 0 && !isAttacked)
            {
                // �󒆂ŐÎ~
                m_rb2d.bodyType = RigidbodyType2D.Static;
            }

            if (IsGround() && isAttacked)
            {
                isRotationAttacking = false;
                SetAnimId((int)ANIM_ID.RotationAttack_Finish);
                break;
            }

            yield return null;
        }

        // ���s���Ă��Ȃ���΁A�N�[���_�E���̃R���[�`�����J�n
        string cooldownKey = COROUTINE.AttackCooldown.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(AttackCooldown(attackCoolTime * 1.5f, () => {
                RemoveAndStopCoroutineByKey(cooldownKey);
            }));
            managedCoroutines.Add(cooldownKey, coroutine);
        }

        onFinished?.Invoke();
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
                if (collidersEnemies[i].gameObject.tag == "Player" && !hitPlayers.Contains(collidersEnemies[i].gameObject))
                {
                    hitPlayers.Add(collidersEnemies[i].gameObject);
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
        Idle();
        yield return new WaitForSeconds(time);
        isAttacking = false;
        onFinished?.Invoke();
    }

    #endregion

    #region �ړ������֘A

    /// <summary>
    /// �ǐՂ��鏈��
    /// </summary>
    protected override void Tracking()
    {
        SetAnimId((int)ANIM_ID.Run);
        Vector2 speedVec = Vector2.zero;
        if (IsFall() || IsWall())
        {
            speedVec = new Vector2(0f, m_rb2d.linearVelocity.y);
        }
        else
        {
            float distToPlayer = target.transform.position.x - this.transform.position.x;
            speedVec = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed, m_rb2d.linearVelocity.y);
        }
        m_rb2d.linearVelocity = speedVec;
    }

    /// <summary>
    /// �󒆏�Ԃł̈ړ�����
    /// </summary>
    void AirMovement()
    {
        SetAnimId((int)ANIM_ID.Run);

        // �W�����v(����)���Ƀv���C���[�Ɍ������Ĉړ�����
        float distToPlayer = target.transform.position.x - this.transform.position.x;
        Vector3 targetVelocity = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed, m_rb2d.linearVelocity.y);
        Vector3 velocity = Vector3.zero;
        m_rb2d.linearVelocity = Vector3.SmoothDamp(m_rb2d.linearVelocity, targetVelocity, ref velocity, 0.05f);
    }

    #endregion

    #region �q�b�g�����֘A

    /// <summary>
    /// �_���[�W���󂯂��Ƃ��̏���
    /// </summary>
    protected override void OnHit()
    {
        base.OnHit();
        isRotationAttacking = false;
        SetAnimId((int)ANIM_ID.Hit);
        m_rb2d.bodyType = RigidbodyType2D.Dynamic;
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
                animator.Play("Carcass_Damage_Animation", 0, 0);
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
        isRotationAttacking = false;
        m_rb2d.bodyType = RigidbodyType2D.Dynamic;
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
