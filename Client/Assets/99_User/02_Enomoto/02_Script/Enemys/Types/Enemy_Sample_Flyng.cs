//**************************************************
//  �G�l�~�[�̃T���v���N���X(��s�^)
//  Author:r-enomoto
//**************************************************
using DG.Tweening;
using HardLight2DUtil;
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Sample_Flyng : EnemyController
{
    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum ANIM_ID
    {
        Idle = 1,
        Attack,
        Run,
        Hit,
        Fall,
        Dead,
    }

    /// <summary>
    /// �U�����@
    /// </summary>
    public enum ATTACK_TYPE_ID
    {
        None,
        RangeType,
    }

    #region �I���W�i���X�e�[�^�X
    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    float tweenMoveRange = 10f;
    #endregion

    #region �U�����@�ɂ���
    [Header("�U�����@")]
    [SerializeField] ATTACK_TYPE_ID attackType = ATTACK_TYPE_ID.None;
    [SerializeField] GameObject throwableObject;    // �������U���̒e(��)
    [SerializeField] Transform aimTransform;        // �������U���̒e�̐����ʒu
    #endregion

    #region �`�F�b�N����
    [Header("�`�F�b�N����")]
    // �ǂƒn�ʃ`�F�b�N
    [SerializeField] Transform wallCheck;
    [SerializeField] Vector2 wallCheckRadius = new Vector2(0, 1.5f);
    [SerializeField] LayerMask terrainLayerMask;
    #endregion

    #region �^�[�Q�b�g�Ƃ̋���
    [SerializeField] float disToTargetMin = 2.5f;
    #endregion

    Vector2? startPatorolPoint = null;
    float randomDecision;

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
        if (canAttack && projectileChecker.CanFireProjectile(throwableObject, true) && !sightChecker.IsObstructed() && attackType != ATTACK_TYPE_ID.None)
        {
            chaseAI.StopChase();
            Attack();
        }
        else if (speed > 0 && canChaseTarget && target)
        {
            Tracking();
        }
        else if (speed > 0 && canPatrol && !isPatrolPaused)
        {
            StartCoroutine(Patorol());
        }
        else
        {
            chaseAI.StopChase();
            Idle();
        }
    }

    /// <summary>
    /// �A�C�h������
    /// </summary>
    protected override void Idle()
    {
        //SetAnimId((int)ANIM_ID.Idle);
        m_rb2d.linearVelocity = new Vector2(0f, m_rb2d.linearVelocity.y);
    }

    /// <summary>
    /// �U������
    /// </summary>
    void Attack()
    {
        doOnceDecision = false;
        isAttacking = true;
        //SetAnimId((int)ANIM_ID.Attack);
        m_rb2d.linearVelocity = Vector2.zero;
        chaseAI.StopChase();
        StartCoroutine(RangeAttack());
    }

    /// <summary>
    /// �������U������
    /// </summary>
    IEnumerator RangeAttack()
    {
        yield return new WaitForSeconds(0.5f);  // �U���J�n��x��

        GameObject target = this.target;
        for (int i = 0; i < bulletNum; i++)
        {
            GameObject throwableProj = Instantiate(throwableObject, aimTransform.position, Quaternion.identity);
            Vector3 direction = target.transform.position - transform.position;
            throwableProj.GetComponent<Projectile>().Initialize(direction, gameObject);
            yield return new WaitForSeconds(shotsPerSecond);
        }
        StartCoroutine(AttackCooldown(attackCoolTime));
    }

    /// <summary>
    /// �ǐՂ��鏈��
    /// </summary>
    protected override void Tracking()
    {
        StopPatorol();
        chaseAI.DoChase(target);
    }

    /// <summary>
    /// ���񂷂鏈��
    /// </summary>
    protected override IEnumerator Patorol()
    {
        float pauseTime = 3f;
        if (startPatorolPoint == null)
        {
            startPatorolPoint = transform.position;
        }

        if (IsWall()) Flip();

        if (transform.localScale.x > 0)
        {
            if (transform.position.x >= startPatorolPoint.Value.x + tweenMoveRange)
            {
                isPatrolPaused = true;
                Idle();
                yield return new WaitForSeconds(pauseTime);
                isPatrolPaused = false;
                Flip();
            }
        }
        else if (transform.localScale.x < 0)
        {
            if (transform.position.x <= startPatorolPoint.Value.x - tweenMoveRange)
            {
                isPatrolPaused = true;
                Idle();
                yield return new WaitForSeconds(pauseTime);
                isPatrolPaused = false;
                Flip();
            }
        }

        Vector2 speedVec = Vector2.zero;
        speedVec = new Vector2(transform.localScale.x * speed / 2, m_rb2d.linearVelocity.y);
        m_rb2d.linearVelocity = speedVec;
    }

    /// <summary>
    /// ���񏈗����~����
    /// </summary>
    void StopPatorol()
    {
        startPatorolPoint = null;
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

            //SetAnimId((int)ANIM_ID.Hit);
            life -= Mathf.Abs(damage);
            DoKnokBack(damage);

            if (life > 0)
            {
                StartCoroutine(HitTime());
            }
            else if (!isDead)
            {
                StartCoroutine(DestroyEnemy(attacker.gameObject.GetComponent<Player>()));
            }
        }
    }

    /// <summary>
    /// �_���[�W�K�����̖��G����
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator HitTime()
    {
        //SetAnimId((int)ANIM_ID.Hit);
        yield return null;
        base.HitTime();
    }

    /// <summary>
    /// �ǂ����邩�ǂ���
    /// </summary>
    /// <returns></returns>
    bool IsWall()
    {
        return Physics2D.OverlapBox(wallCheck.position, wallCheckRadius, 0f, terrainLayerMask);
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
    /// ���S�A�j���[�V����
    /// </summary>
    /// <returns></returns>
    protected override void PlayDeadAnim()
    {
        //SetAnimId((int)ANIM_ID.Dead);
    }

    /// <summary>
    /// ���o�͈͂̕`�揈��
    /// </summary>
    protected override void DrawDetectionGizmos()
    {
        // �U���J�n����
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackDist);

        // �ː�
        if (sightChecker != null)
        {
            projectileChecker.DrawProjectileRayGizmo(throwableObject, true);
        }

        // �ǂ̔���
        if (wallCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(wallCheck.transform.position, wallCheckRadius);
        }
    }
}
