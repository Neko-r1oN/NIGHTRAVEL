//**************************************************
//  [敵] ヴァルクス・コードクリスタルのクラス
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
using Unity.Cinemachine;

public class ValksCodeCrystal : EnemyBase
{
    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        Attack_NormalCombo,
        Attack_PunchCombo,
        Attack_Dive,
        Attack_Laser,
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
        Attack_NormalCombo,
        Attack_PunchCombo,
        Attack_Dive,
        Attack_Laser,
        Tracking,
        BackOff,
    }
    DECIDE_TYPE nextDecide = DECIDE_TYPE.Waiting;
    DECIDE_TYPE lastAttackPattern;

    /// <summary>
    /// 管理するコルーチンの種類
    /// </summary>
    public enum COROUTINE
    {
        NextDecision,
        Attack_NormalCombo,
        Attack_PunchCombo,
        Attack_Dive,
        Attack_Laser,
        AttackCooldown,
        Tracking,
        BackOff,
    }

    #region チェック関連
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

    #region 氷の岩のパーティクル生成関連
    [Foldout("攻撃関連")]
    [SerializeField]
    List<GameObject> iceRockPsPrefabs = new List<GameObject>();

    [Foldout("攻撃関連")]
    [SerializeField]
    List<Transform> iceRockPsPoints = new List<Transform>();

    [Foldout("攻撃関連")]
    [SerializeField]
    List<float> iceRockPsPointsY = new List<float>();
    #endregion

    [Foldout("攻撃関連")]
    [SerializeField]
    List<GameObject> damageColliders = new List<GameObject>();

    [Foldout("攻撃関連")]
    [SerializeField]
    Vector2 stageCenterPosition;

    Vector2? targetPos;
    const float warpPosY = 5f;

    // 落下攻撃、レーザー攻撃を発動できるまでの条件回数
    const int attackDiveUnlockCount = 2;
    const int attackLaserUnlockCount = 4;
    int nonDiveAttackCount = 0;
    int nonLaserAttackCount = 0;
    #endregion

    #region カメラ関連
    [Foldout("カメラ関連")]
    [SerializeField]
    Transform bone1;

    [Foldout("カメラ関連")]
    [SerializeField]
    string targetGroupName;

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
            doOnceDecision = false;

            switch (nextDecide)
            {
                case DECIDE_TYPE.Waiting:
                    Idle();
                    NextDecision();
                    break;
                case DECIDE_TYPE.Attack_NormalCombo:
                    AttackNormalCombo();
                    break;
                case DECIDE_TYPE.Attack_PunchCombo:
                    AttackPunchCombo();
                    break;
                case DECIDE_TYPE.Attack_Dive:
                    AttackDive();
                    break;
                case DECIDE_TYPE.Attack_Laser:
                    AttackLaser();
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
        bool wasAttacking = nextDecide == DECIDE_TYPE.Attack_NormalCombo || nextDecide == DECIDE_TYPE.Attack_PunchCombo || nextDecide == DECIDE_TYPE.Attack_Dive || nextDecide == DECIDE_TYPE.Attack_Laser;
        bool canAttackNormal = IsNormalAttack();
        bool canAttackSmash = hp <= maxHp * 0.75f && lastAttackPattern != DECIDE_TYPE.Attack_Dive && nonDiveAttackCount >= attackDiveUnlockCount;
        bool canAttackLaser = hp <= maxHp * 0.5f && lastAttackPattern != DECIDE_TYPE.Attack_Laser && nonLaserAttackCount >= attackLaserUnlockCount;

        // 条件を基に該当する行動パターンに重み付け
        if (canChaseTarget && !IsGround())
        {
            weights[DECIDE_TYPE.Waiting] = 10;
        }
        else if (canAttackNormal || canAttackSmash || canAttackLaser)
        {
            if (!IsBackFall() && nextDecide != DECIDE_TYPE.Tracking && nextDecide != DECIDE_TYPE.BackOff) weights[DECIDE_TYPE.BackOff] = wasAttacking ? 15 : 5;

            if (canAttackNormal) weights[DECIDE_TYPE.Attack_NormalCombo] = wasAttacking ? 5 : 15;
            if (canAttackNormal) weights[DECIDE_TYPE.Attack_PunchCombo] = wasAttacking ? 5 : 15;
            if (canAttackSmash) weights[DECIDE_TYPE.Attack_Dive] = 30;
            if (canAttackLaser) weights[DECIDE_TYPE.Attack_Laser] = 30;
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
    /// 全てのダメージ用コライダーを非アクティブにする
    /// </summary>
    public void DisableAllDamageColliders()
    {
        foreach (var collider in damageColliders)
        {
            collider.SetActive(false);
        }
    }

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
        RemoveTargetGroup();
        DisableAllDamageColliders();
        isAttacking = true;
        Idle();
        yield return new WaitForSeconds(time);
        SelectNewTargetInBossRoom();
        isAttacking = false;
        NextDecision();
        onFinished?.Invoke();
    }

    #endregion

    #region 通常のコンボ攻撃

    /// <summary>
    /// 通常のコンボ技
    /// </summary>
    void AttackNormalCombo()
    {
        nonDiveAttackCount++;
        nonLaserAttackCount++;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_NormalCombo);
    }

    /// <summary>
    /// [AnimationEventから呼び出し] ターゲットのいる方向を向き 
    /// </summary>
    public override void OnAttackAnimEvent()
    {
        m_rb2d.linearVelocity = Vector2.zero;
        if (!IsNormalAttack())
        {
            var nearTarget = GetNearPlayer();
            if(!nearTarget) target = nearTarget;
        }
        LookAtTarget();
    }

    #endregion

    #region パンチのコンボ攻撃

    /// <summary>
    /// パンチのコンボ技
    /// </summary>
    void AttackPunchCombo()
    {
        nonDiveAttackCount++;
        nonLaserAttackCount++;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_PunchCombo);
    }

    /// <summary>
    /// [AnimationEventから呼び出し] 勢い良く前進する
    /// </summary>
    public override void OnAttackAnim2Event()
    {
        m_rb2d.linearVelocity = Vector2.zero;
        if (!IsNormalAttack())
        {
            var nearTarget = GetNearPlayer();
            if (!nearTarget) target = nearTarget;
        }
        LookAtTarget();

        Vector2 vec = new Vector2(TransformUtils.GetFacingDirection(transform) * moveSpeed * 2, 0f);
        m_rb2d.AddForce(vec, ForceMode2D.Impulse);
    }

    /// <summary>
    /// [AnimationEventから呼び出し] さらに勢い良く前進する
    /// </summary>
    public override void OnEndAttackAnim2Event()
    {
        m_rb2d.linearVelocity = Vector2.zero;
        if (!IsNormalAttack())
        {
            var nearTarget = GetNearPlayer();
            if (!nearTarget) target = nearTarget;
        }
        LookAtTarget();

        Vector2 vec = new Vector2(TransformUtils.GetFacingDirection(transform) * moveSpeed * 4, 0f);
        m_rb2d.AddForce(vec, ForceMode2D.Impulse);
    }

    #endregion

    #region 落下攻撃

    /// <summary>
    /// 落下攻撃
    /// </summary>
    void AttackDive()
    {
        // 目標座標をキープしておく
        targetPos = GetGroundPointFrom(target);
        if (targetPos == null)
        {
            NextDecision();
            return;
        }

        nonLaserAttackCount++;
        nonDiveAttackCount = 0;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Dive);
    }

    /// <summary>
    /// [AnimationEventから呼び出し] ターゲットの座標にワープする 
    /// </summary>
    public override void OnAttackAnim3Event()
    {
        m_rb2d.linearVelocity = Vector2.zero;
        if (!target) SelectNewTargetInBossRoom();
        if (target) targetPos = target.transform.position;
        transform.position = (Vector2)targetPos;
        LookAtTarget();
    }

    /// <summary>
    /// [AnimationEventから呼び出し] 一番近いターゲットのいる方向を向く
    /// </summary>
    public override void OnEndAttackAnim3Event()
    {
        var nearTarget = GetNearPlayer();
        if (nearTarget) target = nearTarget;
        if (target) LookAtTarget();
    }

    #endregion

    #region レーザー攻撃

    /// <summary>
    /// レーザー攻撃
    /// </summary>
    void AttackLaser()
    {
        nonDiveAttackCount++;
        nonLaserAttackCount = 0;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Laser);
    }

    /// <summary>
    /// [AnimationEventから呼び出し] ステージの中央にワープする
    /// </summary>
    public override void OnAttackAnim4Event()
    {
        // ステージの中央に移動
        transform.position = stageCenterPosition;

        AddTargetGroup();
    }

    /// <summary>
    /// [AnimationEventから呼び出し] 一番近いターゲットのいる方向を向く
    /// </summary>
    public override void OnEndAttackAnim4Event()
    {
        var nearTarget = GetNearPlayer();
        if(nearTarget) target = nearTarget;
        if (target) LookAtTarget();
    }

    #endregion

    /// <summary>
    /// [AnimationEventから呼び出し] 前方向に氷の岩のパーティクルを生成する
    /// </summary>
    public override void OnAnimEventOption1()
    {
        Vector2 point = new Vector2(iceRockPsPoints[0].position.x, iceRockPsPointsY[0]);
        var iceRock = Instantiate(iceRockPsPrefabs[1], point, iceRockPsPrefabs[1].transform.rotation);

        float rotationY = transform.localScale.x > 0 ? 90 : -90;
        iceRock.transform.eulerAngles = Vector2.up * rotationY;
    }

    /// <summary>
    /// [AnimationEventから呼び出し] 前後に氷の岩のパーティクルを生成する
    /// </summary>
    public override void OnAnimEventOption2()
    {
        Vector2 point = new Vector2(iceRockPsPoints[0].position.x, iceRockPsPointsY[0]);
        Instantiate(iceRockPsPrefabs[0], point, iceRockPsPrefabs[0].transform.rotation);

        point = new Vector2(iceRockPsPoints[1].position.x, iceRockPsPointsY[1]);
        Instantiate(iceRockPsPrefabs[1], point, iceRockPsPrefabs[1].transform.rotation);
    }

    #endregion

    #region カメラ関連

    /// <summary>
    /// 一時的にCinemachineのTargetGroupにbone1を加える
    /// </summary>
    void AddTargetGroup()
    {
        var targetGroup = GameObject.Find(targetGroupName).GetComponent<CinemachineTargetGroup>();
        var newTarget = new CinemachineTargetGroup.Target
        {
            Object = bone1,
            Radius = 1f,
            Weight = 1f
        };
        targetGroup.Targets.Add(newTarget);
    }

    /// <summary>
    /// ChinemachineのTargetGroupからbone1を除いたリストに修正する
    /// </summary>
    void RemoveTargetGroup()
    {
        var targetGroup = GameObject.Find(targetGroupName).GetComponent<CinemachineTargetGroup>();
        var newTarget = new CinemachineTargetGroup.Target
        {
            Object = CharacterManager.Instance.PlayerObjSelf.transform,
            Radius = 1f,
            Weight = 1f
        };
        targetGroup.Targets = new List<CinemachineTargetGroup.Target>() { newTarget};
    }

    #endregion

    #region 移動処理関連

    /// <summary>
    /// 追従開始
    /// </summary>
    void StartTracking()
    {
        m_rb2d.linearVelocity = new Vector2(0, m_rb2d.linearVelocity.y);
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
        bool isNormakAttack = false;

        while (true)
        {
            // 途中でターゲットを見失う || ターゲットと最低距離まで近づいたら強制終了
            if (!target || disToTargetX <= attackDist) break;

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
            SelectNewTargetInBossRoom();
        }

        onFinished?.Invoke();
        NextDecision(false);
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
        const float coroutineTime = 1.2f;
        m_rb2d.linearVelocity = new Vector2(0, m_rb2d.linearVelocity.y);
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
        float waitSec = 0.01f;
        float currentTime = 0f;
        const float backOffTime = 0.4f;
        const float breakeTime = 0.1f;

        while (currentTime < backOffTime + breakeTime)
        {
            currentTime += waitSec;
            float speedRate = currentTime <= backOffTime ? -1.5f : -1f;

            BackOff(speedRate);
            yield return new WaitForSeconds(waitSec);
        }

        m_rb2d.linearVelocity = new Vector2(0, m_rb2d.linearVelocity.y);
        yield return new WaitForSeconds(time - backOffTime - breakeTime);

        var nearTarget = GetNearPlayer();
        if (IsNormalAttack(nearTarget))
        {
            target = nearTarget;
        }

        NextDecision(false);
        onFinished?.Invoke();
    }

    /// <summary>
    /// 距離をとる
    /// </summary>
    void BackOff(float speedRate)
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
            speedVec = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed * speedRate, m_rb2d.linearVelocity.y);
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

        if (target == null) SelectNewTargetInBossRoom();
        DisableAllDamageColliders();
        DecideBehavior();
    }

    #endregion

    #region チェック処理関連

    /// <summary>
    /// 通常攻撃が可能かどうか
    /// </summary>
    /// <returns></returns>
    bool IsNormalAttack(GameObject target = null)
    {
        if(target == null) target = this.target;
        if (target)
        {
            // ターゲットとの距離を取得する
            disToTarget = Vector3.Distance(target.transform.position, this.transform.position);
            disToTargetX = MathF.Abs(target.transform.position.x - transform.position.x);
        }
        else
        {
            disToTarget = float.MaxValue;
            disToTargetX = float.MaxValue;
        }

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
