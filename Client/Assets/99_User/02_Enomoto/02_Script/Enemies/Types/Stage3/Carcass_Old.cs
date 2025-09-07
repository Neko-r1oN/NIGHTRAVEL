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

public class Carcass_Old : EnemyBase
{
    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        RotateAttack,
        RotateAttack_Rotate,
        RotateAttack_Finish,
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
        RotateAttack,
        MeleeAttack,
        Tracking,
        AirMovement
    }
    DECIDE_TYPE nextDecide = DECIDE_TYPE.Waiting;

    /// <summary>
    /// �Ǘ�����R���[�`���̎��
    /// </summary>
    public enum COROUTINE
    {
        NextDecision,
        RotateAnim,
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

    #region ���I�֘A
    float decisionTimeMax = 2f;
    float randomDecision;
    bool endDecision;
    #endregion

    #region �^�[�Q�b�g�Ɨ�������
    readonly float disToTargetMin = 0.25f;
    #endregion

    #region �I���W�i��
    [Foldout("�I���W�i��")]
    [SerializeField]
    GameObject selfCurledObj;

    [Foldout("�I���W�i��")]
    [SerializeField]
    float rotateSpeed;
    #endregion

    protected override void Start()
    {
        isAttacking = false;
        doOnceDecision = true;
        NextDecision();
        base.Start();
    }

    /// <summary>
    /// �s���p�^�[�����s����
    /// </summary>
    protected override void DecideBehavior()
    {
        if (doOnceDecision)
        {
            doOnceDecision = false;

            switch (nextDecide)
            {
                case DECIDE_TYPE.Waiting:
                    Idle();
                    NextDecision(1f);
                    break;
                case DECIDE_TYPE.RotateAttack:
                    Attack(nextDecide);
                    break;
                case DECIDE_TYPE.MeleeAttack:
                    Attack(nextDecide);
                    break;
                case DECIDE_TYPE.Tracking:
                    Tracking();
                    NextDecision(0);
                    break;
                case DECIDE_TYPE.AirMovement:
                    AirMovement();
                    NextDecision(0);
                    break;
            }
        }
    }

    /// <summary>
    /// �A�C�h������
    /// </summary>
    protected override void Idle()
    {
        SetAnimId((int)ANIM_ID.Idle);
        m_rb2d.linearVelocity = new Vector2(0f, m_rb2d.linearVelocity.y);
    }

    #region ���I�����֘A

    /// <summary>
    /// ���I�������Ă�
    /// </summary>
    /// <param name="time"></param>
    void NextDecision(float? time = null)
    {
        if (time == null) time = UnityEngine.Random.Range(0.1f, decisionTimeMax);

        // ���s���Ă��Ȃ���΁A�s���̒��I�̃R���[�`�����J�n
        string key = COROUTINE.NextDecision.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(NextDecisionCoroutine((float)time, () => { RemoveAndStopCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// ���̍s���p�^�[�����菈��
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator NextDecisionCoroutine(float time, Action onFinished)
    {
        yield return new WaitForSeconds(time);

        #region �e�s���p�^�[���̏d�ݕt��

        Dictionary<DECIDE_TYPE, int> weights = new Dictionary<DECIDE_TYPE, int>();
        if (isSpawn)
        {
            weights[DECIDE_TYPE.Waiting] = 100;
        }
        else if (canChaseTarget && !IsGround())
        {
            weights[DECIDE_TYPE.AirMovement] = 100;
        }
        else if (canAttack && !sightChecker.IsObstructed() && disToTarget <= attackDist)
        {
            weights[DECIDE_TYPE.Waiting] = 15;

            if (disToTarget <= attackDist / 4) 
            {
                weights[DECIDE_TYPE.RotateAttack] = 25;
                weights[DECIDE_TYPE.MeleeAttack] = 50;
            }
            else if (disToTarget <= attackDist)
            {
                weights[DECIDE_TYPE.RotateAttack] = 50;
                weights[DECIDE_TYPE.MeleeAttack] = 25;
            }
        }
        else if (moveSpeed > 0 && canChaseTarget && Mathf.Abs(disToTargetX) > disToTargetMin)
        {
            if (canChaseTarget && IsWall() && !canJump)
            {
                weights[DECIDE_TYPE.Waiting] = 50;
            }
            else if (canChaseTarget && target)
            {
                weights[DECIDE_TYPE.Tracking] = 50;
            }
        }
        else weights[DECIDE_TYPE.Waiting] = 100;

        weights.Values.OrderBy(x => x); ;  // �����ɕ��ёւ�
        #endregion

        // �S�̂̒������g���Ē��I
        int totalWeight = weights.Values.Sum();
        randomDecision = UnityEngine.Random.Range(1, totalWeight + 1);

        // ���I�����l�Ŏ��̍s���p�^�[�������肷��
        int currentWeight = 0;
        foreach(var weight in weights)
        {
            currentWeight += weight.Value;
            if (currentWeight >= randomDecision)
            {
                nextDecide = weight.Key;
                break;
            }
        }

        doOnceDecision = true;
        onFinished?.Invoke();
    }

    #endregion

    #region �U�������֘A

    /// <summary>
    /// �U������
    /// </summary>
    public void Attack(DECIDE_TYPE attackType)
    {
        if(attackType == DECIDE_TYPE.RotateAttack || attackType == DECIDE_TYPE.MeleeAttack)
        {
            isAttacking = true;
            m_rb2d.linearVelocity = Vector2.zero;

            switch (attackType)
            {
                case DECIDE_TYPE.RotateAttack:
                    SetAnimId((int)ANIM_ID.RotateAttack);
                    break;
                case DECIDE_TYPE.MeleeAttack:
                    SetAnimId((int)ANIM_ID.MeleeAttack);
                    break;
            }
        }
        else
        {
            NextDecision(0);
        }
    }

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] ��]�U������
    /// </summary>

    public override void OnAttackAnimEvent()
    {
        m_rb2d.linearVelocity = Vector3.zero;
        m_rb2d.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);

        // ���s���Ă��Ȃ���΁A��]�U���̃R���[�`�����J�n
        string cooldownKey = COROUTINE.RotateAnim.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(RotateAttackCoroutine(() =>
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
    IEnumerator RotateAttackCoroutine(Action onFinished)
    {
        bool isAttacked = false;
        float startAttackSec = 2f;
        float currentSec = 0;

        while (true)
        {
            currentSec += Time.deltaTime;
            //selfCurledObj.transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);

            if (currentSec >= startAttackSec && !isAttacked)
            {
                isAttacked = true;

                // �^�[�Q�b�g�̂�������ɓːi
                m_rb2d.bodyType = RigidbodyType2D.Dynamic;
                m_rb2d.linearVelocity = Vector3.zero;
                m_rb2d.AddForce((target.transform.position - transform.position).normalized * jumpPower * 1.2f, ForceMode2D.Impulse);
            }
            else if (currentSec >= startAttackSec / 2 && !isAttacked)
            {
                // �󒆂ŐÎ~
                m_rb2d.bodyType = RigidbodyType2D.Static;
            }

            if (IsGround() && isAttacked)
            {
                SetAnimId((int)ANIM_ID.RotateAttack_Finish);
                break;
            }

            yield return null;
        }

        // ���s���Ă��Ȃ���΁A�N�[���_�E���̃R���[�`�����J�n
        string cooldownKey = COROUTINE.AttackCooldown.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(AttackCooldown(attackCoolTime, () => {
                RemoveAndStopCoroutineByKey(cooldownKey);
            }));
            managedCoroutines.Add(cooldownKey, coroutine);
        }

        onFinished?.Invoke();
    }

    /// <summary>
    /// �U�����̃N�[���_�E������
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time, Action onFinished)
    {
        isAttacking = true;
        yield return new WaitForSeconds(time);
        isAttacking = false;
        Idle();
        NextDecision();
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

        // �O�ɔ�э���
        Vector2 jumpVec = new Vector2(moveSpeed * 1.2f * TransformUtils.GetFacingDirection(transform), jumpPower);
        m_rb2d.linearVelocity = jumpVec;
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
