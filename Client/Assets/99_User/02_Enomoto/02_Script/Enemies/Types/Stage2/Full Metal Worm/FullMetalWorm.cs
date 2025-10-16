//**************************************************
//  [�{�X] �t�����^�����[��(�{��)�̂̊Ǘ��N���X
//  Author:r-enomoto
//**************************************************
using DG.Tweening;
using Pixeye.Unity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Shared.Interfaces.StreamingHubs.EnumManager;

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

    /// <summary>
    /// �s���p�^�[��
    /// </summary>
    public enum DECIDE_TYPE
    {
        None = 0,
        Move,
        Attack,
    }
    DECIDE_TYPE nextDecide = DECIDE_TYPE.None;

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

    float randomDecision;
    #endregion

    #region ��ԊǗ�
    readonly float disToTargetPosMin = 3f;
    #endregion

    #region �G�̐����֘A
    [Foldout("�G�̐����֘A")]
    [SerializeField]
    int generatedMax = 15;
    public int GeneratedMax { get { return generatedMax; } }

    [Foldout("�G�̐����֘A")]
    [SerializeField]
    float distToPlayerMin = 10;
    public float DistToPlayerMin { get { return distToPlayerMin; } }

    // �������Ă���G�̐�
    int generatedEnemyCnt = 0;
    public int GeneratedEnemyCnt { get { return generatedEnemyCnt; } set { generatedEnemyCnt = value; } }
    #endregion

    #region �I���W�i��

    [SerializeField]
    [Foldout("���Ԃ̃��X�g")]
    List<GameObject> joints = new List<GameObject>();

    [SerializeField]
    [Foldout("body�O���[�v�̃��X�g")]
    List<Transform> bodyGroups = new List<Transform>();

    [SerializeField]
    [Foldout("�ړ��͈�")]
    Vector2 maxPos;

    [SerializeField]
    [Foldout("�ړ��͈�")]
    Vector2 minPos;

    Vector2 targetPos;
    #endregion

    #region �I�[�f�B�I�֘A

    [SerializeField]
    [Foldout("�I�[�f�B�I")]
    AudioSource audioMove;

    #endregion

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
        doOnceDecision = false;
        bodys.AddRange(GetComponentsInChildren<FullMetalBody>(true));   // �S�Ă̎q�I�u�W�F�N�g������FullMetalBody���擾

        if (RoomModel.Instance && !RoomModel.Instance.IsMaster)
        {
            // ���Ԃ�Joint�Ȃǂɂ���ĒǏ]���Ȃ��悤�ɂ���
            foreach (var joint in joints)
            {
                joint.GetComponent<HingeJoint2D>().enabled = false;
                joint.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            }

            // �̂̊e���ʂ̐e�q�֌W���������A�ꏏ�Ɉړ����Ȃ��悤�ɂ���
            foreach(var group in bodyGroups)
            {
                group.parent = null;
            }
        }
    }

    protected override void FixedUpdate()
    {
        if (isSpawn || isStun || isAttacking || isInvincible || hp <= 0) return;

        if (!target && characterManager.PlayerObjs.Count > 0)
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
        if (RoomModel.Instance && !RoomModel.Instance.IsMaster) return;

        if (doOnceDecision)
        {
            SelectNewTargetInBossRoom();
            doOnceDecision = false;
            RemoveAndStopCoroutineByKey(COROUTINE.MoveGraduallyCoroutine.ToString());

            if (nextDecide == DECIDE_TYPE.Attack)
            {
                generatedEnemyCnt = CharacterManager.Instance.GetEnemiesBySpawnType(SPAWN_ENEMY_TYPE.ByWorm).Count;
                // �S�Ă̕��ʂ̍s�������s
                isAttacking = true;
                ExecuteAllPartActions();
                MoveGradually(true);
                AttackCooldown();
            }        
            else if (nextDecide == DECIDE_TYPE.Move)
            {
                Move();
            }
            else
            {
                NextDecision();
            }
        }
    }

    #region �R���[�`���Ăяo��

    /// <summary>
    /// ���̍s���p�^�[�����I�R���[�`���Ăяo��
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    void NextDecision(float? time = null)
    {
        if (RoomModel.Instance && !RoomModel.Instance.IsMaster) return;

        // ���s���Ă��Ȃ���΁A�s���̒��I�̃R���[�`�����J�n
        string key = COROUTINE.NextDecision.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(NextDecisionCoroutine(time, () => { RemoveAndStopCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// �ߐڍU���̃R���[�`���Ăяo��
    /// </summary>
    public void MeleeAttack()
    {
        // ���s���Ă��Ȃ���΁A�������ړ�����R���[�`�����J�n
        string key = COROUTINE.MeleeAttackCoroutine.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(MeleeAttackCoroutine(() => { RemoveAndStopCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// �U���N�[���_�E���̃R���[�`���Ăяo��
    /// </summary>
    void AttackCooldown()
    {
        // ���s���Ă��Ȃ���΁A�N�[���_�E���̃R���[�`�����J�n
        string key = COROUTINE.AttackCooldown.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(AttackCooldownCoroutine(attackCoolTime, () => { RemoveAndStopCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// �ړ��̃R���[�`���Ăяo��
    /// </summary>
    void Move()
    {
        // ���s���Ă��Ȃ���΁A�ړ��̃R���[�`�����J�n
        string key = COROUTINE.MoveCoroutine.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(MoveCoroutine(() => { RemoveAndStopCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// �������ړ���������R���[�`���Ăяo��
    /// </summary>
    void MoveGradually(bool isTargetLottery = false)
    {
        // ���s���Ă��Ȃ���΁A�������ړ�����R���[�`�����J�n
        string key = COROUTINE.MoveGraduallyCoroutine.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(MoveGraduallyCoroutine(isTargetLottery));
            managedCoroutines.Add(key, coroutine);
        }
    }

    #endregion

    #region ���I�����֘A
    /// <summary>
    /// ���̍s���p�^�[�����菈��
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator NextDecisionCoroutine(float? time = null, Action onFinished = null)
    {
        if (time == null) time = Mathf.Floor(UnityEngine.Random.Range(decisionTimeMin, decisionTimeMax));
        doOnceDecision = false;
        MoveGradually();
        yield return new WaitForSeconds((float)time);

        // �e�s���p�^�[���̏d��
        int attackWeight = 0, moveWeight = 5;

        if (!isAttacking)
        {
            // �͈͓���Player�����邩�`�F�b�N
            int layerNumber = 1 << LayerMask.NameToLayer("Player");
            Collider2D hit = Physics2D.OverlapCircle(playerCheck.position, playerCheckRange, layerNumber);
            if (hit)
            {
                attackWeight = 10;
            }
        }

        // �S�̂̒������g���Ē��I
        int totalWeight = attackWeight + moveWeight;
        randomDecision = UnityEngine.Random.Range(1, totalWeight + 1);

        // ���I�����l�Ŏ��̍s���p�^�[�������肷��
        if (randomDecision <= attackWeight) nextDecide = DECIDE_TYPE.Attack;
        else nextDecide = DECIDE_TYPE.Move;

        doOnceDecision = true;
        onFinished?.Invoke();
    }

    #endregion

    #region �U�������֘A

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
    /// �ߐڍU���R���[�`��
    /// </summary>
    /// <returns></returns>
    IEnumerator MeleeAttackCoroutine(Action onFinished)
    {
        SetAnimId((int)ANIM_ID.Attack);
        while (!isDead)
        {
            // ���g���G���[�g�̂̏ꍇ�A�t�^�����Ԉُ�̎�ނ��擾����
            DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();

            Collider2D[] collidersEnemies = Physics2D.OverlapBoxAll(meleeAttackCheck.position, meleeAttackRange, 0);
            for (int i = 0; i < collidersEnemies.Length; i++)
            {
                if (collidersEnemies[i].gameObject.tag == "Player")
                {
                    collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power * 10, transform.position,KB_POW.Big, applyEffect);
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
    IEnumerator AttackCooldownCoroutine(float time, Action onFinished)
    {
        yield return new WaitForSeconds(time);
        isAttacking = false;
        NextDecision();
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
        audioMove.Play();

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

        NextDecision();
        onFinished?.Invoke();
    }

    /// <summary>
    /// �O�������~�����܂ł������ƈړ��������鏈��
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveGraduallyCoroutine(bool isTargetLottery = false)
    {
        SetNextTargetPosition(isTargetLottery);
        while (true)
        {
            if (!isTargetLottery)
            {
                // �v���C���[�Ƃ̋�������������Ă���ꍇ�́A�v���C���[�̍��W�Ɍ������Ĉړ�
                float distToNearPlayer = Vector2.Distance(GetNearPlayer().transform.position, transform.position);
                if (Mathf.Abs(distToNearPlayer) > playerCheckRange * 1.5f)
                {
                    SetNextTargetPosition(true);
                }
            }

            RotateTowardsMovementDirection(targetPos, rotationSpeedMin);
            MoveTowardsTarget(moveSpeedMin);
            float disToTargetPos = Mathf.Abs(Vector3.Distance(targetPos, this.transform.position));
            if (disToTargetPos <= disToTargetPosMin)
            {
                SetNextTargetPosition(isTargetLottery);
            }
            yield return null;

            // ���݃X�e�[�W�͈͊O�ɂ���ꍇ�͌��݂̃R���[�`�����I�����AMoveCoroutine�����s
            if (transform.position.x < minPos.x || transform.position.x > maxPos.x
                || transform.position.y < minPos.y || transform.position.y > maxPos.y)
            {
                Move();
                RemoveAndStopCoroutineByKey(COROUTINE.NextDecision.ToString());
                RemoveAndStopCoroutineByKey(COROUTINE.MoveGraduallyCoroutine.ToString());
            }
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
        audioMove.Stop();
        SetAnimId((int)ANIM_ID.Dead);

        // �S�Ă̕��ʂ̎��S�������Ăяo��
        foreach (var body in bodys)
        {
            StartCoroutine(body.DestroyEnemy(null));
        }

        // ���������U�R�G��j������
        foreach (var enemy in CharacterManager.Instance.GetEnemiesBySpawnType(SPAWN_ENEMY_TYPE.ByWorm))
        {
            StartCoroutine(enemy.GetComponent<EnemyBase>().DestroyEnemy(null));
        }

        PlayHitBlendShader(false, 2f, 0.5f);
    }

    /// <summary>
    /// �_���[�W�K�p����
    /// </summary>
    /// <param name="power"></param>
    /// <param name="attacker"></param>
    /// <param name="effectTypes"></param>
    public override void ApplyDamageRequest(int power, GameObject attacker = null, bool isKnokBack = true, bool drawDmgText = true, params DEBUFF_TYPE[] effectTypes)
    {
        base.ApplyDamageRequest(power, attacker, false, drawDmgText, effectTypes);
    }

    #endregion

    #region �A�j���[�V�����C�x���g�֘A

    /// <summary>
    /// ���S�A�j���[�V�����Đ����ɌĂ΂��
    /// </summary>
    public void OnDeathAnimEvent()
    {
        // �S�Ă̕��ʂ̃f�X�|�[���A�j���[�V�����Đ�
        foreach (var body in bodys)
        {
            body.Despown();
        }
    }

    /// <summary>
    /// �X�|�[���A�j�����[�V�����J�n��
    /// </summary>
    public void OnSpawnAnimEventByHead()
    {
        OnSpawnAnimEvent();
    }

    /// <summary>
    /// �X�|�[���A�j���[�V�������I�������Ƃ�
    /// </summary>
    public void OnEndSpawnAnimEventByHead()
    {
        OnEndSpawnAnimEvent();

        MeleeAttack();
        NextDecision();
    }

    #endregion

    #region ���A���^�C�������֘A

    /// <summary>
    /// �}�X�^�N���C�A���g�؂�ւ����ɏ�Ԃ����Z�b�g����
    /// </summary>
    public override void ResetAllStates()
    {
        base.ResetAllStates();

        bool isAttacking = IsBodyAttacking();
        foreach (var body in bodys)
        {
            body.ResetAllStates();
        }

        foreach (var joint in joints)
        {
            joint.GetComponent<HingeJoint2D>().enabled = true;
            joint.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }

        foreach (var group in bodyGroups)
        {
            group.parent = this.transform;
        }

        nextDecide = isAttacking ? DECIDE_TYPE.Attack : DECIDE_TYPE.None;
        MeleeAttack();
        DecideBehavior();
    }

    /// <summary>
    /// ��ł�body���U�������ǂ���
    /// </summary>
    /// <returns></returns>
    bool IsBodyAttacking()
    {
        bool isAttacking = false;
        foreach (var body in bodys)
        {
            FullMetalBody.ANIM_ID animId = (FullMetalBody.ANIM_ID)body.GetAnimId();
            if (animId == FullMetalBody.ANIM_ID.Attack)
            {
                isAttacking = true; break;
            }
        }
        return isAttacking;
    }

    /// <summary>
    /// �������擾
    /// </summary>
    /// <returns></returns>
    public override EnemyData GetEnemyData()
    {
        EnemyData enemyData = new EnemyData();

        // ���Ԃ̓�������������
        List<Quaternion> rotations = joints.Select(obj => obj.transform.rotation).ToList();
        List<Vector2> positions = joints.Select(obj => (Vector2)obj.transform.position).ToList();
        enemyData.Quatarnions.AddRange(rotations);
        enemyData.Vector2s.AddRange(positions);

        return SetEnemyData(enemyData);
    }

    /// <summary>
    /// �������X�V
    /// </summary>
    /// <param name="enemyData"></param>
    public override void UpdateEnemy(EnemyData enemyData)
    {
        base.UpdateEnemy(enemyData);

        // ���Ԃ̉�]�A���W�𓯊�����
        for (int i = 0; i < enemyData.Quatarnions.Count; i++)
        {
            if (i <= joints.Count - 1 && joints[i] != null)
            {
                joints[i].transform.DORotateQuaternion(enemyData.Quatarnions[i], CharacterManager.Instance.UpdateSec).SetEase(Ease.Linear);
            }
        }
        for (int i = 0; i < enemyData.Vector2s.Count; i++)
        {
            if (i <= joints.Count - 1 && joints[i] != null)
            {
                joints[i].transform.DOMove(enemyData.Vector2s[i], CharacterManager.Instance.UpdateSec).SetEase(Ease.Linear);
            }
        }
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
        if (SpawnManager.Instance == null) return;
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
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, terrainCheckRange);
    }

    #endregion

}
