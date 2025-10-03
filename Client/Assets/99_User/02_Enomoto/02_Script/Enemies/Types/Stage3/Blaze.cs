//**************************************************
//  [�G] �u���C�Y�̃N���X
//  Author:r-enomoto
//**************************************************
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Blaze : EnemyBase
{
    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        Attack_Laser,
        Attack_Punch,
        Hit,
        Dead,
    }

    /// <summary>
    /// �s���p�^�[��
    /// </summary>
    public enum DECIDE_TYPE
    {
        Waiting = 1,
        Attack_Laser,
        Attack_Punch,
        AirMovement,
        Tracking,
        BackOff,
    }
    DECIDE_TYPE nextDecide = DECIDE_TYPE.Waiting;

    /// <summary>
    /// �Ǘ�����R���[�`���̎��
    /// </summary>
    public enum COROUTINE
    {
        NextDecision,
        BackOff,
        AttackCooldown,
    }

    #region �U���֘A
    [Foldout("�U���֘A")]
    [SerializeField]
    Transform aimTransform; // AIM����
    #endregion

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
    Transform frontFallCheck;
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Vector2 frontFallCheckRange;

    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Transform backFallCheck;
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Vector2 backFallCheckRange;
    #endregion

    #region ���I�֘A
    float decisionTimeMax = 2f;
    float randomDecision;
    bool endDecision;
    #endregion

    #region �I���W�i��
    EnemyProjectileChecker projectileChecker;
    #endregion


    #region �I�[�f�B�I�֘A

    [SerializeField]
    [Foldout("�I�[�f�B�I")]
    AudioSource audioPunch;

    [SerializeField]
    [Foldout("�I�[�f�B�I")]
    AudioSource audioFlash;

    [SerializeField]
    [Foldout("�I�[�f�B�I")]
    AudioSource audioLaser;

    #endregion

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
                    Idle();
                    NextDecision();
                    break;
                case DECIDE_TYPE.Attack_Laser:
                    AttackLaser();
                    break;
                case DECIDE_TYPE.Attack_Punch:
                    AttackPunch();
                    break;
                case DECIDE_TYPE.AirMovement:
                    AirMovement();
                    NextDecision(false);
                    break;
                case DECIDE_TYPE.Tracking:
                    Tracking();
                    NextDecision(false);
                    break;
                case DECIDE_TYPE.BackOff:
                    StartBackOff();
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
    void NextDecision(bool isRndTime = true, float? rndMaxTime = null)
    {
        float time = 0;
        if (isRndTime)
        {
            if (rndMaxTime == null) rndMaxTime = decisionTimeMax;
            time = UnityEngine.Random.Range(0.1f, (float)rndMaxTime);
        }

        // ���s���Ă��Ȃ���΁A�s���̒��I�̃R���[�`�����J�n
        string key = COROUTINE.NextDecision.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(NextDecisionCoroutine(time, () => { RemoveAndStopCoroutineByKey(key); }));
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

        const float punchDist = 4f;
        bool canAttackPunch = canAttack && !sightChecker.IsObstructed() && disToTarget <= punchDist;
        bool canAttackLaser = canAttack && projectileChecker.CanFireProjectile(target) && !sightChecker.IsObstructed() && 
            disToTarget >= punchDist && disToTarget <= attackDist;
        Dictionary<DECIDE_TYPE, int> weights = new Dictionary<DECIDE_TYPE, int>();

        if (canChaseTarget && !IsGround())
        {
            weights[DECIDE_TYPE.AirMovement] = 10;
        }
        else if (canAttackPunch || canAttackLaser)
        {
            bool wasAttacking = nextDecide == DECIDE_TYPE.Attack_Laser || nextDecide == DECIDE_TYPE.Attack_Punch;
            if(!IsBackFall()) weights[DECIDE_TYPE.BackOff] = wasAttacking ? 15 : 5;

            if (canAttackPunch) weights[DECIDE_TYPE.Attack_Punch] = wasAttacking ? 5 : 15;
            else if(canAttackLaser) weights[DECIDE_TYPE.Attack_Laser] = wasAttacking ? 5 : 15;
        }
        else if (canChaseTarget && target)
        {
            weights[DECIDE_TYPE.Tracking] = 10;
        }
        else
        {
            weights[DECIDE_TYPE.Waiting] = 10;
        }

        // value����ɏ����ŕ��בւ�
        var sortedWeights = weights.OrderBy(x => x.Value);
        #endregion

        // �S�̂̒������g���Ē��I
        int totalWeight = weights.Values.Sum();
        randomDecision = UnityEngine.Random.Range(1, totalWeight + 1);

        // ���I�����l�Ŏ��̍s���p�^�[�������肷��
        int currentWeight = 0;
        foreach (var weight in sortedWeights)
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

    #region �p���`

    /// <summary>
    /// �p���`�ɂ��U�������J�n
    /// </summary>
    void AttackPunch()
    {
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Punch);
    }

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] �p���`����
    /// </summary>
    public override void OnAttackAnim2Event()
    {
        audioPunch.Play();

        // ���g���G���[�g�̂̏ꍇ�A�t�^�����Ԉُ�̎�ނ��擾����
        DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();

        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Player")
            {
                collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position, KB_POW.Big, applyEffect);
            }
        }
    }

    #endregion

    #region ���[�U�[

    /// <summary>
    /// ���[�U�[�ɂ��U�������J�n
    /// </summary>
    void AttackLaser()
    {
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Laser);

        audioFlash.Play();
    }

    /// <summary>
    /// [�A�j���[�V�����C�x���g����Ăяo��] �t���b�V����
    /// </summary>
    public override void OnAttackAnim3Event()
    {
        audioFlash.Play();
    }

    /// <summary>
    /// [�A�j���[�V�����C�x���g����Ăяo��] ���[�U�[��
    /// </summary>
    public override void OnAttackAnim4Event()
    {
        audioLaser.Play();
    }

    #endregion

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] �U���N�[���_�E������
    /// </summary>
    public override void OnEndAttackAnimEvent()
    {
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
    /// �U�����̃N�[���_�E������
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time, Action onFinished)
    {
        isAttacking = true;
        Idle();
        yield return new WaitForSeconds(time);
        isAttacking = false;
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
        SetAnimId((int)ANIM_ID.Idle);
        Vector2 speedVec = Vector2.zero;
        if (IsFrontFall() || IsWall())
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
        SetAnimId((int)ANIM_ID.Idle);

        // �W�����v(����)���Ƀv���C���[�Ɍ������Ĉړ�����
        float distToPlayer = target.transform.position.x - this.transform.position.x;
        Vector3 targetVelocity = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed, m_rb2d.linearVelocity.y);
        Vector3 velocity = Vector3.zero;
        m_rb2d.linearVelocity = Vector3.SmoothDamp(m_rb2d.linearVelocity, targetVelocity, ref velocity, 0.05f);
    }

    /// <summary>
    /// BackOffCoroutine���Ăяo��
    /// </summary>
    void StartBackOff()
    {
        float coroutineTime = UnityEngine.Random.Range(1f, 3f);

        // ���s���Ă��Ȃ���΁A�N�[���_�E���̃R���[�`�����J�n
        string cooldownKey = COROUTINE.BackOff.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(BackOffCoroutine(coroutineTime, () => {
                RemoveAndStopCoroutineByKey(cooldownKey);
            }));
            managedCoroutines.Add(cooldownKey, coroutine);
        }
    }

    /// <summary>
    /// �������Ƃ葱����R���[�`��
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator BackOffCoroutine(float time, Action onFinished)
    {
        float waitSec = 0.1f;
        while (time > 0)
        {
            time -= waitSec;
            BackOff();
            yield return new WaitForSeconds(waitSec);
        }

        NextDecision(false);
        onFinished?.Invoke();
    }

    /// <summary>
    /// �������Ƃ�
    /// </summary>
    void BackOff()
    {
        SetAnimId((int)ANIM_ID.Idle);

        Vector2 speedVec = Vector2.zero;
        if (IsBackFall())
        {
            speedVec = new Vector2(0f, m_rb2d.linearVelocity.y);
        }
        else
        {
            float distToPlayer = target.transform.position.x - this.transform.position.x;
            speedVec = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed * -0.8f, m_rb2d.linearVelocity.y);
        }
        m_rb2d.linearVelocity = speedVec;
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
    /// �X�|�[���A�j���[�V�������I�������Ƃ�
    /// </summary>
    public override void OnEndSpawnAnimEvent()
    {
        base.OnEndSpawnAnimEvent();
        ApplyStun(0.5f, false);
    }

    /// <summary>
    /// �w�肵���A�j���[�V����ID���q�b�g�A�j���[�V�������ǂ���
    /// </summary>
    /// <param name="animationId"></param>
    /// <returns></returns>
    public override bool IsHitAnimIdFrom(int animationId)
    {
        return animationId == (int)ANIM_ID.Hit;
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
                animator.Play("Hit_Blaze2", 0, 0);
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

        DecideBehavior();
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
    /// �ڂ̑O�ɑ��ꂪ�Ȃ����ǂ���
    /// </summary>
    /// <returns></returns>
    bool IsFrontFall()
    {
        return !Physics2D.OverlapBox(frontFallCheck.position, frontFallCheckRange, 0, terrainLayerMask);
    }

    /// <summary>
    /// ���ɑ��ꂪ�Ȃ����ǂ���
    /// </summary>
    /// <returns></returns>
    bool IsBackFall()
    {
        return !Physics2D.OverlapBox(backFallCheck.position, frontFallCheckRange, 0, terrainLayerMask);
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
        if (frontFallCheck && backFallCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(frontFallCheck.position, frontFallCheckRange);
            Gizmos.DrawWireCube(backFallCheck.position, backFallCheckRange);
        }
    }

    #endregion
}
