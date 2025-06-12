//**************************************************
//  エネミーの抽象クラス
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class EnemyBase : CharacterBase
{
    //  マネージャークラスからPlayerを取得できるのが理想(変数削除予定、またはSerializeField削除予定)
    #region プレイヤー・ターゲット
    [Header("プレイヤー・ターゲット")]
    protected GameObject target;   // SerializeFieldはDebug用
    public GameObject Target { get { return target; } set { target = value; } }

    [SerializeField] List<GameObject> players = new List<GameObject>();
    public List<GameObject> Players { get { return players; } set { players = value; } }
    #endregion

    #region コンポーネント
    protected EnemyProjectileChecker projectileChecker;
    protected EnemySightChecker sightChecker;
    protected EnemyChaseAI chaseAI;
    protected Rigidbody2D m_rb2d;
    protected Coroutine attackCoroutine;
    Animator animator;
    #endregion

    #region チェック判定
    protected LayerMask terrainLayerMask; // 壁と地面のレイヤー
    #endregion

    #region ステータス
    [Foldout("ステータス")]
    [SerializeField]
    protected int bulletNum = 3;

    [Foldout("ステータス")]
    [SerializeField]
    [Tooltip("弾の発射間隔")]
    protected float shotsPerSecond = 0.5f;

    [Foldout("ステータス")]
    [SerializeField]
    [Tooltip("攻撃のクールタイム")]
    protected float attackCoolTime = 0.5f;

    [Foldout("ステータス")]
    [SerializeField]
    [Tooltip("攻撃を開始する距離")]
    protected float attackDist = 1.5f;

    [Foldout("ステータス")]
    [SerializeField]
    [Tooltip("追跡可能範囲")]
    protected float trackingRange = 20f;
    
    [Foldout("ステータス")]
    [SerializeField]
    protected float hitTime = 0.5f;

    [Foldout("オプション")]
    [SerializeField]
    protected bool isBoss = false;

    [Foldout("オプション")]
    [SerializeField]
    protected bool isElite = false;
    #endregion

    #region ステータス外部参照用プロパティ

    public float AttackDist { get { return attackDist; } }

    public bool IsBoss { get { return isBoss; } set { isBoss = value; } }

    public bool IsElite { get { return isElite; } set { isElite = value; } }
    #endregion

    #region オプション
    [Foldout("オプション")]
    [Tooltip("接触でダメージを与えることが可能")]
    [SerializeField] 
    protected bool canDamageOnContact;

    [Foldout("オプション")]
    [Tooltip("常に動き回ることが可能")]
    [SerializeField] 
    protected bool canPatrol;

    [Foldout("オプション")]
    [Tooltip("ターゲットを追跡可能")]
    [SerializeField] 
    protected bool canChaseTarget;
    
    [Foldout("オプション")]
    [Tooltip("攻撃可能")]
    [SerializeField]
    protected bool canAttack;
    
    [Foldout("オプション")]
    [Tooltip("ジャンプ可能")]
    [SerializeField] 
    protected bool canJump;

    [Foldout("オプション")]
    [Tooltip("攻撃中にヒットした場合、攻撃をキャンセル可能")]
    [SerializeField]
    protected bool canCancelAttackOnHit = true;
    #endregion

    #region 状態管理
    protected bool isStun;
    protected bool isInvincible;
    protected bool doOnceDecision;
    protected bool isAttacking;
    protected bool isDead;
    protected bool isPatrolPaused;
    #endregion

    #region ターゲットとの距離
    protected float disToTarget;
    protected float disToTargetX;
    #endregion

    #region その他
    [Foldout("その他")]
    [Tooltip("判定を描画するかどうか")]
    [SerializeField]
    protected bool canDrawRay = false;

    [Foldout("その他")]
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
            // ターゲットを探す
            target = sightChecker.GetTargetInSight();
        }
        else if (canChaseTarget && target && disToTarget > trackingRange || !canChaseTarget && target && !sightChecker.IsTargetVisible())
        {// 追跡範囲外or追跡しない場合は視線が遮るとターゲットを見失う
            target = null;
            if (chaseAI) chaseAI.StopChase();
        }

        if (!target)
        {
            if (canPatrol && !isPatrolPaused) Patorol();
            else Idle();
            return;
        }

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
    /// 行動パターン実行処理
    /// </summary>
    abstract protected void DecideBehavior();

    /// <summary>
    /// アイドル処理
    /// </summary>
    abstract protected void Idle();

    /// <summary>
    /// 追跡する処理
    /// </summary>
    abstract protected void Tracking();

    /// <summary>
    /// 巡回する処理
    /// </summary>
    protected virtual void Patorol() { }

    /// <summary>
    /// ダメージ適応処理
    /// </summary>
    /// <param name="damage"></param>
    public void ApplyDamage(int damage, Transform attacker = null)
    {
        if (!isInvincible)
        {
            hp -= Mathf.Abs(damage);

            // アタッカーが居る方向にテクスチャを反転させ、ノックバックをさせる
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
    /// 他の検知範囲の描画処理
    /// </summary>
    abstract protected void DrawDetectionGizmos();

    /// <summary>
    /// 死亡アニメーションを再生
    /// </summary>
    abstract protected void PlayDeadAnim();

    /// <summary>
    /// ヒットアニメーションを再生
    /// </summary>
    abstract protected void PlayHitAnim();

    /// <summary>
    /// ダメージを受けたときの処理
    /// </summary>
    virtual protected void OnHit()
    {
        PlayHitAnim();
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine); // 攻撃処理を中断する
        }
        isAttacking = false;
        doOnceDecision = true;
    }

    /// <summary>
    /// アニメーション設定処理
    /// </summary>
    /// <param name="id"></param>
    public void SetAnimId(int id)
    {
        if (animator != null) animator.SetInteger("animation_id", id);
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
    /// 一番近いプレイヤーをターゲットに設定する
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
    /// その場に待機する処理
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
    /// 死亡処理
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
            m_rb2d.excludeLayers = LayerMask.GetMask("TransparentFX") | LayerMask.GetMask("Player"); ;  // プレイヤーとの判定を消す
            m_rb2d.linearVelocity = new Vector2(0, m_rb2d.linearVelocity.y);
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }
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
    /// スタン処理
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
    /// ダメージ適応時の無敵時間
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
    /// 一定時間スタンさせる処理
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
    /// [ デバック用 ] Gizmosを使用して検出範囲を描画
    /// </summary>
    private void OnDrawGizmos()
    {   
        if (!canDrawRay) return;

        // 追跡範囲
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, trackingRange);

        // 視線描画
        if(sightChecker != null) 
        { 
            sightChecker.DrawSightLine(canChaseTarget);
        }

        DrawDetectionGizmos();
    }
}