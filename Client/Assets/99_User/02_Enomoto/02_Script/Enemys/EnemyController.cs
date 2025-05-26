//**************************************************
//  [親]エネミーのコントローラークラス
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [Tooltip("接触でダメージを与えることが可能")]
    [SerializeField] 
    protected bool canDamageOnContact;

    [Foldout("共通の行動パターン")]
    [Tooltip("常に動き回ることが可能")]
    [SerializeField] 
    protected bool canPatrol;

    [Foldout("共通の行動パターン")]
    [Tooltip("ターゲットを追跡可能")]
    [SerializeField] 
    protected bool canChaseTarget;
    
    [Foldout("共通の行動パターン")]
    [Tooltip("攻撃可能")]
    [SerializeField]
    protected bool canAttack;
    
    [Foldout("共通の行動パターン")]
    [Tooltip("ジャンプ可能")]
    [SerializeField] 
    protected bool canJump;
    #endregion

    #region 状態管理
    protected bool isInvincible;
    protected bool doOnceDecision;
    protected bool isAttacking;
    #endregion

    #region ターゲットとの距離
    protected float disToTarget;
    protected float disToTargetX;
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
        // ターゲットを探す
        if (!target && Players.Count > 0) target = sightChecker.GetTargetInSight(Players);
        else if (canChaseTarget && target && disToTarget > trackingRange || !canChaseTarget && target && !sightChecker.IsTargetVisible(target))
        {// 追跡範囲外or追跡しない場合は視線が遮るとターゲットを見失う
            target = null;
            if(chaseAI) chaseAI.StopChase();
        }

        if (!target && canPatrol) Run();
        else if (!target && !canPatrol) Idle();

        if (!target || isAttacking || isInvincible || hp <= 0 || !doOnceDecision) return;

        // ターゲットとの距離を取得する
        disToTarget = Vector3.Distance(target.transform.position, this.transform.position);
        disToTargetX = target.transform.position.x - transform.position.x;

        // ターゲットのいる方向にテクスチャを反転
        if (canChaseTarget)
        {
            if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
                || target.transform.position.x > transform.position.x && transform.localScale.x < 0) Flip();
        }

        DecideBehavior();
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
                StartCoroutine(HitTime());
            }
            collision.gameObject.GetComponent<Player>().ApplyDamage(2, transform.position);
        }
    }

    /// <summary>
    /// 次の行動パターンを決める処理
    /// </summary>
    abstract protected void DecideBehavior();

    /// <summary>
    /// アイドル処理
    /// </summary>
    abstract protected void Idle();

    /// <summary>
    /// 走る処理
    /// </summary>
    abstract protected void Run();

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
    protected void DoKnokBack(int damage)
    {
        int direction = damage / Mathf.Abs(damage);
        transform.gameObject.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, 0);
        transform.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(direction * 200f, 100f));
    }

    /// <summary>
    /// ダメージ適応時の無敵時間
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator HitTime()
    {
        isInvincible = true;
        yield return new WaitForSeconds(hitTime);
        isInvincible = false;
    }

    /// <summary>
    /// [ デバック用 ] Gizmosを使用して検出範囲を描画
    /// </summary>
    protected virtual void OnDrawGizmos()
    {   
        // 追跡範囲
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, trackingRange);

        // 視線描画
        if(sightChecker != null) 
        { 
            sightChecker.DrawSightLine(players, target, canChaseTarget);
        }
    }
}
