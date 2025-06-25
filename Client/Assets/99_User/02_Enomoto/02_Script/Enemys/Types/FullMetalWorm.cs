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
    float rotationSpeed = 90f; // ���̉�]���x
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

    #region ��ԊǗ�
    [SerializeField] float decisionTime = 5f;
    readonly float disToTargetPosMin = 3f;
    float randomDecision;
    bool endDecision;

    [SerializeField] float maxMoveTime = 5f;
    #endregion

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
        doOnceDecision = false;
        endDecision = true;
        StartCoroutine(NextDecision(decisionTime));
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
    IEnumerator NextDecision(float time)
    {
        doOnceDecision = false;
        StartCoroutine(MoveGraduallyCoroutine());
        yield return new WaitForSeconds(time);
        StopCoroutine(MoveGraduallyCoroutine());
        randomDecision = Random.Range(0, 1);
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
    /// �ߐڍU������ [Animation�C�x���g����̌Ăяo��]
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

        cancellCoroutines.Add(StartCoroutine(AttackCooldown(attackCoolTime)));
    }

    /// <summary>
    /// �U�����̃N�[���_�E������
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time)
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

        m_rb2d.linearVelocity = transform.up * speed;
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
        Vector3 targetPos = GetNextTargetPosition();
        bool isTargetPosReached = false;
        float time = 0;

        while (time < maxMoveTime)
        {
            float disToTargetPos = Mathf.Abs(Vector3.Distance(targetPos, this.transform.position));
            if (disToTargetPos > disToTargetPosMin || isTargetPosReached)
            {
                // �ڕW�n�_���B��͉�]�������Ɉړ��𑱂���
                if (!isTargetPosReached)
                {
                    RotateTowardsMovementDirection(targetPos, rotationSpeed);
                }
                MoveTowardsTarget(moveSpeed);
            }
            else
            {
                // �ڕW�n�_�ɓ��B��A�J�E���g�J�n����
                isTargetPosReached = true;
            }
            yield return new WaitForSeconds(0.1f);

            if (isTargetPosReached) time += 0.1f;
        }

        StartCoroutine(NextDecision(decisionTime));
    }

    /// <summary>
    /// �O�������~�����܂ł������ƈړ��������鏈��
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveGraduallyCoroutine()
    {
        Vector3 targetPos = GetNextTargetPosition();
        while (true)
        {
            RotateTowardsMovementDirection(targetPos, rotationSpeed / 2);
            MoveTowardsTarget(moveSpeed / 10);
            float disToTargetPos = Mathf.Abs(Vector3.Distance(targetPos, this.transform.position));
            if (disToTargetPos <= disToTargetPosMin / 2)
            {
                targetPos = GetNextTargetPosition();
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
            if (player)
            {
                alivePlayers.Add(player);
            }
        }

        if (alivePlayers.Count > 0) target = alivePlayers[Random.Range(0, alivePlayers.Count)];

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
    /// ���̖ڕW�n�_���擾����
    /// </summary>
    /// <returns></returns>
    Vector2 GetNextTargetPosition()
    {
        Vector2 targetPos = stageCenter;
        if (target && SetRandomTargetPlayer())
        {
            targetPos = target.transform.position;
        }
        else
        {
            for (int i = 0; i < 10; i++)
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
        return targetPos;
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
