////**************************************************
////  近接タイプの敵クラス
////  Author:r-enomoto
////**************************************************
//using System.Collections;
//using UnityEngine;

//public class EnemyMelee : EnemyBase
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

//    #region チェック判定
//    [Header("チェック判定")]
//    // 近距離攻撃の範囲
//    [SerializeField] Transform meleeAttackCheck;
//    [SerializeField] float meleeAttackRange = 0.9f;

//    // 壁・地面チェック
//    [SerializeField] Transform wallCheck;
//    [SerializeField] Vector2 wallCheckRadius = new Vector2(0, 1.5f);
//    [SerializeField] Transform groundCheck;
//    [SerializeField] Vector2 groundCheckRadius = new Vector2(0.5f, 0.2f);

//    // 落下チェック
//    [SerializeField] Transform frontFallCheck;
//    [SerializeField] float frontFallCheckRange = 0.9f;
//    #endregion

//    #region ターゲットと離す距離
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
//        if (canChaseTarget && !IsGround())
//        {
//            AirMovement();
//        }
//        else if (canAttack && !sightChecker.IsObstructed() && disToTarget <= attackDist)
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
//                Teleport();
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

//    /// <summary>
//    /// 攻撃処理
//    /// </summary>
//    public void Attack()
//    {
//        doOnceDecision = false;
//        isAttacking = true;
//        SetAnimId((int)ANIM_ID.Attack);
//        m_rb2d.linearVelocity = Vector2.zero;
//        if (chaseAI) chaseAI.Stop();

//        MeleeAttack();
//    }

//    /// <summary>
//    /// 近接攻撃処理
//    /// </summary>
//    void MeleeAttack()
//    {
//        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
//        for (int i = 0; i < collidersEnemies.Length; i++)
//        {
//            if (collidersEnemies[i].Object.tag == "Player")
//            {
//                collidersEnemies[i].Object.GetComponent<PlayerBase>().ApplyDamageRequest(power, transform.position);
//            }
//        }
//        cancellCoroutines.Add(StartCoroutine(AttackCooldownCoroutine(attackCoolTime)));
//    }

//    /// <summary>
//    /// 追跡する処理
//    /// </summary>
//    protected override void Teleport()
//    {
//        SetAnimId((int)ANIM_ID.Run);
//        Vector2 speedVec = Vector2.zero;

//        if (IsFrontFall() || IsWall())
//        {
//            speedVec = new Vector2(0f, m_rb2d.linearVelocity.y);
//        }
//        else
//        {
//            float distToPlayer = target.transform.position.x - this.transform.position.x;
//            speedVec = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed, m_rb2d.linearVelocity.y);
//        }

//        m_rb2d.linearVelocity = speedVec;
//    }

//    /// <summary>
//    /// 巡回する処理
//    /// </summary>
//    protected override void Patorol()
//    {
//        SetAnimId((int)ANIM_ID.Run);
//        if (IsFrontFall() || IsWall()) Flip();
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

//    /// <summary>
//    /// ダメージを受けたときの処理
//    /// </summary>
//    protected override void OnHit()
//    {
//        base.OnHit();
//        //SetAnimId((int)ANIM_ID.Hit);
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

//    /// <summary>
//    /// 死亡するときに呼ばれる処理処理
//    /// </summary>
//    /// <returns></returns>
//    protected override void OnDead()
//    {
//        //SetAnimId((int)ANIM_ID.Dead);
//    }

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
//    bool IsFrontFall()
//    {
//        return !Physics2D.OverlapCircle(frontFallCheck.position, frontFallCheckRange, terrainLayerMask);
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
//        if (frontFallCheck)
//        {
//            Gizmos.color = Color.green;
//            Gizmos.DrawWireSphere(frontFallCheck.position, frontFallCheckRange);
//        }
//    }
//}
