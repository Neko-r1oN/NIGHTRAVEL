using UnityEngine;

abstract public class Enemy_abs : MonoBehaviour
{
    #region �X�e�[�^�X
    int hp;
    int power;
    int speed;
    float viewRadius;
    float attackRange;

    public int HP {  get { return hp; } set { hp = value; } }
    public int Power { get { return power; } set { power = value; } }
    public int Speed { get { return speed; } set { speed = value; } }
    public float ViewRadius { get { return viewRadius; } set { viewRadius = value; } }
    public float AttackRange { get { return attackRange; } set { attackRange = value; } }
    #endregion

    #region �R���|�[�l���g
    Animator animator;
    public Animator AnimatorSelf { get { return animator; } set { animator = value; } }
    #endregion

    /// <summary>
    /// �����]��
    /// </summary>
    public void Flip()
    {
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    /// <summary>
    /// �A�C�h������
    /// </summary>
    abstract public void Idle();

    /// <summary>
    /// �U������
    /// </summary>
    abstract public void Attack();

    /// <summary>
    /// ���鏈��
    /// </summary>
    abstract public void Run();

    /// <summary>
    /// �_���[�W�K������
    /// </summary>
    /// <param name="damage"></param>
    abstract public void ApplyDamage(int damage);

    /// <summary>
    /// �A�j���[�V�����ݒ菈��
    /// </summary>
    /// <param name="id"></param>
    public void SetAnimId(int id)
    {
        animator.SetInteger("animation_id", id);
    }
}
