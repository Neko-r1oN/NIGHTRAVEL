//**************************************************
//  [�{�X] �t�����^�����[��(�{��)�̂̊Ǘ��N���X
//  Author:r-enomoto
//**************************************************
using DG.Tweening;
using NUnit.Framework;
using Pixeye.Unity;
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

    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Transform stageCenter;
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

    // ���Ƃŏ���
    [SerializeField] Transform minRange;
    [SerializeField] Transform maxRange;
    public Transform MinRange { get { return minRange; } }
    public Transform MaxRange { get { return maxRange; } }

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
        cancellCoroutines.Add(StartCoroutine(NextDecision()));
        MeleeAttack();
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
                    cancellCoroutines.Add(StartCoroutine(AttackCooldown(attackCoolTime)));
                }
                else
                {
                    StartCoroutine(NextDecision(0.5f));
                }
            }
            else if (randomDecision <= 0.8f)
            {
                StopCoroutine("MoveGraduallyCoroutine");
                cancellCoroutines.Add(StartCoroutine(MoveCoroutine()));
            }
            else
            {
                StartCoroutine(NextDecision());
            }
        }
    }

    /// <summary>
    /// ���̍s���p�^�[�����菈��
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator NextDecision(float time = 0)
    {
        if (time == 0) time = Mathf.Floor(Random.Range(decisionTimeMin, decisionTimeMax));
        doOnceDecision = false;
        StartCoroutine("MoveGraduallyCoroutine");
        yield return new WaitForSeconds(time);
        randomDecision = Random.Range(0f, 1f);
        doOnceDecision = true;
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
        cancellCoroutines.Add(StartCoroutine(MeleeAttackCoroutine()));
    }

    /// <summary>
    /// �ߐڍU���R���[�`��
    /// </summary>
    /// <returns></returns>
    IEnumerator MeleeAttackCoroutine()
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
    }

    /// <summary>
    /// �N�[���_�E������
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time)
    {
        yield return new WaitForSeconds(time);
        isAttacking = false;
        cancellCoroutines.Add(StartCoroutine(NextDecision()));
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
    IEnumerator MoveCoroutine()
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
                else currentSpeed -= Time.deltaTime * moveSpeed * 0.7f;
            }

            MoveTowardsTarget(currentSpeed);
            yield return null;
        }
        StartCoroutine(NextDecision());
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
        if (alivePlayers.Count > 0) target = alivePlayers[Random.Range(0, alivePlayers.Count)];
        else target = null;
        return target;
    }

    /// <summary>
    /// �X�e�[�W�̒����̍��W��ݒ�
    /// </summary>
    /// <param name="stageCenter"></param>
    public void SetStageCenterParam(Transform stageCenter)
    {
        this.stageCenter = stageCenter;
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
            float rnd = Random.Range(0f, 1f);
            if (target) targetPos = target.transform.position;
        }

        if (!target || !isTargetLottery)
        {
            for (int i = 0; i < 100; i++)
            {
                // �����_���Ȓn�_�𒊑I
                Vector2 maxPos = (Vector2)stageCenter.position + Vector2.one * moveRange;
                Vector2 minPos = (Vector2)stageCenter.position + Vector2.one * -moveRange;
                targetPos = new Vector2(Random.Range(minPos.x, maxPos.x + 1), Random.Range(minPos.y, maxPos.y + 1));

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
        if (stageCenter != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere((Vector2)stageCenter.position, moveRange);
        }

        // ��Q�������m����͈�
        //Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(transform.position, terrainCheckRange);
    }

    #endregion

}
