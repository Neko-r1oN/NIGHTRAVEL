//**************************************************
//  [�G] ���@���N�X�E�R�[�h�N���X�^���̃N���X
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
using Unity.Cinemachine;

public class ValksCodeCrystal : EnemyBase
{
    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        Attack_NormalCombo,
        Attack_PunchCombo,
        Attack_Dive,
        Attack_Laser,
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
        Attack_NormalCombo,
        Attack_PunchCombo,
        Attack_Dive,
        Attack_Laser,
        Tracking,
        BackOff,
    }
    DECIDE_TYPE nextDecide = DECIDE_TYPE.Waiting;
    DECIDE_TYPE lastAttackPattern;

    /// <summary>
    /// �Ǘ�����R���[�`���̎��
    /// </summary>
    public enum COROUTINE
    {
        NextDecision,
        Attack_NormalCombo,
        Attack_PunchCombo,
        Attack_Dive,
        Attack_Laser,
        AttackCooldown,
        Tracking,
        BackOff,
    }

    #region �`�F�b�N�֘A
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

    #region �X�̊�̃p�[�e�B�N�������֘A
    [Foldout("�U���֘A")]
    [SerializeField]
    List<GameObject> iceRockPsPrefabs = new List<GameObject>();

    [Foldout("�U���֘A")]
    [SerializeField]
    List<Transform> iceRockPsPoints = new List<Transform>();

    [Foldout("�U���֘A")]
    [SerializeField]
    List<float> iceRockPsPointsY = new List<float>();
    #endregion

    [Foldout("�U���֘A")]
    [SerializeField]
    List<GameObject> damageColliders = new List<GameObject>();

    [Foldout("�U���֘A")]
    [SerializeField]
    Vector2 stageCenterPosition;

    Vector2? targetPos;
    const float warpPosY = 5f;

    // �����U���A���[�U�[�U���𔭓��ł���܂ł̏�����
    const int attackDiveUnlockCount = 2;
    const int attackLaserUnlockCount = 4;
    int nonDiveAttackCount = 0;
    int nonLaserAttackCount = 0;
    #endregion

    #region �J�����֘A
    [Foldout("�J�����֘A")]
    [SerializeField]
    Transform bone1;

    [Foldout("�J�����֘A")]
    [SerializeField]
    string targetGroupName;

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
            doOnceDecision = false;

            switch (nextDecide)
            {
                case DECIDE_TYPE.Waiting:
                    Idle();
                    NextDecision();
                    break;
                case DECIDE_TYPE.Attack_NormalCombo:
                    AttackNormalCombo();
                    break;
                case DECIDE_TYPE.Attack_PunchCombo:
                    AttackPunchCombo();
                    break;
                case DECIDE_TYPE.Attack_Dive:
                    AttackDive();
                    break;
                case DECIDE_TYPE.Attack_Laser:
                    AttackLaser();
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
        bool wasAttacking = nextDecide == DECIDE_TYPE.Attack_NormalCombo || nextDecide == DECIDE_TYPE.Attack_PunchCombo || nextDecide == DECIDE_TYPE.Attack_Dive || nextDecide == DECIDE_TYPE.Attack_Laser;
        bool canAttackNormal = IsNormalAttack();
        bool canAttackSmash = hp <= maxHp * 0.75f && lastAttackPattern != DECIDE_TYPE.Attack_Dive && nonDiveAttackCount >= attackDiveUnlockCount;
        bool canAttackLaser = hp <= maxHp * 0.5f && lastAttackPattern != DECIDE_TYPE.Attack_Laser && nonLaserAttackCount >= attackLaserUnlockCount;

        // ��������ɊY������s���p�^�[���ɏd�ݕt��
        if (canChaseTarget && !IsGround())
        {
            weights[DECIDE_TYPE.Waiting] = 10;
        }
        else if (canAttackNormal || canAttackSmash || canAttackLaser)
        {
            if (!IsBackFall() && nextDecide != DECIDE_TYPE.Tracking && nextDecide != DECIDE_TYPE.BackOff) weights[DECIDE_TYPE.BackOff] = wasAttacking ? 15 : 5;

            if (canAttackNormal) weights[DECIDE_TYPE.Attack_NormalCombo] = wasAttacking ? 5 : 15;
            if (canAttackNormal) weights[DECIDE_TYPE.Attack_PunchCombo] = wasAttacking ? 5 : 15;
            if (canAttackSmash) weights[DECIDE_TYPE.Attack_Dive] = 30;
            if (canAttackLaser) weights[DECIDE_TYPE.Attack_Laser] = 30;
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
    /// �S�Ẵ_���[�W�p�R���C�_�[���A�N�e�B�u�ɂ���
    /// </summary>
    public void DisableAllDamageColliders()
    {
        foreach (var collider in damageColliders)
        {
            collider.SetActive(false);
        }
    }

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
        RemoveTargetGroup();
        DisableAllDamageColliders();
        isAttacking = true;
        Idle();
        yield return new WaitForSeconds(time);
        SelectNewTargetInBossRoom();
        isAttacking = false;
        NextDecision();
        onFinished?.Invoke();
    }

    #endregion

    #region �ʏ�̃R���{�U��

    /// <summary>
    /// �ʏ�̃R���{�Z
    /// </summary>
    void AttackNormalCombo()
    {
        nonDiveAttackCount++;
        nonLaserAttackCount++;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_NormalCombo);
    }

    /// <summary>
    /// [AnimationEvent����Ăяo��] �^�[�Q�b�g�̂������������ 
    /// </summary>
    public override void OnAttackAnimEvent()
    {
        m_rb2d.linearVelocity = Vector2.zero;
        if (!IsNormalAttack())
        {
            var nearTarget = GetNearPlayer();
            if(!nearTarget) target = nearTarget;
        }
        LookAtTarget();
    }

    #endregion

    #region �p���`�̃R���{�U��

    /// <summary>
    /// �p���`�̃R���{�Z
    /// </summary>
    void AttackPunchCombo()
    {
        nonDiveAttackCount++;
        nonLaserAttackCount++;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_PunchCombo);
    }

    /// <summary>
    /// [AnimationEvent����Ăяo��] �����ǂ��O�i����
    /// </summary>
    public override void OnAttackAnim2Event()
    {
        m_rb2d.linearVelocity = Vector2.zero;
        if (!IsNormalAttack())
        {
            var nearTarget = GetNearPlayer();
            if (!nearTarget) target = nearTarget;
        }
        LookAtTarget();

        Vector2 vec = new Vector2(TransformUtils.GetFacingDirection(transform) * moveSpeed * 2, 0f);
        m_rb2d.AddForce(vec, ForceMode2D.Impulse);
    }

    /// <summary>
    /// [AnimationEvent����Ăяo��] ����ɐ����ǂ��O�i����
    /// </summary>
    public override void OnEndAttackAnim2Event()
    {
        m_rb2d.linearVelocity = Vector2.zero;
        if (!IsNormalAttack())
        {
            var nearTarget = GetNearPlayer();
            if (!nearTarget) target = nearTarget;
        }
        LookAtTarget();

        Vector2 vec = new Vector2(TransformUtils.GetFacingDirection(transform) * moveSpeed * 4, 0f);
        m_rb2d.AddForce(vec, ForceMode2D.Impulse);
    }

    #endregion

    #region �����U��

    /// <summary>
    /// �����U��
    /// </summary>
    void AttackDive()
    {
        // �ڕW���W���L�[�v���Ă���
        targetPos = GetGroundPointFrom(target);
        if (targetPos == null)
        {
            NextDecision();
            return;
        }

        nonLaserAttackCount++;
        nonDiveAttackCount = 0;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Dive);
    }

    /// <summary>
    /// [AnimationEvent����Ăяo��] �^�[�Q�b�g�̍��W�Ƀ��[�v���� 
    /// </summary>
    public override void OnAttackAnim3Event()
    {
        m_rb2d.linearVelocity = Vector2.zero;
        if (!target) SelectNewTargetInBossRoom();
        if (target) targetPos = target.transform.position;
        transform.position = (Vector2)targetPos;
        LookAtTarget();
    }

    /// <summary>
    /// [AnimationEvent����Ăяo��] ��ԋ߂��^�[�Q�b�g�̂������������
    /// </summary>
    public override void OnEndAttackAnim3Event()
    {
        var nearTarget = GetNearPlayer();
        if (nearTarget) target = nearTarget;
        if (target) LookAtTarget();
    }

    #endregion

    #region ���[�U�[�U��

    /// <summary>
    /// ���[�U�[�U��
    /// </summary>
    void AttackLaser()
    {
        nonDiveAttackCount++;
        nonLaserAttackCount = 0;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Laser);
    }

    /// <summary>
    /// [AnimationEvent����Ăяo��] �X�e�[�W�̒����Ƀ��[�v����
    /// </summary>
    public override void OnAttackAnim4Event()
    {
        // �X�e�[�W�̒����Ɉړ�
        transform.position = stageCenterPosition;

        AddTargetGroup();
    }

    /// <summary>
    /// [AnimationEvent����Ăяo��] ��ԋ߂��^�[�Q�b�g�̂������������
    /// </summary>
    public override void OnEndAttackAnim4Event()
    {
        var nearTarget = GetNearPlayer();
        if(nearTarget) target = nearTarget;
        if (target) LookAtTarget();
    }

    #endregion

    /// <summary>
    /// [AnimationEvent����Ăяo��] �O�����ɕX�̊�̃p�[�e�B�N���𐶐�����
    /// </summary>
    public override void OnAnimEventOption1()
    {
        Vector2 point = new Vector2(iceRockPsPoints[0].position.x, iceRockPsPointsY[0]);
        var iceRock = Instantiate(iceRockPsPrefabs[1], point, iceRockPsPrefabs[1].transform.rotation);

        float rotationY = transform.localScale.x > 0 ? 90 : -90;
        iceRock.transform.eulerAngles = Vector2.up * rotationY;
    }

    /// <summary>
    /// [AnimationEvent����Ăяo��] �O��ɕX�̊�̃p�[�e�B�N���𐶐�����
    /// </summary>
    public override void OnAnimEventOption2()
    {
        Vector2 point = new Vector2(iceRockPsPoints[0].position.x, iceRockPsPointsY[0]);
        Instantiate(iceRockPsPrefabs[0], point, iceRockPsPrefabs[0].transform.rotation);

        point = new Vector2(iceRockPsPoints[1].position.x, iceRockPsPointsY[1]);
        Instantiate(iceRockPsPrefabs[1], point, iceRockPsPrefabs[1].transform.rotation);
    }

    #endregion

    #region �J�����֘A

    /// <summary>
    /// �ꎞ�I��Cinemachine��TargetGroup��bone1��������
    /// </summary>
    void AddTargetGroup()
    {
        var targetGroup = GameObject.Find(targetGroupName).GetComponent<CinemachineTargetGroup>();
        var newTarget = new CinemachineTargetGroup.Target
        {
            Object = bone1,
            Radius = 1f,
            Weight = 1f
        };
        targetGroup.Targets.Add(newTarget);
    }

    /// <summary>
    /// Chinemachine��TargetGroup����bone1�����������X�g�ɏC������
    /// </summary>
    void RemoveTargetGroup()
    {
        var targetGroup = GameObject.Find(targetGroupName).GetComponent<CinemachineTargetGroup>();
        var newTarget = new CinemachineTargetGroup.Target
        {
            Object = CharacterManager.Instance.PlayerObjSelf.transform,
            Radius = 1f,
            Weight = 1f
        };
        targetGroup.Targets = new List<CinemachineTargetGroup.Target>() { newTarget};
    }

    #endregion

    #region �ړ������֘A

    /// <summary>
    /// �Ǐ]�J�n
    /// </summary>
    void StartTracking()
    {
        m_rb2d.linearVelocity = new Vector2(0, m_rb2d.linearVelocity.y);
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
        bool isNormakAttack = false;

        while (true)
        {
            // �r���Ń^�[�Q�b�g�������� || �^�[�Q�b�g�ƍŒ዗���܂ŋ߂Â����狭���I��
            if (!target || disToTargetX <= attackDist) break;

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
            SelectNewTargetInBossRoom();
        }

        onFinished?.Invoke();
        NextDecision(false);
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
        const float coroutineTime = 1.2f;
        m_rb2d.linearVelocity = new Vector2(0, m_rb2d.linearVelocity.y);
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
        float waitSec = 0.01f;
        float currentTime = 0f;
        const float backOffTime = 0.4f;
        const float breakeTime = 0.1f;

        while (currentTime < backOffTime + breakeTime)
        {
            currentTime += waitSec;
            float speedRate = currentTime <= backOffTime ? -1.5f : -1f;

            BackOff(speedRate);
            yield return new WaitForSeconds(waitSec);
        }

        m_rb2d.linearVelocity = new Vector2(0, m_rb2d.linearVelocity.y);
        yield return new WaitForSeconds(time - backOffTime - breakeTime);

        var nearTarget = GetNearPlayer();
        if (IsNormalAttack(nearTarget))
        {
            target = nearTarget;
        }

        NextDecision(false);
        onFinished?.Invoke();
    }

    /// <summary>
    /// �������Ƃ�
    /// </summary>
    void BackOff(float speedRate)
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
            speedVec = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed * speedRate, m_rb2d.linearVelocity.y);
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

        if (target == null) SelectNewTargetInBossRoom();
        DisableAllDamageColliders();
        DecideBehavior();
    }

    #endregion

    #region �`�F�b�N�����֘A

    /// <summary>
    /// �ʏ�U�����\���ǂ���
    /// </summary>
    /// <returns></returns>
    bool IsNormalAttack(GameObject target = null)
    {
        if(target == null) target = this.target;
        if (target)
        {
            // �^�[�Q�b�g�Ƃ̋������擾����
            disToTarget = Vector3.Distance(target.transform.position, this.transform.position);
            disToTargetX = MathF.Abs(target.transform.position.x - transform.position.x);
        }
        else
        {
            disToTarget = float.MaxValue;
            disToTargetX = float.MaxValue;
        }

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
