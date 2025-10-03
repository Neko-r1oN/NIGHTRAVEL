//**************************************************
//  [敵] ブレイズのクラス
//  Author:r-enomoto
//**************************************************
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Blaze : EnemyBase
{
    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        Attack_Laser,
        Attack_Punch,
        Hit,
        Dead,
    }

    /// <summary>
    /// 行動パターン
    /// </summary>
    public enum DECIDE_TYPE
    {
        Waiting = 1,
        Attack_Laser,
        Attack_Punch,
        AirMovement,
        Tracking,
        BackOff,
    }
    DECIDE_TYPE nextDecide = DECIDE_TYPE.Waiting;

    /// <summary>
    /// 管理するコルーチンの種類
    /// </summary>
    public enum COROUTINE
    {
        NextDecision,
        BackOff,
        AttackCooldown,
    }

    #region 攻撃関連
    [Foldout("攻撃関連")]
    [SerializeField]
    Transform aimTransform; // AIM部分
    #endregion

    #region チェック関連
    // 近距離攻撃の範囲
    [Foldout("チェック関連")]
    [SerializeField]
    Transform meleeAttackCheck;
    [Foldout("チェック関連")]
    [SerializeField] float meleeAttackRange = 0.9f;

    // 壁・地面チェック
    [Foldout("チェック関連")]
    [SerializeField]
    Transform wallCheck;
    [Foldout("チェック関連")]
    [SerializeField]
    Vector2 wallCheckRadius = new Vector2(0, 1.5f);
    [Foldout("チェック関連")]
    [SerializeField]
    Transform groundCheck;
    [Foldout("チェック関連")]
    [SerializeField]
    Vector2 groundCheckRadius = new Vector2(0.5f, 0.2f);

    // 落下チェック
    [Foldout("チェック関連")]
    [SerializeField]
    Transform frontFallCheck;
    [Foldout("チェック関連")]
    [SerializeField]
    Vector2 frontFallCheckRange;

    [Foldout("チェック関連")]
    [SerializeField]
    Transform backFallCheck;
    [Foldout("チェック関連")]
    [SerializeField]
    Vector2 backFallCheckRange;
    #endregion

    #region 抽選関連
    float decisionTimeMax = 2f;
    float randomDecision;
    bool endDecision;
    #endregion

    #region オリジナル
    EnemyProjectileChecker projectileChecker;
    #endregion


    #region オーディオ関連

    [SerializeField]
    [Foldout("オーディオ")]
    AudioSource audioPunch;

    [SerializeField]
    [Foldout("オーディオ")]
    AudioSource audioFlash;

    [SerializeField]
    [Foldout("オーディオ")]
    AudioSource audioLaser;

    #endregion

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
                    Idle();
                    NextDecision();
                    break;
                case DECIDE_TYPE.Attack_Laser:
                    AttackLaser();
                    break;
                case DECIDE_TYPE.Attack_Punch:
                    AttackPunch();
                    break;
                case DECIDE_TYPE.AirMovement:
                    AirMovement();
                    NextDecision(false);
                    break;
                case DECIDE_TYPE.Tracking:
                    Tracking();
                    NextDecision(false);
                    break;
                case DECIDE_TYPE.BackOff:
                    StartBackOff();
                    break;
            }
        }
    }

    /// <summary>
    /// アイドル処理
    /// </summary>
    protected override void Idle()
    {
        SetAnimId((int)ANIM_ID.Idle);
        m_rb2d.linearVelocity = new Vector2(0f, m_rb2d.linearVelocity.y);
    }

    #region 抽選処理関連

    /// <summary>
    /// 抽選処理を呼ぶ
    /// </summary>
    /// <param name="time"></param>
    void NextDecision(bool isRndTime = true, float? rndMaxTime = null)
    {
        float time = 0;
        if (isRndTime)
        {
            if (rndMaxTime == null) rndMaxTime = decisionTimeMax;
            time = UnityEngine.Random.Range(0.1f, (float)rndMaxTime);
        }

        // 実行していなければ、行動の抽選のコルーチンを開始
        string key = COROUTINE.NextDecision.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(NextDecisionCoroutine(time, () => { RemoveAndStopCoroutineByKey(key); }));
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

        #region 各行動パターンの重み付け

        const float punchDist = 4f;
        bool canAttackPunch = canAttack && !sightChecker.IsObstructed() && disToTarget <= punchDist;
        bool canAttackLaser = canAttack && projectileChecker.CanFireProjectile(target) && !sightChecker.IsObstructed() && 
            disToTarget >= punchDist && disToTarget <= attackDist;
        Dictionary<DECIDE_TYPE, int> weights = new Dictionary<DECIDE_TYPE, int>();

        if (canChaseTarget && !IsGround())
        {
            weights[DECIDE_TYPE.AirMovement] = 10;
        }
        else if (canAttackPunch || canAttackLaser)
        {
            bool wasAttacking = nextDecide == DECIDE_TYPE.Attack_Laser || nextDecide == DECIDE_TYPE.Attack_Punch;
            if(!IsBackFall()) weights[DECIDE_TYPE.BackOff] = wasAttacking ? 15 : 5;

            if (canAttackPunch) weights[DECIDE_TYPE.Attack_Punch] = wasAttacking ? 5 : 15;
            else if(canAttackLaser) weights[DECIDE_TYPE.Attack_Laser] = wasAttacking ? 5 : 15;
        }
        else if (canChaseTarget && target)
        {
            weights[DECIDE_TYPE.Tracking] = 10;
        }
        else
        {
            weights[DECIDE_TYPE.Waiting] = 10;
        }

        // valueを基準に昇順で並べ替え
        var sortedWeights = weights.OrderBy(x => x.Value);
        #endregion

        // 全体の長さを使って抽選
        int totalWeight = weights.Values.Sum();
        randomDecision = UnityEngine.Random.Range(1, totalWeight + 1);

        // 抽選した値で次の行動パターンを決定する
        int currentWeight = 0;
        foreach (var weight in sortedWeights)
        {
            currentWeight += weight.Value;
            if (currentWeight >= randomDecision)
            {
                nextDecide = weight.Key;
                break;
            }
        }

        doOnceDecision = true;
        onFinished?.Invoke();
    }

    #endregion

    #region 攻撃処理関連

    #region パンチ

    /// <summary>
    /// パンチによる攻撃処理開始
    /// </summary>
    void AttackPunch()
    {
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Punch);
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] パンチ処理
    /// </summary>
    public override void OnAttackAnim2Event()
    {
        audioPunch.Play();

        // 自身がエリート個体の場合、付与する状態異常の種類を取得する
        DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();

        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Player")
            {
                collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position, KB_POW.Big, applyEffect);
            }
        }
    }

    #endregion

    #region レーザー

    /// <summary>
    /// レーザーによる攻撃処理開始
    /// </summary>
    void AttackLaser()
    {
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Laser);

        audioFlash.Play();
    }

    /// <summary>
    /// [アニメーションイベントから呼び出し] フラッシュ音
    /// </summary>
    public override void OnAttackAnim3Event()
    {
        audioFlash.Play();
    }

    /// <summary>
    /// [アニメーションイベントから呼び出し] レーザー音
    /// </summary>
    public override void OnAttackAnim4Event()
    {
        audioLaser.Play();
    }

    #endregion

    /// <summary>
    /// [Animationイベントからの呼び出し] 攻撃クールダウン処理
    /// </summary>
    public override void OnEndAttackAnimEvent()
    {
        // 実行していなければ、クールダウンのコルーチンを開始
        string cooldownKey = COROUTINE.AttackCooldown.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(AttackCooldown(attackCoolTime, () => {
                RemoveAndStopCoroutineByKey(cooldownKey);
            }));
            managedCoroutines.Add(cooldownKey, coroutine);
        }
    }

    /// <summary>
    /// 攻撃時のクールダウン処理
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time, Action onFinished)
    {
        isAttacking = true;
        Idle();
        yield return new WaitForSeconds(time);
        isAttacking = false;
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
        SetAnimId((int)ANIM_ID.Idle);
        Vector2 speedVec = Vector2.zero;
        if (IsFrontFall() || IsWall())
        {
            speedVec = new Vector2(0f, m_rb2d.linearVelocity.y);
        }
        else
        {
            float distToPlayer = target.transform.position.x - this.transform.position.x;
            speedVec = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed, m_rb2d.linearVelocity.y);
        }
        m_rb2d.linearVelocity = speedVec;
    }

    /// <summary>
    /// 空中状態での移動処理
    /// </summary>
    void AirMovement()
    {
        SetAnimId((int)ANIM_ID.Idle);

        // ジャンプ(落下)中にプレイヤーに向かって移動する
        float distToPlayer = target.transform.position.x - this.transform.position.x;
        Vector3 targetVelocity = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed, m_rb2d.linearVelocity.y);
        Vector3 velocity = Vector3.zero;
        m_rb2d.linearVelocity = Vector3.SmoothDamp(m_rb2d.linearVelocity, targetVelocity, ref velocity, 0.05f);
    }

    /// <summary>
    /// BackOffCoroutineを呼び出す
    /// </summary>
    void StartBackOff()
    {
        float coroutineTime = UnityEngine.Random.Range(1f, 3f);

        // 実行していなければ、クールダウンのコルーチンを開始
        string cooldownKey = COROUTINE.BackOff.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(BackOffCoroutine(coroutineTime, () => {
                RemoveAndStopCoroutineByKey(cooldownKey);
            }));
            managedCoroutines.Add(cooldownKey, coroutine);
        }
    }

    /// <summary>
    /// 距離をとり続けるコルーチン
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator BackOffCoroutine(float time, Action onFinished)
    {
        float waitSec = 0.1f;
        while (time > 0)
        {
            time -= waitSec;
            BackOff();
            yield return new WaitForSeconds(waitSec);
        }

        NextDecision(false);
        onFinished?.Invoke();
    }

    /// <summary>
    /// 距離をとる
    /// </summary>
    void BackOff()
    {
        SetAnimId((int)ANIM_ID.Idle);

        Vector2 speedVec = Vector2.zero;
        if (IsBackFall())
        {
            speedVec = new Vector2(0f, m_rb2d.linearVelocity.y);
        }
        else
        {
            float distToPlayer = target.transform.position.x - this.transform.position.x;
            speedVec = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed * -0.8f, m_rb2d.linearVelocity.y);
        }
        m_rb2d.linearVelocity = speedVec;
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
    /// 死亡するときに呼ばれる処理
    /// </summary>
    /// <returns></returns>
    protected override void OnDead()
    {
        SetAnimId((int)ANIM_ID.Dead);
    }

    #endregion

    #region テクスチャ・アニメーション関連

    /// <summary>
    /// スポーンアニメーションが終了したとき
    /// </summary>
    public override void OnEndSpawnAnimEvent()
    {
        base.OnEndSpawnAnimEvent();
        ApplyStun(0.5f, false);
    }

    /// <summary>
    /// 指定したアニメーションIDがヒットアニメーションかどうか
    /// </summary>
    /// <param name="animationId"></param>
    /// <returns></returns>
    public override bool IsHitAnimIdFrom(int animationId)
    {
        return animationId == (int)ANIM_ID.Hit;
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
                animator.Play("Hit_Blaze2", 0, 0);
                break;
            default:
                break;
        }
    }
    #endregion

    #region リアルタイム同期関連

    /// <summary>
    /// マスタクライアント切り替え時に状態をリセットする
    /// </summary>
    public override void ResetAllStates()
    {
        base.ResetAllStates();

        if (target == null)
        {
            target = sightChecker.GetTargetInSight();
        }

        DecideBehavior();
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
    /// 目の前に足場がないかどうか
    /// </summary>
    /// <returns></returns>
    bool IsFrontFall()
    {
        return !Physics2D.OverlapBox(frontFallCheck.position, frontFallCheckRange, 0, terrainLayerMask);
    }

    /// <summary>
    /// 後ろに足場がないかどうか
    /// </summary>
    /// <returns></returns>
    bool IsBackFall()
    {
        return !Physics2D.OverlapBox(backFallCheck.position, frontFallCheckRange, 0, terrainLayerMask);
    }

    /// <summary>
    /// 地面判定
    /// </summary>
    /// <returns></returns>
    bool IsGround()
    {
        // 足元に２つの始点と終点を作成する
        Vector3 leftStartPosition = groundCheck.transform.position + Vector3.left * groundCheckRadius.x / 2;
        Vector3 rightStartPosition = groundCheck.transform.position + Vector3.right * groundCheckRadius.x / 2;
        Vector3 endPosition = groundCheck.transform.position + Vector3.down * groundCheckRadius.y;

        return Physics2D.Linecast(leftStartPosition, endPosition, terrainLayerMask)
            || Physics2D.Linecast(rightStartPosition, endPosition, terrainLayerMask);
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

        // 壁の判定
        if (wallCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(wallCheck.transform.position, wallCheckRadius);
        }

        // 地面判定
        if (groundCheck)
        {
            Vector3 leftStartPosition = groundCheck.transform.position + Vector3.left * groundCheckRadius.x / 2;
            Vector3 rightStartPosition = groundCheck.transform.position + Vector3.right * groundCheckRadius.x / 2;
            Vector3 endPosition = groundCheck.transform.position + Vector3.down * groundCheckRadius.y;
            Gizmos.DrawLine(leftStartPosition, endPosition);
            Gizmos.DrawLine(rightStartPosition, endPosition);
        }

        // 落下チェック
        if (frontFallCheck && backFallCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(frontFallCheck.position, frontFallCheckRange);
            Gizmos.DrawWireCube(backFallCheck.position, backFallCheckRange);
        }
    }

    #endregion
}
