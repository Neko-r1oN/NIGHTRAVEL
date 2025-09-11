////**************************************************
////  [�{�X] �{�b�N�X�K�C�X�g�̃N���X
////  Author:r-enomoto
////**************************************************
//using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
//using Pixeye.Unity;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using Unity.VisualScripting;
//using UnityEngine;
//using static Shared.Interfaces.StreamingHubs.EnumManager;

//public class Boxgeist : EnemyBase
//{
//    /// <summary>
//    /// �A�j���[�V����ID
//    /// </summary>
//    public enum ANIM_ID
//    {
//        Spawn = 0,
//        Idle,
//        Attack,
//        Dead,
//    }

//    /// <summary>
//    /// �s���p�^�[��
//    /// </summary>
//    public enum DECIDE_TYPE
//    {
//        Waiting = 1,
//        Attack_Range,
//        Attack_Shotgun,
//        Attack_Golem,
//        Attack_FallBlock,
//        Tracking,
//        RndMove,
//    }
//    DECIDE_TYPE nextDecide = DECIDE_TYPE.Waiting;

//    /// <summary>
//    /// �Ǘ�����R���[�`���̎��
//    /// </summary>
//    public enum COROUTINE
//    {
//        NextDecision,
//        PatorolCoroutine,
//        AttackCooldown,
//    }

//    #region �U���֘A
//    [Foldout("�U���֘A")]
//    [SerializeField]
//    Transform aimTransform; // �e��AIM����

//    [Foldout("�U���֘A")]
//    [SerializeField]
//    List<GameObject> nodeBulletPrefabs = new List<GameObject>();
//    int lastNodeBulletIndex;    // �O��ˏo�����e�̃C���f�b�N�X�ԍ�
//    #endregion

//    #region �`�F�b�N�֘A

//    // �ǁE�n�ʃ`�F�b�N
//    [Foldout("�`�F�b�N�֘A")]
//    [SerializeField]
//    Transform wallCheck;
//    [Foldout("�`�F�b�N�֘A")]
//    [SerializeField]
//    Vector2 wallCheckRadius = new Vector2(0, 1.5f);
//    #endregion

//    #region ���I�֘A
//    float decisionTimeMax = 2f;
//    float randomDecision;
//    bool endDecision;
//    #endregion

//    #region �I���W�i��

//    [Foldout("�I���W�i��")]
//    [SerializeField]
//    float patorolRange = 10f;

//    EnemyProjectileChecker projectileChecker;
//    Vector3 targetPos;
//    Vector2? startPatorolPoint = null;
//    #endregion

//    protected override void Start()
//    {
//        base.Start();
//        lastNodeBulletIndex = nodeBulletPrefabs.Count - 1;
//        projectileChecker = aimTransform.GetComponent<EnemyProjectileChecker>();
//        isAttacking = false;
//        doOnceDecision = true;
//        targetPos = Vector3.zero;
//        NextDecision();
//    }

//    /// <summary>
//    /// �s���p�^�[�����s����
//    /// </summary>
//    protected override void DecideBehavior()
//    {
//        if (doOnceDecision)
//        {
//            doOnceDecision = false;

//            switch (nextDecide)
//            {
//                case DECIDE_TYPE.Waiting:
//                    chaseAI.Stop();
//                    Idle();
//                    NextDecision();
//                    break;
//                case DECIDE_TYPE.Attack:
//                    chaseAI.Stop();
//                    Attack();
//                    break;
//                case DECIDE_TYPE.Tracking:
//                    Tracking();
//                    NextDecision();
//                    break;
//                case DECIDE_TYPE.Patrol:
//                    Patorol();
//                    break;
//                case DECIDE_TYPE.RndMove:
//                    chaseAI.DoRndMove();
//                    NextDecision(2f);
//                    break;
//            }
//        }
//    }


//    /// <summary>
//    /// �A�C�h������
//    /// </summary>
//    protected override void Idle()
//    {
//        SetAnimId((int)ANIM_ID.Idle);
//        m_rb2d.linearVelocity = new Vector2(0f, m_rb2d.linearVelocity.y);
//    }

//    #region ���I�����֘A

//    /// <summary>
//    /// ���I�������Ă�
//    /// </summary>
//    /// <param name="time"></param>
//    void NextDecision(float? rndMaxTime = null)
//    {
//        if (rndMaxTime == null) rndMaxTime = decisionTimeMax;
//        float time = UnityEngine.Random.Range(0.1f, (float)rndMaxTime);

//        // ���s���Ă��Ȃ���΁A�s���̒��I�̃R���[�`�����J�n
//        string key = COROUTINE.NextDecision.ToString();
//        if (!ContaintsManagedCoroutine(key))
//        {
//            Coroutine coroutine = StartCoroutine(NextDecisionCoroutine(time, () => { RemoveAndStopCoroutineByKey(key); }));
//            managedCoroutines.Add(key, coroutine);
//        }
//    }

//    /// <summary>
//    /// ���̍s���p�^�[�����菈��
//    /// </summary>
//    /// <param name="time"></param>
//    /// <returns></returns>
//    IEnumerator NextDecisionCoroutine(float time, Action onFinished)
//    {
//        yield return new WaitForSeconds(time);

//        #region �e�s���p�^�[���̏d�ݕt��

//        Dictionary<DECIDE_TYPE, int> weights = new Dictionary<DECIDE_TYPE, int>();

//        // �U�����\�ȏꍇ
//        if (canAttack && projectileChecker.CanFireProjectile(target) && !sightChecker.IsObstructed())
//        {
//            int weightRate = nextDecide == DECIDE_TYPE.Attack ? 3 : 1;
//            if (!isAttacking) weights[DECIDE_TYPE.Attack] = 10 / weightRate;
//            weights[DECIDE_TYPE.RndMove] = 5 * weightRate;
//        }
//        else if (canChaseTarget && target)
//        {
//            weights[DECIDE_TYPE.Tracking] = 10;
//        }
//        else if (canPatrol && !isPatrolPaused)
//        {
//            weights[DECIDE_TYPE.Patrol] = 10;
//        }
//        else
//        {
//            weights[DECIDE_TYPE.Waiting] = 10;
//        }

//        // value����ɏ����ŕ��בւ�
//        var sortedWeights = weights.OrderBy(x => x.Value);
//        #endregion

//        // �S�̂̒������g���Ē��I
//        int totalWeight = weights.Values.Sum();
//        randomDecision = UnityEngine.Random.Range(1, totalWeight + 1);

//        // ���I�����l�Ŏ��̍s���p�^�[�������肷��
//        int currentWeight = 0;
//        foreach (var weight in sortedWeights)
//        {
//            currentWeight += weight.Value;
//            if (currentWeight >= randomDecision)
//            {
//                nextDecide = weight.Key;
//                break;
//            }
//        }

//        doOnceDecision = true;
//        onFinished?.Invoke();
//    }

//    #endregion

//    #region �U�������֘A

//    /// <summary>
//    /// �U������
//    /// </summary>
//    public void Attack()
//    {
//        if (target == null)
//        {
//            targetPos = Vector3.zero;
//            NextDecision();
//            return;
//        }

//        targetPos = target.transform.position;
//        doOnceDecision = false;
//        isAttacking = true;
//        m_rb2d.linearVelocity = Vector2.zero;
//        chaseAI.Stop();
//        StopPatorol();
//        SetAnimId((int)ANIM_ID.Attack);
//    }

//    /// <summary>
//    /// [Animation�C�x���g����̌Ăяo��] �e���ˏ���
//    /// </summary>

//    public async override void OnAttackAnimEvent()
//    {
//        if (targetPos != Vector3.zero)
//        {
//            if (!target || target && target.GetComponent<CharacterBase>().HP <= 0) target = sightChecker.GetTargetInSight();
//            if (target) targetPos = target.transform.position;

//            // ���g���G���[�g�̂̏ꍇ�A�t�^�����Ԉُ�̎�ނ��擾����
//            DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();
//            List<DEBUFF_TYPE> debuffs = new List<DEBUFF_TYPE>();
//            if (applyEffect != null) debuffs.Add((DEBUFF_TYPE)applyEffect);
//            var shootVec = (targetPos - aimTransform.position).normalized * 20;

//            // �ˏo����e���擾
//            lastNodeBulletIndex = lastNodeBulletIndex == nodeBulletPrefabs.Count - 1 ? 0 : lastNodeBulletIndex + 1;
//            GameObject bullet = nodeBulletPrefabs[lastNodeBulletIndex];
//            PROJECTILE_TYPE bulletType = bullet.GetComponent<ProjectileBase>().TypeId;

//            if (RoomModel.Instance && RoomModel.Instance.IsMaster)
//            {
//                // �e�̐������N�G�X�g
//                await RoomModel.Instance.ShootBulletAsync(bulletType, debuffs, power, aimTransform.position, shootVec, Quaternion.identity);
//            }
//            else if (!RoomModel.Instance)
//            {
//                var bulletObj = Instantiate(bullet, aimTransform.position, Quaternion.identity);
//                bulletObj.GetComponent<ProjectileBase>().Init(debuffs, power);
//                bulletObj.GetComponent<ProjectileBase>().Shoot(shootVec);
//            }
//        }
//    }

//    /// <summary>
//    /// [Animation�C�x���g����̌Ăяo��] �U���N�[���_�E������
//    /// </summary>
//    public override void OnEndAttackAnimEvent()
//    {
//        // ���s���Ă��Ȃ���΁A�N�[���_�E���̃R���[�`�����J�n
//        string cooldownKey = COROUTINE.AttackCooldown.ToString();
//        if (!ContaintsManagedCoroutine(cooldownKey))
//        {
//            Coroutine coroutine = StartCoroutine(AttackCooldown(attackCoolTime, () => {
//                RemoveAndStopCoroutineByKey(cooldownKey);
//            }));
//            managedCoroutines.Add(cooldownKey, coroutine);
//        }
//    }

//    /// <summary>
//    /// �U�����̃N�[���_�E������
//    /// </summary>
//    /// <returns></returns>
//    IEnumerator AttackCooldown(float time, Action onFinished)
//    {
//        isAttacking = true;
//        Idle();
//        yield return new WaitForSeconds(time);
//        isAttacking = false;
//        NextDecision();
//        onFinished?.Invoke();
//    }

//    #endregion

//    #region �ړ������֘A

//    /// <summary>
//    /// �ǐՂ��鏈��
//    /// </summary>
//    protected override void Tracking()
//    {
//        SetAnimId((int)ANIM_ID.Idle);

//        aimTransform.localEulerAngles = Vector3.back * 90f; // �e�̌�����������
//        StopPatorol();
//        chaseAI.DoChase(target);
//    }

//    /// <summary>
//    /// ���񏈗�
//    /// </summary>
//    protected override void Patorol()
//    {
//        SetAnimId((int)ANIM_ID.Idle);

//        aimTransform.localEulerAngles = Vector3.back * 90f; // �e�̌�����������

//        // ���s���Ă��Ȃ���΁A�p�g���[���̃R���[�`�����J�n
//        string key = COROUTINE.PatorolCoroutine.ToString();
//        if (!ContaintsManagedCoroutine(key))
//        {
//            Coroutine coroutine = StartCoroutine(PatorolCoroutine(() => { RemoveAndStopCoroutineByKey(key); }));
//            managedCoroutines.Add(key, coroutine);
//        }
//    }

//    /// <summary>
//    /// ���񂷂鏈��
//    /// </summary>
//    IEnumerator PatorolCoroutine(Action onFinished)
//    {
//        float pauseTime = 2f;
//        if (startPatorolPoint == null)
//        {
//            startPatorolPoint = transform.position;
//        }

//        if (IsWall()) Flip();

//        if (TransformUtils.GetFacingDirection(transform) > 0)
//        {
//            if (transform.position.x >= startPatorolPoint.Value.x + patorolRange)
//            {
//                isPatrolPaused = true;
//                Idle();
//                yield return new WaitForSeconds(pauseTime);
//                isPatrolPaused = false;
//                Flip();
//            }
//        }
//        else if (TransformUtils.GetFacingDirection(transform) < 0)
//        {
//            if (transform.position.x <= startPatorolPoint.Value.x - patorolRange)
//            {
//                isPatrolPaused = true;
//                Idle();
//                yield return new WaitForSeconds(pauseTime);
//                isPatrolPaused = false;
//                Flip();
//            }
//        }

//        Vector2 speedVec = Vector2.zero;
//        speedVec = new Vector2(TransformUtils.GetFacingDirection(transform) * moveSpeed / 2, m_rb2d.linearVelocity.y);
//        m_rb2d.linearVelocity = speedVec;
//        NextDecision();
//        onFinished?.Invoke();
//    }

//    /// <summary>
//    /// ���񏈗����~����
//    /// </summary>
//    void StopPatorol()
//    {
//        startPatorolPoint = null;
//    }

//    #endregion

//    #region �q�b�g�����֘A

//    /// <summary>
//    /// �_���[�W���󂯂��Ƃ��̏���
//    /// </summary>
//    protected override void OnHit()
//    {
//        base.OnHit();
//        chaseAI.Stop();
//        StopPatorol();
//        SetAnimId((int)ANIM_ID.Hit);
//    }

//    /// <summary>
//    /// ���S����Ƃ��ɌĂ΂�鏈��
//    /// </summary>
//    /// <returns></returns>
//    protected override void OnDead()
//    {
//        SetAnimId((int)ANIM_ID.Dead);
//    }

//    #endregion

//    #region �e�N�X�`���E�A�j���[�V�����֘A

//    /// <summary>
//    /// �X�|�[���A�j���[�V�������I�������Ƃ�
//    /// </summary>
//    public override void OnEndSpawnAnimEvent()
//    {
//        base.OnEndSpawnAnimEvent();
//        chaseAI.Stop();
//        ApplyStun(0.5f, false);
//    }

//    /// <summary>
//    /// �A�j���[�V�����ݒ菈��
//    /// </summary>
//    /// <param name="id"></param>
//    public override void SetAnimId(int id)
//    {
//        if (animator == null) return;
//        animator.SetInteger("animation_id", id);

//        switch (id)
//        {
//            case (int)ANIM_ID.Hit:
//                animator.Play("Hit_NodeCode", 0, 0);
//                break;
//            default:
//                break;
//        }
//    }
//    #endregion

//    #region ���A���^�C�������֘A

//    /// <summary>
//    /// �}�X�^�N���C�A���g�؂�ւ����ɏ�Ԃ����Z�b�g����
//    /// </summary>
//    public override void ResetAllStates()
//    {
//        base.ResetAllStates();

//        if (target == null)
//        {
//            target = sightChecker.GetTargetInSight();
//        }

//        DecideBehavior();
//    }

//    #endregion

//    #region �`�F�b�N�����֘A

//    /// <summary>
//    /// �ǂ����邩�ǂ���
//    /// </summary>
//    /// <returns></returns>
//    bool IsWall()
//    {
//        return Physics2D.OverlapBox(wallCheck.position, wallCheckRadius, 0f, terrainLayerMask);
//    }

//    /// <summary>
//    /// ���o�͈͂̕`�揈��
//    /// </summary>
//    protected override void DrawDetectionGizmos()
//    {
//        // �U���J�n����
//        Gizmos.color = Color.blue;
//        Gizmos.DrawWireSphere(transform.position, attackDist);

//        // �ǂ̔���
//        if (wallCheck)
//        {
//            Gizmos.color = Color.green;
//            Gizmos.DrawWireCube(wallCheck.transform.position, wallCheckRadius);
//        }
//    }

//    #endregion

//}
