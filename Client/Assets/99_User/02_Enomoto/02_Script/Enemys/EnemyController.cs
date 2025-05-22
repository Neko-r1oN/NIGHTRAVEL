//**************************************************
//  [親]エネミーのコントローラークラス
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class EnemyController : MonoBehaviour
{
    //  マネージャークラスからPlayerを取得できるのが理想(変数削除予定、またはSerializeField削除予定)
    #region プレイヤー・ターゲット
    [Header("プレイヤー・ターゲット")]
    protected GameObject target;
    [SerializeField] List<GameObject> players = new List<GameObject>();
    public List<GameObject> Players { get { return players; } set { players = value; } }
    #endregion

    #region コンポーネント
    protected EnemySightChecker sightChecker;
    protected EnemyChaseAI chaseAI;
    protected Rigidbody2D m_rb2d;
    Animator animator;
    #endregion

    #region ステータス
    [Foldout("基本ステータス")]
    [SerializeField]
    protected int hp = 10;

    [Foldout("基本ステータス")]
    [SerializeField]
    protected int power = 2;

    [Foldout("基本ステータス")]
    [SerializeField]
    protected int speed = 5;

    [Foldout("基本ステータス")]
    [SerializeField]
    protected float jumpPower = 19;

    [Foldout("基本ステータス")]
    [SerializeField]
    protected int bulletNum = 3;

    [Foldout("基本ステータス")]
    [SerializeField]
    [Tooltip("弾の発射間隔")]
    protected float shotsPerSecond = 0.5f;

    [Foldout("基本ステータス")]
    [SerializeField]
    [Tooltip("攻撃のクールタイム")]
    protected float attackCoolTime = 0.5f;

    [Foldout("基本ステータス")]
    [SerializeField]
    [Tooltip("攻撃を開始する距離")]
    protected float attackDist = 1.5f;

    [Foldout("基本ステータス")]
    [SerializeField]
    [Tooltip("追跡可能範囲")]
    protected float trackingRange = 12f;
    
    [Foldout("基本ステータス")]
    [SerializeField]
    protected float hitTime = 0.5f;
    #endregion

    #region 共通の行動パターン
    [Foldout("共通の行動パターン")]
    [SerializeField] 
    protected bool canDamageOnContact;      // 接触でダメージを与えることが可能

    [Foldout("共通の行動パターン")]
    [SerializeField] 
    protected bool canPatrol;               // 常に動き回ることが可能

    [Foldout("共通の行動パターン")]
    [SerializeField] 
    protected bool canChaseTarget;          // ターゲットを追跡可能
    
    [Foldout("共通の行動パターン")]
    [SerializeField]
    protected bool canAttack;               // 攻撃可能
    
    [Foldout("共通の行動パターン")]
    [SerializeField] 
    protected bool canJump;                 // ジャンプ可能
    #endregion

    #region 状態管理
    protected bool isObstacle;
    protected bool isPlat;
    protected bool isInvincible;
    #endregion

    #region アニメーションID
    protected int hitAnimationId;
    #endregion

    protected virtual void Start()
    {
        m_rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sightChecker = GetComponent<EnemySightChecker>();
        chaseAI = GetComponent<EnemyChaseAI>();
    }

    /// <summary>
    /// 触れてきたプレイヤーにダメージを適応させる
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (canDamageOnContact && collision.gameObject.tag == "Player" && hp > 0 && !isInvincible)
        {
            if (!target)
            {
                // ターゲットを設定し、ターゲットの方向を向く
                target = collision.gameObject;
                if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
                    || target.transform.position.x > transform.position.x && transform.localScale.x < 0)
                {
                    Flip();
                }
                SetAnimId(hitAnimationId);
                StartCoroutine(HitTime());
            }
            collision.gameObject.GetComponent<Player>().ApplyDamage(2f, transform.position);
        }
    }

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
        if(animator != null) animator.SetInteger("animation_id", id);
    }

    /// <summary>
    /// アニメーションID取得処理
    /// </summary>
    /// <returns></returns>
    public int GetAnimId()
    {
        return animator != null ? animator.GetInteger("animation_id") : 0;
    }

    /// <summary>
    /// 方向転換
    /// </summary>
    protected void Flip()
    {
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    /// <summary>
    /// ノックバック処理
    /// </summary>
    /// <param name="damage"></param>
    protected void DoKnokBack(float damage)
    {
        float direction = damage / Mathf.Abs(damage);
        transform.gameObject.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, 0);
        transform.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(direction * 200f, 100f));
    }

    /// <summary>
    /// ダメージ適応時の無敵時間
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
