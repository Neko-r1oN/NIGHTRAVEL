//**************************************************
//  [敵] ドローンを制御するクラス
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Drone : EnemyBase
{
    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        Hit,
        Dead,
    }

    /// <summary>
    /// 行動パターン
    /// </summary>
    public enum DECIDE_TYPE
    {
        Waiting = 1,
        Attack,
        Patrole,
        Tracking,
        RndMove,
    }
    DECIDE_TYPE nextDecide = DECIDE_TYPE.Waiting;

    /// <summary>
    /// 管理するコルーチンの種類
    /// </summary>
    public enum COROUTINE
    {
        NextDecision,
        RangeAttack,
        AttackCooldown,
        PatorolCoroutine
    }

    #region コンポーネント
    EnemyProjectileChecker projectileChecker;
    #endregion

    #region オリジナルステータス
    [Foldout("ステータス")]
    [SerializeField]
    float patorolRange = 10f;
    #endregion

    #region 攻撃関連
    [Foldout("攻撃関連")]
    [SerializeField] 
    Transform aimTransform; // 銃のAIM部分
    [Foldout("攻撃関連")]
    [SerializeField] 
    GunParticleController gunPsController;
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

    #region 抽選関連
    float decisionTimeMax = 2f;
    float randomDecision;
    bool endDecision;
    #endregion

    Vector2? startPatorolPoint = null;

    protected override void Start()
    {
        base.Start();
        projectileChecker = aimTransform.GetComponent<EnemyProjectileChecker>();
        isAttacking = false;
        doOnceDecision = true;
        NextDecision();
    }

    /// <summary>
    /// 行動パターン実行処理
    /// </summary>
    protected override void DecideBehavior()
    {
        if (doOnceDecision)
        {
            doOnceDecision = false;

            switch (nextDecide)
            {
                case DECIDE_TYPE.Waiting:
                    chaseAI.Stop();
                    Idle();
                    NextDecision();
                    break;
                case DECIDE_TYPE.Attack:
                    chaseAI.Stop();
                    Attack();
                    break;
                case DECIDE_TYPE.Patrole:
                    Patorol();
                    break;
                case DECIDE_TYPE.Tracking:
                    Tracking();
                    NextDecision();
                    break;
                case DECIDE_TYPE.RndMove:
                    chaseAI.DoRndMove();
                    NextDecision(2f);
                    break;
            }

            SetAnimId((int)ANIM_ID.Idle);
        }
    }

    /// <summary>
    /// アイドル処理
    /// </summary>
    protected override void Idle()
    {
        m_rb2d.linearVelocity = new Vector2(0f, m_rb2d.linearVelocity.y);
    }

    #region 抽選処理関連

    /// <summary>
    /// 抽選処理を呼ぶ
    /// </summary>
    /// <param name="time"></param>
    void NextDecision(float? rndMaxTime = null)
    {
        if (rndMaxTime == null) rndMaxTime = decisionTimeMax;
        float time = UnityEngine.Random.Range(0.1f, (float)rndMaxTime);

        // 実行していなければ、行動の抽選のコルーチンを開始
        string key = COROUTINE.NextDecision.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(NextDecisionCoroutine(time, () => { RemoveCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// 次の行動パターン決定処理
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator NextDecisionCoroutine(float time, Action onFinished)
    {
        yield return new WaitForSeconds(time);

        // 各行動パターンの重み
        int waitingWeight = 0, attackWeight = 0, patorolWeight = 0, trackingWeight = 0, rndMoveWeight = 0;

        // 攻撃が可能な場合
        if (canAttack && projectileChecker.CanFireProjectile(target) && !sightChecker.IsObstructed())
        {
            int weightRate = nextDecide == DECIDE_TYPE.Attack ? 3 : 1;
            attackWeight = 10 / weightRate;
            rndMoveWeight = 5 * weightRate;
        }
        else if(target)
        {
            trackingWeight = 10;
        }
        else if (canPatrol && !isPatrolPaused)
        {
            patorolWeight = 10;
        }
        else
        {
            waitingWeight = 10;
        }

        // 全体の長さを使って抽選
        int totalWeight = waitingWeight + attackWeight + patorolWeight + trackingWeight + rndMoveWeight;
        randomDecision = UnityEngine.Random.Range(1, totalWeight + 1);

        // 抽選した値で次の行動パターンを決定する
        if (randomDecision <= waitingWeight) nextDecide = DECIDE_TYPE.Waiting;
        else if (randomDecision <= attackWeight) nextDecide = DECIDE_TYPE.Attack;
        else if (randomDecision <= patorolWeight) nextDecide = DECIDE_TYPE.Patrole;
        else if (randomDecision <= trackingWeight) nextDecide = DECIDE_TYPE.Tracking;
        else nextDecide = DECIDE_TYPE.RndMove;

        doOnceDecision = true;
        onFinished?.Invoke();
    }

    #endregion

    #region テクスチャ・アニメーション関連

    /// <summary>
    /// スポーンアニメメーション開始時
    /// </summary>
    public override void OnSpawnAnimEvent()
    {
        base.OnSpawnAnimEvent();
        SetAnimId((int)ANIM_ID.Idle);
        chaseAI.DoRndMove();
    }

    /// <summary>
    /// スポーンアニメーションが終了したとき
    /// </summary>
    public override void OnEndSpawnAnimEvent()
    {
        base.OnEndSpawnAnimEvent();
        chaseAI.Stop();
        ApplyStun(0.5f, false);
    }

    /// <summary>
    /// アニメーション設定処理
    /// </summary>
    /// <param name="id"></param>
    public override void SetAnimId(int id)
    {
        if (animator == null) return;
        animator.SetInteger("animation_id", id);

        switch (id)
        {
            case (int)ANIM_ID.Hit:
                animator.Play("Hit");
                break;
            default:
                break;
        }
    }

    #endregion

    #region 攻撃処理関連

    /// <summary>
    /// 攻撃処理
    /// </summary>
    void Attack()
    {
        doOnceDecision = false;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        chaseAI.Stop();

        // 実行していなければ、遠距離攻撃のコルーチンを開始
        string key = COROUTINE.RangeAttack.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(RangeAttack(() => { RemoveCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// 遠距離攻撃処理
    /// </summary>
    IEnumerator RangeAttack(Action onFinished)
    {
        yield return new WaitForSeconds(0.25f);  // 攻撃開始を遅延
        gunPsController.StartShooting();

        float time = 0;
        float waitSec = 0.05f;
        while (time < shotsPerSecond)
        {
            // ターゲットのいる方向に向かってエイム
            if (target)
            {
                // 強すぎので一旦コメントアウト
                //if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
                //    || target.transform.position.x > transform.position.x && transform.localScale.x < 0) Flip();

                Vector3 direction = (target.transform.position - transform.position).normalized;
                projectileChecker.RotateAimTransform(direction);
            }
            yield return new WaitForSeconds(waitSec);
            time += waitSec;
        }

        // 実行していなければ、クールダウンのコルーチンを開始
        string key = COROUTINE.AttackCooldown.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(AttackCooldown(attackCoolTime, () => { RemoveCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
        onFinished?.Invoke();
    }

    /// <summary>
    /// 攻撃時のクールダウン処理
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time, Action onFinished)
    {
        gunPsController.StopShooting();
        isAttacking = true;
        yield return new WaitForSeconds(time);
        isAttacking = false;
        doOnceDecision = true;
        Idle();
        NextDecision();
        onFinished?.Invoke();
    }

    #endregion

    #region 移動処理関連

    /// <summary>
    /// 追跡する処理
    /// </summary>
    protected override void Tracking()
    {
        aimTransform.localEulerAngles = Vector3.back * 90f; // 銃の向きを初期化
        StopPatorol();
        chaseAI.DoChase(target);
    }

    /// <summary>
    /// 巡回処理
    /// </summary>
    protected override void Patorol()
    {
        aimTransform.localEulerAngles = Vector3.back * 90f; // 銃の向きを初期化

        // 実行していなければ、パトロールのコルーチンを開始
        string key = COROUTINE.PatorolCoroutine.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(PatorolCoroutine(() => { RemoveCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// 巡回する処理
    /// </summary>
    IEnumerator PatorolCoroutine(Action onFinished)
    {
        float pauseTime = 2f;
        if (startPatorolPoint == null)
        {
            startPatorolPoint = transform.position;
        }

        if (IsWall()) Flip();

        if (TransformUtils.GetFacingDirection(transform) > 0)
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
        else if (TransformUtils.GetFacingDirection(transform) < 0)
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
        speedVec = new Vector2(TransformUtils.GetFacingDirection(transform) * moveSpeed / 2, m_rb2d.linearVelocity.y);
        m_rb2d.linearVelocity = speedVec;
        NextDecision();
        onFinished?.Invoke();
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
        SetAnimId((int)ANIM_ID.Hit);
        gunPsController.StopShooting();
        if(hp > 0) NextDecision();
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
        if (projectileChecker != null)
        {
            projectileChecker.DrawProjectileRayGizmo(target);
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
