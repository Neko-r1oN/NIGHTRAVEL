//**************************************************
//  [敵] ヴァルクスのクラス
//  Author:r-enomoto
//**************************************************
using DG.Tweening;
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Valcus : EnemyBase
{
    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        Attack_Normal,
        Attack_Smash1,
        Attack_Smash2,
        Tracking,
        Backoff,
        Dead,
    }

    /// <summary>
    /// 行動パターン
    /// </summary>
    public enum DECIDE_TYPE
    {
        Waiting = 1,
        Attack_Normal,
        Attack_SmashCombo,
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
        MeleeAttack,
        AttackCombo,
        AttackCooldown,
        Tracking,
        BackOff,
    }

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

    #region 追従関連
    const float disToTargetMin = 0.5f;  // プレイヤーとの最低距離
    #endregion

    #region 攻撃関連
    
    List<GameObject> hitPlayers = new List<GameObject>();   // 攻撃を受けたプレイヤーのリスト

    #region コンボ技

    // 飛び跳ねるときの着地地点
    Vector2? targetPos;
    const float targetPosOffsetY = 3.5f;
    const float targetPosOffsetX = 2.5f;

    #endregion

    #endregion

    #region オーディオ関連
    [SerializeField]
    [Foldout("オーディオ")]
    AudioSource audioStamp;

    [SerializeField]
    [Foldout("オーディオ")]
    AudioSource audioSpurn;

    [SerializeField]
    [Foldout("オーディオ")]
    AudioSource audioWind;
    #endregion

    protected override void Start()
    {
        base.Start();
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
            m_rb2d.gravityScale = 5;
            hitPlayers = new List<GameObject>();
            doOnceDecision = false;

            switch (nextDecide)
            {
                case DECIDE_TYPE.Waiting:
                    Idle();
                    NextDecision();
                    break;
                case DECIDE_TYPE.Attack_Normal:
                    AttackNormal();
                    break;
                case DECIDE_TYPE.Attack_SmashCombo:
                    AttackSmash1();
                    break;
                case DECIDE_TYPE.Tracking:
                    StartTracking();
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
    }

    #region 抽選処理関連

    /// <summary>
    /// 抽選処理を呼ぶ
    /// </summary>
    /// <param name="time"></param>
    void NextDecision(bool isRndTime = true, float? rndMaxTime = null, float rndMinTime = 0.1f)
    {
        float time = 0;
        if (isRndTime)
        {
            if (rndMaxTime == null) rndMaxTime = decisionTimeMax;
            time = UnityEngine.Random.Range(rndMinTime, (float)rndMaxTime);
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
        Dictionary<DECIDE_TYPE, int> weights = new Dictionary<DECIDE_TYPE, int>();
        bool wasAttacking = nextDecide == DECIDE_TYPE.Attack_Normal || nextDecide == DECIDE_TYPE.Attack_SmashCombo;
        bool canAttackNormal = IsNormalAttack();

        // 連続してコンボ技をしないよう調整
        bool canAttackSmashCombo = false;
        if (canAttack && nextDecide != DECIDE_TYPE.Attack_SmashCombo)
        {
            canAttackSmashCombo = SelectNewTargetInBossRoom();
        }

        // 条件を基に該当する行動パターンに重み付け
        if (canChaseTarget && !IsGround())
        {
            weights[DECIDE_TYPE.Waiting] = 10;
        }
        else if (canAttackNormal || canAttackSmashCombo)
        {
            if(!IsBackFall()) weights[DECIDE_TYPE.BackOff] = wasAttacking ? 15 : 5;

            if (canAttackNormal) weights[DECIDE_TYPE.Attack_Normal] = wasAttacking ? 5 : 15;
            else if(canAttackSmashCombo) weights[DECIDE_TYPE.Attack_SmashCombo] = wasAttacking ? 5 : 15;
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

    #region 攻撃の共通処理

    /// <summary>
    /// [AnimationEventからの呼び出し] 攻撃の当たり判定処理
    /// </summary>
    public override void OnAttackAnimEvent()
    {
        if(nextDecide == DECIDE_TYPE.Attack_Normal)
        {
            audioSpurn.Play();
        }
        else
        {
            audioStamp.Play();
        }

        m_rb2d.gravityScale = 5;

        // 実行していなければ、攻撃の判定を繰り返すコルーチンを開始
        string attackKey = COROUTINE.MeleeAttack.ToString();
        if (!ContaintsManagedCoroutine(attackKey))
        {
            Coroutine coroutine = StartCoroutine(MeleeAttack());
            managedCoroutines.Add(attackKey, coroutine);
        }
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] 攻撃の判定処理を終了
    /// </summary>
    public override void OnEndAttackAnimEvent()
    {
        RemoveAndStopCoroutineByKey(COROUTINE.MeleeAttack.ToString());
    }

    /// <summary>
    /// 近接攻撃の判定を繰り返す
    /// </summary>
    /// <returns></returns>
    IEnumerator MeleeAttack()
    {
        while (true)
        {
            // 自身がエリート個体の場合、付与する状態異常の種類を取得する
            DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();

            Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
            for (int i = 0; i < collidersEnemies.Length; i++)
            {
                if (collidersEnemies[i].gameObject.tag == "Player" && !hitPlayers.Contains(collidersEnemies[i].gameObject))
                {
                    hitPlayers.Add(collidersEnemies[i].gameObject);
                    collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position, KB_POW.Medium, applyEffect);
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] 攻撃クールダウン処理
    /// </summary>
    public override void OnEndAttackAnim2Event()
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

    #region 通常攻撃

    /// <summary>
    /// 踏みつける攻撃
    /// </summary>
    void AttackNormal()
    {
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Normal);
    }
    #endregion

    #region コンボ攻撃(前半)

    /// <summary>
    /// 目標座標に向かってジャンプする
    /// </summary>
    void JumpToTargetPosition(float duration)
    {
        if (targetPos != null)
        {
            m_rb2d.gravityScale = 0;
            LookAtTarget();

            Vector2 addVec = new Vector2(-targetPosOffsetX * TransformUtils.GetFacingDirection(transform), targetPosOffsetY);
            transform.DOJump((Vector2)targetPos + addVec, jumpPower, 1, duration).SetEase(Ease.InOutQuad);
            audioWind.Play();
        }
    }

    /// <summary>
    /// 条件を満たしている場合、コンボ技を開始
    /// </summary>
    void AttackSmash1()
    {
        targetPos = GetGroundPointFrom(target);
        if(targetPos == null)
        {
            NextDecision();
            return;
        }

        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Smash1);
    }

    /// <summary>
    /// [Animationからの呼び出し] 目標地点に向かってジャンプを開始
    /// </summary>
    public override void OnAttackAnim3Event()
    {
        const float duration = 0.9f;
        var endValue = GetGroundPointFrom(target);
        if (endValue != null) targetPos = endValue;
        JumpToTargetPosition(duration);
    }

    /// <summary>
    /// [Animationからの呼び出し] Smash1が完了後、攻撃が命中しなかったらAttackSmash2を実行する
    /// </summary>
    public override void OnEndAttackAnim3Event()
    {
        if (!target)
        {
            SelectNewTargetInBossRoom();
        }
        else if(!hitPlayers.Contains(target))
        {
            // 攻撃が現在のターゲットに命中しなかったらAttackSmash2を実行する
            AttackSmash2();
        }
    }

    #endregion

    #region コンボ攻撃(後半)

    /// <summary>
    /// 条件を満たしている場合、コンボ技を開始
    /// </summary>
    void AttackSmash2()
    {
        hitPlayers = new List<GameObject>();
        targetPos = GetGroundPointFrom(target);
        if (targetPos == null)
        {
            NextDecision();
            return;
        }

        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Smash2);
        LookAtTarget();
    }

    /// <summary>
    /// [Animationからの呼び出し] 目標地点に向かってジャンプを開始
    /// </summary>
    public override void OnAttackAnim4Event()
    {
        const float duration = 0.6f;
        var endValue = GetGroundPointFrom(target);
        if (endValue != null) targetPos = endValue;
        JumpToTargetPosition(duration);
    }

    #endregion

    #endregion

    #region 移動処理関連

    /// <summary>
    ///  追従開始
    /// </summary>
    void StartTracking()
    {
        string cooldownKey = COROUTINE.Tracking.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(TrackingCoroutine(() => {
                RemoveAndStopCoroutineByKey(cooldownKey);
            }));
            managedCoroutines.Add(cooldownKey, coroutine);
        }
    }

    /// <summary>
    /// 一定時間orターゲットに接近できるまで追従する
    /// </summary>
    /// <param name="onFinished"></param>
    /// <returns></returns>
    IEnumerator TrackingCoroutine(Action onFinished)
    {
        const float waitSec = 0.1f;
        float trackingTime = 2f;
        bool isNormakAttack = false;

        while (trackingTime > 0)
        {
            // 途中でターゲットを見失う || ターゲットと最低距離まで近づいたら強制終了
            if (!target || disToTargetX <= disToTargetMin) break;

            trackingTime -=waitSec;
            Tracking();

            if (IsNormalAttack())
            {
                isNormakAttack = true;
                break;
            }

            yield return new WaitForSeconds(waitSec);
        }

        // ターゲットに追いつけなかった場合
        if (!isNormakAttack)
        {
            bool isTarget = SelectNewTargetInBossRoom();
            if (isTarget)
            {
                nextDecide = DECIDE_TYPE.Attack_SmashCombo;
                doOnceDecision = true;
            }
            else
            {
                NextDecision(false);
            }
        }
        else
        {
            nextDecide = DECIDE_TYPE.Attack_Normal;
            doOnceDecision = true;
        }

        onFinished?.Invoke();
    }

    /// <summary>
    /// 追跡する処理
    /// </summary>
    protected override void Tracking()
    {
        SetAnimId((int)ANIM_ID.Tracking);
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
    /// BackOffCoroutineを呼び出す
    /// </summary>
    void StartBackOff()
    {
        float coroutineTime = 0.3f;

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
        audioWind.Play();
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
        SetAnimId((int)ANIM_ID.Backoff);

        Vector2 speedVec = Vector2.zero;
        if (IsBackFall())
        {
            speedVec = new Vector2(0f, m_rb2d.linearVelocity.y);
        }
        else
        {
            float distToPlayer = target.transform.position.x - this.transform.position.x;
            speedVec = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed * -2, m_rb2d.linearVelocity.y);
        }
        m_rb2d.linearVelocity = speedVec;
    }

    #endregion

    #region ヒット処理関連

    /// <summary>
    /// 死亡するときに呼ばれる処理
    /// </summary>
    /// <returns></returns>
    protected override void OnDead()
    {
        foreach (var spriteRenderer in SpriteRenderers)
        {
            ColorUtility.TryParseHtmlString("#2A5CFF", out Color color);
            spriteRenderer.material.SetColor("_HitEffectColor", color);
        }
        PlayHitBlendShader(true, 0.4f);

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
    /// 通常攻撃が可能かどうか
    /// </summary>
    /// <returns></returns>
    bool IsNormalAttack()
    {
        return canAttack && !sightChecker.IsObstructed() && disToTarget <= attackDist;
    }

    /// <summary>
    /// ターゲットを起点に下向きにRayを飛ばし、地面の座標を取得
    /// </summary>
    /// <returns></returns>
    Vector2? GetGroundPointFrom(GameObject target)
    {
        const float rayLength = 100;
        Vector2? targetPoint = null;

        if (target != null)
        {
            // 起点(target)から下方向にRayを発射
            var hit = Physics2D.Raycast((Vector2)target.transform.position, Vector2.down, rayLength, terrainLayerMask);

            if (hit.collider != null)
            {
                targetPoint = hit.point;
            }
        }
        
        return targetPoint;
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
