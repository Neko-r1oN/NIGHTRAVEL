//**************************************************
//  [�{�X] �t�����^�����[���̃N���X
//  Author:r-enomoto
//**************************************************
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
    Vector2 stageCenter = Vector2.zero;
    [Foldout("�`�F�b�N�֘A")]
    [SerializeField]
    float moveRange;
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

    #region ���̑������o�ϐ�
    [Foldout("���̑�")]
    [SerializeField]
    List<Transform> enemySpawnPoints = new List<Transform>();   // �G�𐶐����镔��

    Vector2 targetPos;
    #endregion

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
        doOnceDecision = false;
        endDecision = true;
        StartCoroutine(NextDecision());
    }

    protected override void FixedUpdate()
    {
        if (isStun || isAttacking || isInvincible || hp <= 0) return;

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
            //// �^�[�Q�b�g�Ƌ������߂��ꍇ�͊m���ōU����D��
            //if (disToTarget < disToTargetMin)
            //{
            //    if (randomDecision > 0.3f)
            //    {
            //        Attack();
            //    }
            //    else
            //    {
            //        Tracking();
            //    }
            //}
            //// �^�[�Q�b�g�Ƌ����������ꍇ�͊m���ňړ���D��
            //else
            //{
            //    if (randomDecision > 0.3f)
            //    {
            //        Tracking();
            //    }
            //    else
            //    {
            //        Attack();
            //    }
            //}
            StartCoroutine(MoveCoroutine());
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
        Debug.Log(time);
        doOnceDecision = false;
        StartCoroutine("MoveGraduallyCoroutine");
        yield return new WaitForSeconds(time);
        StopCoroutine("MoveGraduallyCoroutine");
        randomDecision = Random.Range(0, 1);
        doOnceDecision = true;
    }

    #region �U�������֘A

    /// <summary>
    /// �U�R�G�𕡐��������鏈��
    /// </summary>
    void RunEnemySpawnLoops()
    {
        foreach (Transform point in enemySpawnPoints)
        {
            int maxEnemies = Random.Range(1, 4);
            cancellCoroutines.Add(StartCoroutine(GenerateEnemeiesCoroutine(point, maxEnemies)));
        }
        cancellCoroutines.Add(StartCoroutine(Cooldown(attackCoolTime)));
    }

    /// <summary>
    /// �U�R�G�𕡐��������鏈��
    /// </summary>
    IEnumerator GenerateEnemeiesCoroutine(Transform point, int maxEnemies)
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            float time = Random.Range(0f, 0.5f);
            yield return new WaitForSeconds(time);

            // �����ɐ������鏈�� && �n�b�`���J���A�j���[�V���� �� �����ʒu���ǂɖ��܂��Ă��Ȃ��ꍇ
            // �n�b�`����o�Ă���悤�ȉ��o(�����x��0����n�܂�A�f����1�ɂȂ�悤�ɂ���B�����x��1�ɂȂ�܂Ŗ��G
            // �h���[���F�����_���ȍ��W�Ɍ������Ĉړ�������, ���F�U���̂�𗘗p
        }
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
        // �O�ɔ�э���
        Vector2 jumpVec = new Vector2(18 * TransformHelper.GetFacingDirection(transform), 10);
        m_rb2d.linearVelocity = jumpVec;

        // ���g���G���[�g�̂̏ꍇ�A�t�^�����Ԉُ�̎�ނ��擾����
        bool isElite = this.isElite && enemyElite != null;
        StatusEffectController.EFFECT_TYPE? applyEffect = null;
        if (isElite)
        {
            applyEffect = enemyElite.GetAddStatusEffectEnum();
        }

        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Player")
            {
                collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position, applyEffect);
            }
        }

        cancellCoroutines.Add(StartCoroutine(Cooldown(attackCoolTime)));
    }

    /// <summary>
    /// �N�[���_�E������
    /// </summary>
    /// <returns></returns>
    IEnumerator Cooldown(float time)
    {
        isAttacking = true;
        yield return new WaitForSeconds(time);
        isAttacking = false;
        doOnceDecision = true;
        Idle();
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
        List<GameObject> alivePlayers = new List<GameObject>();
        foreach (GameObject player in Players)
        {
            if (player && player.GetComponent<CharacterBase>().HP > 0)
            {
                alivePlayers.Add(player);
            }
        }

        if (alivePlayers.Count > 0) target = alivePlayers[Random.Range(0, alivePlayers.Count)];
        else target = null;
        return target;
    }

    /// <summary>
    /// �X�e�[�W�̒����̍��W��ݒ�
    /// </summary>
    /// <param name="stageCenter"></param>
    public void SetStageCenterParam(Vector2 stageCenter)
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
                Vector2 maxPos = stageCenter + Vector2.one * moveRange;
                Vector2 minPos = stageCenter + Vector2.one * -moveRange;
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
        if (stageCenter != Vector2.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(stageCenter, moveRange);
        }
    }

    #endregion

}
