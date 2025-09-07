//**************************************************
//  [�G] �f���{�b�g�𐧌䂷��N���X
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;
public class Delibot : EnemyBase
{
    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        Attack,
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
    GameObject boxBulletPrefab;
    #endregion

    #region �`�F�b�N����
    // �ǃ`�F�b�N
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Transform wallCheck;
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Vector2 wallCheckRadius = new Vector2(0, 1.5f);
    #endregion

    #region ���I�֘A
    float decisionTimeMax = 2f;
    float randomDecision;
    bool endDecision;
    #endregion

    #region �I���W�i��
    Vector3 targetPos;
    #endregion

    protected override void Start()
    {
        base.Start();
        projectileChecker = aimTransform.GetComponent<EnemyProjectileChecker>();
        isAttacking = false;
        doOnceDecision = true;
        targetPos = Vector3.zero;
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
                case DECIDE_TYPE.Tracking:
                    Tracking();
                    NextDecision();
                    break;
                case DECIDE_TYPE.RndMove:
                    chaseAI.DoRndMove();
                    NextDecision(2f);
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
    void NextDecision(float? rndMaxTime = null)
    {
        if (rndMaxTime == null) rndMaxTime = decisionTimeMax;
        float time = UnityEngine.Random.Range(0.1f, (float)rndMaxTime);

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

        // �U�����\�ȏꍇ
        if (canAttack && projectileChecker.CanFireProjectile(target) && !sightChecker.IsObstructed())
        {
            int weightRate = nextDecide == DECIDE_TYPE.Attack ? 3 : 1;
            weights[DECIDE_TYPE.Attack] = 10 / weightRate;
            weights[DECIDE_TYPE.RndMove] = 5 * weightRate;
        }
        else if (target)
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
                animator.Play("Haitatu_Damage_Animation");
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
        if (target == null)
        {
            targetPos = Vector3.zero;
            NextDecision();
            return;
        }

        targetPos = target.transform.position;
        doOnceDecision = false;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        chaseAI.Stop();
        SetAnimId((int)ANIM_ID.Attack);
    }

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] �_���{�[���e�𔭎�
    /// </summary>
    public override async void OnAttackAnimEvent()
    {
        if(targetPos != Vector3.zero)
        {
            // ���g���G���[�g�̂̏ꍇ�A�t�^�����Ԉُ�̎�ނ��擾����
            DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();
            List<DEBUFF_TYPE> debuffs = new List<DEBUFF_TYPE>();
            if (applyEffect != null) debuffs.Add((DEBUFF_TYPE)applyEffect);
            var shootVec = (targetPos - aimTransform.position).normalized * 20;

            if (RoomModel.Instance && RoomModel.Instance.IsMaster)
            {
                // �e�̐������N�G�X�g
                await RoomModel.Instance.ShootBulletAsync(PROJECTILE_TYPE.BoxBullet, debuffs, power, aimTransform.position, shootVec);
            }
            else
            {
                var bulletObj = Instantiate(boxBulletPrefab, aimTransform.position, Quaternion.identity);
                List<DEBUFF_TYPE> dEBUFFs = new List<DEBUFF_TYPE>();
                bulletObj.GetComponent<ProjectileBase>().Init(debuffs, power);
                bulletObj.GetComponent<ProjectileBase>().Shoot(shootVec);
            }
        }

        // ���s���Ă��Ȃ���΁A�N�[���_�E���̃R���[�`�����J�n
        string key = COROUTINE.AttackCooldown.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(AttackCooldown(attackCoolTime, () => { RemoveAndStopCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// �U�����̃N�[���_�E������
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time, Action onFinished)
    {
        isAttacking = true;
        yield return new WaitForSeconds(time);
        Idle();
        isAttacking = false;
        doOnceDecision = true;
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
        chaseAI.DoChase(target);
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
        if (hp > 0) NextDecision();
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

    #region ���A���^�C�������֘A

    /// <summary>
    /// �}�X�^�N���C�A���g�؂�ւ����ɏ�Ԃ����Z�b�g����
    /// </summary>
    public override void ResetAllStates()
    {
        base.ResetAllStates();

        ANIM_ID id = (ANIM_ID)GetAnimId();
        nextDecide = id switch
        {
            ANIM_ID.Attack => DECIDE_TYPE.Attack,
            _ => DECIDE_TYPE.Waiting,
        };

        DecideBehavior();
    }

    /// <summary>
    /// �I���W�i����EnemyData�擾����
    /// </summary>
    /// <returns></returns>
    public override EnemyData GetEnemyData()
    {
        EnemyData enemyData = new EnemyData();
        enemyData.Quatarnions.Add(aimTransform.localRotation);
        return SetEnemyData(enemyData);
    }

    /// <summary>
    /// �h���[���̓��������X�V����
    /// </summary>
    /// <param name="enemyData"></param>
    public override void UpdateEnemy(EnemyData enemyData)
    {
        base.UpdateEnemy(enemyData);
        aimTransform.localRotation = enemyData.Quatarnions[0];
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
