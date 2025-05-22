//**************************************************
//  [�e]�G�l�~�[�̃R���g���[���[�N���X
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class EnemyController : MonoBehaviour
{
    //  �}�l�[�W���[�N���X����Player���擾�ł���̂����z(�ϐ��폜�\��A�܂���SerializeField�폜�\��)
    #region �v���C���[�E�^�[�Q�b�g
    [Header("�v���C���[�E�^�[�Q�b�g")]
    protected GameObject target;
    [SerializeField] List<GameObject> players = new List<GameObject>();
    public List<GameObject> Players { get { return players; } set { players = value; } }
    #endregion

    #region �R���|�[�l���g
    [Header("�R���|�[�l���g")]
    [SerializeField] protected EnemySightChecker sightChecker;
    [SerializeField] protected EnemyChaseAI chaseAI;
    protected Rigidbody2D m_rb2d;
    Animator animator;
    #endregion

    #region �X�e�[�^�X
    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    [Range(0, 100)]
    protected int hp = 10;

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    [Range(0, 20)] 
    protected int power = 2;

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    [Range(0, 20)] 
    protected int speed = 5;

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    [Range(0, 30)]
    protected float jumpPower = 19;

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    [Range(0, 10)]
    protected int bulletNum = 3;

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    [Range(0, 5)]
    [Tooltip("�e�̔��ˊԊu")]
    protected float shotsPerSecond = 0.5f;

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    [Range(0, 10)]
    [Tooltip("�U���̃N�[���^�C��")]
    protected float attackCoolTime = 0.5f;

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    [Range(0, 10)]
    [Tooltip("�U�����J�n���鋗��")]
    protected float attackDist = 1.5f;

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    [Range(0, 20)]
    [Tooltip("�ǐՉ\�͈�")]
    protected float trackingRange = 12f;

    [Foldout("��{�X�e�[�^�X")]
    [SerializeField]
    [Range(0, 5)]
    float hitTime = 0.5f;
    #endregion

    #region ���ʂ̍s���p�^�[��
    [Header("���ʂ̍s���p�^�[��")]
    [SerializeField] protected bool canDamageOnContact; // �ڐG�Ń_���[�W��^���邱�Ƃ��\
    [SerializeField] protected bool canPatrol;          // ��ɓ�����邱�Ƃ��\
    [SerializeField] protected bool canChaseTarget;     // �^�[�Q�b�g��ǐՉ\
    [SerializeField] protected bool canAttack;          // �U���\
    [SerializeField] protected bool canJump;            // �W�����v�\
    #endregion

    #region ��ԊǗ�
    protected bool isObstacle;
    protected bool isPlat;
    protected bool isInvincible;
    #endregion

    #region �A�j���[�V����ID
    protected int hitAnimationId;
    #endregion

    protected virtual void Start()
    {
        m_rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
                SetAnimId(hitAnimationId);
                StartCoroutine(HitTime());
            }
            collision.gameObject.GetComponent<CharacterController2D>().ApplyDamage(2f, transform.position);
        }
    }

    /// <summary>
    /// �_���[�W�K������
    /// </summary>
    /// <param name="damage"></param>
    abstract public void ApplyDamage(int damage, Transform attacker);

    /// <summary>
    /// �A�j���[�V�����ݒ菈��
    /// </summary>
    /// <param name="id"></param>
    public void SetAnimId(int id)
    {
        //animator.SetInteger("animation_id", id);
    }

    /// <summary>
    /// �A�j���[�V����ID�擾����
    /// </summary>
    /// <returns></returns>
    public int GetAnimId()
    {
        //return animator.GetInteger("animation_id");
        return 1;
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
    protected void DoKnokBack(float damage)
    {
        float direction = damage / Mathf.Abs(damage);
        transform.gameObject.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, 0);
        transform.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(direction * 200f, 100f));
    }

    /// <summary>
    /// �_���[�W�K�����̖��G����
    /// </summary>
    /// <returns></returns>
    protected IEnumerator HitTime()
    {
        isInvincible = true;
        yield return new WaitForSeconds(hitTime);
        isInvincible = false;
    }

    private void OnDrawGizmos()
    {
        sightChecker.DrawSightLine(players, target, canChaseTarget);
    }
}
