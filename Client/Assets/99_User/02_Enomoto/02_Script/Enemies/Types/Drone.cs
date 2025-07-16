//**************************************************
//  [�G] �h���[���𐧌䂷��N���X
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Drone : EnemyBase
{
    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        Dead,
    }

    /// <summary>
    /// �Ǘ�����R���[�`���̎��
    /// </summary>
    public enum COROUTINE
    {
        RangeAttack,
        AttackCooldown,
        PatorolCoroutine
    }

    /// <summary>
    /// �U�����@
    /// </summary>
    public enum ATTACK_TYPE_ID
    {
        None,
        RangeType,
    }

    #region �R���|�[�l���g
    EnemyProjectileChecker projectileChecker;
    #endregion

    #region �I���W�i���X�e�[�^�X
    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    float patorolRange = 10f;
    #endregion

    #region �U���֘A
    [Foldout("�U���֘A")]
    [SerializeField] 
    Transform aimTransform; // �e��AIM����
    [Foldout("�U���֘A")]
    [SerializeField] 
    GunParticleController gunPsController;
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

    Vector2? startPatorolPoint = null;

    protected override void Start()
    {
        base.Start();
        projectileChecker = aimTransform.GetComponent<EnemyProjectileChecker>();
        isAttacking = false;
        doOnceDecision = true;
    }

    /// <summary>
    /// �s���p�^�[�����s����
    /// </summary>
    protected override void DecideBehavior()
    {
        // �s���p�^�[��
        if (canAttack && projectileChecker.CanFireProjectile(target) && !sightChecker.IsObstructed())
        {
            chaseAI.Stop();
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
            chaseAI.Stop();
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

    /// <summary>
    /// �X�v���C�g�������ɂȂ�Ƃ��ɌĂ΂�鏈��
    /// </summary>
    protected override void OnTransparentSprites()
    {
        SetAnimId((int)ANIM_ID.Idle);

        // �����_���ȏꏊ�Ɍ������ď����ړ�
        float moveRange = 2f;
        float posX = transform.position.x + UnityEngine.Random.Range(-moveRange, moveRange);
        float posY = transform.position.y + UnityEngine.Random.Range(-moveRange, moveRange);
        Vector2 targetPoint = new Vector2(posX, posY);
        chaseAI.DoMove(targetPoint);
    }

    /// <summary>
    /// �t�F�[�h�C�������������Ƃ��ɌĂ΂�鏈��
    /// </summary>
    protected override void OnFadeInComp()
    {
        chaseAI.Stop();
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
        chaseAI.Stop();

        // ���s���Ă��Ȃ���΁A�������U���̃R���[�`�����J�n
        string key = COROUTINE.RangeAttack.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(RangeAttack(() => { RemoveCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// �������U������
    /// </summary>
    IEnumerator RangeAttack(Action onFinished)
    {
        yield return new WaitForSeconds(0.25f);  // �U���J�n��x��
        gunPsController.StartShooting();

        float time = 0;
        float waitSec = 0.05f;
        while (time < shotsPerSecond)
        {
            // �^�[�Q�b�g�̂�������Ɍ������ăG�C��
            if (target)
            {
                if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
                    || target.transform.position.x > transform.position.x && transform.localScale.x < 0) Flip();

                Vector3 direction = (target.transform.position - transform.position).normalized;
                projectileChecker.RotateAimTransform(direction);
            }
            yield return new WaitForSeconds(waitSec);
            time += waitSec;
        }

        // ���s���Ă��Ȃ���΁A�N�[���_�E���̃R���[�`�����J�n
        string key = COROUTINE.AttackCooldown.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(AttackCooldown(attackCoolTime, () => { RemoveCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
        onFinished?.Invoke();
    }

    /// <summary>
    /// �U�����̃N�[���_�E������
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time, Action onFinished)
    {
        gunPsController.StopShooting();
        isAttacking = true;
        yield return new WaitForSeconds(time);
        isAttacking = false;
        doOnceDecision = true;
        Idle();
        onFinished?.Invoke();
    }

    #endregion

    #region �ړ������֘A

    /// <summary>
    /// �ǐՂ��鏈��
    /// </summary>
    protected override void Tracking()
    {
        aimTransform.localEulerAngles = Vector3.back * 90f; // �e�̌�����������
        StopPatorol();
        chaseAI.DoChase(target);
    }

    /// <summary>
    /// ���񏈗�
    /// </summary>
    protected override void Patorol()
    {
        aimTransform.localEulerAngles = Vector3.back * 90f; // �e�̌�����������

        // ���s���Ă��Ȃ���΁A�p�g���[���̃R���[�`�����J�n
        string key = COROUTINE.PatorolCoroutine.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(PatorolCoroutine(() => { RemoveCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// ���񂷂鏈��
    /// </summary>
    IEnumerator PatorolCoroutine(Action onFinished)
    {
        float pauseTime = 2f;
        if (startPatorolPoint == null)
        {
            startPatorolPoint = transform.position;
        }

        if (IsWall()) Flip();

        if (TransformUtils.GetFacingDirection(transform) > 0)
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
        else if (TransformUtils.GetFacingDirection(transform) < 0)
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
        speedVec = new Vector2(TransformUtils.GetFacingDirection(transform) * moveSpeed / 2, m_rb2d.linearVelocity.y);
        m_rb2d.linearVelocity = speedVec;
        onFinished?.Invoke();
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
        if (projectileChecker != null)
        {
            projectileChecker.DrawProjectileRayGizmo(target);
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
