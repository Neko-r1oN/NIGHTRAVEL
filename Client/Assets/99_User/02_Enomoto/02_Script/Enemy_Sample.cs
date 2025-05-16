//**************************************************
//  �G�l�~�[�̃T���v���N���X
//  Author:r-enomoto
//**************************************************
using HardLight2DUtil;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy_Sample : EnemyController
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

    #region �I���W�i���̃X�e�[�^�X�Ǘ�
    [Header("�I���W�i���X�e�[�^�X")]
    [SerializeField] int bulletNum = 3;
    [SerializeField] float jumpPower = 19;
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
    [SerializeField] Transform groundCheck;
    [SerializeField] Vector2 groundCheckRadius = new Vector2(0.5f, 0.2f);
    [SerializeField] LayerMask groundLayerMask;

    // �����`�F�b�N
    [SerializeField] Transform fallCheck;
    [SerializeField] float fallCheckRange = 0.9f;
    #endregion

    #region �R���|�[�l���g
    SpriteRenderer m_spriteRenderer;
    #endregion

    #region �^�[�Q�b�g
    float disToTarget;
    float disToTargetX;
    readonly float disToTargetMin = 0.25f;
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
        isPlat = Physics2D.OverlapCircle(fallCheck.position, fallCheckRange, groundLayerMask);
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

        if (canChaseTarget && isObstacle && IsGround() && Mathf.Abs(disToTargetX) > disToTargetMin) Jump();
        else if (canChaseTarget && !IsGround()) AirMovement();
        else if (canAttack && !IsObstructed(target) && disToTarget <= attackDist && !isAttacking && attackType != ATTACK_TYPE_ID.None) Attack();
        else if (canPatrol && !canChaseTarget 
            || canPatrol && canChaseTarget && Mathf.Abs(disToTargetX) > disToTargetMin 
            || canChaseTarget && Mathf.Abs(disToTargetX) > disToTargetMin) Run();
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
        for (int i = 0; i < bulletNum; i++)
        {
            GameObject throwableProj = Instantiate(throwableObject, transform.position + new Vector3(transform.localScale.x * 0.5f, -0.2f), Quaternion.identity);
            throwableProj.GetComponent<ThrowableProjectile>().owner = gameObject;
            Vector2 direction = new Vector2(transform.localScale.x, 0f);
            throwableProj.GetComponent<ThrowableProjectile>().direction = direction;
            yield return new WaitForSeconds(attackCooldown / bulletNum);
        }
        StartCoroutine(AttackCooldown(attackCooldown));
    }

    /// <summary>
    /// ���鏈��
    /// </summary>
    protected override void Run()
    {
        SetAnimId((int)ANIM_ID.Run);
        base.Run();
    }

    /// <summary>
    /// �󒆏�Ԃł̈ړ�����
    /// </summary>
    void AirMovement()
    {
        SetAnimId((int)ANIM_ID.Fall);

        // �W�����v(����)���Ƀv���C���[�Ɍ������Ĉړ�����
        float distToPlayer = target.transform.position.x - this.transform.position.x;
        Vector3 targetVelocity = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * speed, m_rb2d.linearVelocity.y);
        Vector3 velocity = Vector3.zero;
        m_rb2d.linearVelocity = Vector3.SmoothDamp(m_rb2d.linearVelocity, targetVelocity, ref velocity, 0.05f);
    }

    /// <summary>
    /// �W�����v����
    /// </summary>
    void Jump()
    {
        SetAnimId((int)ANIM_ID.Fall);

        transform.position += Vector3.up * groundCheckRadius.y;
        m_rb2d.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
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
    /// �n�ʔ���
    /// </summary>
    /// <returns></returns>
    bool IsGround()
    {
        // �����ɂQ�̎n�_�ƏI�_���쐬����
        Vector3 leftStartPosition = groundCheck.transform.position + Vector3.left * groundCheckRadius.x / 2;
        Vector3 rightStartPosition = groundCheck.transform.position + Vector3.right * groundCheckRadius.x / 2;
        Vector3 endPosition = groundCheck.transform.position + Vector3.down * groundCheckRadius.y;

        return Physics2D.Linecast(leftStartPosition, endPosition, groundLayerMask)
            || Physics2D.Linecast(rightStartPosition, endPosition, groundLayerMask);
    }

    /// <summary>
    /// [ �f�o�b�N�p ] Gizmos���g�p���Č��o�͈͂�`��
    /// </summary>
    void OnDrawGizmos()
    {
        // �U���͈�
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(meleeAttackCheck.transform.position, meleeAttackRange);

        // �ǂ̔���
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(wallCheck.transform.position, wallCheckRadius);

        // �n�ʔ���
        Vector3 leftStartPosition = groundCheck.transform.position + Vector3.left * groundCheckRadius.x / 2;
        Vector3 rightStartPosition = groundCheck.transform.position + Vector3.right * groundCheckRadius.x / 2;
        Vector3 endPosition = groundCheck.transform.position + Vector3.down * groundCheckRadius.y;
        Gizmos.DrawLine(leftStartPosition, endPosition);
        Gizmos.DrawLine(rightStartPosition, endPosition);

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

        // �����`�F�b�N
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(fallCheck.position, fallCheckRange);
    }
}