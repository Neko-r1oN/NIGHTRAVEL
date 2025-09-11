////**************************************************
////  �ߐڃ^�C�v�̓G�N���X
////  Author:r-enomoto
////**************************************************
//using System.Collections;
//using UnityEngine;

//public class EnemyMelee : EnemyBase
//{
//    /// <summary>
//    /// �A�j���[�V����ID
//    /// </summary>
//    public enum ANIM_ID
//    {
//        Idle = 1,
//        Attack,
//        Run,
//        Hit,
//        Fall,
//        Dead,
//    }

//    #region �`�F�b�N����
//    [Header("�`�F�b�N����")]
//    // �ߋ����U���͈̔�
//    [SerializeField] Transform meleeAttackCheck;
//    [SerializeField] float meleeAttackRange = 0.9f;

//    // �ǁE�n�ʃ`�F�b�N
//    [SerializeField] Transform wallCheck;
//    [SerializeField] Vector2 wallCheckRadius = new Vector2(0, 1.5f);
//    [SerializeField] Transform groundCheck;
//    [SerializeField] Vector2 groundCheckRadius = new Vector2(0.5f, 0.2f);

//    // �����`�F�b�N
//    [SerializeField] Transform frontFallCheck;
//    [SerializeField] float frontFallCheckRange = 0.9f;
//    #endregion

//    #region �^�[�Q�b�g�Ɨ�������
//    readonly float disToTargetMin = 0.25f;
//    #endregion

//    protected override void Start()
//    {
//        base.Start();
//        isAttacking = false;
//        doOnceDecision = true;
//    }

//    /// <summary>
//    /// �s���p�^�[�����s����
//    /// </summary>
//    protected override void DecideBehavior()
//    {
//        // �s���p�^�[��
//        if (canChaseTarget && !IsGround())
//        {
//            AirMovement();
//        }
//        else if (canAttack && !sightChecker.IsObstructed() && disToTarget <= attackDist)
//        {
//            Attack();
//        }
//        else if (moveSpeed > 0 && canPatrol && Mathf.Abs(disToTargetX) > disToTargetMin
//            || moveSpeed > 0 && canChaseTarget && Mathf.Abs(disToTargetX) > disToTargetMin)
//        {
//            if (canChaseTarget && IsWall() && !canJump)
//            {
//                Idle();
//            }
//            else if (canChaseTarget && target)
//            {
//                Teleport();
//            }
//            else if (canPatrol)
//            {
//                Patorol();
//            }
//        }
//        else Idle();
//    }

//    /// <summary>
//    /// �A�C�h������
//    /// </summary>
//    protected override void Idle()
//    {
//        SetAnimId((int)ANIM_ID.Idle);
//        m_rb2d.linearVelocity = new Vector2(0f, m_rb2d.linearVelocity.y);
//    }

//    /// <summary>
//    /// �U������
//    /// </summary>
//    public void Attack()
//    {
//        doOnceDecision = false;
//        isAttacking = true;
//        SetAnimId((int)ANIM_ID.Attack);
//        m_rb2d.linearVelocity = Vector2.zero;
//        if (chaseAI) chaseAI.Stop();

//        MeleeAttack();
//    }

//    /// <summary>
//    /// �ߐڍU������
//    /// </summary>
//    void MeleeAttack()
//    {
//        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
//        for (int i = 0; i < collidersEnemies.Length; i++)
//        {
//            if (collidersEnemies[i].Object.tag == "Player")
//            {
//                collidersEnemies[i].Object.GetComponent<PlayerBase>().ApplyDamageRequest(power, transform.position);
//            }
//        }
//        cancellCoroutines.Add(StartCoroutine(AttackCooldownCoroutine(attackCoolTime)));
//    }

//    /// <summary>
//    /// �ǐՂ��鏈��
//    /// </summary>
//    protected override void Teleport()
//    {
//        SetAnimId((int)ANIM_ID.Run);
//        Vector2 speedVec = Vector2.zero;

//        if (IsFrontFall() || IsWall())
//        {
//            speedVec = new Vector2(0f, m_rb2d.linearVelocity.y);
//        }
//        else
//        {
//            float distToPlayer = target.transform.position.x - this.transform.position.x;
//            speedVec = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed, m_rb2d.linearVelocity.y);
//        }

//        m_rb2d.linearVelocity = speedVec;
//    }

//    /// <summary>
//    /// ���񂷂鏈��
//    /// </summary>
//    protected override void Patorol()
//    {
//        SetAnimId((int)ANIM_ID.Run);
//        if (IsFrontFall() || IsWall()) Flip();
//        Vector2 speedVec = new Vector2(TransformUtils.GetFacingDirection(transform) * moveSpeed, m_rb2d.linearVelocity.y);
//        m_rb2d.linearVelocity = speedVec;
//    }

//    /// <summary>
//    /// �󒆏�Ԃł̈ړ�����
//    /// </summary>
//    void AirMovement()
//    {
//        SetAnimId((int)ANIM_ID.Fall);

//        // �W�����v(����)���Ƀv���C���[�Ɍ������Ĉړ�����
//        float distToPlayer = target.transform.position.x - this.transform.position.x;
//        Vector3 targetVelocity = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed, m_rb2d.linearVelocity.y);
//        Vector3 velocity = Vector3.zero;
//        m_rb2d.linearVelocity = Vector3.SmoothDamp(m_rb2d.linearVelocity, targetVelocity, ref velocity, 0.05f);
//    }

//    /// <summary>
//    /// �_���[�W���󂯂��Ƃ��̏���
//    /// </summary>
//    protected override void OnHit()
//    {
//        base.OnHit();
//        //SetAnimId((int)ANIM_ID.Hit);
//    }

//    /// <summary>
//    /// �U�����̃N�[���_�E������
//    /// </summary>
//    /// <returns></returns>
//    IEnumerator AttackCooldownCoroutine(float time)
//    {
//        isAttacking = true;
//        yield return new WaitForSeconds(time);
//        isAttacking = false;
//        doOnceDecision = true;
//        Idle();
//    }

//    /// <summary>
//    /// ���S����Ƃ��ɌĂ΂�鏈������
//    /// </summary>
//    /// <returns></returns>
//    protected override void OnDead()
//    {
//        //SetAnimId((int)ANIM_ID.Dead);
//    }

//    /// <summary>
//    /// �ǂ����邩�ǂ���
//    /// </summary>
//    /// <returns></returns>
//    bool IsWall()
//    {
//        return Physics2D.OverlapBox(wallCheck.position, wallCheckRadius, 0f, terrainLayerMask);
//    }

//    /// <summary>
//    /// ���������ǂ���
//    /// </summary>
//    /// <returns></returns>
//    bool IsFrontFall()
//    {
//        return !Physics2D.OverlapCircle(frontFallCheck.position, frontFallCheckRange, terrainLayerMask);
//    }

//    /// <summary>
//    /// �n�ʔ���
//    /// </summary>
//    /// <returns></returns>
//    bool IsGround()
//    {
//        // �����ɂQ�̎n�_�ƏI�_���쐬����
//        Vector3 leftStartPosition = groundCheck.transform.position + Vector3.left * groundCheckRadius.x / 2;
//        Vector3 rightStartPosition = groundCheck.transform.position + Vector3.right * groundCheckRadius.x / 2;
//        Vector3 endPosition = groundCheck.transform.position + Vector3.down * groundCheckRadius.y;

//        return Physics2D.Linecast(leftStartPosition, endPosition, terrainLayerMask)
//            || Physics2D.Linecast(rightStartPosition, endPosition, terrainLayerMask);
//    }

//    /// <summary>
//    /// ���o�͈͂̕`�揈��
//    /// </summary>
//    protected override void DrawDetectionGizmos()
//    {
//        // �U���J�n����
//        Gizmos.color = Color.blue;
//        Gizmos.DrawWireSphere(transform.position, attackDist);

//        // �U���͈�
//        if (meleeAttackCheck)
//        {
//            Gizmos.color = Color.red;
//            Gizmos.DrawWireSphere(meleeAttackCheck.transform.position, meleeAttackRange);
//        }

//        // �ǂ̔���
//        if (wallCheck)
//        {
//            Gizmos.color = Color.green;
//            Gizmos.DrawWireCube(wallCheck.transform.position, wallCheckRadius);
//        }

//        // �n�ʔ���
//        if (groundCheck)
//        {
//            Vector3 leftStartPosition = groundCheck.transform.position + Vector3.left * groundCheckRadius.x / 2;
//            Vector3 rightStartPosition = groundCheck.transform.position + Vector3.right * groundCheckRadius.x / 2;
//            Vector3 endPosition = groundCheck.transform.position + Vector3.down * groundCheckRadius.y;
//            Gizmos.DrawLine(leftStartPosition, endPosition);
//            Gizmos.DrawLine(rightStartPosition, endPosition);
//        }

//        // �����`�F�b�N
//        if (frontFallCheck)
//        {
//            Gizmos.color = Color.green;
//            Gizmos.DrawWireSphere(frontFallCheck.position, frontFallCheckRange);
//        }
//    }
//}
