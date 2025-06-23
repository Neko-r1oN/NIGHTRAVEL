//**************************************************
//  [�G] �h���[���𐧌䂷��N���X
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System.Collections;
using UnityEngine;

public class Drone : EnemyBase
{
    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum ANIM_ID
    {
        Idle = 1,
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
    float patorolRange = 10f;

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    float aimRotetionSpeed = 3f;
    #endregion

    #region �U���֘A
    [Foldout("�U���֘A")]
    [SerializeField] 
    Transform aimTransform;
    [Foldout("�U���֘A")]
    [SerializeField] 
    GunParticleController gunPsController;
    [Foldout("�U���֘A")]
    [SerializeField] 
    float gunBulletWidth;
    #endregion

    #region �`�F�b�N����
    // �ǂƒn�ʃ`�F�b�N
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField] 
    Transform wallCheck;
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField] 
    Vector2 wallCheckRadius = new Vector2(0, 1.5f);
    #endregion

    #region �^�[�Q�b�g�Ƃ̋���
    [SerializeField] float disToTargetMin = 2.5f;
    #endregion

    Coroutine patorolCoroutine;
    Vector2? startPatorolPoint = null;

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
        if (canAttack && projectileChecker.CanFireProjectile(gunBulletWidth, true) && !sightChecker.IsObstructed())
        {
            chaseAI.StopChase();
            Attack();
        }
        else if (moveSpeed > 0 && canChaseTarget && target)
        {
            Tracking();
        }
        else if (moveSpeed > 0 && canPatrol && !isPatrolPaused)
        {
            Patorol();
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
        m_rb2d.linearVelocity = new Vector2(0f, m_rb2d.linearVelocity.y);
    }

    #region �U�������֘A

    /// <summary>
    /// �U������
    /// </summary>
    void Attack()
    {
        doOnceDecision = false;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        chaseAI.StopChase();
        cancellCoroutines.Add(StartCoroutine(RangeAttack()));
    }

    /// <summary>
    /// �������U������
    /// </summary>
    IEnumerator RangeAttack()
    {
        yield return new WaitForSeconds(0.25f);  // �U���J�n��x��
        gunPsController.StartShooting();

        float time = 0;
        while (time < shotsPerSecond)
        {
            // �^�[�Q�b�g�̂�������Ɍ������ăG�C��
            if (target)
            {
                if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
                    || target.transform.position.x > transform.position.x && transform.localScale.x < 0) Flip();

                Vector3 direction = target.transform.position - transform.position;
                Quaternion quaternion = Quaternion.Euler(0, 0, projectileChecker.ClampAngleToTarget(direction));
                aimTransform.rotation = Quaternion.RotateTowards(aimTransform.rotation, quaternion, aimRotetionSpeed);
            }
            yield return new WaitForSeconds(0.1f);
            time += 0.1f;
        }

        cancellCoroutines.Add(StartCoroutine(AttackCooldown(attackCoolTime)));
    }

    /// <summary>
    /// �U�����̃N�[���_�E������
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time)
    {
        gunPsController.StopShooting();
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
        StopPatorol();
        chaseAI.DoChase(target);
    }

    /// <summary>
    /// ���񏈗�
    /// </summary>
    protected override void Patorol()
    {
        if (patorolCoroutine == null)
        {
            cancellCoroutines.Add(StartCoroutine(PatorolCoroutine()));
        }
    }

    /// <summary>
    /// ���񂷂鏈��
    /// </summary>
    IEnumerator PatorolCoroutine()
    {
        float pauseTime = 2f;
        if (startPatorolPoint == null)
        {
            startPatorolPoint = transform.position;
        }

        if (IsWall()) Flip();

        if (TransformHelper.GetFacingDirection(transform) > 0)
        {
            if (transform.position.x >= startPatorolPoint.Value.x + patorolRange)
            {
                isPatrolPaused = true;
                Idle();
                yield return new WaitForSeconds(pauseTime);
                isPatrolPaused = false;
                Flip();
            }
        }
        else if (TransformHelper.GetFacingDirection(transform) < 0)
        {
            if (transform.position.x <= startPatorolPoint.Value.x - patorolRange)
            {
                isPatrolPaused = true;
                Idle();
                yield return new WaitForSeconds(pauseTime);
                isPatrolPaused = false;
                Flip();
            }
        }

        Vector2 speedVec = Vector2.zero;
        speedVec = new Vector2(TransformHelper.GetFacingDirection(transform) * moveSpeed / 2, m_rb2d.linearVelocity.y);
        m_rb2d.linearVelocity = speedVec;
        patorolCoroutine = null;
    }

    /// <summary>
    /// ���񏈗����~����
    /// </summary>
    void StopPatorol()
    {
        startPatorolPoint = null;
    }

    #endregion

    #region �q�b�g�����֘A

    /// <summary>
    /// �_���[�W���󂯂��Ƃ��̏���
    /// </summary>
    protected override void OnHit()
    {
        base.OnHit();
        gunPsController.StopShooting();
    }

    /// <summary>
    /// ���S����Ƃ��ɌĂ΂�鏈������
    /// </summary>
    /// <returns></returns>
    protected override void OnDead()
    {
        gunPsController.StopShooting();
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
            projectileChecker.DrawProjectileRayGizmo(gunBulletWidth, true);
        }

        // �ǂ̔���
        if (wallCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(wallCheck.transform.position, wallCheckRadius);
        }
    }

    #endregion
}
