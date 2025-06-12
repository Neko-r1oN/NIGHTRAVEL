//**************************************************
//  �G�l�~�[�̒��ۃN���X
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class EnemyBase : CharacterBase
{
    //  �}�l�[�W���[�N���X����Player���擾�ł���̂����z(�ϐ��폜�\��A�܂���SerializeField�폜�\��)
    #region �v���C���[�E�^�[�Q�b�g
    [Header("�v���C���[�E�^�[�Q�b�g")]
    protected GameObject target;   // SerializeField��Debug�p
    public GameObject Target { get { return target; } set { target = value; } }

    [SerializeField] List<GameObject> players = new List<GameObject>();
    public List<GameObject> Players { get { return players; } set { players = value; } }
    #endregion

    #region �R���|�[�l���g
    protected EnemyProjectileChecker projectileChecker;
    protected EnemySightChecker sightChecker;
    protected EnemyChaseAI chaseAI;
    protected Rigidbody2D m_rb2d;
    protected Coroutine attackCoroutine;
    Animator animator;
    #endregion

    #region �`�F�b�N����
    protected LayerMask terrainLayerMask; // �ǂƒn�ʂ̃��C���[
    #endregion

    #region �X�e�[�^�X
    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected int bulletNum = 3;

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    [Tooltip("�e�̔��ˊԊu")]
    protected float shotsPerSecond = 0.5f;

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    [Tooltip("�U���̃N�[���^�C��")]
    protected float attackCoolTime = 0.5f;

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    [Tooltip("�U�����J�n���鋗��")]
    protected float attackDist = 1.5f;

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    [Tooltip("�ǐՉ\�͈�")]
    protected float trackingRange = 20f;
    
    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected float hitTime = 0.5f;

    [Foldout("�I�v�V����")]
    [SerializeField]
    protected bool isBoss = false;

    [Foldout("�I�v�V����")]
    [SerializeField]
    protected bool isElite = false;
    #endregion

    #region �X�e�[�^�X�O���Q�Ɨp�v���p�e�B

    public float AttackDist { get { return attackDist; } }

    public bool IsBoss { get { return isBoss; } set { isBoss = value; } }

    public bool IsElite { get { return isElite; } set { isElite = value; } }
    #endregion

    #region �I�v�V����
    [Foldout("�I�v�V����")]
    [Tooltip("�ڐG�Ń_���[�W��^���邱�Ƃ��\")]
    [SerializeField] 
    protected bool canDamageOnContact;

    [Foldout("�I�v�V����")]
    [Tooltip("��ɓ�����邱�Ƃ��\")]
    [SerializeField] 
    protected bool canPatrol;

    [Foldout("�I�v�V����")]
    [Tooltip("�^�[�Q�b�g��ǐՉ\")]
    [SerializeField] 
    protected bool canChaseTarget;
    
    [Foldout("�I�v�V����")]
    [Tooltip("�U���\")]
    [SerializeField]
    protected bool canAttack;
    
    [Foldout("�I�v�V����")]
    [Tooltip("�W�����v�\")]
    [SerializeField] 
    protected bool canJump;

    [Foldout("�I�v�V����")]
    [Tooltip("�U�����Ƀq�b�g�����ꍇ�A�U�����L�����Z���\")]
    [SerializeField]
    protected bool canCancelAttackOnHit = true;
    #endregion

    #region ��ԊǗ�
    protected bool isStun;
    protected bool isInvincible;
    protected bool doOnceDecision;
    protected bool isAttacking;
    protected bool isDead;
    protected bool isPatrolPaused;
    #endregion

    #region �^�[�Q�b�g�Ƃ̋���
    protected float disToTarget;
    protected float disToTargetX;
    #endregion

    #region ���̑�
    [Foldout("���̑�")]
    [Tooltip("�����`�悷�邩�ǂ���")]
    [SerializeField]
    protected bool canDrawRay = false;

    [Foldout("���̑�")]
    [SerializeField]
    protected int exp = 100;
    #endregion

    protected override void Start()
    {
        terrainLayerMask = LayerMask.GetMask("Default");
        m_rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        projectileChecker = GetComponent<EnemyProjectileChecker>();
        sightChecker = GetComponent<EnemySightChecker>();
        chaseAI = GetComponent<EnemyChaseAI>();
    }

    private void FixedUpdate()
    {
        if (isStun || isAttacking || isInvincible || hp <= 0 || !doOnceDecision || !sightChecker) return;

        if (!target && Players.Count > 0)
        {
            // �^�[�Q�b�g��T��
            target = sightChecker.GetTargetInSight();
        }
        else if (canChaseTarget && target && disToTarget > trackingRange || !canChaseTarget && target && !sightChecker.IsTargetVisible())
        {// �ǐՔ͈͊Oor�ǐՂ��Ȃ��ꍇ�͎������Ղ�ƃ^�[�Q�b�g��������
            target = null;
            if (chaseAI) chaseAI.StopChase();
        }

        if (!target)
        {
            if (canPatrol && !isPatrolPaused) Patorol();
            else Idle();
            return;
        }

        // �^�[�Q�b�g�Ƃ̋������擾����
        disToTarget = Vector3.Distance(target.transform.position, this.transform.position);
        disToTargetX = target.transform.position.x - transform.position.x;

        // �^�[�Q�b�g�̂�������Ƀe�N�X�`���𔽓]
        if (canChaseTarget)
        {
            if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
                || target.transform.position.x > transform.position.x && transform.localScale.x < 0) Flip();
        }

        DecideBehavior();
    }

    /// <summary>
    /// �G��Ă����v���C���[�Ƀ_���[�W��K��������
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (canDamageOnContact && collision.gameObject.tag == "Player" && hp > 0 && !isInvincible)
        {
            if (!target)
            {
                // �^�[�Q�b�g��ݒ肵�A�^�[�Q�b�g�̕���������
                target = collision.gameObject;
                if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
                    || target.transform.position.x > transform.position.x && transform.localScale.x < 0)
                {
                    Flip();
                }
                StartCoroutine(HitTime());
            }
            collision.gameObject.GetComponent<Player>().ApplyDamage(2, transform.position);
        }
    }

    /// <summary>
    /// �s���p�^�[�����s����
    /// </summary>
    abstract protected void DecideBehavior();

    /// <summary>
    /// �A�C�h������
    /// </summary>
    abstract protected void Idle();

    /// <summary>
    /// �ǐՂ��鏈��
    /// </summary>
    abstract protected void Tracking();

    /// <summary>
    /// ���񂷂鏈��
    /// </summary>
    protected virtual void Patorol() { }

    /// <summary>
    /// �_���[�W�K������
    /// </summary>
    /// <param name="damage"></param>
    public void ApplyDamage(int damage, Transform attacker = null)
    {
        if (!isInvincible)
        {
            hp -= Mathf.Abs(damage);

            // �A�^�b�J�[����������Ƀe�N�X�`���𔽓]�����A�m�b�N�o�b�N��������
            if (attacker)
            {
                if (attacker.position.x < transform.position.x && transform.localScale.x > 0
                || attacker.position.x > transform.position.x && transform.localScale.x < 0) Flip();
                DoKnokBack(damage);
            }

            if (hp > 0)
            {
                StartCoroutine(HitTime());
            }
            else if (!isDead)
            {
                Player player = attacker ? attacker.gameObject.GetComponent<Player>() : null;
                StartCoroutine(DestroyEnemy(player));
            }
        }
    }

    /// <summary>
    /// ���̌��m�͈͂̕`�揈��
    /// </summary>
    abstract protected void DrawDetectionGizmos();

    /// <summary>
    /// ���S�A�j���[�V�������Đ�
    /// </summary>
    abstract protected void PlayDeadAnim();

    /// <summary>
    /// �q�b�g�A�j���[�V�������Đ�
    /// </summary>
    abstract protected void PlayHitAnim();

    /// <summary>
    /// �_���[�W���󂯂��Ƃ��̏���
    /// </summary>
    virtual protected void OnHit()
    {
        PlayHitAnim();
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine); // �U�������𒆒f����
        }
        isAttacking = false;
        doOnceDecision = true;
    }

    /// <summary>
    /// �A�j���[�V�����ݒ菈��
    /// </summary>
    /// <param name="id"></param>
    public void SetAnimId(int id)
    {
        if (animator != null) animator.SetInteger("animation_id", id);
    }

    /// <summary>
    /// �A�j���[�V����ID�擾����
    /// </summary>
    /// <returns></returns>
    public int GetAnimId()
    {
        return animator != null ? animator.GetInteger("animation_id") : 0;
    }

    /// <summary>
    /// ��ԋ߂��v���C���[���^�[�Q�b�g�ɐݒ肷��
    /// </summary>
    public void SetNearTarget()
    {
        GameObject target = null;
        float dist = float.MaxValue;
        foreach (GameObject player in Players)
        {
            if (player != null)
            {
                float distToPlayer = Vector2.Distance(transform.position, player.transform.position);
                if (Mathf.Abs(distToPlayer) < dist)
                {
                    target = player;
                    dist = distToPlayer;
                }
            }
        }

        if (target != null)
        {
            this.target = target;
        }
    }

    /// <summary>
    /// ���̏�ɑҋ@���鏈��
    /// </summary>
    /// <param name="waitingTime"></param>
    /// <returns></returns>
    protected IEnumerator Waiting(float waitingTime)
    {
        doOnceDecision = false;
        Idle();
        yield return new WaitForSeconds(waitingTime);
        doOnceDecision = true;
    }

    /// <summary>
    /// ���S����
    /// </summary>
    /// <returns></returns>
    protected IEnumerator DestroyEnemy(Player player)
    {
        if (!isDead)
        {
            isDead = true;
            if(player) player.GetExp(exp);
            PlayDeadAnim();
            if (GameManager.Instance) GameManager.Instance.CrushEnemy(this);
            yield return new WaitForSeconds(0.25f);
            m_rb2d.excludeLayers = LayerMask.GetMask("TransparentFX") | LayerMask.GetMask("Player"); ;  // �v���C���[�Ƃ̔��������
            m_rb2d.linearVelocity = new Vector2(0, m_rb2d.linearVelocity.y);
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// �����]��
    /// </summary>
    protected void Flip()
    {
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    /// <summary>
    /// �m�b�N�o�b�N����
    /// </summary>
    /// <param name="damage"></param>
    protected void DoKnokBack(int damage)
    {
        int direction = damage / Mathf.Abs(damage);
        transform.gameObject.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, 0);
        transform.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(direction * 200f, 100f));
    }

    /// <summary>
    /// �X�^������
    /// </summary>
    /// <param name="time"></param>
    public void ApplyStun(float time)
    {
        if (!isStun)
        {
            StartCoroutine(StunTime(time));
        }
    }

    /// <summary>
    /// �_���[�W�K�����̖��G����
    /// </summary>
    /// <returns></returns>
    protected IEnumerator HitTime()
    {
        if (canCancelAttackOnHit)
        {
            isInvincible = true;
            OnHit();
            yield return new WaitForSeconds(hitTime);
            isInvincible = false;
        }
    }

    /// <summary>
    /// ��莞�ԃX�^�������鏈��
    /// </summary>
    /// <returns></returns>
    IEnumerator StunTime(float stunTime)
    {
        isStun = true;
        OnHit();
        yield return new WaitForSeconds(stunTime);
        isStun = false;
    }

    /// <summary>
    /// [ �f�o�b�N�p ] Gizmos���g�p���Č��o�͈͂�`��
    /// </summary>
    private void OnDrawGizmos()
    {   
        if (!canDrawRay) return;

        // �ǐՔ͈�
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, trackingRange);

        // �����`��
        if(sightChecker != null) 
        { 
            sightChecker.DrawSightLine(canChaseTarget);
        }

        DrawDetectionGizmos();
    }
}