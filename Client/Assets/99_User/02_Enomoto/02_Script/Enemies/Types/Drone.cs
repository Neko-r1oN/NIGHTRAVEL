//**************************************************
//  [敵] ドローンを制御するクラス
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System.Collections;
using UnityEngine;

public class Drone : EnemyBase
{
    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        Idle = 1,
        Dead,
    }

    /// <summary>
    /// 攻撃方法
    /// </summary>
    public enum ATTACK_TYPE_ID
    {
        None,
        RangeType,
    }

    #region オリジナルステータス
    [Foldout("ステータス")]
    [SerializeField]
    float patorolRange = 10f;

    [Foldout("ステータス")]
    [SerializeField]
    float aimRotetionSpeed = 3f;
    #endregion

    #region 攻撃関連
    [Foldout("攻撃関連")]
    [SerializeField] 
    Transform aimTransform;
    [Foldout("攻撃関連")]
    [SerializeField] 
    GunParticleController gunPsController;
    [Foldout("攻撃関連")]
    [SerializeField] 
    float gunBulletWidth;
    #endregion

    #region チェック判定
    // 壁と地面チェック
    [Foldout("チェック関連")]
    [SerializeField] 
    Transform wallCheck;
    [Foldout("チェック関連")]
    [SerializeField] 
    Vector2 wallCheckRadius = new Vector2(0, 1.5f);
    #endregion

    #region ターゲットとの距離
    [SerializeField] float disToTargetMin = 2.5f;
    #endregion

    Coroutine patorolCoroutine;
    Vector2? startPatorolPoint = null;

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
        doOnceDecision = true;
    }

    /// <summary>
    /// 行動パターン実行処理
    /// </summary>
    protected override void DecideBehavior()
    {
        // 行動パターン
        if (canAttack && projectileChecker.CanFireProjectile(gunBulletWidth, true) && !sightChecker.IsObstructed())
        {
            chaseAI.StopChase();
            Attack();
        }
        else if (moveSpeed > 0 && canChaseTarget && target)
        {
            Tracking();
        }
        else if (moveSpeed > 0 && canPatrol && !isPatrolPaused)
        {
            Patorol();
        }
        else
        {
            chaseAI.StopChase();
            Idle();
        }
    }

    /// <summary>
    /// アイドル処理
    /// </summary>
    protected override void Idle()
    {
        m_rb2d.linearVelocity = new Vector2(0f, m_rb2d.linearVelocity.y);
    }

    #region 攻撃処理関連

    /// <summary>
    /// 攻撃処理
    /// </summary>
    void Attack()
    {
        doOnceDecision = false;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        chaseAI.StopChase();
        cancellCoroutines.Add(StartCoroutine(RangeAttack()));
    }

    /// <summary>
    /// 遠距離攻撃処理
    /// </summary>
    IEnumerator RangeAttack()
    {
        yield return new WaitForSeconds(0.25f);  // 攻撃開始を遅延
        gunPsController.StartShooting();

        float time = 0;
        while (time < shotsPerSecond)
        {
            // ターゲットのいる方向に向かってエイム
            if (target)
            {
                if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
                    || target.transform.position.x > transform.position.x && transform.localScale.x < 0) Flip();

                Vector3 direction = target.transform.position - transform.position;
                Quaternion quaternion = Quaternion.Euler(0, 0, projectileChecker.ClampAngleToTarget(direction));
                aimTransform.rotation = Quaternion.RotateTowards(aimTransform.rotation, quaternion, aimRotetionSpeed);
            }
            yield return new WaitForSeconds(0.1f);
            time += 0.1f;
        }

        cancellCoroutines.Add(StartCoroutine(AttackCooldown(attackCoolTime)));
    }

    /// <summary>
    /// 攻撃時のクールダウン処理
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time)
    {
        gunPsController.StopShooting();
        isAttacking = true;
        yield return new WaitForSeconds(time);
        isAttacking = false;
        doOnceDecision = true;
        Idle();
    }

    #endregion

    #region 移動処理関連

    /// <summary>
    /// 追跡する処理
    /// </summary>
    protected override void Tracking()
    {
        StopPatorol();
        chaseAI.DoChase(target);
    }

    /// <summary>
    /// 巡回処理
    /// </summary>
    protected override void Patorol()
    {
        if (patorolCoroutine == null)
        {
            cancellCoroutines.Add(StartCoroutine(PatorolCoroutine()));
        }
    }

    /// <summary>
    /// 巡回する処理
    /// </summary>
    IEnumerator PatorolCoroutine()
    {
        float pauseTime = 2f;
        if (startPatorolPoint == null)
        {
            startPatorolPoint = transform.position;
        }

        if (IsWall()) Flip();

        if (TransformHelper.GetFacingDirection(transform) > 0)
        {
            if (transform.position.x >= startPatorolPoint.Value.x + patorolRange)
            {
                isPatrolPaused = true;
                Idle();
                yield return new WaitForSeconds(pauseTime);
                isPatrolPaused = false;
                Flip();
            }
        }
        else if (TransformHelper.GetFacingDirection(transform) < 0)
        {
            if (transform.position.x <= startPatorolPoint.Value.x - patorolRange)
            {
                isPatrolPaused = true;
                Idle();
                yield return new WaitForSeconds(pauseTime);
                isPatrolPaused = false;
                Flip();
            }
        }

        Vector2 speedVec = Vector2.zero;
        speedVec = new Vector2(TransformHelper.GetFacingDirection(transform) * moveSpeed / 2, m_rb2d.linearVelocity.y);
        m_rb2d.linearVelocity = speedVec;
        patorolCoroutine = null;
    }

    /// <summary>
    /// 巡回処理を停止する
    /// </summary>
    void StopPatorol()
    {
        startPatorolPoint = null;
    }

    #endregion

    #region ヒット処理関連

    /// <summary>
    /// ダメージを受けたときの処理
    /// </summary>
    protected override void OnHit()
    {
        base.OnHit();
        gunPsController.StopShooting();
    }

    /// <summary>
    /// 死亡するときに呼ばれる処理処理
    /// </summary>
    /// <returns></returns>
    protected override void OnDead()
    {
        gunPsController.StopShooting();
        SetAnimId((int)ANIM_ID.Dead);
    }

    #endregion

    #region チェック処理関連

    /// <summary>
    /// 壁があるかどうか
    /// </summary>
    /// <returns></returns>
    bool IsWall()
    {
        return Physics2D.OverlapBox(wallCheck.position, wallCheckRadius, 0f, terrainLayerMask);
    }

    /// <summary>
    /// 検出範囲の描画処理
    /// </summary>
    protected override void DrawDetectionGizmos()
    {
        // 攻撃開始距離
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackDist);

        // 射線
        if (sightChecker != null)
        {
            projectileChecker.DrawProjectileRayGizmo(gunBulletWidth, true);
        }

        // 壁の判定
        if (wallCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(wallCheck.transform.position, wallCheckRadius);
        }
    }

    #endregion
}
