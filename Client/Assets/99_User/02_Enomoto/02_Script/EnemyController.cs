using System.Collections;
using UnityEngine;

abstract public class EnemyController : MonoBehaviour
{
    #region �X�e�[�^�X
    int _hp;
    int _power;
    int _speed;

    public int HP {  get { return _hp; } set { _hp = value; } }
    public int Power { get { return _power; } set { _power = value; } }
    public int Speed { get { return _speed; } set { _speed = value; } }
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
}
