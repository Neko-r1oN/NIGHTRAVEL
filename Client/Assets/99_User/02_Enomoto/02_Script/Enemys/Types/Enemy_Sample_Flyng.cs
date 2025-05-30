//**************************************************
//  エネミーのサンプルクラス(飛行型)
//  Author:r-enomoto
//**************************************************
using HardLight2DUtil;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy_Sample_Flyng : EnemyController
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

    #region 攻撃方法について
    [Header("攻撃方法")]
    [SerializeField] ATTACK_TYPE_ID attackType = ATTACK_TYPE_ID.None;
    [SerializeField] GameObject throwableObject;    // 遠距離攻撃の弾(仮)
    #endregion

    #region チェック判定
    [Header("チェック判定")]
    // 壁と地面チェック
    [SerializeField] Transform wallCheck;
    [SerializeField] Vector2 wallCheckRadius = new Vector2(0, 1.5f);
    [SerializeField] LayerMask terrainLayerMask;
    #endregion

    #region 状態管理
    bool isDead;
    #endregion

    #region ターゲットとの距離
    [SerializeField] float disToTargetMin = 2.5f;
    #endregion

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
        doOnceDecision = true;
    }

    /// <summary>
    /// 行動パターンを決める処理
    /// </summary>
    protected override void DecideBehavior()
    {
        // 行動パターン
        //if (speed > 0 && canChaseTarget && disToTarget < disToTargetMin)
        //{
        //    Run();
        //}
        if (canAttack && sightChecker.CanFireProjectile(throwableObject, true) && !sightChecker.IsObstructed() && attackType != ATTACK_TYPE_ID.None)
        {
            chaseAI.StopChase();
            Attack();
        }
        else if (speed > 0 && canPatrol || speed > 0 && canChaseTarget)
        {
            Run();
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
        StartCoroutine(RangeAttack());
    }

    /// <summary>
    /// 遠距離攻撃処理
    /// </summary>
    IEnumerator RangeAttack()
    {
        GameObject target = this.target;
        for (int i = 0; i < bulletNum; i++)
        {
            GameObject throwableProj = Instantiate(throwableObject, transform.position, Quaternion.identity);
            Vector3 direction = target.transform.position - transform.position;
            throwableProj.GetComponent<Projectile>().Initialize(direction, gameObject);
            yield return new WaitForSeconds(shotsPerSecond);
        }
        StartCoroutine(AttackCooldown(attackCoolTime));
    }

    /// <summary>
    /// 走る処理
    /// </summary>
    protected override void Run()
    {
        //SetAnimId((int)ANIM_ID.Run);
        Vector2 speedVec = Vector2.zero;
        //if (canChaseTarget && target && disToTarget < disToTargetMin)
        //{
        //    chaseAI.ReturnToPreviousDestination();
        //}
        if (canChaseTarget && target)
        {
            chaseAI.DoChase(target);
        }
        else if (canPatrol)
        {
            if (IsWall()) Flip();
            speedVec = new Vector2(transform.localScale.x * speed, m_rb2d.linearVelocity.y);
        }

        m_rb2d.linearVelocity = speedVec;
    }

    /// <summary>
    /// ダメージ適応処理
    /// </summary>
    /// <param name="damage"></param>
    public override void ApplyDamage(int damage, Transform attacker)
    {
        if (!isInvincible)
        {
            // ターゲットの方向にテクスチャを反転
            if (attacker.position.x < transform.position.x && transform.localScale.x > 0
            || attacker.position.x > transform.position.x && transform.localScale.x < 0) Flip();

            //SetAnimId((int)ANIM_ID.Hit);
            life -= Mathf.Abs(damage);
            DoKnokBack(damage);

            if (life > 0)
            {
                StartCoroutine(HitTime());
            }
            else
            {
                StartCoroutine(DestroyEnemy());
            }
        }
    }

    /// <summary>
    /// ダメージ適応時の無敵時間
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator HitTime()
    {
        //SetAnimId((int)ANIM_ID.Hit);
        yield return null;
        base.HitTime();
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
    /// 死亡処理
    /// </summary>
    /// <returns></returns>
    IEnumerator DestroyEnemy()
    {
        isDead = true;
        //SetAnimId((int)ANIM_ID.Dead);
        yield return new WaitForSeconds(0.25f);
        GetComponent<CapsuleCollider2D>().direction = CapsuleDirection2D.Horizontal;
        m_rb2d.linearVelocity = new Vector2(0, m_rb2d.linearVelocity.y);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
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
            sightChecker.DrawProjectileRayGizmo(throwableObject, true);
        }

        // 壁の判定
        if (wallCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(wallCheck.transform.position, wallCheckRadius);
        }
    }
}
