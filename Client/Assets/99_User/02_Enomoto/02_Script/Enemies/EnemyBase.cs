//**************************************************
//  エネミーの抽象クラス
//  Author:r-enomoto
//**************************************************
using Grpc.Core;
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    protected EnemyElite enemyElite;
    protected EnemySightChecker sightChecker;
    protected EnemyChaseAI chaseAI;
    protected Rigidbody2D m_rb2d;
    #endregion

    #region テクスチャ・アニメーション関連
    [Foldout("テクスチャ・アニメーション")]
    [SerializeField]
    List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
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
    protected List<Coroutine> cancellCoroutines = new List<Coroutine>();  // ヒット時にキャンセルするコルーチン
    protected bool isStun;
    protected bool isInvincible;
    protected bool doOnceDecision;
    protected bool isAttacking;
    protected bool isDead;
    protected bool isPatrolPaused;
    protected bool isSpawn = true; // スポーン中かどうか
    protected bool isStartComp;
    #endregion

    #region ターゲットとの距離
    protected float disToTarget;
    protected float disToTargetX;
    #endregion

    #region その他
    [Foldout("その他")]
    [Tooltip("生成されるときの地面からの距離")]
    [SerializeField]
    float spawnGroundOffset;

    [Foldout("その他")]
    [Tooltip("判定を描画するかどうか")]
    [SerializeField]
    protected bool canDrawRay = false;

    [Foldout("その他")]
    [SerializeField]
    protected int exp = 100;
    #endregion

    #region 外部参照用プロパティ

    public float AttackDist { get { return attackDist; } }

    public bool IsBoss { get { return isBoss; } set { isBoss = value; } }

    public bool IsElite { get { return isElite; } }

    public float SpawnGroundOffset { get { return spawnGroundOffset; } }

    public List<SpriteRenderer> SpriteRenderers { get { return spriteRenderers; } }
    #endregion

    protected virtual void Start()
    {
        terrainLayerMask = LayerMask.GetMask("Default");
        m_rb2d = GetComponent<Rigidbody2D>();
        sightChecker = GetComponent<EnemySightChecker>();
        chaseAI = GetComponent<EnemyChaseAI>();
        enemyElite = GetComponent<EnemyElite>();
        isStartComp = true;
    }

    protected virtual void FixedUpdate()
    {
        if (isSpawn || isStun || isAttacking || isInvincible || hp <= 0 || !doOnceDecision || !sightChecker) return;

        if (Players.Count > 0 && !target || Players.Count > 0 && target.GetComponent<CharacterBase>().HP <= 0)
        {
            // 新しくターゲットを探す
            target = sightChecker.GetTargetInSight();
        }
        else if (canChaseTarget && target && disToTarget > trackingRange || !canChaseTarget && target && !sightChecker.IsTargetVisible())
        {// 追跡範囲外or追跡しない場合は視線が遮るとターゲットを見失う
            target = null;
            if (chaseAI) chaseAI.Stop();
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
            collision.gameObject.GetComponent<PlayerBase>().ApplyDamage(2, transform.position);
        }
    }

    /// <summary>
    /// 行動パターン実行処理
    /// </summary>
    abstract protected void DecideBehavior();

    /// <summary>
    /// アイドル処理
    /// </summary>
    protected virtual void Idle() { }

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
    /// エリート個体にする処理
    /// </summary>
    public void PromoteToElite(EnemyElite.ELITE_TYPE type)
    {
        if (!isElite)
        {
            isElite = true;
            if (!enemyElite) enemyElite = GetComponent<EnemyElite>();
            enemyElite.Init(type);
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
    /// 生存しているプレイヤーの取得処理
    /// </summary>
    /// <returns></returns>
    protected List<GameObject> GetAlivePlayers()
    {
        List<GameObject> alivePlayers = new List<GameObject>();
        foreach (GameObject player in Players)
        {
            if (player && player.GetComponent<CharacterBase>().HP > 0)
            {
                alivePlayers.Add(player);
            }
        }
        return alivePlayers;
    }

    /// <summary>
    /// 一番近いプレイヤーを取得する
    /// </summary>
    /// <returns></returns>
    public GameObject GetNearPlayer()
    {
        GameObject nearPlayer = null;
        float dist = float.MaxValue;
        foreach (GameObject player in GetAlivePlayers())
        {
            if (player != null)
            {
                float distToPlayer = Vector2.Distance(transform.position, player.transform.position);
                if (Mathf.Abs(distToPlayer) < dist)
                {
                    nearPlayer = player;
                    dist = distToPlayer;
                }
            }
        }
        return nearPlayer;
    }

    /// <summary>
    /// 一番近いプレイヤーをターゲットに設定する
    /// </summary>
    public void SetNearTarget()
    {
        GameObject target = GetNearPlayer();
        if (target != null)
        {
            this.target = target;
        }
    }

    #region 移動処理関連

    /// <summary>
    /// 追跡する処理
    /// </summary>
    protected virtual void Tracking() { }

    /// <summary>
    /// 巡回する処理
    /// </summary>
    protected virtual void Patorol() { }

    #endregion

    #region ヒット処理関連

    /// <summary>
    /// 死亡時に呼ばれる処理 (専用アニメーションなど)
    /// </summary>
    abstract protected void OnDead();

    /// <summary>
    /// ノックバック処理
    /// </summary>
    /// <param name="damage"></param>
    protected void DoKnokBack(int damage)
    {
        int direction = damage / Mathf.Abs(damage);
        transform.gameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        transform.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(direction * 200f, 100f));
    }

    /// <summary>
    /// ダメージを受けたときの処理
    /// </summary>
    protected virtual void OnHit()
    {
        isAttacking = false;
        doOnceDecision = true;
    }

    /// <summary>
    /// ダメージ適用処理
    /// </summary>
    /// <param name="damage"></param>
    public void ApplyDamage(int power, Transform attacker = null, params StatusEffectController.EFFECT_TYPE[] effectTypes)
    {
        if (isInvincible || isDead) return;

        var damage = CalculationLibrary.CalcDamage(power, Defense);
        var hitPoint = TransformUtils.GetHitPointToTarget(transform, attacker.position);
        if (hitPoint == null) hitPoint = transform.position;
        UIManager.Instance.PopDamageUI(damage, (Vector2)hitPoint, false);   // ダメージ表記
        hp -= Mathf.Abs(damage);

        // アタッカーが居る方向にテクスチャを反転させ、ノックバックをさせる
        if (attacker)
        {
            // ヒット時に攻撃処理などを停止する
            if (canCancelAttackOnHit)
            {
                foreach(Coroutine coroutine in cancellCoroutines)
                {
                    StopCoroutine(coroutine);
                }
            }

            // 状態異常を付与する
            if (effectTypes.Length > 0)
            {
                effectController.ApplyStatusEffect(effectTypes);
            }

            if (attacker.position.x < transform.position.x && transform.localScale.x > 0
            || attacker.position.x > transform.position.x && transform.localScale.x < 0) Flip();
            
            DoKnokBack(damage);

            if (hp > 0) StartCoroutine(HitTime());
        }

        if (hp <= 0)
        {
            // 全てのコルーチン(攻撃処理やスタン処理など)を停止する
            StopAllCoroutines();

            PlayerBase player = attacker ? attacker.gameObject.GetComponent<PlayerBase>() : null;
            StartCoroutine(DestroyEnemy(player));
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
    /// 死亡処理
    /// </summary>
    /// <returns></returns>
    protected IEnumerator DestroyEnemy(PlayerBase player)
    {
        if (!isDead)
        {
            isDead = true;
            if (player) player.GetExp(exp);
            OnDead();
            if (GameManager.Instance) GameManager.Instance.CrushEnemy(this);
            yield return new WaitForSeconds(0.25f);
            m_rb2d.excludeLayers = LayerMask.GetMask("BlinkPlayer") | LayerMask.GetMask("Player"); ;  // プレイヤーとの判定を消す
            m_rb2d.linearVelocity = new Vector2(0, m_rb2d.linearVelocity.y);
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }
    }

    #endregion

    #region スタン処理関連

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

    #endregion

    #region Debug描画処理関連

    /// <summary>
    /// 他の検知範囲の描画処理
    /// </summary>
    abstract protected void DrawDetectionGizmos();

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
        if (sightChecker != null)
        {
            sightChecker.DrawSightLine(canChaseTarget);
        }

        DrawDetectionGizmos();
    }

    #endregion

    #region テクスチャ・アニメーション関連

    /// <summary>
    /// 攻撃アニメーションのイベント通知で攻撃処理を実行する
    /// </summary>
    public virtual void OnAttackAnimEvent() { }

    /// <summary>
    /// スプライトを透明にするときに呼ばれる処理
    /// </summary>
    protected virtual void OnTransparentSprites() { }

    /// <summary>
    /// フェードインが完了したときに呼ばれる処理
    /// </summary>
    protected virtual void OnFadeInComp() { }

    /// <summary>
    /// スプライトを透明にする
    /// </summary>
    public void TransparentSprites()
    {
        if (!isStartComp) Start();   // Startが実行されていない場合
        isSpawn = false;
        isInvincible = true;    // 無敵状態にする & 本来の行動不可

        foreach (var spriteRenderer in spriteRenderers)
        {
            Color color = spriteRenderer.color;
            color.a = 0;
            spriteRenderer.color = color;
        }

        InvokeRepeating("FadeIn", 0, 0.1f);
        OnTransparentSprites();
    }

    /// <summary>
    /// フェードイン処理
    /// </summary>
    protected void FadeIn()
    {
        foreach (var spriteRenderer in spriteRenderers)
        {
            Color color = spriteRenderer.color;
            color.a += 0.2f;
            spriteRenderer.color = color;
        }

        // フェードインが完了したら、無敵状態解除
        if (isInvincible && spriteRenderers[0].color.a >= 1)
        {
            isInvincible = false;
            OnFadeInComp();
            CancelInvoke("FadeIn");
        }
    }


    /// <summary>
    /// スポーンアニメーションが終了したとき
    /// </summary>
    public void OnEndSpawnAnim()
    {
        isSpawn = false;   // 行動できるようにする
    }

    /// <summary>
    /// アニメーション設定処理
    /// </summary>
    /// <param name="id"></param>
    public void SetAnimId(int id)
    {
        if (animator != null)
        {
            animator.SetInteger("animation_id", id);
        }
    }

    /// <summary>
    /// アニメーションID取得処理
    /// </summary>
    /// <returns></returns>
    public int GetAnimId()
    {
        return animator != null ? animator.GetInteger("animation_id") : 0;
    }
    #endregion
}