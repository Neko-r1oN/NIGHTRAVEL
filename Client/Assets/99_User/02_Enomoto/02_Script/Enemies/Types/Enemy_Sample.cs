////**************************************************
////  エネミーのサンプルクラス
////  Author:r-enomoto
////**************************************************
//using HardLight2DUtil;
//using Pixeye.Unity;
//using System.Collections;
//using System.Collections.Generic;
//using Unity.VisualScripting;
//using UnityEngine;

//public class Enemy_Sample : EnemyBase
//{
//    /// <summary>
//    /// アニメーションID
//    /// </summary>
//    public enum ANIM_ID
//    {
//        Idle = 1,
//        Attack,
//        Run,
//        Hit,
//        Fall,
//        Dead,
//    }

//    /// <summary>
//    /// 攻撃方法
//    /// </summary>
//    public enum ATTACK_TYPE_ID
//    {
//        None,
//        MeleeType,
//        RangeType,
//    }

//    #region コンポーネント
//    EnemyProjectileChecker projectileChecker;
//    #endregion

//    #region 攻撃関連
//    [Foldout("攻撃関連")]
//    [SerializeField] 
//    ATTACK_TYPE_ID attackType = ATTACK_TYPE_ID.None;
//    [Foldout("攻撃関連")]
//    [SerializeField] 
//    GameObject throwableObject;    // 遠距離攻撃の弾(仮)
//    #endregion

//    #region チェック判定
//    // 近距離攻撃の範囲
//    [Foldout("チェック関連")]
//    [SerializeField] 
//    Transform meleeAttackCheck;
//    [Foldout("チェック関連")]
//    [SerializeField] 
//    float meleeAttackRange = 0.9f;

//    // 壁・地面チェック
//    [Foldout("チェック関連")]
//    [SerializeField] 
//    Transform wallCheck;
//    [Foldout("チェック関連")]
//    [SerializeField] 
//    Vector2 wallCheckRadius = new Vector2(0, 1.5f);
//    [Foldout("チェック関連")]
//    [SerializeField] 
//    Transform groundCheck;
//    [Foldout("チェック関連")]
//    [SerializeField] 
//    Vector2 groundCheckRadius = new Vector2(0.5f, 0.2f);

//    // 落下チェック
//    [Foldout("チェック関連")]
//    [SerializeField] 
//    Transform fallCheck;
//    [Foldout("チェック関連")]
//    [SerializeField] 
//    float fallCheckRange = 0.9f;
//    #endregion

//    #region ターゲットとの距離
//    readonly float disToTargetMin = 0.25f;
//    #endregion

//    protected override void Start()
//    {
//        base.Start();
//        isAttacking = false;
//        doOnceDecision = true;
//    }

//    /// <summary>
//    /// 行動パターン実行処理
//    /// </summary>
//    protected override void DecideBehavior()
//    {
//        // 行動パターン
//        if (canChaseTarget && IsWall() && IsGround() && Mathf.Abs(disToTargetX) > disToTargetMin && canJump)
//        {
//            Jump();
//        }
//        else if (canChaseTarget && !IsGround())
//        {
//            AirMovement();
//        }
//        else if (attackType == ATTACK_TYPE_ID.MeleeType && canAttack && !sightChecker.IsObstructed() && disToTarget <= attackDist)
//        {
//            Attack();
//        }
//        else if (attackType == ATTACK_TYPE_ID.RangeType && canAttack && projectileChecker.CanFireProjectile(target, throwableObject.GetComponent<SpriteRenderer>().bounds.size.y / 2, 270f))
//        {
//            Attack();
//        }
//        else if (moveSpeed > 0 && canPatrol && Mathf.Abs(disToTargetX) > disToTargetMin 
//            || moveSpeed > 0 && canChaseTarget && Mathf.Abs(disToTargetX) > disToTargetMin)
//        {
//            if (canChaseTarget && IsWall() && !canJump)
//            {
//                Idle();
//            }
//            else if (canChaseTarget && target)
//            {
//                Tracking();
//            }
//            else if (canPatrol)
//            {
//                Patorol();
//            }
//        }
//        else Idle();
//    }

//    /// <summary>
//    /// アイドル処理
//    /// </summary>
//    protected override void Idle()
//    {
//        SetAnimId((int)ANIM_ID.Idle);
//        m_rb2d.linearVelocity = new Vector2(0f, m_rb2d.linearVelocity.y);
//    }

//    #region 攻撃処理関連
//    /// <summary>
//    /// 攻撃開始処理
//    /// </summary>
//    void Attack()
//    {
//        doOnceDecision = false;
//        isAttacking = true;
//        m_rb2d.linearVelocity = Vector2.zero;
//        SetAnimId((int)ANIM_ID.Attack);
//        if (chaseAI) chaseAI.Stop();

//        // デバック用
//        OnAttackEvent();
//    }

//    /// <summary>
//    /// 攻撃実行処理（基本的にアニメーションイベントから呼ばれる）
//    /// </summary>
//    public void OnAttackEvent()
//    {
//        if (attackType == ATTACK_TYPE_ID.MeleeType)
//        {
//            MeleeAttack();
//        }
//        else if (attackType == ATTACK_TYPE_ID.RangeType)
//        {
//            cancellCoroutines.Add(StartCoroutine(RangeAttack()));
//        }
//    }

//    /// <summary>
//    /// 近接攻撃処理
//    /// </summary>
//    void MeleeAttack()
//    {
//        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
//        for (int i = 0; i < collidersEnemies.Length; i++)
//        {
//            if (collidersEnemies[i].gameObject.tag == "Player")
//            {
//                collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position);
//            }
//        }
//        cancellCoroutines.Add(StartCoroutine(AttackCooldownCoroutine(attackCoolTime)));
//    }

//    /// <summary>
//    /// 遠距離攻撃処理
//    /// </summary>
//    IEnumerator RangeAttack()
//    {
//        GameObject target = this.target;
//        for (int i = 0; i < bulletNum; i++)
//        {
//            GameObject throwableProj = Instantiate(throwableObject, transform.position + new Vector3(TransformUtils.GetFacingDirection(transform) * 0.5f, -0.2f), Quaternion.identity);
//            throwableProj.GetComponent<ThrowableProjectile>().owner = gameObject;
//            Vector2 direction = new Vector2(TransformUtils.GetFacingDirection(transform), 0f);
//            throwableProj.GetComponent<ThrowableProjectile>().direction = direction;
//            yield return new WaitForSeconds(shotsPerSecond);
//        }
//        cancellCoroutines.Add(StartCoroutine(AttackCooldownCoroutine(attackCoolTime)));
//    }

//    /// <summary>
//    /// 攻撃時のクールダウン処理
//    /// </summary>
//    /// <returns></returns>
//    IEnumerator AttackCooldownCoroutine(float time)
//    {
//        isAttacking = true;
//        yield return new WaitForSeconds(time);
//        isAttacking = false;
//        doOnceDecision = true;
//        Idle();
//    }

//    #endregion

//    #region 移動処理関連

//    /// <summary>
//    /// ジャンプ処理
//    /// </summary>
//    void Jump()
//    {
//        SetAnimId((int)ANIM_ID.Fall);

//        transform.position += Vector3.up * groundCheckRadius.y;
//        m_rb2d.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
//    }

//    /// <summary>
//    /// 追跡する処理
//    /// </summary>
//    protected override void Tracking()
//    {
//        SetAnimId((int)ANIM_ID.Run);
//        float distToPlayer = target.transform.position.x - this.transform.position.x;
//        Vector2 speedVec = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed, m_rb2d.linearVelocity.y);
//        m_rb2d.linearVelocity = speedVec;
//    }

//    /// <summary>
//    /// 巡回する処理
//    /// </summary>
//    protected override void Patorol()
//    {
//        SetAnimId((int)ANIM_ID.Run);
//        if (IsFall() || IsWall()) Flip();
//        Vector2 speedVec = new Vector2(TransformUtils.GetFacingDirection(transform) * moveSpeed, m_rb2d.linearVelocity.y);
//        m_rb2d.linearVelocity = speedVec;
//    }

//    /// <summary>
//    /// 空中状態での移動処理
//    /// </summary>
//    void AirMovement()
//    {
//        SetAnimId((int)ANIM_ID.Fall);

//        // ジャンプ(落下)中にプレイヤーに向かって移動する
//        float distToPlayer = target.transform.position.x - this.transform.position.x;
//        Vector3 targetVelocity = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed, m_rb2d.linearVelocity.y);
//        Vector3 velocity = Vector3.zero;
//        m_rb2d.linearVelocity = Vector3.SmoothDamp(m_rb2d.linearVelocity, targetVelocity, ref velocity, 0.05f);
//    }

//    #endregion

//    #region ヒット処理関連

//    /// <summary>
//    /// ダメージを受けたときの処理
//    /// </summary>
//    protected override void OnHit()
//    {
//        base.OnHit();
//        SetAnimId((int)ANIM_ID.Hit);
//    }

//    /// <summary>
//    /// 死亡するときに呼ばれる処理処理
//    /// </summary>
//    /// <returns></returns>
//    protected override void OnDead()
//    {
//        SetAnimId((int)ANIM_ID.Dead);
//    }

//    #endregion

//    #region チェック処理関連
//    /// <summary>
//    /// 壁があるかどうか
//    /// </summary>
//    /// <returns></returns>
//    bool IsWall()
//    {
//        return Physics2D.OverlapBox(wallCheck.position, wallCheckRadius, 0f, terrainLayerMask);
//    }

//    /// <summary>
//    /// 落下中かどうか
//    /// </summary>
//    /// <returns></returns>
//    bool IsFall()
//    {
//        return !Physics2D.OverlapCircle(fallCheck.position, fallCheckRange, terrainLayerMask);
//    }

//    /// <summary>
//    /// 地面判定
//    /// </summary>
//    /// <returns></returns>
//    bool IsGround()
//    {
//        // 足元に２つの始点と終点を作成する
//        Vector3 leftStartPosition = groundCheck.transform.position + Vector3.left * groundCheckRadius.x / 2;
//        Vector3 rightStartPosition = groundCheck.transform.position + Vector3.right * groundCheckRadius.x / 2;
//        Vector3 endPosition = groundCheck.transform.position + Vector3.down * groundCheckRadius.y;

//        return Physics2D.Linecast(leftStartPosition, endPosition, terrainLayerMask)
//            || Physics2D.Linecast(rightStartPosition, endPosition, terrainLayerMask);
//    }

//    /// <summary>
//    /// 検出範囲の描画処理
//    /// </summary>
//    protected override void DrawDetectionGizmos()
//    {
//        // 攻撃開始距離
//        Gizmos.color = Color.blue;
//        Gizmos.DrawWireSphere(transform.position, attackDist);

//        // 攻撃範囲
//        if (meleeAttackCheck)
//        {
//            Gizmos.color = Color.red;
//            Gizmos.DrawWireSphere(meleeAttackCheck.transform.position, meleeAttackRange);
//        }

//        // 遠距離攻撃の射線
//        if (attackType == ATTACK_TYPE_ID.RangeType && sightChecker)
//        {
//            projectileChecker.DrawProjectileRayGizmo(target, throwableObject.GetComponent<SpriteRenderer>().bounds.size.y / 2, 270f);
//        }

//        // 壁の判定
//        if (wallCheck)
//        {
//            Gizmos.color = Color.green;
//            Gizmos.DrawWireCube(wallCheck.transform.position, wallCheckRadius);
//        }

//        // 地面判定
//        if (groundCheck)
//        {
//            Vector3 leftStartPosition = groundCheck.transform.position + Vector3.left * groundCheckRadius.x / 2;
//            Vector3 rightStartPosition = groundCheck.transform.position + Vector3.right * groundCheckRadius.x / 2;
//            Vector3 endPosition = groundCheck.transform.position + Vector3.down * groundCheckRadius.y;
//            Gizmos.DrawLine(leftStartPosition, endPosition);
//            Gizmos.DrawLine(rightStartPosition, endPosition);
//        }

//        // 落下チェック
//        if (fallCheck)
//        {
//            Gizmos.color = Color.green;
//            Gizmos.DrawWireSphere(fallCheck.position, fallCheckRange);
//        }
//    }

//    #endregion
//}