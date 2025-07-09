//**************************************************
//  [�{�X] �t�����^�����[��(�{��)�̂̊Ǘ��N���X
//  Author:r-enomoto
//**************************************************
using DG.Tweening;
using NUnit.Framework;
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
    List<Transform> enemySpawnPoints = new List<Transform>();   // �G�𐶐����镔��

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
        bodys.AddRange(GetComponentsInChildren<FullMetalBody>(true));
        //cancellCoroutines.Add(StartCoroutine(NextDecision()));
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
            if (!isAttacking && randomDecision <= 1f)
            {
                // ���Ɏ��S���Ă��鐶���ς݂̓G�̗v�f���폜
                //generatedEnemies.RemoveAll(item => item == null);

                //if (generatedEnemies.Count < generatedMax)
                //{
                //    bool isGenerateSucsess = false;
                //    foreach (var body in bodys)
                //    {
                //        if (body.HP > 0 && body.RoleType == FullMetalBody.ROLE_TYPE.Spawner)
                //        {
                //            bool result = body.RunEnemySpawn();
                //            if (!isGenerateSucsess) isGenerateSucsess = result;
                //        }
                //    }
                //    float time = isGenerateSucsess ? attackCoolTime : 0f;
                //    cancellCoroutines.Add(StartCoroutine(AttackCooldown(time)));
                //    return;
                //}


                foreach (var body in bodys)
                {
                    if (body.HP > 0 && body.RoleType == FullMetalBody.ROLE_TYPE.Attacker)
                    {

                    }
                }
            }
            else
            {
                StopCoroutine("MoveGraduallyCoroutine");
                cancellCoroutines.Add(StartCoroutine(MoveCoroutine()));
            }
        }
    }

    [ContextMenu("CallAttackMethodTest")]
    public void CallAttackMethodTest()
    {
        foreach (var body in bodys)
        {
            if (body.HP > 0 && body.RoleType == FullMetalBody.ROLE_TYPE.Attacker)
            {
                body.Attack();
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

    //IEnumerator RangeAttack()
    //{
    //    yield return new WaitForSeconds(0.25f);  // �U���J�n��x��
    //    gunPsControllerList.StartShooting();

    //    float time = 0;
    //    while (time < shotsPerSecond)
    //    {
    //        // �^�[�Q�b�g�̂�������Ɍ������ăG�C��
    //        if (target)
    //        {
    //            if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
    //                || target.transform.position.x > transform.position.x && transform.localScale.x < 0) Flip();

    //            Vector3 direction = target.transform.position - transform.position;
    //            Quaternion quaternion = Quaternion.Euler(0, 0, projectileChecker.ClampAngleToTarget(direction));
    //            aimTransformList.rotation = Quaternion.RotateTowards(aimTransformList.rotation, quaternion, aimRotetionSpeed);
    //        }
    //        yield return new WaitForSeconds(0.1f);
    //        time += 0.1f;
    //    }

    //    cancellCoroutines.Add(StartCoroutine(AttackCooldown(attackCoolTime)));
    //}

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
        float rnd = Random.Range(0f, 1f);

        if (target) targetPos = target.transform.position;

        if (!target || target && rnd < 0.3f)
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
