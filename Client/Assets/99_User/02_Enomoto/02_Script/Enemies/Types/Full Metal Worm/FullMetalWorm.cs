//**************************************************
//  [�{�X] �t�����^�����[��(�{��)�̂̊Ǘ��N���X
//  Author:r-enomoto
//**************************************************
using DG.Tweening;
using NUnit.Framework;
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class FullMetalWorm : EnemyBase
{
    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum ANIM_ID
    {
        Idle = 0,
        Attack,
        Dead,
    }

    /// <summary>
    /// �Ǘ�����R���[�`���̎��
    /// </summary>
    public enum COROUTINE
    {
        NextDecision,
        MeleeAttackCoroutine,
        AttackCooldown,
        MoveCoroutine,
        MoveGraduallyCoroutine,
    }

    #region �R���|�[�l���g
    List<FullMetalBody> bodys = new List<FullMetalBody>();
    #endregion

    #region �X�e�[�^�X
    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    float rotationSpeed = 50f; // ���̉�]���x

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    float rotationSpeedMin = 5f; // �ŏ���]���x

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    float moveSpeedMin = 2.5f;    // �ŏ��ړ����x
    #endregion

    #region �`�F�b�N����
    // �߂��Ƀv���C���[�����邩�`�F�b�N����͈�
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Transform playerCheck;
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    float playerCheckRange;

    // �ߋ����U���͈̔�
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Transform meleeAttackCheck;
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Vector2 meleeAttackRange = Vector2.zero;

    // �ړ��\�͈͊֘A
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    float moveRange;

    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    float terrainCheckRange;    // �n�`�ɖ��܂��Ă��Ȃ����`�F�b�N����͈�
    public float TerrainCheckRane { get { return terrainCheckRange; } }
    #endregion

    #region ���I�����֘A
    [Foldout("���I�֘A")]
    [SerializeField] 
    float decisionTimeMax = 6f;

    [Foldout("���I�֘A")]
    [SerializeField]
    float decisionTimeMin = 2f;
    #endregion

    #region ��ԊǗ�
    readonly float disToTargetPosMin = 3f;
    float randomDecision;
    bool endDecision;
    #endregion

    #region �G�̐����֘A
    [Foldout("�G�̐����֘A")]
    [SerializeField]
    List<GameObject> enemyPrefabs = new List<GameObject>();
    public List<GameObject> EnemyPrefabs { get { return enemyPrefabs; } }

    [Foldout("�G�̐����֘A")]
    [SerializeField]
    int generatedMax = 15;
    public int GeneratedMax { get { return generatedMax; } }

    [Foldout("�G�̐����֘A")]
    [SerializeField]
    float distToPlayerMin = 6;
    public float DistToPlayerMin { get { return distToPlayerMin; } }

    // �����ς݂̓G
    List<GameObject> generatedEnemies = new List<GameObject>();
    public List<GameObject> GeneratedEnemies { get { return generatedEnemies; } set { generatedEnemies = value; } }
    #endregion

    #region ���̑������o�ϐ�
    Vector2 targetPos;
    #endregion

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
        doOnceDecision = false;
        endDecision = true;
        isSpawn = false;
        isInvincible = false;
        bodys.AddRange(GetComponentsInChildren<FullMetalBody>(true));   // �S�Ă̎q�I�u�W�F�N�g������FullMetalBody���擾
        MeleeAttack();

        // ���s���Ă��Ȃ���΁A�s���̒��I�̃R���[�`�����J�n
        if (!ContaintsManagedCoroutine(COROUTINE.NextDecision.ToString()))
        {
            Coroutine coroutine = StartCoroutine(NextDecision());
            managedCoroutines.Add(COROUTINE.NextDecision.ToString(), coroutine);
        }
    }

    protected override void FixedUpdate()
    {
        if (isSpawn || isStun || isAttacking || isInvincible || hp <= 0) return;

        if (!target && Players.Count > 0)
        {
            // �^�[�Q�b�g��T��
            SetRandomTargetPlayer();
        }

        if (target)
        {
            // �^�[�Q�b�g�Ƃ̋������擾����
            disToTarget = Vector3.Distance(target.transform.position, this.transform.position);
            disToTargetX = target.transform.position.x - transform.position.x;
        }

        DecideBehavior();
    }

    /// <summary>
    /// �s���p�^�[�����s����
    /// </summary>
    protected override void DecideBehavior()
    {
        if (doOnceDecision)
        {
            doOnceDecision = false;
            if (!isAttacking && randomDecision <= 0.4f)
            {
                // ���Ɏ��S���Ă��鐶���ς݂̓G�̗v�f���폜
                generatedEnemies.RemoveAll(item => item == null);

                // �͈͓���Player�����邩�`�F�b�N
                int layerNumber = 1 << LayerMask.NameToLayer("Player");
                Collider2D hit = Physics2D.OverlapCircle(playerCheck.position, playerCheckRange, layerNumber);
                if (hit)
                {
                    // �S�Ă̕��ʂ̍s�������s
                    isAttacking = true;
                    ExecuteAllPartActions();

                    // ���s���Ă��Ȃ���΁A�N�[���_�E���̃R���[�`�����J�n
                    string key = COROUTINE.AttackCooldown.ToString();
                    if (!ContaintsManagedCoroutine(key))
                    {
                        Coroutine coroutine = StartCoroutine(AttackCooldown(attackCoolTime, () => { RemoveCoroutineByKey(key); }));
                        managedCoroutines.Add(key, coroutine);
                    }
                }
                else
                {
                    // ���s���Ă��Ȃ���΁A�s���̒��I�̃R���[�`�����J�n
                    string key = COROUTINE.NextDecision.ToString();
                    if (!ContaintsManagedCoroutine(key))
                    {
                        Coroutine coroutine = StartCoroutine(NextDecision(0.5f));
                        managedCoroutines.Add(key, coroutine);
                    }
                }
            }
            else if (randomDecision <= 0.8f)
            {
                RemoveCoroutineByKey(COROUTINE.MoveGraduallyCoroutine.ToString());

                // ���s���Ă��Ȃ���΁A�ړ��̃R���[�`�����J�n
                string key = COROUTINE.MoveCoroutine.ToString();
                if (!ContaintsManagedCoroutine(key))
                {
                    Coroutine coroutine = StartCoroutine(MoveCoroutine(() => { RemoveCoroutineByKey(key); }));
                    managedCoroutines.Add(key, coroutine);
                }
            }
            else
            {
                // ���s���Ă��Ȃ���΁A�s���̒��I�̃R���[�`�����J�n
                string key = COROUTINE.NextDecision.ToString();
                if (!ContaintsManagedCoroutine(key))
                {
                    Coroutine coroutine = StartCoroutine(NextDecision());
                    managedCoroutines.Add(key, coroutine);
                }
            }
        }
    }

    /// <summary>
    /// ���̍s���p�^�[�����菈��
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator NextDecision(float? time = null, Action onFinished = null)
    {
        if (time == null) time = Mathf.Floor(UnityEngine.Random.Range(decisionTimeMin, decisionTimeMax));
        doOnceDecision = false;

        // ���s���Ă��Ȃ���΁A�������ړ�����R���[�`�����J�n
        string key = COROUTINE.MoveGraduallyCoroutine.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(MoveGraduallyCoroutine());
            managedCoroutines.Add(key, coroutine);
        }

        yield return new WaitForSeconds((float)time);
        randomDecision = UnityEngine.Random.Range(0f, 1f);
        doOnceDecision = true;
        onFinished?.Invoke();
    }

    #region �U�������֘A

    [ContextMenu("ExecuteAllPartActions")]
    /// <summary>
    /// �S�Ă̕��ʂ̍s�������s
    /// </summary>
    public void ExecuteAllPartActions()
    {
        foreach (var body in bodys)
        {
            if (body.HP > 0)
            {
                body.ActByRoleType();
            }
        }
    }

    /// <summary>
    /// �ߐڍU������
    /// </summary>
    public void MeleeAttack()
    {
        // ���s���Ă��Ȃ���΁A�������ړ�����R���[�`�����J�n
        string key = COROUTINE.MeleeAttackCoroutine.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(MeleeAttackCoroutine(() => { RemoveCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// �ߐڍU���R���[�`��
    /// </summary>
    /// <returns></returns>
    IEnumerator MeleeAttackCoroutine(Action onFinished)
    {
        SetAnimId((int)ANIM_ID.Attack);
        while (!isDead)
        {
            // ���g���G���[�g�̂̏ꍇ�A�t�^�����Ԉُ�̎�ނ��擾����
            StatusEffectController.EFFECT_TYPE? applyEffect = GetStatusEffectToApply();

            Collider2D[] collidersEnemies = Physics2D.OverlapBoxAll(meleeAttackCheck.position, meleeAttackRange, 0);
            for (int i = 0; i < collidersEnemies.Length; i++)
            {
                if (collidersEnemies[i].gameObject.tag == "Player")
                {
                    collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position, applyEffect);
                }
            }
            yield return null;
        }
        onFinished?.Invoke();
    }

    /// <summary>
    /// �N�[���_�E������
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time, Action onFinished)
    {
        yield return new WaitForSeconds(time);
        isAttacking = false;

        // ���s���Ă��Ȃ���΁A�s���̒��I����R���[�`�����J�n
        string key = COROUTINE.NextDecision.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(NextDecision(null, () => { RemoveCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
        onFinished?.Invoke();
    }

    #endregion

    #region �ړ������֘A

    /// <summary>
    /// �i�s�����Ɍ������Ĉړ����鏈��
    /// </summary>
    /// <param name="targetPos"></param>
    /// <param name="direction"></param>
    void MoveTowardsTarget(float speed)
    {
        m_rb2d.linearVelocity = transform.up.normalized * speed;
    }

    /// <summary>
    /// ���݂̐i�s�����Ɍ������ĉ�]������
    /// </summary>
    void RotateTowardsMovementDirection(Vector3 targetPos, float rotationSpeed)
    {
        Vector2 direction = (targetPos - this.transform.position).normalized;
        // �����Ă��Ȃ��ꍇ�͉�]�����Ȃ�
        if (direction == Vector2.zero) return;

        // �x�N�g������p�x���v�Z�i���W�A������x���ɕϊ����A-90�x�Ő^���0�x�ɂ���j
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        // ���݂̉�]�p�x����ڕW�p�x�֊��炩�ɕ�Ԃ��ARigidbody2D�̉�]���X�V
        float newAngle = Mathf.LerpAngle(m_rb2d.rotation, targetAngle, Time.fixedDeltaTime * rotationSpeed);
        m_rb2d.MoveRotation(newAngle);
    }

    /// <summary>
    /// �ڕW�n�_�܂ňړ����鏈��
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveCoroutine(Action onFinished)
    {
        SetNextTargetPosition(true);
        float currentSpeed = moveSpeed;
        bool isTargetPos = false;

        while (true)
        {
            // �ڕW�n�_�ɓ��B����܂ŁA�p�x��Ǐ]����
            float disToTargetPos = Mathf.Abs(Vector3.Distance(targetPos, this.transform.position));
            if (!isTargetPos && disToTargetPos <= disToTargetPosMin) isTargetPos = true;
            if (!isTargetPos) RotateTowardsMovementDirection(targetPos, rotationSpeed);

            // �ڕW�n�_���B��A���X�Ɍ�������
            if (isTargetPos)
            {
                if (currentSpeed <= moveSpeedMin) break;
                else currentSpeed -= Time.deltaTime * moveSpeed;
            }

            MoveTowardsTarget(currentSpeed);
            yield return null;
        }

        // ���s���Ă��Ȃ���΁A�s���̒��I����R���[�`�����J�n
        if (!ContaintsManagedCoroutine(COROUTINE.NextDecision.ToString()))
        {
            Coroutine coroutine = StartCoroutine(NextDecision());
            managedCoroutines.Add(COROUTINE.NextDecision.ToString(), coroutine);
        }
        onFinished?.Invoke();
    }

    /// <summary>
    /// �O�������~�����܂ł������ƈړ��������鏈��
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveGraduallyCoroutine()
    {
        SetNextTargetPosition(false);
        while (true)
        {
            RotateTowardsMovementDirection(targetPos, rotationSpeedMin);
            MoveTowardsTarget(moveSpeedMin);
            float disToTargetPos = Mathf.Abs(Vector3.Distance(targetPos, this.transform.position));
            if (disToTargetPos <= disToTargetPosMin / 2)
            {
                SetNextTargetPosition(false);
            }
            yield return null;
        }
    }

    #endregion

    #region �q�b�g�����֘A

    /// <summary>
    /// ���S����Ƃ��ɌĂ΂�鏈������
    /// </summary>
    /// <returns></returns>
    protected override void OnDead()
    {
        SetAnimId((int)ANIM_ID.Dead);

        // �S�Ă̕��ʂ̎��S�������Ăяo��
        foreach (var body in bodys)
        {
            StartCoroutine(body.DestroyEnemy(null));
        }
    }

    /// <summary>
    /// �_���[�W�K�p����
    /// </summary>
    /// <param name="power"></param>
    /// <param name="attacker"></param>
    /// <param name="effectTypes"></param>
    public override void ApplyDamage(int power, Transform attacker = null, bool drawDmgText = true, params StatusEffectController.EFFECT_TYPE[] effectTypes)
    {
        attacker = null;
        base.ApplyDamage(power, attacker, true, effectTypes);
    }

    #endregion

    #region �`�F�b�N�����֘A

    /// <summary>
    /// player���X�g���烉���_���Ƀ^�[�Q�b�g�����߂鏈��
    /// </summary>
    bool SetRandomTargetPlayer()
    {
        List<GameObject> alivePlayers = GetAlivePlayers();
        if (alivePlayers.Count > 0) target = alivePlayers[UnityEngine.Random.Range(0, alivePlayers.Count)];
        else target = null;
        return target;
    }

    /// <summary>
    /// ���̖ڕW�n�_��ݒ肷��
    /// </summary>
    /// <returns></returns>
    void SetNextTargetPosition(bool isTargetLottery = true)
    {
        if (isTargetLottery)
        {
            SetRandomTargetPlayer();
            float rnd = UnityEngine.Random.Range(0f, 1f);
            if (target) targetPos = target.transform.position;
        }

        if (!target || !isTargetLottery)
        {
            for (int i = 0; i < 100; i++)
            {
                // �����_���Ȓn�_�𒊑I
                Vector2 maxPos = SpawnManager.Instance.StageMaxPoint.position;
                Vector2 minPos = SpawnManager.Instance.StageMinPoint.position;
                targetPos = new Vector2(UnityEngine.Random.Range(minPos.x, maxPos.x + 1), UnityEngine.Random.Range(minPos.y, maxPos.y + 1));

                // �����_���ȖڕW�n�_�Ƃ̋��������ȏ㗣��Ă���Ίm�肷��
                float distance = Mathf.Abs(Vector3.Distance(targetPos, this.transform.position));
                if (distance >= moveRange)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// �X�e�[�W�͈̔͂�`�悷��
    /// </summary>
    void DrawRectGizmos()
    {
        Vector2 min = SpawnManager.Instance.StageMinPoint.position;
        Vector2 max = SpawnManager.Instance.StageMaxPoint.position;
        Vector2 bl = new Vector2(min.x, min.y);
        Vector2 br = new Vector2(max.x, min.y);
        Vector2 tr = new Vector2(max.x, max.y);
        Vector2 tl = new Vector2(min.x, max.y);

        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);
    }

    /// <summary>
    /// ���o�͈͂̕`�揈��
    /// </summary>
    protected override void DrawDetectionGizmos()
    {
        // Player���m�͈�
        if (playerCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerCheck.transform.position, playerCheckRange);
        }

        // �U���͈�
        if (meleeAttackCheck)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(meleeAttackCheck.transform.position, meleeAttackRange);
        }

        // �ړ��͈�
        DrawRectGizmos();

        // ��Q�������m����͈�
        //Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(transform.position, terrainCheckRange);
    }

    #endregion

}
