//**************************************************
//  [ボス] フルメタルワームのクラス
//  Author:r-enomoto
//**************************************************
using NUnit.Framework;
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullMetalWorm : EnemyBase
{
    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        None = 0,
        Idle,
        Attack,
        Run,
        Hit,
        Dead,
    }

    #region ステータス
    [Foldout("ステータス")]
    [SerializeField]
    float rotationSpeed = 90f; // 頭の回転速度
    #endregion

    #region チェック判定
    // 近距離攻撃の範囲
    [Foldout("チェック関連")]
    [SerializeField]
    Transform meleeAttackCheck;
    [Foldout("チェック関連")]
    [SerializeField] 
    float meleeAttackRange = 0.9f;

    [Foldout("チェック関連")]
    [SerializeField]
    Vector2 stageCenter = Vector2.zero;
    [Foldout("チェック関連")]
    [SerializeField]
    float moveRange;
    #endregion

    #region 状態管理
    [SerializeField] float decisionTime = 5f;
    readonly float disToTargetPosMin = 3f;
    float randomDecision;
    bool endDecision;

    [SerializeField] float maxMoveTime = 5f;
    #endregion

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
        doOnceDecision = false;
        endDecision = true;
        StartCoroutine(NextDecision(decisionTime));
    }

    protected override void FixedUpdate()
    {
        if (isStun || isAttacking || isInvincible || hp <= 0) return;

        if (!target && Players.Count > 0)
        {
            // ターゲットを探す
            SetRandomTargetPlayer();
        }

        if (!target)
        {
            //if (canPatrol && !isPatrolPaused) Patorol();
            //else Idle();
            //return;

            // ランダムな座標に向かって移動する
        }

        if (target)
        {
            // ターゲットとの距離を取得する
            disToTarget = Vector3.Distance(target.transform.position, this.transform.position);
            disToTargetX = target.transform.position.x - transform.position.x;
        }

        DecideBehavior();
    }

    /// <summary>
    /// 行動パターン実行処理
    /// </summary>
    protected override void DecideBehavior()
    {
        if (doOnceDecision)
        {
            doOnceDecision = false;
            //// ターゲットと距離が近い場合は確率で攻撃を優先
            //if (disToTarget < disToTargetMin)
            //{
            //    if (randomDecision > 0.3f)
            //    {
            //        Attack();
            //    }
            //    else
            //    {
            //        Tracking();
            //    }
            //}
            //// ターゲットと距離が遠い場合は確率で移動を優先
            //else
            //{
            //    if (randomDecision > 0.3f)
            //    {
            //        Tracking();
            //    }
            //    else
            //    {
            //        Attack();
            //    }
            //}
            StartCoroutine(MoveCoroutine());
        }
    }

    /// <summary>
    /// 次の行動パターン決定処理
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator NextDecision(float time)
    {
        doOnceDecision = false;
        StartCoroutine(MoveGraduallyCoroutine());
        yield return new WaitForSeconds(time);
        StopCoroutine(MoveGraduallyCoroutine());
        randomDecision = Random.Range(0, 1);
        doOnceDecision = true;
    }

    #region 攻撃処理関連

    /// <summary>
    /// 攻撃処理
    /// </summary>
    public void Attack()
    {
        doOnceDecision = false;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack);
    }

    /// <summary>
    /// 近接攻撃処理 [Animationイベントからの呼び出し]
    /// </summary>

    public override void OnAttackAnimEvent()
    {
        // 前に飛び込む
        Vector2 jumpVec = new Vector2(18 * TransformHelper.GetFacingDirection(transform), 10);
        m_rb2d.linearVelocity = jumpVec;

        // 自身がエリート個体の場合、付与する状態異常の種類を取得する
        bool isElite = this.isElite && enemyElite != null;
        StatusEffectController.EFFECT_TYPE? applyEffect = null;
        if (isElite)
        {
            applyEffect = enemyElite.GetAddStatusEffectEnum();
        }

        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Player")
            {
                collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position, applyEffect);
            }
        }

        cancellCoroutines.Add(StartCoroutine(AttackCooldown(attackCoolTime)));
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

    #endregion

    #region 移動処理関連

    /// <summary>
    /// 進行方向に向かって移動する処理
    /// </summary>
    /// <param name="targetPos"></param>
    /// <param name="direction"></param>
    void MoveTowardsTarget(float speed)
    {
        //Vector2 direction = (targetPos - transform.position).normalized;
        //m_rb2d.linearVelocity = direction * speed;
        //RotateTowardsMovementDirection();

        m_rb2d.linearVelocity = transform.up * speed;
    }

    /// <summary>
    /// 現在の進行方向に向かって回転させる
    /// </summary>
    void RotateTowardsMovementDirection(Vector3 targetPos, float rotationSpeed)
    {
        Vector2 direction = (targetPos - this.transform.position).normalized;
        // 動いていない場合は回転させない
        if (direction == Vector2.zero) return;

        // ベクトルから角度を計算（ラジアンから度数に変換し、-90度で真上を0度にする）
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        // 現在の回転角度から目標角度へ滑らかに補間し、Rigidbody2Dの回転を更新
        float newAngle = Mathf.LerpAngle(m_rb2d.rotation, targetAngle, Time.fixedDeltaTime * rotationSpeed);
        m_rb2d.MoveRotation(newAngle);
    }

    /// <summary>
    /// 目標地点まで移動する処理
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveCoroutine()
    {
        Vector3 targetPos = GetNextTargetPosition();
        bool isTargetPosReached = false;
        float time = 0;

        while (time < maxMoveTime)
        {
            float disToTargetPos = Mathf.Abs(Vector3.Distance(targetPos, this.transform.position));
            if (disToTargetPos > disToTargetPosMin || isTargetPosReached)
            {
                // 目標地点到達後は回転させずに移動を続ける
                if (!isTargetPosReached)
                {
                    RotateTowardsMovementDirection(targetPos, rotationSpeed);
                }
                MoveTowardsTarget(moveSpeed);
            }
            else
            {
                // 目標地点に到達後、カウント開始する
                isTargetPosReached = true;
            }
            yield return new WaitForSeconds(0.1f);

            if (isTargetPosReached) time += 0.1f;
        }

        StartCoroutine(NextDecision(decisionTime));
    }

    /// <summary>
    /// 外部から停止されるまでゆっくりと移動し続ける処理
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveGraduallyCoroutine()
    {
        Vector3 targetPos = GetNextTargetPosition();
        while (true)
        {
            RotateTowardsMovementDirection(targetPos, rotationSpeed / 2);
            MoveTowardsTarget(moveSpeed / 10);
            float disToTargetPos = Mathf.Abs(Vector3.Distance(targetPos, this.transform.position));
            if (disToTargetPos <= disToTargetPosMin / 2)
            {
                targetPos = GetNextTargetPosition();
            }
            yield return null;
        }
    }

    #endregion

    #region ヒット処理関連

    /// <summary>
    /// ダメージを受けたときの処理
    /// </summary>
    protected override void OnHit()
    {
        base.OnHit();
        SetAnimId((int)ANIM_ID.Hit);
    }

    /// <summary>
    /// 死亡するときに呼ばれる処理処理
    /// </summary>
    /// <returns></returns>
    protected override void OnDead()
    {
        SetAnimId((int)ANIM_ID.Dead);
    }

    #endregion

    #region チェック処理関連

    /// <summary>
    /// playerリストからランダムにターゲットを決める処理
    /// </summary>
    bool SetRandomTargetPlayer()
    {
        List<GameObject> alivePlayers = new List<GameObject>();
        foreach (GameObject player in Players)
        {
            if (player)
            {
                alivePlayers.Add(player);
            }
        }

        if (alivePlayers.Count > 0) target = alivePlayers[Random.Range(0, alivePlayers.Count)];

        return target;
    }

    /// <summary>
    /// ステージの中央の座標を設定
    /// </summary>
    /// <param name="stageCenter"></param>
    public void SetStageCenterParam(Vector2 stageCenter)
    {
        this.stageCenter = stageCenter;
    }

    /// <summary>
    /// 次の目標地点を取得する
    /// </summary>
    /// <returns></returns>
    Vector2 GetNextTargetPosition()
    {
        Vector2 targetPos = stageCenter;
        if (target && SetRandomTargetPlayer())
        {
            targetPos = target.transform.position;
        }
        else
        {
            for (int i = 0; i < 10; i++)
            {
                // ターゲットのプレイヤーが存在しない場合、ランダムな地点に移動する
                Vector2 maxPos = stageCenter + Vector2.one * moveRange;
                Vector2 minPos = stageCenter + Vector2.one * -moveRange;
                targetPos = new Vector2(Random.Range(minPos.x, maxPos.x + 1), Random.Range(minPos.y, maxPos.y + 1));

                // ランダムな目標地点との距離が一定以上離れていれば確定する
                float distance = Mathf.Abs(Vector3.Distance(targetPos, this.transform.position));
                if (distance >= moveRange)
                {
                    break;
                }
            }
        }
        return targetPos;
    }

    /// <summary>
    /// 検出範囲の描画処理
    /// </summary>
    protected override void DrawDetectionGizmos()
    {
        // 攻撃開始距離
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackDist);

        // 攻撃範囲
        if (meleeAttackCheck)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleeAttackCheck.transform.position, meleeAttackRange);
        }

        // 移動範囲
        if (stageCenter != Vector2.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(stageCenter, moveRange);
        }
    }

    #endregion

}
