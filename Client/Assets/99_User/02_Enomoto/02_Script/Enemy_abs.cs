using UnityEngine;

abstract public class Enemy_abs : MonoBehaviour
{
    #region ステータス
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

    #region コンポーネント
    Animator animator;
    public Animator AnimatorSelf { get { return animator; } set { animator = value; } }
    #endregion

    /// <summary>
    /// 方向転換
    /// </summary>
    public void Flip()
    {
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    /// <summary>
    /// アイドル処理
    /// </summary>
    abstract public void Idle();

    /// <summary>
    /// 攻撃処理
    /// </summary>
    abstract public void Attack();

    /// <summary>
    /// 走る処理
    /// </summary>
    abstract public void Run();

    /// <summary>
    /// ダメージ適応処理
    /// </summary>
    /// <param name="damage"></param>
    abstract public void ApplyDamage(int damage);

    /// <summary>
    /// アニメーション設定処理
    /// </summary>
    /// <param name="id"></param>
    public void SetAnimId(int id)
    {
        animator.SetInteger("animation_id", id);
    }
}
