//**************************************************
//  [�{�X] �{�b�N�X�K�C�X�g�̃N���X
//  Author:r-enomoto
//**************************************************
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Pixeye.Unity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Boxgeist : EnemyBase
{
    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        Attack_Range,
        Attack_Shotgun,
        Attack_Golem,
        Attack_FallBlock,
        Dead,
    }

    /// <summary>
    /// �s���p�^�[��
    /// </summary>
    public enum DECIDE_TYPE
    {
        Waiting = 1,
        Attack_Range,
        Attack_Shotgun,
        Attack_Golem,
        Attack_FallBlock,
    }
    DECIDE_TYPE nextDecide = DECIDE_TYPE.Waiting;

    /// <summary>
    /// �Ǘ�����R���[�`���̎��
    /// </summary>
    public enum COROUTINE
    {
        NextDecision,
        AttackRange,
        AttackShotgun,
        AttackGolem,
        AttackFallBlock,
        AttackCooldown,
    }

    #region �U���֘A
    [Foldout("�U���֘A")]
    [SerializeField]
    GameObject boxBulletPrefab;

    [Foldout("�U���֘A")]
    [SerializeField]
    List<Transform> rangedAttackSpawnPoints = new List<Transform>();
    int currentRangeAttackPoint = 0;

    [Foldout("�U���֘A")]
    [SerializeField]
    List<Transform> shotgunAttackSpawnPoints = new List<Transform>();
    #endregion

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

    #region �I�[�f�B�I�֘A

    [SerializeField]
    [Foldout("�I�[�f�B�I")]
    AudioSource audioCharge;

    [SerializeField]
    [Foldout("�I�[�f�B�I")]
    AudioSource audioGolem;

    [SerializeField]
    [Foldout("�I�[�f�B�I")]
    AudioSource audioFallBlock;
    #endregion

    #region �I���W�i��

    [SerializeField]
    GameObject finishBoxParticleObj;

    Vector3 targetPos;
    float defaultGravityScale;

    [SerializeField]
    float maxMoveTime = 2f;
    [SerializeField]
    float minMoveTime = 0.5f;
    #endregion

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
        doOnceDecision = true;
        targetPos = Vector3.zero;
        defaultGravityScale = m_rb2d.gravityScale;
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
                case DECIDE_TYPE.Attack_Range:
                    StartAttackRange();
                    break;
                case DECIDE_TYPE.Attack_Shotgun:
                    StartAttackShotgun();
                    break;
                case DECIDE_TYPE.Attack_Golem:
                    StartAttackGolem();
                    break;
                case DECIDE_TYPE.Attack_FallBlock:
                    StartAttackFallBlock();
                    break;
            }
        }
    }

    /// <summary>
    /// �A�C�h������
    /// </summary>
    protected override void Idle()
    {
        m_rb2d.bodyType = RigidbodyType2D.Dynamic;
        SetAnimId((int)ANIM_ID.Idle);
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
        RemoveAndStopCoroutineByKey(key);
        Coroutine coroutine = StartCoroutine(NextDecisionCoroutine(time, () => { RemoveAndStopCoroutineByKey(key); }));
        managedCoroutines.Add(key, coroutine);
    }

    /// <summary>
    /// ���̍s���p�^�[�����菈��
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator NextDecisionCoroutine(float time, Action onFinished)
    {
        yield return new WaitForSeconds(time);

        SelectNewTargetInBossRoom();
        CalculateDistanceToTarget();

        #region �e�s���p�^�[���̏d�ݕt��

        Dictionary<DECIDE_TYPE, int> weights = new Dictionary<DECIDE_TYPE, int>();

        // ���I�̃��[���F�A�����ē����s�������Ȃ��悤�ɂ���
        // -----------------------------------------------------------
        // Range    �F�^�[�Q�b�g�Ƃ͔��Ε����Ɉړ����Ă���U���J�n
        // Shotgun  �F�^�[�Q�b�g�ɋ߂Â��Ă���U���J�n
        // Golem    �F�^�[�Q�b�g�Ə��������𗣂��čU���J�n
        // FallBlock�F�^�[�Q�b�g�ɋ߂Â��Ă���U���J�n

        if (canAttack && target)
        {
            weights[DECIDE_TYPE.Attack_Range] = nextDecide != DECIDE_TYPE.Attack_Range ? 30 : 0;
            weights[DECIDE_TYPE.Attack_Shotgun] = nextDecide != DECIDE_TYPE.Attack_Shotgun ? 30 : 0;
            weights[DECIDE_TYPE.Attack_Golem] = nextDecide != DECIDE_TYPE.Attack_Golem ? 20 : 0;
            weights[DECIDE_TYPE.Attack_FallBlock] = nextDecide != DECIDE_TYPE.Attack_FallBlock ? 20 : 0;
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

    /// <summary>
    /// �U������
    /// </summary>
    void PrepareAttack()
    {
        if (target == null)
        {
            targetPos = Vector3.zero;
            NextDecision();
            return;
        }

        m_rb2d.bodyType = RigidbodyType2D.Dynamic;
        targetPos = target.transform.position;
        doOnceDecision = false;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
    }

    #region AttackRange

    /// <summary>
    /// AttackRange�J�n
    /// </summary>
    void StartAttackRange()
    {
        PrepareAttack();
        currentRangeAttackPoint = 0;

        // ���s���Ă��Ȃ���΁A�N�[���_�E���̃R���[�`�����J�n
        string cooldownKey = COROUTINE.AttackRange.ToString();
        RemoveAndStopCoroutineByKey(cooldownKey);
        Coroutine coroutine = StartCoroutine(AttackRangeCoroutine(() =>
        {
            RemoveAndStopCoroutineByKey(cooldownKey);
        }));
        managedCoroutines.Add(cooldownKey, coroutine);
    }

    /// <summary>
    /// [AttackRange] �^�[�Q�b�g�Ƌ������Ƃ��Ă���U�����J�n����
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackRangeCoroutine(Action onFinished)
    {
        const float waitSec = 0.1f;
        float currentSec = 0;
        Vector2 targetPos = target.transform.position;
        while (Vector2.Distance(targetPos, transform.position) < attackDist * 2 && currentSec <= maxMoveTime)
        {
            BackOff(targetPos);
            yield return new WaitForSeconds(waitSec);
            currentSec += waitSec;
        }

        // �U���J�n
        isInvincible = true;
        m_rb2d.linearVelocity = Vector2.zero;
        m_rb2d.bodyType = RigidbodyType2D.Static;
        SetAnimId((int)ANIM_ID.Attack_Range);
        onFinished?.Invoke();
    }

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] AttackRange�̒e���ˏ���
    /// </summary>
    public async override void OnAttackAnimEvent()
    {
        if (targetPos != Vector3.zero)
        {
            if (!target || target && target.GetComponent<CharacterBase>().HP <= 0) target = sightChecker.GetTargetInSight();
            if (target) targetPos = target.transform.position;

            // ���g���G���[�g�̂̏ꍇ�A�t�^�����Ԉُ�̎�ނ��擾����
            DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();
            List<DEBUFF_TYPE> debuffs = new List<DEBUFF_TYPE>();
            if (applyEffect != null) debuffs.Add((DEBUFF_TYPE)applyEffect);

            // �����ʒu�ƈړ��x�N�g�����擾
            Vector3 spawnPoint = rangedAttackSpawnPoints[currentRangeAttackPoint].position;
            var shootVec = (targetPos - spawnPoint).normalized * 30;
            currentRangeAttackPoint = currentRangeAttackPoint >= rangedAttackSpawnPoints.Count-1 ? 0 : currentRangeAttackPoint+1;

            ShootBulletData shootBulletData = new ShootBulletData()
            {
                Type = PROJECTILE_TYPE.BoxBullet_Big,
                Debuffs = debuffs,
                Power = power,
                SpawnPos = spawnPoint,
                ShootVec = shootVec,
                Rotation = Quaternion.identity,
            };

            if (RoomModel.Instance && RoomModel.Instance.IsMaster)
            {
                // �e�̐������N�G�X�g
                await RoomModel.Instance.ShootBulletAsync(shootBulletData);
            }
            else if (!RoomModel.Instance)
            {
                var bulletObj = Instantiate(boxBulletPrefab, spawnPoint, Quaternion.identity);
                bulletObj.GetComponent<ProjectileBase>().Init(debuffs, power);
                bulletObj.GetComponent<ProjectileBase>().Shoot(shootVec);
            }
        }
    }

    #endregion

    #region AttackShotgun

    /// <summary>
    /// [AttackShotgun] AttackShotgun�J�n
    /// </summary>
    void StartAttackShotgun()
    {
        PrepareAttack();

        // ���s���Ă��Ȃ���΁A�N�[���_�E���̃R���[�`�����J�n
        string cooldownKey = COROUTINE.AttackShotgun.ToString();
        RemoveAndStopCoroutineByKey(cooldownKey);
        Coroutine coroutine = StartCoroutine(AttackShotgunCoroutine(() =>
        {
            RemoveAndStopCoroutineByKey(cooldownKey);
        }));
        managedCoroutines.Add(cooldownKey, coroutine);
    }

    /// <summary>
    /// [AttackShotgun] �^�[�Q�b�g�ɋ߂Â��Ă���U���J�n
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackShotgunCoroutine(Action onFinished)
    {
        const float waitSec = 0.1f;
        float currentSec = 0;
        while (disToTarget > attackDist && currentSec <= maxMoveTime)
        {
            if (!target)
            {
                onFinished?.Invoke();
                CancellAttack();
                yield break;
            }

            CloseIn();
            yield return new WaitForSeconds(waitSec);
            currentSec += waitSec;
        }

        isInvincible = true;
        SetAnimId((int)ANIM_ID.Attack_Shotgun);
        onFinished?.Invoke();
    }

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] AttackShotgun�̒e���ˏ���
    /// </summary>
    public async override void OnAttackAnim2Event()
    {
        if (targetPos != Vector3.zero)
        {
            if (!target || target && target.GetComponent<CharacterBase>().HP <= 0) target = sightChecker.GetTargetInSight();
            if (target) targetPos = target.transform.position;

            // ���g���G���[�g�̂̏ꍇ�A�t�^�����Ԉُ�̎�ނ��擾����
            DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();
            List<DEBUFF_TYPE> debuffs = new List<DEBUFF_TYPE>();
            if (applyEffect != null) debuffs.Add((DEBUFF_TYPE)applyEffect);

            List<ShootBulletData> shootBulletDatas = new List<ShootBulletData>();
            int currentIndex = 0;
            foreach(var point in shotgunAttackSpawnPoints)
            {
                // �p�x�����W�A���ɕϊ������ړ��x�N�g���Ɛ����ʒu���擾
                currentIndex++;
                float angle = 360 - (360 / shotgunAttackSpawnPoints.Count * currentIndex);
                float rad = angle * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
                var shootVec = direction.normalized * 30;
                currentRangeAttackPoint = currentRangeAttackPoint >= rangedAttackSpawnPoints.Count - 1 ? 0 : currentRangeAttackPoint + 1;

                ShootBulletData data = new ShootBulletData()
                {
                    Type = PROJECTILE_TYPE.BoxBullet_Big,
                    Debuffs = debuffs,
                    Power = power,
                    SpawnPos = point.position,
                    ShootVec = shootVec,
                    Rotation = Quaternion.identity,
                };

                shootBulletDatas.Add(data);
            }

            if (RoomModel.Instance && RoomModel.Instance.IsMaster)
            {
                // �e�̐������N�G�X�g
                await RoomModel.Instance.ShootBulletAsync(shootBulletDatas.ToArray());
            }
            else if (!RoomModel.Instance)
            {
                foreach(var data in shootBulletDatas)
                {
                    var bulletObj = Instantiate(boxBulletPrefab, data.SpawnPos, data.Rotation);
                    bulletObj.GetComponent<ProjectileBase>().Init(data.Debuffs, data.Power);
                    bulletObj.GetComponent<ProjectileBase>().Shoot(data.ShootVec);
                }
            }
        }
    }

    #endregion

    #region AttackGolem

    /// <summary>
    /// AttackGolem�J�n
    /// </summary>
    void StartAttackGolem()
    {
        PrepareAttack();

        // ���s���Ă��Ȃ���΁A�N�[���_�E���̃R���[�`�����J�n
        string cooldownKey = COROUTINE.AttackGolem.ToString();
        RemoveAndStopCoroutineByKey(cooldownKey);
        Coroutine coroutine = StartCoroutine(AttackGolemCoroutine(() =>
        {
            RemoveAndStopCoroutineByKey(cooldownKey);
        }));
        managedCoroutines.Add(cooldownKey, coroutine);
    }

    /// <summary>
    /// [AttackGolem] �^�[�Q�b�g�ɋ߂Â��Ă���U���J�n
    /// </summary>
    /// <param name="onFinished"></param>
    /// <returns></returns>
    IEnumerator AttackGolemCoroutine(Action onFinished)
    {
        const float waitSec = 0.1f;
        float currentSec = 0;
        while (disToTargetX > attackDist && currentSec <= maxMoveTime)
        {
            if (!target)
            {
                onFinished?.Invoke();
                CancellAttack();
                yield break;
            }

            CloseIn();
            yield return new WaitForSeconds(waitSec);
            currentSec += waitSec;
        }

        isInvincible = true;
        SetAnimId((int)ANIM_ID.Attack_Golem);
        audioCharge.Play();

        yield return new WaitForSeconds(0.45f);     // �S�[�����Ɍ`�ԕω����������鎞��

        // �^�[�Q�b�g�̂�������Ƀe�N�X�`���𔽓]
        LookAtTarget();

        onFinished?.Invoke();
    }

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] ���݌����Ă�������Ɍ������ă_�b�V��
    /// </summary>
    public override void OnAttackAnim3Event()
    {
        const float forcePower = 30;
        m_rb2d.AddForce(new Vector2(TransformUtils.GetFacingDirection(transform) * forcePower, 0), ForceMode2D.Impulse);
        audioGolem.Play();
    }

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] �_�b�V�����~����
    /// </summary>
    public override void OnEndAttackAnim3Event()
    {
        m_rb2d.linearVelocity = Vector2.zero;
    }

    #endregion

    #region AttackFallBlock

    /// <summary>
    /// AttackFallBlock�J�n
    /// </summary>
    void StartAttackFallBlock()
    {
        PrepareAttack();

        // ���s���Ă��Ȃ���΁A�N�[���_�E���̃R���[�`�����J�n
        string cooldownKey = COROUTINE.AttackFallBlock.ToString();
        RemoveAndStopCoroutineByKey(cooldownKey);
        Coroutine coroutine = StartCoroutine(AttackFakkBlockCoroutine(() =>
        {
            RemoveAndStopCoroutineByKey(cooldownKey);
        }));
        managedCoroutines.Add(cooldownKey, coroutine);
    }

    /// <summary>
    /// [AttackFallBlock] �^�[�Q�b�g�ɋ߂Â��Ă���U���J�n
    /// </summary>
    /// <param name="onFinished"></param>
    /// <returns></returns>
    IEnumerator AttackFakkBlockCoroutine(Action onFinished)
    {
        const float targetDist = 0.5f;
        const float waitSec = 0.1f;
        float currentSec = 0;
        while (disToTargetX > targetDist && currentSec <= maxMoveTime)
        {
            if (!target)
            {
                onFinished?.Invoke();
                CancellAttack();
                yield break;
            }

            CloseIn();
            yield return null;
            currentSec += waitSec;
        }

        // �U���J�n
        isInvincible = true;
        m_rb2d.linearVelocity = Vector2.zero;
        m_rb2d.bodyType = RigidbodyType2D.Static;
        SetAnimId((int)ANIM_ID.Attack_FallBlock);
        audioCharge.Play();

        // �u���b�N�̌����𐳂�������
        Vector2 direction = transform.localScale;
        var isRightDir = TransformUtils.GetFacingDirection(transform) > 1;
        transform.localScale = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));

        onFinished?.Invoke();
    }

    /// <summary>
    /// [AnimationEvent����Ăяo��] ���n���Đ�
    /// </summary>
    public override void OnEndAttackAnim4Event()
    {
        audioFallBlock.Play();
    }

    #endregion

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] �U���N�[���_�E������
    /// </summary>
    public override void OnEndAttackAnimEvent()
    {
        // ���s���Ă��Ȃ���΁A�N�[���_�E���̃R���[�`�����J�n
        string cooldownKey = COROUTINE.AttackCooldown.ToString();
        RemoveAndStopCoroutineByKey(cooldownKey);
        Coroutine coroutine = StartCoroutine(AttackCooldown(attackCoolTime, () =>
        {
            RemoveAndStopCoroutineByKey(cooldownKey);
        }));
        managedCoroutines.Add(cooldownKey, coroutine);
    }

    /// <summary>
    /// �U�����̃N�[���_�E������
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time, Action onFinished)
    {
        // �^�[�Q�b�g�̂�������Ƀe�N�X�`���𔽓]
        if (canChaseTarget)
        {
            LookAtTarget();
        }

        isInvincible = false;
        m_rb2d.bodyType = RigidbodyType2D.Dynamic;
        isAttacking = true;
        Idle();
        yield return new WaitForSeconds(time);
        isAttacking = false;
        NextDecision();
        onFinished?.Invoke();
    }

    /// <summary>
    /// �U���L�����Z������
    /// </summary>
    void CancellAttack()
    {
        if (canChaseTarget)
        {
            LookAtTarget();
        }

        Idle();
        isInvincible = false;
        isAttacking = false;
        m_rb2d.bodyType = RigidbodyType2D.Dynamic;
        StopAllManagedCoroutines();
        NextDecision();
    }

    #endregion

    #region �ړ������֘A

    /// <summary>
    /// �^��ɔ��
    /// </summary>
    void JumpUp()
    {
        m_rb2d.linearVelocity = Vector2.zero;
        m_rb2d.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
    }

    /// <summary>
    /// �^�[�Q�b�g�ɐڋ߂���
    /// </summary>
    void CloseIn()
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
    /// �^�[�Q�b�g�Ƃ̋������Ƃ�
    /// </summary>
    void BackOff(Vector2? optionPos = null)
    {
        Vector2 targetPos = optionPos == null ? target.transform.position : (Vector2)optionPos;
        SetAnimId((int)ANIM_ID.Idle);

        Vector2 speedVec = Vector2.zero;
        if (IsBackFall())
        {
            speedVec = new Vector2(0f, m_rb2d.linearVelocity.y);
        }
        else
        {
            float distToPlayer = targetPos.x - this.transform.position.x;
            speedVec = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed * -1, m_rb2d.linearVelocity.y);
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
        SetAnimId((int)ANIM_ID.Dead);
        PlayHitBlendShader(false, 0.5f);
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
        m_rb2d.bodyType = RigidbodyType2D.Dynamic;

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
