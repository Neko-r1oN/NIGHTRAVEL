//**************************************************
//  エネミーのサンプルクラス(飛行型)
//  Author:r-enomoto
//**************************************************
using DG.Tweening;
using HardLight2DUtil;
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Sample_Flyng : EnemyBase
{
    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        Idle = 1,
        Attack,
        Run,
        Hit,
        Fall,
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
    #endregion

    #region 攻撃方法について
    [Header("攻撃方法")]
    [SerializeField] ATTACK_TYPE_ID attackType = ATTACK_TYPE_ID.None;
    [SerializeField] GameObject throwableObject;    // 遠距離攻撃の弾(仮)
    [SerializeField] Transform aimTransform;        // 遠距離攻撃の弾の生成位置
    #endregion

    #region チェック判定
    [Header("チェック判定")]
    // 壁と地面チェック
    [SerializeField] Transform wallCheck;
    [SerializeField] Vector2 wallCheckRadius = new Vector2(0, 1.5f);
    #endregion

    #region ターゲットとの距離
    [SerializeField] float disToTargetMin = 2.5f;
    #endregion

    Coroutine patorolCoroutine;
    Vector2? startPatorolPoint = null;
    float randomDecision;

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
        if (canAttack && projectileChecker.CanFireProjectile(throwableObject, true) && !sightChecker.IsObstructed() && attackType != ATTACK_TYPE_ID.None)
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
        //SetAnimId((int)ANIM_ID.Idle);
        m_rb2d.linearVelocity = new Vector2(0f, m_rb2d.linearVelocity.y);
    }

    /// <summary>
    /// 攻撃処理
    /// </summary>
    void Attack()
    {
        doOnceDecision = false;
        isAttacking = true;
        //SetAnimId((int)ANIM_ID.Attack);
        m_rb2d.linearVelocity = Vector2.zero;
        chaseAI.StopChase();
        StartCoroutine(RangeAttack());
    }

    /// <summary>
    /// 遠距離攻撃処理
    /// </summary>
    IEnumerator RangeAttack()
    {
        yield return new WaitForSeconds(0.5f);  // 攻撃開始を遅延

        GameObject target = this.target;
        for (int i = 0; i < bulletNum; i++)
        {
            GameObject throwableProj = Instantiate(throwableObject, aimTransform.position, Quaternion.identity);
            Vector3 direction = target.transform.position - transform.position;
            throwableProj.GetComponent<Projectile>().Initialize(direction, gameObject);
            yield return new WaitForSeconds(shotsPerSecond);
        }
        StartCoroutine(AttackCooldown(attackCoolTime));
    }

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
            StartCoroutine(PatorolCoroutine());
        }
    }

    /// <summary>
    /// 巡回する処理
    /// </summary>
    IEnumerator PatorolCoroutine()
    {
        float pauseTime = 3f;
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

    /// <summary>
    /// ダメージを受けたときの処理
    /// </summary>
    protected override void OnHit()
    {
        base.OnHit();
        //SetAnimId((int)ANIM_ID.Hit);
    }

    /// <summary>
    /// 壁があるかどうか
    /// </summary>
    /// <returns></returns>
    bool IsWall()
    {
        return Physics2D.OverlapBox(wallCheck.position, wallCheckRadius, 0f, terrainLayerMask);
    }

    /// <summary>
    /// 攻撃時のクールダウン処理
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time)
    {
        isAttacking = true;
        yield return new WaitForSeconds(time);
        isAttacking = false;
        doOnceDecision = true;
        Idle();
    }

    /// <summary>
    /// 死亡アニメーション
    /// </summary>
    /// <returns></returns>
    protected override void PlayDeadAnim()
    {
        //SetAnimId((int)ANIM_ID.Dead);
    }

    /// <summary>
    /// ヒットアニメーション
    /// </summary>
    /// <returns></returns>
    protected override void PlayHitAnim()
    {
        //SetAnimId((int)ANIM_ID.Hit);
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
            projectileChecker.DrawProjectileRayGizmo(throwableObject, true);
        }

        // 壁の判定
        if (wallCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(wallCheck.transform.position, wallCheckRadius);
        }
    }
}
