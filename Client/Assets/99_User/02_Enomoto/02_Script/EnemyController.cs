using System.Collections;
using UnityEngine;

abstract public class EnemyController : MonoBehaviour
{
    #region ステータス
    int _hp;
    int _power;
    int _speed;

    public int HP {  get { return _hp; } set { _hp = value; } }
    public int Power { get { return _power; } set { _power = value; } }
    public int Speed { get { return _speed; } set { _speed = value; } }
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
    abstract public void ApplyDamage(int damage, Transform attacker);

    /// <summary>
    /// アニメーション設定処理
    /// </summary>
    /// <param name="id"></param>
    public void SetAnimId(int id)
    {
        animator.SetInteger("animation_id", id);
    }

    /// <summary>
    /// アニメーションID取得処理
    /// </summary>
    /// <returns></returns>
    public int GetAnimId()
    {
        return animator.GetInteger("animation_id");
    }
}
