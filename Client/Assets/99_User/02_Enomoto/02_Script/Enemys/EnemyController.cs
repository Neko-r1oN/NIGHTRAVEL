//**************************************************
//  [�e]�G�l�~�[�̃R���g���[���[�N���X
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

abstract public class EnemyController : MonoBehaviour
{
    //  �}�l�[�W���[�N���X����Player���擾�ł���̂����z(�ϐ��폜�\��A�܂���SerializeField�폜�\��)
    #region �v���C���[�E�^�[�Q�b�g
    [Header("�v���C���[�E�^�[�Q�b�g")]
    [SerializeField] protected GameObject target;   // SerializeField��Debug�p
    public GameObject Target { get { return target; } set { target = value; } }

    [SerializeField] List<GameObject> players = new List<GameObject>();
    public List<GameObject> Players { get { return players; } set { players = value; } }
    #endregion

    #region �R���|�[�l���g
    protected EnemySightChecker sightChecker;
    protected EnemyChaseAI chaseAI;
    protected Rigidbody2D m_rb2d;
    Animator animator;
    #endregion

    #region �X�e�[�^�X
    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    protected bool isBoss = false;
    public bool IsBoss { get { return isBoss; } set { isBoss = value; } }

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    protected int life = 10;

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    protected int power = 2;

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    protected int speed = 5;

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    protected float jumpPower = 19;

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    protected int bulletNum = 3;

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    [Tooltip("�e�̔��ˊԊu")]
    protected float shotsPerSecond = 0.5f;

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    [Tooltip("�U���̃N�[���^�C��")]
    protected float attackCoolTime = 0.5f;

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    [Tooltip("�U�����J�n���鋗��")]
    protected float attackDist = 1.5f;
    public float AttackDist { get { return attackDist; } }

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    [Tooltip("�ǐՉ\�͈�")]
    protected float trackingRange = 12f;
    
    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    protected float hitTime = 0.5f;
    #endregion

    #region ���ʂ̍s���p�^�[��
    [Foldout("���ʂ̍s���p�^�[��")]
    [Tooltip("�ڐG�Ń_���[�W��^���邱�Ƃ��\")]
    [SerializeField] 
    protected bool canDamageOnContact;

    [Foldout("���ʂ̍s���p�^�[��")]
    [Tooltip("��ɓ�����邱�Ƃ��\")]
    [SerializeField] 
    protected bool canPatrol;

    [Foldout("���ʂ̍s���p�^�[��")]
    [Tooltip("�^�[�Q�b�g��ǐՉ\")]
    [SerializeField] 
    protected bool canChaseTarget;
    
    [Foldout("���ʂ̍s���p�^�[��")]
    [Tooltip("�U���\")]
    [SerializeField]
    protected bool canAttack;
    
    [Foldout("���ʂ̍s���p�^�[��")]
    [Tooltip("�W�����v�\")]
    [SerializeField] 
    protected bool canJump;
    #endregion

    #region ��ԊǗ�
    protected bool isInvincible;
    protected bool doOnceDecision;
    protected bool isAttacking;
    #endregion

    #region �^�[�Q�b�g�Ƃ̋���
    protected float disToTarget;
    protected float disToTargetX;
    #endregion

    #region ���̑�
    [Foldout("�f�o�b�N�p")]
    [Tooltip("�����`�悷�邩�ǂ���")]
    [SerializeField]
    protected bool canDrawRay = false;
    #endregion

    protected virtual void Start()
    {
        m_rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sightChecker = GetComponent<EnemySightChecker>();
        chaseAI = GetComponent<EnemyChaseAI>();
    }

    private void FixedUpdate()
    {
        if (isAttacking || isInvincible || life <= 0 || !doOnceDecision || !sightChecker) return;

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
            if (canPatrol) Run();
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
        if (canDamageOnContact && collision.gameObject.tag == "Player" && life > 0 && !isInvincible)
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
    /// ���̍s���p�^�[�������߂鏈��
    /// </summary>
    abstract protected void DecideBehavior();

    /// <summary>
    /// �A�C�h������
    /// </summary>
    abstract protected void Idle();

    /// <summary>
    /// ���鏈��
    /// </summary>
    abstract protected void Run();

    /// <summary>
    /// �_���[�W�K������
    /// </summary>
    /// <param name="damage"></param>
    abstract public void ApplyDamage(int damage, Transform attacker);

    /// <summary>
    /// ���̌��m�͈͂̕`�揈��
    /// </summary>
    abstract protected void DrawDetectionGizmos();

    /// <summary>
    /// ���S����
    /// </summary>
    /// <returns></returns>
    protected IEnumerator DestroyEnemy()
    {
        PlayDeadAnim();
        yield return new WaitForSeconds(0.25f);
        GetComponent<CapsuleCollider2D>().direction = CapsuleDirection2D.Horizontal;
        m_rb2d.linearVelocity = new Vector2(0, m_rb2d.linearVelocity.y);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    /// <summary>
    /// ���S�A�j���[�V�������Đ�
    /// </summary>
    abstract protected void PlayDeadAnim();

    /// <summary>
    /// �A�j���[�V�����ݒ菈��
    /// </summary>
    /// <param name="id"></param>
    public void SetAnimId(int id)
    {
        if(animator != null) animator.SetInteger("animation_id", id);
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
    /// �_���[�W�K�����̖��G����
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator HitTime()
    {
        isInvincible = true;
        yield return new WaitForSeconds(hitTime);
        isInvincible = false;
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