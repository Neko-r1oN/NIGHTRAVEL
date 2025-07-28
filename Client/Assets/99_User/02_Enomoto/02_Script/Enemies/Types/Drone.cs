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
        Hit,
        Dead,
    }

    /// <summary>
    /// �s���p�^�[��
    /// </summary>
    public enum DECIDE_TYPE
    {
        Waiting = 1,
        Attack,
        Patrole,
        Tracking,
        RndMove,
    }
    DECIDE_TYPE nextDecide = DECIDE_TYPE.Waiting;

    /// <summary>
    /// �Ǘ�����R���[�`���̎��
    /// </summary>
    public enum COROUTINE
    {
        NextDecision,
        RangeAttack,
        AttackCooldown,
        PatorolCoroutine
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

    #region ���I�֘A
    float decisionTimeMax = 2f;
    float randomDecision;
    bool endDecision;
    #endregion

    Vector2? startPatorolPoint = null;

    protected override void Start()
    {
        base.Start();
        projectileChecker = aimTransform.GetComponent<EnemyProjectileChecker>();
        isAttacking = false;
        doOnceDecision = true;
        NextDecision();
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
                    chaseAI.Stop();
                    Idle();
                    NextDecision();
                    break;
                case DECIDE_TYPE.Attack:
                    chaseAI.Stop();
                    Attack();
                    break;
                case DECIDE_TYPE.Patrole:
                    Patorol();
                    break;
                case DECIDE_TYPE.Tracking:
                    Tracking();
                    NextDecision();
                    break;
                case DECIDE_TYPE.RndMove:
                    chaseAI.DoRndMove();
                    NextDecision(2f);
                    break;
            }

            SetAnimId((int)ANIM_ID.Idle);
        }
    }

    /// <summary>
    /// �A�C�h������
    /// </summary>
    protected override void Idle()
    {
        m_rb2d.linearVelocity = new Vector2(0f, m_rb2d.linearVelocity.y);
    }

    #region ���I�����֘A

    /// <summary>
    /// ���I�������Ă�
    /// </summary>
    /// <param name="time"></param>
    void NextDecision(float? rndMaxTime = null)
    {
        if (rndMaxTime == null) rndMaxTime = decisionTimeMax;
        float time = UnityEngine.Random.Range(0.1f, (float)rndMaxTime);

        // ���s���Ă��Ȃ���΁A�s���̒��I�̃R���[�`�����J�n
        string key = COROUTINE.NextDecision.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(NextDecisionCoroutine(time, () => { RemoveCoroutineByKey(key); }));
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

        // �e�s���p�^�[���̏d��
        int waitingWeight = 0, attackWeight = 0, patorolWeight = 0, trackingWeight = 0, rndMoveWeight = 0;

        // �U�����\�ȏꍇ
        if (canAttack && projectileChecker.CanFireProjectile(target) && !sightChecker.IsObstructed())
        {
            int weightRate = nextDecide == DECIDE_TYPE.Attack ? 3 : 1;
            attackWeight = 10 / weightRate;
            rndMoveWeight = 5 * weightRate;
        }
        else if(target)
        {
            trackingWeight = 10;
        }
        else if (canPatrol && !isPatrolPaused)
        {
            patorolWeight = 10;
        }
        else
        {
            waitingWeight = 10;
        }

        // �S�̂̒������g���Ē��I
        int totalWeight = waitingWeight + attackWeight + patorolWeight + trackingWeight + rndMoveWeight;
        randomDecision = UnityEngine.Random.Range(1, totalWeight + 1);

        // ���I�����l�Ŏ��̍s���p�^�[�������肷��
        if (randomDecision <= waitingWeight) nextDecide = DECIDE_TYPE.Waiting;
        else if (randomDecision <= attackWeight) nextDecide = DECIDE_TYPE.Attack;
        else if (randomDecision <= patorolWeight) nextDecide = DECIDE_TYPE.Patrole;
        else if (randomDecision <= trackingWeight) nextDecide = DECIDE_TYPE.Tracking;
        else nextDecide = DECIDE_TYPE.RndMove;

        doOnceDecision = true;
        onFinished?.Invoke();
    }

    #endregion

    #region �e�N�X�`���E�A�j���[�V�����֘A

    /// <summary>
    /// �X�|�[���A�j�����[�V�����J�n��
    /// </summary>
    public override void OnSpawnAnimEvent()
    {
        base.OnSpawnAnimEvent();
        SetAnimId((int)ANIM_ID.Idle);
        chaseAI.DoRndMove();
    }

    /// <summary>
    /// �X�|�[���A�j���[�V�������I�������Ƃ�
    /// </summary>
    public override void OnEndSpawnAnimEvent()
    {
        base.OnEndSpawnAnimEvent();
        chaseAI.Stop();
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
                animator.Play("Hit");
                break;
            default:
                break;
        }
    }

    #endregion

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
                // �������̂ň�U�R�����g�A�E�g
                //if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
                //    || target.transform.position.x > transform.position.x && transform.localScale.x < 0) Flip();

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
        NextDecision();
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
        SetAnimId((int)ANIM_ID.Hit);
        gunPsController.StopShooting();
        if(hp > 0) NextDecision();
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
