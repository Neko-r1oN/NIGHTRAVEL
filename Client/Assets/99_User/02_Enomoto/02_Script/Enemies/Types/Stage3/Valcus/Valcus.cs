//**************************************************
//  [�G] ���@���N�X�̃N���X
//  Author:r-enomoto
//**************************************************
using DG.Tweening;
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Valcus : EnemyBase
{
    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        Attack_Normal,
        Attack_Smash1,
        Attack_Smash2,
        Tracking,
        Backoff,
        Dead,
    }

    /// <summary>
    /// �s���p�^�[��
    /// </summary>
    public enum DECIDE_TYPE
    {
        Waiting = 1,
        Attack_Normal,
        Attack_SmashCombo,
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
        MeleeAttack,
        AttackCombo,
        AttackCooldown,
        Tracking,
        BackOff,
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

    #region �Ǐ]�֘A
    const float disToTargetMin = 0.5f;  // �v���C���[�Ƃ̍Œ዗��
    #endregion

    #region �U���֘A
    
    List<GameObject> hitPlayers = new List<GameObject>();   // �U�����󂯂��v���C���[�̃��X�g

    #region �R���{�Z

    // ��ђ��˂�Ƃ��̒��n�n�_
    Vector2? targetPos;
    const float targetPosOffsetY = 3.5f;
    const float targetPosOffsetX = 2.5f;

    #endregion

    #endregion

    #region �I�[�f�B�I�֘A
    [SerializeField]
    [Foldout("�I�[�f�B�I")]
    AudioSource audioStamp;

    [SerializeField]
    [Foldout("�I�[�f�B�I")]
    AudioSource audioSpurn;

    [SerializeField]
    [Foldout("�I�[�f�B�I")]
    AudioSource audioWind;
    #endregion

    protected override void Start()
    {
        base.Start();
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
            m_rb2d.gravityScale = 5;
            hitPlayers = new List<GameObject>();
            doOnceDecision = false;

            switch (nextDecide)
            {
                case DECIDE_TYPE.Waiting:
                    Idle();
                    NextDecision();
                    break;
                case DECIDE_TYPE.Attack_Normal:
                    AttackNormal();
                    break;
                case DECIDE_TYPE.Attack_SmashCombo:
                    AttackSmash1();
                    break;
                case DECIDE_TYPE.Tracking:
                    StartTracking();
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
    }

    #region ���I�����֘A

    /// <summary>
    /// ���I�������Ă�
    /// </summary>
    /// <param name="time"></param>
    void NextDecision(bool isRndTime = true, float? rndMaxTime = null, float rndMinTime = 0.1f)
    {
        float time = 0;
        if (isRndTime)
        {
            if (rndMaxTime == null) rndMaxTime = decisionTimeMax;
            time = UnityEngine.Random.Range(rndMinTime, (float)rndMaxTime);
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
        Dictionary<DECIDE_TYPE, int> weights = new Dictionary<DECIDE_TYPE, int>();
        bool wasAttacking = nextDecide == DECIDE_TYPE.Attack_Normal || nextDecide == DECIDE_TYPE.Attack_SmashCombo;
        bool canAttackNormal = IsNormalAttack();

        // �A�����ăR���{�Z�����Ȃ��悤����
        bool canAttackSmashCombo = false;
        if (canAttack && nextDecide != DECIDE_TYPE.Attack_SmashCombo)
        {
            canAttackSmashCombo = SelectNewTargetInBossRoom();
        }

        // ��������ɊY������s���p�^�[���ɏd�ݕt��
        if (canChaseTarget && !IsGround())
        {
            weights[DECIDE_TYPE.Waiting] = 10;
        }
        else if (canAttackNormal || canAttackSmashCombo)
        {
            if(!IsBackFall()) weights[DECIDE_TYPE.BackOff] = wasAttacking ? 15 : 5;

            if (canAttackNormal) weights[DECIDE_TYPE.Attack_Normal] = wasAttacking ? 5 : 15;
            else if(canAttackSmashCombo) weights[DECIDE_TYPE.Attack_SmashCombo] = wasAttacking ? 5 : 15;
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

    #region �U���̋��ʏ���

    /// <summary>
    /// [AnimationEvent����̌Ăяo��] �U���̓����蔻�菈��
    /// </summary>
    public override void OnAttackAnimEvent()
    {
        if(nextDecide == DECIDE_TYPE.Attack_Normal)
        {
            audioSpurn.Play();
        }
        else
        {
            audioStamp.Play();
        }

        m_rb2d.gravityScale = 5;

        // ���s���Ă��Ȃ���΁A�U���̔�����J��Ԃ��R���[�`�����J�n
        string attackKey = COROUTINE.MeleeAttack.ToString();
        if (!ContaintsManagedCoroutine(attackKey))
        {
            Coroutine coroutine = StartCoroutine(MeleeAttack());
            managedCoroutines.Add(attackKey, coroutine);
        }
    }

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] �U���̔��菈�����I��
    /// </summary>
    public override void OnEndAttackAnimEvent()
    {
        RemoveAndStopCoroutineByKey(COROUTINE.MeleeAttack.ToString());
    }

    /// <summary>
    /// �ߐڍU���̔�����J��Ԃ�
    /// </summary>
    /// <returns></returns>
    IEnumerator MeleeAttack()
    {
        while (true)
        {
            // ���g���G���[�g�̂̏ꍇ�A�t�^�����Ԉُ�̎�ނ��擾����
            DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();

            Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
            for (int i = 0; i < collidersEnemies.Length; i++)
            {
                if (collidersEnemies[i].gameObject.tag == "Player" && !hitPlayers.Contains(collidersEnemies[i].gameObject))
                {
                    hitPlayers.Add(collidersEnemies[i].gameObject);
                    collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position, KB_POW.Medium, applyEffect);
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] �U���N�[���_�E������
    /// </summary>
    public override void OnEndAttackAnim2Event()
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

    #region �ʏ�U��

    /// <summary>
    /// ���݂���U��
    /// </summary>
    void AttackNormal()
    {
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Normal);
    }
    #endregion

    #region �R���{�U��(�O��)

    /// <summary>
    /// �ڕW���W�Ɍ������ăW�����v����
    /// </summary>
    void JumpToTargetPosition(float duration)
    {
        if (targetPos != null)
        {
            m_rb2d.gravityScale = 0;
            LookAtTarget();

            Vector2 addVec = new Vector2(-targetPosOffsetX * TransformUtils.GetFacingDirection(transform), targetPosOffsetY);
            transform.DOJump((Vector2)targetPos + addVec, jumpPower, 1, duration).SetEase(Ease.InOutQuad);
            audioWind.Play();
        }
    }

    /// <summary>
    /// �����𖞂����Ă���ꍇ�A�R���{�Z���J�n
    /// </summary>
    void AttackSmash1()
    {
        targetPos = GetGroundPointFrom(target);
        if(targetPos == null)
        {
            NextDecision();
            return;
        }

        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Smash1);
    }

    /// <summary>
    /// [Animation����̌Ăяo��] �ڕW�n�_�Ɍ������ăW�����v���J�n
    /// </summary>
    public override void OnAttackAnim3Event()
    {
        const float duration = 0.9f;
        var endValue = GetGroundPointFrom(target);
        if (endValue != null) targetPos = endValue;
        JumpToTargetPosition(duration);
    }

    /// <summary>
    /// [Animation����̌Ăяo��] Smash1��������A�U�����������Ȃ�������AttackSmash2�����s����
    /// </summary>
    public override void OnEndAttackAnim3Event()
    {
        if (!target)
        {
            SelectNewTargetInBossRoom();
        }
        else if(!hitPlayers.Contains(target))
        {
            // �U�������݂̃^�[�Q�b�g�ɖ������Ȃ�������AttackSmash2�����s����
            AttackSmash2();
        }
    }

    #endregion

    #region �R���{�U��(�㔼)

    /// <summary>
    /// �����𖞂����Ă���ꍇ�A�R���{�Z���J�n
    /// </summary>
    void AttackSmash2()
    {
        hitPlayers = new List<GameObject>();
        targetPos = GetGroundPointFrom(target);
        if (targetPos == null)
        {
            NextDecision();
            return;
        }

        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Smash2);
        LookAtTarget();
    }

    /// <summary>
    /// [Animation����̌Ăяo��] �ڕW�n�_�Ɍ������ăW�����v���J�n
    /// </summary>
    public override void OnAttackAnim4Event()
    {
        const float duration = 0.6f;
        var endValue = GetGroundPointFrom(target);
        if (endValue != null) targetPos = endValue;
        JumpToTargetPosition(duration);
    }

    #endregion

    #endregion

    #region �ړ������֘A

    /// <summary>
    ///  �Ǐ]�J�n
    /// </summary>
    void StartTracking()
    {
        string cooldownKey = COROUTINE.Tracking.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(TrackingCoroutine(() => {
                RemoveAndStopCoroutineByKey(cooldownKey);
            }));
            managedCoroutines.Add(cooldownKey, coroutine);
        }
    }

    /// <summary>
    /// ��莞��or�^�[�Q�b�g�ɐڋ߂ł���܂ŒǏ]����
    /// </summary>
    /// <param name="onFinished"></param>
    /// <returns></returns>
    IEnumerator TrackingCoroutine(Action onFinished)
    {
        const float waitSec = 0.1f;
        float trackingTime = 2f;
        bool isNormakAttack = false;

        while (trackingTime > 0)
        {
            // �r���Ń^�[�Q�b�g�������� || �^�[�Q�b�g�ƍŒ዗���܂ŋ߂Â����狭���I��
            if (!target || disToTargetX <= disToTargetMin) break;

            trackingTime -=waitSec;
            Tracking();

            if (IsNormalAttack())
            {
                isNormakAttack = true;
                break;
            }

            yield return new WaitForSeconds(waitSec);
        }

        // �^�[�Q�b�g�ɒǂ����Ȃ������ꍇ
        if (!isNormakAttack)
        {
            bool isTarget = SelectNewTargetInBossRoom();
            if (isTarget)
            {
                nextDecide = DECIDE_TYPE.Attack_SmashCombo;
                doOnceDecision = true;
            }
            else
            {
                NextDecision(false);
            }
        }
        else
        {
            nextDecide = DECIDE_TYPE.Attack_Normal;
            doOnceDecision = true;
        }

        onFinished?.Invoke();
    }

    /// <summary>
    /// �ǐՂ��鏈��
    /// </summary>
    protected override void Tracking()
    {
        SetAnimId((int)ANIM_ID.Tracking);
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
    /// BackOffCoroutine���Ăяo��
    /// </summary>
    void StartBackOff()
    {
        float coroutineTime = 0.3f;

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
        audioWind.Play();
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
        SetAnimId((int)ANIM_ID.Backoff);

        Vector2 speedVec = Vector2.zero;
        if (IsBackFall())
        {
            speedVec = new Vector2(0f, m_rb2d.linearVelocity.y);
        }
        else
        {
            float distToPlayer = target.transform.position.x - this.transform.position.x;
            speedVec = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed * -2, m_rb2d.linearVelocity.y);
        }
        m_rb2d.linearVelocity = speedVec;
    }

    #endregion

    #region �q�b�g�����֘A

    /// <summary>
    /// ���S����Ƃ��ɌĂ΂�鏈��
    /// </summary>
    /// <returns></returns>
    protected override void OnDead()
    {
        foreach (var spriteRenderer in SpriteRenderers)
        {
            ColorUtility.TryParseHtmlString("#2A5CFF", out Color color);
            spriteRenderer.material.SetColor("_HitEffectColor", color);
        }
        PlayHitBlendShader(true, 0.4f);

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
    /// �ʏ�U�����\���ǂ���
    /// </summary>
    /// <returns></returns>
    bool IsNormalAttack()
    {
        return canAttack && !sightChecker.IsObstructed() && disToTarget <= attackDist;
    }

    /// <summary>
    /// �^�[�Q�b�g���N�_�ɉ�������Ray���΂��A�n�ʂ̍��W���擾
    /// </summary>
    /// <returns></returns>
    Vector2? GetGroundPointFrom(GameObject target)
    {
        const float rayLength = 100;
        Vector2? targetPoint = null;

        if (target != null)
        {
            // �N�_(target)���牺������Ray�𔭎�
            var hit = Physics2D.Raycast((Vector2)target.transform.position, Vector2.down, rayLength, terrainLayerMask);

            if (hit.collider != null)
            {
                targetPoint = hit.point;
            }
        }
        
        return targetPoint;
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
