//**************************************************
//  [�{�X] �t�����^�����[���̃N���X
//  Author:r-enomoto
//**************************************************
using DG.Tweening;
using NUnit.Framework;
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullMetalWorm : EnemyBase
{
    /// <summary>
    /// �A�j���[�V����ID
    /// </summary>
    public enum ANIM_ID
    {
        None = 0,
        Idle,
        Attack,
        Run,
        Hit,
        Dead,
    }

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
    // �ߋ����U���͈̔�
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Transform meleeAttackCheck;
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField] 
    float meleeAttackRange = 0.9f;

    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    Transform stageCenter;
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    float moveRange;

    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    float terrainCheckRange;    // �n�`�ɖ��܂��Ă��Ȃ����`�F�b�N����͈�
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
    List<Transform> enemySpawnPoints = new List<Transform>();   // �G�𐶐����镔��

    [Foldout("�G�̐����֘A")]
    [SerializeField]
    List<GameObject> enemyPrefabs = new List<GameObject>();

    [Foldout("�G�̐����֘A")]
    [SerializeField]
    int generatedMax = 15;

    [Foldout("�G�̐����֘A")]
    [SerializeField]
    float distToPlayerMin = 6;

    List<GameObject> generatedEnemies = new List<GameObject>();

    // ���Ƃŏ���
    [SerializeField] Transform minRange;
    [SerializeField] Transform maxRange;

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
        cancellCoroutines.Add(StartCoroutine(NextDecision()));
    }

    protected override void FixedUpdate()
    {
        if (isSpawn || isStun || isAttacking || isInvincible || hp <= 0) return;

        if (!target && Players.Count > 0)
        {
            // �^�[�Q�b�g��T��
            SetRandomTargetPlayer();
        }

        if (!target)
        {
            //if (canPatrol && !isPatrolPaused) Patorol();
            //else Idle();
            //return;

            // �����_���ȍ��W�Ɍ������Ĉړ�����
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
            if (!isAttacking && randomDecision <= 0.5f)
            {
                // ���Ɏ��S���Ă���G�̗v�f���폜
                generatedEnemies.RemoveAll(s => s == null);

                if (generatedEnemies.Count < generatedMax)
                {
                    RunEnemySpawnLoops();
                    return;
                }
            }
            else
            {
                StopCoroutine("MoveGraduallyCoroutine");
                cancellCoroutines.Add(StartCoroutine(MoveCoroutine()));
            }
        }
    }

    /// <summary>
    /// ���̍s���p�^�[�����菈��
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator NextDecision()
    {
        SetNextTargetPosition();
        float time = Mathf.Floor(Random.Range(decisionTimeMin, decisionTimeMax));
        doOnceDecision = false;
        StartCoroutine("MoveGraduallyCoroutine");
        yield return new WaitForSeconds(time);
        randomDecision = Random.Range(0f, 1f);
        doOnceDecision = true;
    }

    #region �U�������֘A

    /// <summary>
    /// �U�R�G�𕡐���������R���[�`���̎��s
    /// </summary>
    void RunEnemySpawnLoops()
    {
        isAttacking = true;
        bool isGenerateSucsess = false;
        int generatedEnemiesCnt = generatedEnemies.Count;
        var alivePlayers = GetAlivePlayers();

        foreach (Transform point in enemySpawnPoints)
        {
            if (generatedEnemiesCnt >= generatedMax) continue;

            bool isPlayerNearby = false;
            foreach (var playerObj in alivePlayers)
            {
                float distToPlayer = Vector2.Distance(point.position, playerObj.transform.position);
                if (distToPlayer <= distToPlayerMin) isPlayerNearby = true;
            }

            // �����ʒu�̋߂��Ƀv���C���[������ && �����ʒu���X�e�[�W�͈͓̔� && �����ʒu���ǂɖ��܂��Ă��Ȃ��ꍇ
            if (isPlayerNearby
                && TransformUtils.IsWithinBounds(point, minRange, maxRange)
                && !Physics2D.OverlapCircle(point.position, terrainCheckRange, terrainLayerMask))
            {
                isGenerateSucsess = true;
                int maxEnemies = Random.Range(1, 4);
                if (generatedEnemiesCnt + maxEnemies > generatedMax) maxEnemies = generatedMax - generatedEnemiesCnt;

                cancellCoroutines.Add(StartCoroutine(GenerateEnemeiesCoroutine(point, maxEnemies)));
            }
        }

        // ��x�ł��G�̐��������������ꍇ�̓N�[���^�C��������������s����
        float time = isGenerateSucsess ? attackCoolTime : 0f;
        cancellCoroutines.Add(StartCoroutine(AttackCooldown(time)));
    }

    /// <summary>
    /// �U�R�G�𕡐���������R���[�`��
    /// </summary>
    IEnumerator GenerateEnemeiesCoroutine(Transform point, int maxEnemies)
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            Random.InitState(System.DateTime.Now.Millisecond);  // �����̃V�[�h�l���X�V
            float time = Random.Range(0f, 0.5f);
            yield return new WaitForSeconds(time);

            // �����ɐ������鏈�� && �n�b�`���J���A�j���[�V����####################################
            Random.InitState(System.DateTime.Now.Millisecond);  // �����̃V�[�h�l���X�V
            generatedEnemies.Add(GenerateEnemy(point.transform.position));
        }
    }

    /// <summary>
    /// �U�R�G�𐶐����鏈��
    /// </summary>
    GameObject GenerateEnemy(Vector2 point)
    {
        var enemyObj = Instantiate(enemyPrefabs[(int)Random.Range(0, enemyPrefabs.Count)], point, Quaternion.identity).gameObject;
        EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();

        if ((int)Random.Range(0, 2) == 0) enemy.Flip();    // �m���Ō������ς��
        enemy.TransparentSprites();
        return enemyObj;
    }

    /// <summary>
    /// �U������
    /// </summary>
    public void Attack()
    {
        doOnceDecision = false;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack);
    }

    /// <summary>
    /// ��莞�ԁA�^���b�g�ōU�����鏈��
    /// </summary>

    public override void OnAttackAnimEvent()
    {
        //// �O�ɔ�э���
        //Vector2 jumpVec = new Vector2(18 * TransformUtils.GetFacingDirection(transform), 10);
        //m_rb2d.linearVelocity = jumpVec;

        //// ���g���G���[�g�̂̏ꍇ�A�t�^�����Ԉُ�̎�ނ��擾����
        //bool isElite = this.isElite && enemyElite != null;
        //StatusEffectController.EFFECT_TYPE? applyEffect = null;
        //if (isElite)
        //{
        //    applyEffect = enemyElite.GetAddStatusEffectEnum();
        //}

        //Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
        //for (int i = 0; i < collidersEnemies.Length; i++)
        //{
        //    if (collidersEnemies[i].gameObject.tag == "Player")
        //    {
        //        collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position, applyEffect);
        //    }
        //}

        cancellCoroutines.Add(StartCoroutine(AttackCooldown(attackCoolTime)));
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
        //Vector2 direction = (targetPos - transform.position).normalized;
        //m_rb2d.linearVelocity = direction * speed;
        //RotateTowardsMovementDirection();

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
        while (true)
        {
            RotateTowardsMovementDirection(targetPos, rotationSpeedMin);
            MoveTowardsTarget(moveSpeedMin);
            float disToTargetPos = Mathf.Abs(Vector3.Distance(targetPos, this.transform.position));
            if (disToTargetPos <= disToTargetPosMin / 2)
            {
                SetNextTargetPosition();
            }
            yield return null;
        }
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
    /// ���S����Ƃ��ɌĂ΂�鏈������
    /// </summary>
    /// <returns></returns>
    protected override void OnDead()
    {
        SetAnimId((int)ANIM_ID.Dead);
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
        if(isTargetLottery) SetRandomTargetPlayer();

        if (target)
        {
            targetPos = target.transform.position;
        }
        else
        {
            for (int i = 0; i < 100; i++)
            {
                // �^�[�Q�b�g�̃v���C���[�����݂��Ȃ��ꍇ�A�����_���Ȓn�_�Ɉړ�����
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
        // �U���J�n����
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackDist);

        // �U���͈�
        if (meleeAttackCheck)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleeAttackCheck.transform.position, meleeAttackRange);
        }

        // �ړ��͈�
        if (stageCenter != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere((Vector2)stageCenter.position, moveRange);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, terrainCheckRange);
    }

    #endregion

}
