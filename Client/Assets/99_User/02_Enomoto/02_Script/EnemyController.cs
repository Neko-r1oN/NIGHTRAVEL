//**************************************************
//  [�e]�G�l�~�[�̃R���g���[���[�N���X
//  Author:r-enomoto
//**************************************************
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

abstract public class EnemyController : MonoBehaviour
{
    #region �X�e�[�^�X
    [Header("��{�X�e�[�^�X")]
    [SerializeField] protected int hp = 10;
    [SerializeField] protected int power = 2;
    [SerializeField] protected int speed = 5;

    public int HP { get { return hp; } set { hp = value; } }
    public int Power { get { return power; } set { power = value; } }
    public int Speed { get { return speed; } set { speed = value; } }
    #endregion

    #region �s���p�^�[��
    [Header("�s���p�^�[��")]
    [SerializeField] protected bool isPatrolling;
    [SerializeField] protected bool isChasingTarget;
    [SerializeField] protected bool isAttacking;
    #endregion

    #region ����ݒ�
    [Header("����")]
    [SerializeField] protected LayerMask targetLayerMask; // ���F����Layer
    [SerializeField] protected float viewAngleMax = 45;
    [SerializeField] protected float viewDistMax = 6f;
    [SerializeField] protected float trackingRange = 12f;

    public LayerMask TargetLayerMask { get { return targetLayerMask; } set { targetLayerMask = value; } }
    public float ViewAngleMax { get { return viewAngleMax; } set { viewAngleMax = value; } }
    public float ViewDistMax { get { return viewDistMax; } set { viewDistMax = value; } }
    public float TrackingRange { get { return trackingRange; } set { trackingRange = value; } }
    #endregion

    #region �R���|�[�l���g
    [Header("�R���|�[�l���g")]
    [SerializeField] Animator animator;
    #endregion

    #region ���̑�
    [Header("���̑�")]
    // �}�l�[�W���[�N���X����Player���擾�ł���̂����z(�폜�\��)
    [SerializeField] List<GameObject> players = new List<GameObject>();
    public List<GameObject> Players { get { return players; } set { players = value; } }
    [SerializeField] protected GameObject target;
    protected Rigidbody2D m_rb2d;

    protected bool isObstacle;
    protected bool isPlat;
    #endregion

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
    /// ���鏈��
    /// </summary>
    protected virtual void Run()
    {
        Vector2 speedVec = Vector2.zero;
        if (isChasingTarget && target)
        {
            float distToPlayer = target.transform.position.x - this.transform.position.x;
            speedVec = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * speed, m_rb2d.linearVelocity.y);
        }
        else if (isPatrolling)
        {
            if(!isPlat || isObstacle) Flip();
            speedVec = new Vector2(transform.localScale.x * speed, m_rb2d.linearVelocity.y);
        }

        m_rb2d.linearVelocity = speedVec;
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
        animator.SetInteger("animation_id", id);
    }

    /// <summary>
    /// �A�j���[�V����ID�擾����
    /// </summary>
    /// <returns></returns>
    public int GetAnimId()
    {
        return animator.GetInteger("animation_id");
    }

    /// <summary>
    /// ����͈͓��̃v���C���[���擾����
    /// </summary>
    /// <returns></returns>
    protected GameObject GetTargetInSight()
    {
        GameObject target = null;
        float minTargetDist = float.MaxValue;

        foreach (GameObject player in players)
        {
            Vector2 dirToTarget = player.transform.position - transform.position;
            Vector2 angleVec = new Vector2(transform.localScale.x, 0);
            float angle = Vector2.Angle(dirToTarget, angleVec);
            RaycastHit2D hit2D = Physics2D.Raycast(transform.position, dirToTarget, viewDistMax, targetLayerMask);

            if (angle <= viewAngleMax && hit2D && hit2D.collider.gameObject.CompareTag("Player"))
            {
                float distTotarget = Vector3.Distance(this.transform.position, player.transform.position);
                if (distTotarget < minTargetDist)
                {
                    minTargetDist = distTotarget;
                    target = player;
                }
            }
        }

        return target;
    }

    /// <summary>
    /// �^�[�Q�b�g�Ƃ̊Ԃɏ�Q�������邩�ǂ���
    /// </summary>
    /// <returns></returns>
    protected bool IsObstructed(GameObject target)
    {
        Vector2 dirToTarget = target.transform.position - transform.position;
        float dist = dirToTarget.magnitude;
        RaycastHit2D hit2D = Physics2D.Raycast(transform.position, dirToTarget, dist, targetLayerMask);

        return hit2D && !hit2D.collider.gameObject.CompareTag("Player");
    }
}
