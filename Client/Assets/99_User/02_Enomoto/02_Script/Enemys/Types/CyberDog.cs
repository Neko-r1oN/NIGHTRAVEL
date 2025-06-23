//**************************************************
//  [�G] �T�C�o�[�h�b�N�̃N���X
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System.Collections;
using UnityEngine;

public class CyberDog : EnemyBase
{
    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum ANIM_ID
    {
        None = 0,
        Idle,
        Attack,
        Run,
        Hit,
        Dead,
    }

    #region �`�F�b�N����
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
    float fallCheckRange = 0.9f;
    #endregion

    #region �^�[�Q�b�g�Ɨ�������
    readonly float disToTargetMin = 0.25f;
    #endregion

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
        doOnceDecision = true;
    }

    /// <summary>
    /// �s���p�^�[�����s����
    /// </summary>
    protected override void DecideBehavior()
    {
        // �s���p�^�[��
        if (canChaseTarget && !IsGround())
        {
            AirMovement();
        }
        else if (canAttack && !sightChecker.IsObstructed() && disToTarget <= attackDist)
        {
            Attack();
        }
        else if (moveSpeed > 0 && canPatrol && Mathf.Abs(disToTargetX) > disToTargetMin
            || moveSpeed > 0 && canChaseTarget && Mathf.Abs(disToTargetX) > disToTargetMin)
        {
            if (canChaseTarget && IsWall() && !canJump)
            {
                Idle();
            }
            else if (canChaseTarget && target)
            {
                Tracking();
            }
            else if (canPatrol)
            {
                Patorol();
            }
        }
        else Idle();
    }

    /// <summary>
    /// �A�C�h������
    /// </summary>
    protected override void Idle()
    {
        if (!target)
        {
            SetAnimId((int)ANIM_ID.Idle);
        }
        else
        {
            SetAnimId((int)ANIM_ID.None);
        }

        m_rb2d.linearVelocity = new Vector2(0f, m_rb2d.linearVelocity.y);
    }

    #region �U�������֘A

    /// <summary>
    /// �U������
    /// </summary>
    public void Attack()
    {
        doOnceDecision = false;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack);
    }

    /// <summary>
    /// �ߐڍU������ [Animation�C�x���g����̌Ăяo��]
    /// </summary>
    
    public override void OnAttackAnimEvent()
    {
        // �O�ɔ�э���
        Vector2 jumpVec = new Vector2(18 * TransformHelper.GetFacingDirection(transform), 10);
        m_rb2d.linearVelocity = jumpVec;

        // ���g���G���[�g�̂̏ꍇ�A�t�^�����Ԉُ�̎�ނ��擾����
        bool isElite = this.isElite && enemyElite != null;
        StatusEffectController.EFFECT_TYPE applyEffect = StatusEffectController.EFFECT_TYPE.None;
        if (isElite)
        {
            applyEffect = enemyElite.GetAddStatusEffectEnum();
        }

        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Player")
            {
                collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position);

                if (isElite)
                {
                    // ��Ԉُ��K�p������
                    StatusEffectController statusEffect = collidersEnemies[i].gameObject.GetComponent<StatusEffectController>();
                    if (statusEffect)
                    {
                        statusEffect.ApplyStatusEffect(applyEffect);
                    }
                }
            }
        }
        cancellCoroutines.Add(StartCoroutine(AttackCooldown(attackCoolTime)));
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
    /// ���񂷂鏈��
    /// </summary>
    protected override void Patorol()
    {
        SetAnimId((int)ANIM_ID.Run);
        if (IsFall() || IsWall()) Flip();
        Vector2 speedVec = new Vector2(TransformHelper.GetFacingDirection(transform) * moveSpeed, m_rb2d.linearVelocity.y);
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
    }

    /// <summary>
    /// ���S����Ƃ��ɌĂ΂�鏈������
    /// </summary>
    /// <returns></returns>
    protected override void OnDead()
    {
        SetAnimId((int)ANIM_ID.Dead);
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
        return !Physics2D.OverlapCircle(fallCheck.position, fallCheckRange, terrainLayerMask);
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
            Gizmos.DrawWireSphere(fallCheck.position, fallCheckRange);
        }
    }

    #endregion
}
