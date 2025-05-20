//**************************************************
//  �G�l�~�[�̃T���v���N���X(��s�^)
//  Author:r-enomoto
//**************************************************
using HardLight2DUtil;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy_Sample_Flyng : EnemyController
{
    /// <summary>
    /// �U�����@
    /// </summary>
    public enum ATTACK_TYPE_ID
    {
        None,
        MeleeType,
        RangeType,
    }

    #region �U�����@�ɂ���
    [Header("�U�����@")]
    [SerializeField] ATTACK_TYPE_ID attackType = ATTACK_TYPE_ID.None;
    [SerializeField] GameObject throwableObject;    // �������U���̒e(��)
    #endregion

    #region �I���W�i���̍s���p�^�[���ɂ���
    [Header("�I���W�i���̍s���p�^�[��")]
    [SerializeField] bool canPatrol;          // ��ɓ�����邱�Ƃ��\
    [SerializeField] bool canChaseTarget;     // �^�[�Q�b�g��ǐՉ\
    [SerializeField] bool canAttack;          // �U���\
    #endregion

    #region �I���W�i���̃X�e�[�^�X�Ǘ�
    [Header("�I���W�i���X�e�[�^�X")]
    [SerializeField] int bulletNum = 3;
    [SerializeField] float shotsPerSecond = 0.5f;
    [SerializeField] float attackCooldown = 0.5f;
    [SerializeField] float attackDist = 1.5f;
    bool doOnceDecision;
    bool isAttacking;
    bool isDead;
    #endregion

    #region �`�F�b�N����
    [Header("�`�F�b�N����")]
    // �ߋ����U���͈̔�
    [SerializeField] Transform meleeAttackCheck;
    [SerializeField] float meleeAttackRange = 0.9f;

    // �ǃ`�F�b�N
    [SerializeField] Transform wallCheck;
    [SerializeField] Vector2 wallCheckRadius = new Vector2(0, 1.5f);
    [SerializeField] LayerMask wallLayerMask;

    // �n�ʃ`�F�b�N
    [SerializeField] Transform platCheck;
    [SerializeField] Vector2 platCheckRadius = new Vector2(0, 1.5f);
    [SerializeField] LayerMask platLayerMask;
    #endregion

    #region �R���|�[�l���g
    SpriteRenderer m_spriteRenderer;
    #endregion

    #region �^�[�Q�b�g
    float disToTarget;
    float disToTargetX;
    [SerializeField] float disToTargetMin = 0.25f;
    #endregion

    void Start()
    {
        HP = hp;
        Power = power;
        Speed = speed;
        TargetLayerMask = targetLayerMask;
        ViewAngleMax = viewAngleMax;
        ViewDistMax = viewDistMax;
        TrackingRange = trackingRange;

        //disToTargetMin = attackDist * 0.25f;

        isAttacking = false;
        doOnceDecision = true;

        m_rb2d = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (!target && Players.Count > 0) target = GetTargetInSight();
        else if (canChaseTarget && target && disToTarget > trackingRange
            || !canChaseTarget && target && !IsTargetVisible()) target = null;

        // ��Q���A�n�ʂ����邩�擾
        isObstacle = Physics2D.OverlapBox(wallCheck.position, wallCheckRadius, 0f, wallLayerMask);
        isPlat = Physics2D.OverlapBox(platCheck.position, platCheckRadius, 0f, platLayerMask);
        if (!target && canPatrol) Run();
        else if (!target && !canPatrol) Idle();

        if (!target || isAttacking || isInvincible || hp <= 0 || !doOnceDecision) return;

        // �^�[�Q�b�g�Ƃ̋���
        disToTarget = Vector3.Distance(this.transform.position, target.transform.position);
        disToTargetX = target.transform.position.x - transform.position.x;

        // �^�[�Q�b�g�̂�������Ƀe�N�X�`���𔽓]
        if (canChaseTarget)
        {
            if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
                || target.transform.position.x > transform.position.x && transform.localScale.x < 0) Flip();
        }

        if (canAttack && !IsObstructed(target) && disToTarget <= attackDist && disToTarget > disToTargetMin
            && !isAttacking && attackType != ATTACK_TYPE_ID.None)
        {
            Attack();
        }
        else if (canPatrol && !canChaseTarget
            || canPatrol && canChaseTarget
            || canChaseTarget)
        {
            Run();
        }
        else Idle();
    }

    /// <summary>
    /// �A�C�h������
    /// </summary>
    void Idle()
    {
        SetAnimId((int)ANIM_ID.Idle);
        m_rb2d.linearVelocity = new Vector2(0f, m_rb2d.linearVelocity.y);
    }

    /// <summary>
    /// �U������
    /// </summary>
    void Attack()
    {
        doOnceDecision = false;
        isAttacking = true;
        SetAnimId((int)ANIM_ID.Attack);
        m_rb2d.linearVelocity = Vector2.zero;

        if (attackType == ATTACK_TYPE_ID.MeleeType) MeleeAttack();
        else if (attackType == ATTACK_TYPE_ID.RangeType) StartCoroutine(RangeAttack());
    }

    /// <summary>
    /// �ߐڍU������
    /// </summary>
    void MeleeAttack()
    {
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Player")
            {
                collidersEnemies[i].gameObject.GetComponent<CharacterController2D>().ApplyDamage(power, transform.position);
            }
        }
        StartCoroutine(AttackCooldown(attackCooldown));
    }

    /// <summary>
    /// �������U������
    /// </summary>
    IEnumerator RangeAttack()
    {
        GameObject target = this.target;
        for (int i = 0; i < bulletNum; i++)
        {
            GameObject throwableProj = Instantiate(throwableObject, transform.position, Quaternion.identity);
            Vector3 direction = target.transform.position - transform.position;
            throwableProj.GetComponent<Projectile>().Initialize(direction, gameObject);
            yield return new WaitForSeconds(shotsPerSecond);
        }
        StartCoroutine(AttackCooldown(attackCooldown));
    }

    /// <summary>
    /// ���鏈��
    /// </summary>
    void Run()
    {
        SetAnimId((int)ANIM_ID.Run);
        Vector2 speedVec = Vector2.zero;
        if (canChaseTarget && target)
        {
            Vector2 direction = target.transform.position - transform.position + new Vector3(disToTargetMin, disToTargetMin);
            speedVec = new Vector2(direction.x / Mathf.Abs(direction.x), direction.y / Mathf.Abs(direction.y)) * speed;

            if (isObstacle) speedVec = new Vector2(0f, speedVec.y);
            else if (isPlat) speedVec = new Vector2(speedVec.x, 0);
        }
        else if (canPatrol)
        {
            if (!isPlat || isObstacle) Flip();
            speedVec = new Vector2(transform.localScale.x * speed, m_rb2d.linearVelocity.y);
        }

        m_rb2d.linearVelocity = speedVec;
    }

    /// <summary>
    /// �_���[�W�K������
    /// </summary>
    /// <param name="damage"></param>
    public override void ApplyDamage(int damage, Transform attacker)
    {
        if (!isInvincible)
        {
            // �^�[�Q�b�g�̕����Ƀe�N�X�`���𔽓]
            if (attacker.position.x < transform.position.x && transform.localScale.x > 0
            || attacker.position.x > transform.position.x && transform.localScale.x < 0) Flip();

            SetAnimId((int)ANIM_ID.Hit);
            hp -= Mathf.Abs(damage);
            DoKnokBack(damage);

            if (hp > 0)
            {
                StartCoroutine(HitTime());
            }
            else
            {
                StartCoroutine(DestroyEnemy());
            }
        }
    }

    /// <summary>
    /// �U�����̃N�[���_�E������
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time)
    {
        isAttacking = true;
        yield return new WaitForSeconds(time);
        isAttacking = false;
        doOnceDecision = true;
        Idle();
    }

    /// <summary>
    /// ���S����
    /// </summary>
    /// <returns></returns>
    IEnumerator DestroyEnemy()
    {
        isDead = true;
        SetAnimId((int)ANIM_ID.Dead);
        yield return new WaitForSeconds(0.25f);
        GetComponent<CapsuleCollider2D>().direction = CapsuleDirection2D.Horizontal;
        m_rb2d.linearVelocity = new Vector2(0, m_rb2d.linearVelocity.y);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    /// <summary>
    /// [ �f�o�b�N�p ] Gizmos���g�p���Č��o�͈͂�`��
    /// </summary>
    void OnDrawGizmos()
    {
        // �U���͈�
        if (meleeAttackCheck)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleeAttackCheck.transform.position, meleeAttackRange);
        }

        // �ǂ̔���
        if (wallCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(wallCheck.transform.position, wallCheckRadius);
        }
        if (platCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(platCheck.transform.position, platCheckRadius);
        }

        // ����̕`��
        if (Players.Count > 0)
        {
            foreach (GameObject player in Players)
            {
                Vector2 dirToTarget = player.transform.position - transform.position;
                Vector2 angleVec = new Vector2(transform.localScale.x, 0);
                float angle = Vector2.Angle(dirToTarget, angleVec);
                RaycastHit2D hit2D = Physics2D.Raycast(transform.position, dirToTarget, ViewDistMax, TargetLayerMask);

                if (canChaseTarget && target && target == player)
                {
                    Debug.DrawRay(transform.position, dirToTarget, Color.red);
                }
                if (angle <= ViewAngleMax && hit2D && hit2D.collider.gameObject.CompareTag("Player"))
                {
                    Debug.DrawRay(transform.position, dirToTarget, Color.red);
                }
                else
                {
                    Debug.DrawRay(transform.position, dirToTarget, Color.cyan);
                }
            }
        }

        // �ǐՔ͈�
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, trackingRange);
    }
}
