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
        Dead,
    }

    /// <summary>
    /// 管理するコルーチンの種類
    /// </summary>
    public enum COROUTINE
    {
        RangeAttack,
        AttackCooldown,
        PatorolCoroutine
    }

    /// <summary>
    /// 攻撃方法
    /// </summary>
    public enum ATTACK_TYPE_ID
    {
        None,
        RangeType,
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

    Vector2? startPatorolPoint = null;

    protected override void Start()
    {
        base.Start();
        projectileChecker = aimTransform.GetComponent<EnemyProjectileChecker>();
        isAttacking = false;
        doOnceDecision = true;
    }

    /// <summary>
    /// 行動パターン実行処理
    /// </summary>
    protected override void DecideBehavior()
    {
        // 行動パターン
        if (canAttack && projectileChecker.CanFireProjectile(target) && !sightChecker.IsObstructed())
        {
            chaseAI.Stop();
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
            chaseAI.Stop();
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

    /// <summary>
    /// スプライトが透明になるときに呼ばれる処理
    /// </summary>
    protected override void OnTransparentSprites()
    {
        SetAnimId((int)ANIM_ID.Idle);

        // ランダムな場所に向かって少し移動
        float moveRange = 2f;
        float posX = transform.position.x + UnityEngine.Random.Range(-moveRange, moveRange);
        float posY = transform.position.y + UnityEngine.Random.Range(-moveRange, moveRange);
        Vector2 targetPoint = new Vector2(posX, posY);
        chaseAI.DoMove(targetPoint);
    }

    /// <summary>
    /// フェードインが完了したときに呼ばれる処理
    /// </summary>
    protected override void OnFadeInComp()
    {
        chaseAI.Stop();
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
                if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
                    || target.transform.position.x > transform.position.x && transform.localScale.x < 0) Flip();

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
