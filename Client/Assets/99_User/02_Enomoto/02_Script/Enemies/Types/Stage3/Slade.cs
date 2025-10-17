//**************************************************
//  [�G] �X���C�h�̃N���X
//  Author:r-enomoto
//**************************************************
using KanKikuchi.AudioManager;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Slade : EnemyBase
{
    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        Attack_Rounding_Up,
        Attack_Rounding_Down,
        Attack_Charge,
        Attack_Combo,
        Teleport,
        Hit,
        Dead,
    }

    /// <summary>
    /// �s���p�^�[��
    /// </summary>
    public enum DECIDE_TYPE
    {
        Waiting = 1,
        Attack_Upward_Slash,
        Attack_Charge,
        Attack_Combo_Slash,
        Teleport,
    }
    DECIDE_TYPE nextDecide = DECIDE_TYPE.Waiting;

    /// <summary>
    /// �Ǘ�����R���[�`���̎��
    /// </summary>
    public enum COROUTINE
    {
        NextDecision,
        AttackChargeCoroutine,
        AttackCooldown,
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
    #endregion

    #region ���I�֘A
    float decisionTimeMax = 2f;
    float randomDecision;
    bool endDecision;
    #endregion

    #region �I���W�i��
    [SerializeField]
    CapsuleCollider2D terrainCollider;
    List<GameObject> hitPlayers = new List<GameObject>();
    #endregion

    #region �I�[�f�B�I�֘A

    [SerializeField]
    [Foldout("�I�[�f�B�I")]
    AudioSource audioAttack;

    [SerializeField]
    [Foldout("�I�[�f�B�I")]
    AudioSource audioChargeAttack;

    [SerializeField]
    [Foldout("�I�[�f�B�I")]
    AudioSource audioTeleport;

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
                case DECIDE_TYPE.Attack_Upward_Slash:
                    AttackUpwardSlash();
                    break;
                case DECIDE_TYPE.Attack_Charge:
                    AttackCharge();
                    break;
                case DECIDE_TYPE.Attack_Combo_Slash:
                    AttackComboSlash();
                    break;
                case DECIDE_TYPE.Teleport:
                    Tracking();
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

        const float attackChargeDist = 10f;

        // �e�U���̏���
        // -----------------
        // �؂�グ, �؂艺���E�E�E�F�^�[�Q�b�g���U���͈͓��ɂ���ꍇ
        // �ːi�E�E�E�E�E�E�E�E�E�E�F�؂�グ, �؂艺���͈̔͊O���A�^�[�Q�b�g���U���͈͓��ɂ���ꍇ
        // �R���{�Z�E�E�E�E�E�E�E�E�F����HP�������ȉ����A�^�[�Q�b�g���U���͈͓�
        bool canAttackSlash = canAttack && !sightChecker.IsObstructed() && disToTarget <= attackDist;
        bool canAttackCharge = canAttack && !sightChecker.IsObstructed() && disToTarget > attackDist && disToTarget <= attackChargeDist;
        bool canAttackCombo = hp <= MaxHP / 2 && canAttack && !sightChecker.IsObstructed() && disToTarget <= attackDist;
        Dictionary<DECIDE_TYPE, int> weights = new Dictionary<DECIDE_TYPE, int>();

        if (canAttackSlash)
        {
            weights[DECIDE_TYPE.Attack_Upward_Slash] = nextDecide == DECIDE_TYPE.Attack_Upward_Slash ? 5 : 20;
        }
        if (canAttackCharge)
        {
            weights[DECIDE_TYPE.Attack_Charge] = nextDecide == DECIDE_TYPE.Attack_Charge ? 5 : 20;
        }
        if (canAttackCombo)
        {
            weights[DECIDE_TYPE.Attack_Combo_Slash] = nextDecide == DECIDE_TYPE.Attack_Combo_Slash ? 5 : 30;
        }
        if (canChaseTarget && target && disToTarget > attackDist)
        {
            weights[DECIDE_TYPE.Teleport] = nextDecide == DECIDE_TYPE.Attack_Charge ? 30 : 10;
        }
        if(!target)
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

    #region �U���F�؂�グ�E�؂艺��

    /// <summary>
    /// �U���F�؂�グ
    /// </summary>
    void AttackUpwardSlash()
    {
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Rounding_Up);

        SEManager.Instance.Play(
               audioPath: SEPath.SWORD_ATK, //�Đ��������I�[�f�B�I�̃p�X
               volumeRate: 1.0f,                //���ʂ̔{��
               delay: 0.0f,                //�Đ������܂ł̒x������
               pitch: 1.0f,                //�s�b�`
               isLoop: false,             //���[�v�Đ����邩
               callback: null              //�Đ��I����̏���
               );
    }

    /// <summary>
    /// �U���F�؂艺��
    /// </summary>
    void AttackDownwardSlash()
    {
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Rounding_Down);

        SEManager.Instance.Play(
               audioPath: SEPath.SWORD_ATK, //�Đ��������I�[�f�B�I�̃p�X
               volumeRate: 1.0f,                //���ʂ̔{��
               delay: 0.0f,                //�Đ������܂ł̒x������
               pitch: 1.0f,                //�s�b�`
               isLoop: false,             //���[�v�Đ����邩
               callback: null              //�Đ��I����̏���
               );
    }

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] �؂�グ�E�؂艺���̍U�����菈��
    /// </summary>
    public override void OnAttackAnimEvent()
    {
        audioAttack.Play();

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

    #region �U���F�ːi

    /// <summary>
    /// �U���F�ːi
    /// </summary>
    void AttackCharge()
    {
        terrainCollider.enabled = true;
        hitPlayers.Clear();
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Charge);

        SEManager.Instance.Play(
               audioPath: SEPath.DANBO_ATK, //�Đ��������I�[�f�B�I�̃p�X
               volumeRate: 1.0f,                //���ʂ̔{��
               delay: 0.0f,                //�Đ������܂ł̒x������
               pitch: 1.0f,                //�s�b�`
               isLoop: false,             //���[�v�Đ����邩
               callback: null              //�Đ��I����̏���
               );
    }

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] �ːi�̍U������J�n
    /// </summary>
    public override void OnAttackAnim2Event()
    {
        terrainCollider.enabled = false;

        // ���s���Ă��Ȃ���΁A�N�[���_�E���̃R���[�`�����J�n
        string cooldownKey = COROUTINE.AttackChargeCoroutine.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(AttackChargeCoroutine());
            managedCoroutines.Add(cooldownKey, coroutine);
        }
    }

    /// <summary>
    /// �ːi�̍U������
    /// </summary>
    /// <param name="onFinished"></param>
    /// <returns></returns>
    IEnumerator AttackChargeCoroutine()
    {
        // ���g���G���[�g�̂̏ꍇ�A�t�^�����Ԉُ�̎�ނ��擾����
        DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();

        audioChargeAttack.Play();

        while (true)
        {
            // �ړ�����
            Vector2 speedVec = Vector2.zero;
            if (IsWall())
            {
                speedVec = new Vector2(0f, m_rb2d.linearVelocity.y);
            }
            else
            {
                speedVec = new Vector2(TransformUtils.GetFacingDirection(transform) * moveSpeed, m_rb2d.linearVelocity.y);
            }
            m_rb2d.linearVelocity = speedVec;

            Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
            for (int i = 0; i < collidersEnemies.Length; i++)
            {
                if (collidersEnemies[i].gameObject.tag == "Player" && !hitPlayers.Contains(collidersEnemies[i].gameObject))
                {
                    hitPlayers.Add(collidersEnemies[i].gameObject);
                    collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position, KB_POW.Big, applyEffect);
                }
            }
            yield return null;
        }
    }

    #endregion

    #region �U���F�R���{�Z

    /// <summary>
    /// �U���F�R���{�Z
    /// </summary>
    void AttackComboSlash()
    {
        hitPlayers.Clear();
        canCancelAttackOnHit = false;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Combo);

        SEManager.Instance.Play(
               audioPath: SEPath.SWORD_HIT, //�Đ��������I�[�f�B�I�̃p�X
               volumeRate: 1.0f,                //���ʂ̔{��
               delay: 0.0f,                //�Đ������܂ł̒x������
               pitch: 1.0f,                //�s�b�`
               isLoop: false,             //���[�v�Đ����邩
               callback: null              //�Đ��I����̏���
               );
    }

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] �傫���؂�グ���Ƃ��̍U������
    /// �U�����������Ȃ������ꍇ�͍U���𒆒f����
    /// </summary>
    public override void OnAttackAnim3Event()
    {
        const float addForcePower = 40f;
        bool isHitTarget = false;

        audioAttack.Play();

        // ���g���G���[�g�̂̏ꍇ�A�t�^�����Ԉُ�̎�ނ��擾����
        DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();

        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Player")
            {
                PlayerBase player = collidersEnemies[i].gameObject.GetComponent<PlayerBase>();
                Rigidbody2D rigidbody2D = player.GetComponent<Rigidbody2D>();

                if (player.gameObject == target) isHitTarget = true;
                hitPlayers.Add(player.gameObject);

                player.ApplyDamage((int)(power * 1.5f), transform.position, null, applyEffect);
                player.StartCoroutine(player.Stun(10f));
                rigidbody2D.gravityScale = 0;
                rigidbody2D.linearVelocity = Vector2.zero;
                rigidbody2D.AddForce(Vector2.up * addForcePower, ForceMode2D.Impulse);
            }
        }

        if(!isHitTarget && hitPlayers.Count > 0)
        {
            target = hitPlayers[UnityEngine.Random.Range(0, hitPlayers.Count)];
        }

        // ���s������U���𒆒f
        if (hitPlayers.Count == 0)
        {
            SetAnimId((int)ANIM_ID.Idle);
            OnEndAttackAnimEvent();
        }
    }

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] �傫���؂艺�����Ƃ��̍U������
    /// </summary>
    public override void OnAttackAnim4Event()
    {
        const float addForcePower = 100f;

        audioAttack.Play();

        // ���g���G���[�g�̂̏ꍇ�A�t�^�����Ԉُ�̎�ނ��擾����
        DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();

        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange * 1.5f);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Player")
            {
                PlayerBase player = collidersEnemies[i].gameObject.GetComponent<PlayerBase>();
                Rigidbody2D rigidbody2D = player.GetComponent<Rigidbody2D>();

                player.ApplyDamage((int)(power * 1.5f), transform.position, null, applyEffect);
                rigidbody2D.gravityScale = 0;
                rigidbody2D.linearVelocity = Vector2.zero;
                rigidbody2D.AddForce(new Vector2(TransformUtils.GetFacingDirection(transform) * addForcePower, 0), ForceMode2D.Impulse);
            }
        }
    }

    #endregion

    #region �U���N�[���_�E��

    /// <summary>
    /// [Animation�C�x���g����̌Ăяo��] �U���N�[���_�E������
    /// </summary>
    public override void OnEndAttackAnimEvent()
    {
        RemoveAndStopCoroutineByKey(COROUTINE.AttackChargeCoroutine.ToString());

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
        terrainCollider.enabled = true;
        canCancelAttackOnHit = true;
        isAttacking = true;
        Idle();
        yield return new WaitForSeconds(time);
        isAttacking = false;
        NextDecision();
        onFinished?.Invoke();
    }

    #endregion

    #endregion

    #region �ړ������֘A

    /// <summary>
    /// �e���|�[�g���ĒǐՂ��鏈�����J�n
    /// </summary>
    protected override void Tracking()
    {
        SetAnimId((int)ANIM_ID.Teleport);
        m_rb2d.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// [�A�j���[�V�����C�x���g����Ăяo��] �e���|�[�g�̃A�j���[�V�����C�x���g
    /// </summary>
    public override void OnMoveAnimEvent()
    {
        isInvincible = true;
        m_rb2d.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// [�A�j���[�V�����C�x���g����Ăяo��] �e���|�[�g�̃A�j���[�V�����I����
    /// </summary>
    public override void OnEndMoveAnimEvent()
    {
        audioTeleport.Play();

        Vector2 teleportPos = transform.position;

        // �v���C���[�̔w��Ƀe���|�[�g
        const float offsetX = 1.6f;
        if (target) teleportPos = (Vector2)target.transform.position + Vector2.right * offsetX * -TransformUtils.GetFacingDirection(target.transform);
        transform.position = teleportPos;

        // �v���C���[�̕���������
        LookAtTarget();

        isInvincible = false;
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

        SEManager.Instance.Play(
               audioPath: SEPath.MOB_HIT, //�Đ��������I�[�f�B�I�̃p�X
               volumeRate: 1.0f,                //���ʂ̔{��
               delay: 0.0f,                //�Đ������܂ł̒x������
               pitch: 1.0f,                //�s�b�`
               isLoop: false,             //���[�v�Đ����邩
               callback: null              //�Đ��I����̏���
               );
    }

    /// <summary>
    /// ���S����Ƃ��ɌĂ΂�鏈��
    /// </summary>
    /// <returns></returns>
    protected override void OnDead()
    {
        SetAnimId((int)ANIM_ID.Dead);

        SEManager.Instance.Play(
               audioPath: SEPath.MOB_DEATH, //�Đ��������I�[�f�B�I�̃p�X
               volumeRate: 1.0f,                //���ʂ̔{��
               delay: 0.0f,                //�Đ������܂ł̒x������
               pitch: 1.0f,                //�s�b�`
               isLoop: false,             //���[�v�Đ����邩
               callback: null              //�Đ��I����̏���
               );
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
                animator.Play("Hit_Knight", 0, 0);
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

        terrainCollider.enabled = true;
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
    }

    #endregion
}
