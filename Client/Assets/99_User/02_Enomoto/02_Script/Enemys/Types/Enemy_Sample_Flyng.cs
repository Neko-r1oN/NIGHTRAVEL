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
        MeleeType,
        RangeType,
    }

    #region 攻撃方法について
    [Header("攻撃方法")]
    [SerializeField] ATTACK_TYPE_ID attackType = ATTACK_TYPE_ID.None;
    [SerializeField] GameObject throwableObject;    // 遠距離攻撃の弾(仮)
    #endregion

    #region チェック判定
    [Header("チェック判定")]
    // 近距離攻撃の範囲
    [SerializeField] Transform meleeAttackCheck;
    [SerializeField] float meleeAttackRange = 0.9f;

    // 壁チェック
    [SerializeField] Transform wallCheck;
    [SerializeField] Vector2 wallCheckRadius = new Vector2(0, 1.5f);
    [SerializeField] LayerMask wallLayerMask;

    // 地面チェック
    [SerializeField] Transform platCheck;
    [SerializeField] Vector2 platCheckRadius = new Vector2(0, 1.5f);
    [SerializeField] LayerMask platLayerMask;
    #endregion

    #region 状態管理
    bool doOnceDecision;
    bool isAttacking;
    bool isDead;
    #endregion

    #region ターゲットとの距離
    float disToTarget;
    float disToTargetX;
    [SerializeField] float disToTargetMin = 0.25f;
    #endregion

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
        doOnceDecision = true;
        hitAnimationId = (int)ANIM_ID.Hit;
    }

    private void FixedUpdate()
    {
        if (!target && Players.Count > 0) target = sightChecker.GetTargetInSight(Players);
        else if (canChaseTarget && target && disToTarget > trackingRange
            || !canChaseTarget && target && !sightChecker.IsTargetVisible(target)) target = null;

        // 障害物、地面があるか取得
        isObstacle = Physics2D.OverlapBox(wallCheck.position, wallCheckRadius, 0f, wallLayerMask);
        isPlat = Physics2D.OverlapBox(platCheck.position, platCheckRadius, 0f, platLayerMask);
        if (!target && canPatrol) Run();
        else if (!target && !canPatrol) Idle();

        if (!target || isAttacking || isInvincible || hp <= 0 || !doOnceDecision) return;

        // ターゲットとの距離
        disToTarget = Vector3.Distance(this.transform.position, target.transform.position);
        disToTargetX = target.transform.position.x - transform.position.x;

        // ターゲットのいる方向にテクスチャを反転
        if (canChaseTarget)
        {
            if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
                || target.transform.position.x > transform.position.x && transform.localScale.x < 0) Flip();
        }

        if (canAttack && !sightChecker.IsObstructed(target) && disToTarget <= attackDist && disToTarget > disToTargetMin
            && !isAttacking && attackType != ATTACK_TYPE_ID.None)
        {
            Attack();
        }
        else if (canPatrol && !canChaseTarget
            || canPatrol && canChaseTarget
            || canChaseTarget)
        {
            Run();
        }
        else Idle();
    }

    /// <summary>
    /// アイドル処理
    /// </summary>
    void Idle()
    {
        SetAnimId((int)ANIM_ID.Idle);
        m_rb2d.linearVelocity = new Vector2(0f, m_rb2d.linearVelocity.y);
    }

    /// <summary>
    /// 攻撃処理
    /// </summary>
    void Attack()
    {
        doOnceDecision = false;
        isAttacking = true;
        SetAnimId((int)ANIM_ID.Attack);
        m_rb2d.linearVelocity = Vector2.zero;

        if (attackType == ATTACK_TYPE_ID.MeleeType) MeleeAttack();
        else if (attackType == ATTACK_TYPE_ID.RangeType) StartCoroutine(RangeAttack());
    }

    /// <summary>
    /// 近接攻撃処理
    /// </summary>
    void MeleeAttack()
    {
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Player")
            {
                collidersEnemies[i].gameObject.GetComponent<CharacterController2D>().ApplyDamage(power, transform.position);
            }
        }
        StartCoroutine(AttackCooldown(attackCoolTime));
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
    void Run()
    {
        SetAnimId((int)ANIM_ID.Run);
        Vector2 speedVec = Vector2.zero;
        if (canChaseTarget && target)
        {
            Vector2 direction = target.transform.position - transform.position + new Vector3(disToTargetMin, disToTargetMin);
            speedVec = new Vector2(direction.x / Mathf.Abs(direction.x), direction.y / Mathf.Abs(direction.y)) * speed;

            if (isObstacle) speedVec = new Vector2(0f, speedVec.y);
            else if (isPlat) speedVec = new Vector2(speedVec.x, 0);
        }
        else if (canPatrol)
        {
            if (!isPlat || isObstacle) Flip();
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

            SetAnimId((int)ANIM_ID.Hit);
            hp -= Mathf.Abs(damage);
            DoKnokBack(damage);

            if (hp > 0)
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
        SetAnimId((int)ANIM_ID.Dead);
        yield return new WaitForSeconds(0.25f);
        GetComponent<CapsuleCollider2D>().direction = CapsuleDirection2D.Horizontal;
        m_rb2d.linearVelocity = new Vector2(0, m_rb2d.linearVelocity.y);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    /// <summary>
    /// [ デバック用 ] Gizmosを使用して検出範囲を描画
    /// </summary>
    void OnDrawGizmos()
    {
        // 攻撃範囲
        if (meleeAttackCheck)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleeAttackCheck.transform.position, meleeAttackRange);
        }

        // 壁の判定
        if (wallCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(wallCheck.transform.position, wallCheckRadius);
        }
        if (platCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(platCheck.transform.position, platCheckRadius);
        }

        // 追跡範囲
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, trackingRange);
    }
}
