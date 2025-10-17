//**************************************************
//  [ボス] ボックスガイストのクラス
//  Author:r-enomoto
//**************************************************
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Pixeye.Unity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Boxgeist : EnemyBase
{
    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        Attack_Range,
        Attack_Shotgun,
        Attack_Golem,
        Attack_FallBlock,
        Dead,
    }

    /// <summary>
    /// 行動パターン
    /// </summary>
    public enum DECIDE_TYPE
    {
        Waiting = 1,
        Attack_Range,
        Attack_Shotgun,
        Attack_Golem,
        Attack_FallBlock,
    }
    DECIDE_TYPE nextDecide = DECIDE_TYPE.Waiting;

    /// <summary>
    /// 管理するコルーチンの種類
    /// </summary>
    public enum COROUTINE
    {
        NextDecision,
        AttackRange,
        AttackShotgun,
        AttackGolem,
        AttackFallBlock,
        AttackCooldown,
    }

    #region 攻撃関連
    [Foldout("攻撃関連")]
    [SerializeField]
    GameObject boxBulletPrefab;

    [Foldout("攻撃関連")]
    [SerializeField]
    List<Transform> rangedAttackSpawnPoints = new List<Transform>();
    int currentRangeAttackPoint = 0;

    [Foldout("攻撃関連")]
    [SerializeField]
    List<Transform> shotgunAttackSpawnPoints = new List<Transform>();
    #endregion

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

    #region オーディオ関連

    [SerializeField]
    [Foldout("オーディオ")]
    AudioSource audioCharge;

    [SerializeField]
    [Foldout("オーディオ")]
    AudioSource audioGolem;

    [SerializeField]
    [Foldout("オーディオ")]
    AudioSource audioFallBlock;
    #endregion

    #region オリジナル

    [SerializeField]
    GameObject finishBoxParticleObj;

    Vector3 targetPos;
    float defaultGravityScale;

    [SerializeField]
    float maxMoveTime = 2f;
    [SerializeField]
    float minMoveTime = 0.5f;
    #endregion

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
        doOnceDecision = true;
        targetPos = Vector3.zero;
        defaultGravityScale = m_rb2d.gravityScale;
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
                case DECIDE_TYPE.Attack_Range:
                    StartAttackRange();
                    break;
                case DECIDE_TYPE.Attack_Shotgun:
                    StartAttackShotgun();
                    break;
                case DECIDE_TYPE.Attack_Golem:
                    StartAttackGolem();
                    break;
                case DECIDE_TYPE.Attack_FallBlock:
                    StartAttackFallBlock();
                    break;
            }
        }
    }

    /// <summary>
    /// アイドル処理
    /// </summary>
    protected override void Idle()
    {
        m_rb2d.bodyType = RigidbodyType2D.Dynamic;
        SetAnimId((int)ANIM_ID.Idle);
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
        RemoveAndStopCoroutineByKey(key);
        Coroutine coroutine = StartCoroutine(NextDecisionCoroutine(time, () => { RemoveAndStopCoroutineByKey(key); }));
        managedCoroutines.Add(key, coroutine);
    }

    /// <summary>
    /// 次の行動パターン決定処理
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator NextDecisionCoroutine(float time, Action onFinished)
    {
        yield return new WaitForSeconds(time);

        SelectNewTargetInBossRoom();
        CalculateDistanceToTarget();

        #region 各行動パターンの重み付け

        Dictionary<DECIDE_TYPE, int> weights = new Dictionary<DECIDE_TYPE, int>();

        // 抽選のルール：連続して同じ行動を取らないようにする
        // -----------------------------------------------------------
        // Range    ：ターゲットとは反対方向に移動してから攻撃開始
        // Shotgun  ：ターゲットに近づいてから攻撃開始
        // Golem    ：ターゲットと少し距離を離して攻撃開始
        // FallBlock：ターゲットに近づいてから攻撃開始

        if (canAttack && target)
        {
            weights[DECIDE_TYPE.Attack_Range] = nextDecide != DECIDE_TYPE.Attack_Range ? 30 : 0;
            weights[DECIDE_TYPE.Attack_Shotgun] = nextDecide != DECIDE_TYPE.Attack_Shotgun ? 30 : 0;
            weights[DECIDE_TYPE.Attack_Golem] = nextDecide != DECIDE_TYPE.Attack_Golem ? 20 : 0;
            weights[DECIDE_TYPE.Attack_FallBlock] = nextDecide != DECIDE_TYPE.Attack_FallBlock ? 20 : 0;
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

    /// <summary>
    /// 攻撃処理
    /// </summary>
    void PrepareAttack()
    {
        if (target == null)
        {
            targetPos = Vector3.zero;
            NextDecision();
            return;
        }

        m_rb2d.bodyType = RigidbodyType2D.Dynamic;
        targetPos = target.transform.position;
        doOnceDecision = false;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
    }

    #region AttackRange

    /// <summary>
    /// AttackRange開始
    /// </summary>
    void StartAttackRange()
    {
        PrepareAttack();
        currentRangeAttackPoint = 0;

        // 実行していなければ、クールダウンのコルーチンを開始
        string cooldownKey = COROUTINE.AttackRange.ToString();
        RemoveAndStopCoroutineByKey(cooldownKey);
        Coroutine coroutine = StartCoroutine(AttackRangeCoroutine(() =>
        {
            RemoveAndStopCoroutineByKey(cooldownKey);
        }));
        managedCoroutines.Add(cooldownKey, coroutine);
    }

    /// <summary>
    /// [AttackRange] ターゲットと距離をとってから攻撃を開始する
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackRangeCoroutine(Action onFinished)
    {
        const float waitSec = 0.1f;
        float currentSec = 0;
        Vector2 targetPos = target.transform.position;
        while (Vector2.Distance(targetPos, transform.position) < attackDist * 2 && currentSec <= maxMoveTime)
        {
            BackOff(targetPos);
            yield return new WaitForSeconds(waitSec);
            currentSec += waitSec;
        }

        // 攻撃開始
        isInvincible = true;
        m_rb2d.linearVelocity = Vector2.zero;
        m_rb2d.bodyType = RigidbodyType2D.Static;
        SetAnimId((int)ANIM_ID.Attack_Range);
        onFinished?.Invoke();
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] AttackRangeの弾発射処理
    /// </summary>
    public async override void OnAttackAnimEvent()
    {
        if (targetPos != Vector3.zero)
        {
            if (!target || target && target.GetComponent<CharacterBase>().HP <= 0) target = sightChecker.GetTargetInSight();
            if (target) targetPos = target.transform.position;

            // 自身がエリート個体の場合、付与する状態異常の種類を取得する
            DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();
            List<DEBUFF_TYPE> debuffs = new List<DEBUFF_TYPE>();
            if (applyEffect != null) debuffs.Add((DEBUFF_TYPE)applyEffect);

            // 生成位置と移動ベクトルを取得
            Vector3 spawnPoint = rangedAttackSpawnPoints[currentRangeAttackPoint].position;
            var shootVec = (targetPos - spawnPoint).normalized * 30;
            currentRangeAttackPoint = currentRangeAttackPoint >= rangedAttackSpawnPoints.Count-1 ? 0 : currentRangeAttackPoint+1;

            ShootBulletData shootBulletData = new ShootBulletData()
            {
                Type = PROJECTILE_TYPE.BoxBullet_Big,
                Debuffs = debuffs,
                Power = power,
                SpawnPos = spawnPoint,
                ShootVec = shootVec,
                Rotation = Quaternion.identity,
            };

            if (RoomModel.Instance && RoomModel.Instance.IsMaster)
            {
                // 弾の生成リクエスト
                await RoomModel.Instance.ShootBulletAsync(shootBulletData);
            }
            else if (!RoomModel.Instance)
            {
                var bulletObj = Instantiate(boxBulletPrefab, spawnPoint, Quaternion.identity);
                bulletObj.GetComponent<ProjectileBase>().Init(debuffs, power);
                bulletObj.GetComponent<ProjectileBase>().Shoot(shootVec);
            }
        }
    }

    #endregion

    #region AttackShotgun

    /// <summary>
    /// [AttackShotgun] AttackShotgun開始
    /// </summary>
    void StartAttackShotgun()
    {
        PrepareAttack();

        // 実行していなければ、クールダウンのコルーチンを開始
        string cooldownKey = COROUTINE.AttackShotgun.ToString();
        RemoveAndStopCoroutineByKey(cooldownKey);
        Coroutine coroutine = StartCoroutine(AttackShotgunCoroutine(() =>
        {
            RemoveAndStopCoroutineByKey(cooldownKey);
        }));
        managedCoroutines.Add(cooldownKey, coroutine);
    }

    /// <summary>
    /// [AttackShotgun] ターゲットに近づいてから攻撃開始
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackShotgunCoroutine(Action onFinished)
    {
        const float waitSec = 0.1f;
        float currentSec = 0;
        while (disToTarget > attackDist && currentSec <= maxMoveTime)
        {
            if (!target)
            {
                onFinished?.Invoke();
                CancellAttack();
                yield break;
            }

            CloseIn();
            yield return new WaitForSeconds(waitSec);
            currentSec += waitSec;
        }

        isInvincible = true;
        SetAnimId((int)ANIM_ID.Attack_Shotgun);
        onFinished?.Invoke();
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] AttackShotgunの弾発射処理
    /// </summary>
    public async override void OnAttackAnim2Event()
    {
        if (targetPos != Vector3.zero)
        {
            if (!target || target && target.GetComponent<CharacterBase>().HP <= 0) target = sightChecker.GetTargetInSight();
            if (target) targetPos = target.transform.position;

            // 自身がエリート個体の場合、付与する状態異常の種類を取得する
            DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();
            List<DEBUFF_TYPE> debuffs = new List<DEBUFF_TYPE>();
            if (applyEffect != null) debuffs.Add((DEBUFF_TYPE)applyEffect);

            List<ShootBulletData> shootBulletDatas = new List<ShootBulletData>();
            int currentIndex = 0;
            foreach(var point in shotgunAttackSpawnPoints)
            {
                // 角度をラジアンに変換した移動ベクトルと生成位置を取得
                currentIndex++;
                float angle = 360 - (360 / shotgunAttackSpawnPoints.Count * currentIndex);
                float rad = angle * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
                var shootVec = direction.normalized * 30;
                currentRangeAttackPoint = currentRangeAttackPoint >= rangedAttackSpawnPoints.Count - 1 ? 0 : currentRangeAttackPoint + 1;

                ShootBulletData data = new ShootBulletData()
                {
                    Type = PROJECTILE_TYPE.BoxBullet_Big,
                    Debuffs = debuffs,
                    Power = power,
                    SpawnPos = point.position,
                    ShootVec = shootVec,
                    Rotation = Quaternion.identity,
                };

                shootBulletDatas.Add(data);
            }

            if (RoomModel.Instance && RoomModel.Instance.IsMaster)
            {
                // 弾の生成リクエスト
                await RoomModel.Instance.ShootBulletAsync(shootBulletDatas.ToArray());
            }
            else if (!RoomModel.Instance)
            {
                foreach(var data in shootBulletDatas)
                {
                    var bulletObj = Instantiate(boxBulletPrefab, data.SpawnPos, data.Rotation);
                    bulletObj.GetComponent<ProjectileBase>().Init(data.Debuffs, data.Power);
                    bulletObj.GetComponent<ProjectileBase>().Shoot(data.ShootVec);
                }
            }
        }
    }

    #endregion

    #region AttackGolem

    /// <summary>
    /// AttackGolem開始
    /// </summary>
    void StartAttackGolem()
    {
        PrepareAttack();

        // 実行していなければ、クールダウンのコルーチンを開始
        string cooldownKey = COROUTINE.AttackGolem.ToString();
        RemoveAndStopCoroutineByKey(cooldownKey);
        Coroutine coroutine = StartCoroutine(AttackGolemCoroutine(() =>
        {
            RemoveAndStopCoroutineByKey(cooldownKey);
        }));
        managedCoroutines.Add(cooldownKey, coroutine);
    }

    /// <summary>
    /// [AttackGolem] ターゲットに近づいてから攻撃開始
    /// </summary>
    /// <param name="onFinished"></param>
    /// <returns></returns>
    IEnumerator AttackGolemCoroutine(Action onFinished)
    {
        const float waitSec = 0.1f;
        float currentSec = 0;
        while (disToTargetX > attackDist && currentSec <= maxMoveTime)
        {
            if (!target)
            {
                onFinished?.Invoke();
                CancellAttack();
                yield break;
            }

            CloseIn();
            yield return new WaitForSeconds(waitSec);
            currentSec += waitSec;
        }

        isInvincible = true;
        SetAnimId((int)ANIM_ID.Attack_Golem);
        audioCharge.Play();

        yield return new WaitForSeconds(0.45f);     // ゴーレムに形態変化が完了する時間

        // ターゲットのいる方向にテクスチャを反転
        LookAtTarget();

        onFinished?.Invoke();
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] 現在向いている方向に向かってダッシュ
    /// </summary>
    public override void OnAttackAnim3Event()
    {
        const float forcePower = 30;
        m_rb2d.AddForce(new Vector2(TransformUtils.GetFacingDirection(transform) * forcePower, 0), ForceMode2D.Impulse);
        audioGolem.Play();
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] ダッシュを停止する
    /// </summary>
    public override void OnEndAttackAnim3Event()
    {
        m_rb2d.linearVelocity = Vector2.zero;
    }

    #endregion

    #region AttackFallBlock

    /// <summary>
    /// AttackFallBlock開始
    /// </summary>
    void StartAttackFallBlock()
    {
        PrepareAttack();

        // 実行していなければ、クールダウンのコルーチンを開始
        string cooldownKey = COROUTINE.AttackFallBlock.ToString();
        RemoveAndStopCoroutineByKey(cooldownKey);
        Coroutine coroutine = StartCoroutine(AttackFakkBlockCoroutine(() =>
        {
            RemoveAndStopCoroutineByKey(cooldownKey);
        }));
        managedCoroutines.Add(cooldownKey, coroutine);
    }

    /// <summary>
    /// [AttackFallBlock] ターゲットに近づいてから攻撃開始
    /// </summary>
    /// <param name="onFinished"></param>
    /// <returns></returns>
    IEnumerator AttackFakkBlockCoroutine(Action onFinished)
    {
        const float targetDist = 0.5f;
        const float waitSec = 0.1f;
        float currentSec = 0;
        while (disToTargetX > targetDist && currentSec <= maxMoveTime)
        {
            if (!target)
            {
                onFinished?.Invoke();
                CancellAttack();
                yield break;
            }

            CloseIn();
            yield return null;
            currentSec += waitSec;
        }

        // 攻撃開始
        isInvincible = true;
        m_rb2d.linearVelocity = Vector2.zero;
        m_rb2d.bodyType = RigidbodyType2D.Static;
        SetAnimId((int)ANIM_ID.Attack_FallBlock);
        audioCharge.Play();

        // ブロックの向きを正しくする
        Vector2 direction = transform.localScale;
        var isRightDir = TransformUtils.GetFacingDirection(transform) > 1;
        transform.localScale = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));

        onFinished?.Invoke();
    }

    /// <summary>
    /// [AnimationEventから呼び出し] 着地音再生
    /// </summary>
    public override void OnEndAttackAnim4Event()
    {
        audioFallBlock.Play();
    }

    #endregion

    /// <summary>
    /// [Animationイベントからの呼び出し] 攻撃クールダウン処理
    /// </summary>
    public override void OnEndAttackAnimEvent()
    {
        // 実行していなければ、クールダウンのコルーチンを開始
        string cooldownKey = COROUTINE.AttackCooldown.ToString();
        RemoveAndStopCoroutineByKey(cooldownKey);
        Coroutine coroutine = StartCoroutine(AttackCooldown(attackCoolTime, () =>
        {
            RemoveAndStopCoroutineByKey(cooldownKey);
        }));
        managedCoroutines.Add(cooldownKey, coroutine);
    }

    /// <summary>
    /// 攻撃時のクールダウン処理
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time, Action onFinished)
    {
        // ターゲットのいる方向にテクスチャを反転
        if (canChaseTarget)
        {
            LookAtTarget();
        }

        isInvincible = false;
        m_rb2d.bodyType = RigidbodyType2D.Dynamic;
        isAttacking = true;
        Idle();
        yield return new WaitForSeconds(time);
        isAttacking = false;
        NextDecision();
        onFinished?.Invoke();
    }

    /// <summary>
    /// 攻撃キャンセル処理
    /// </summary>
    void CancellAttack()
    {
        if (canChaseTarget)
        {
            LookAtTarget();
        }

        Idle();
        isInvincible = false;
        isAttacking = false;
        m_rb2d.bodyType = RigidbodyType2D.Dynamic;
        StopAllManagedCoroutines();
        NextDecision();
    }

    #endregion

    #region 移動処理関連

    /// <summary>
    /// 真上に飛ぶ
    /// </summary>
    void JumpUp()
    {
        m_rb2d.linearVelocity = Vector2.zero;
        m_rb2d.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
    }

    /// <summary>
    /// ターゲットに接近する
    /// </summary>
    void CloseIn()
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
    /// ターゲットとの距離をとる
    /// </summary>
    void BackOff(Vector2? optionPos = null)
    {
        Vector2 targetPos = optionPos == null ? target.transform.position : (Vector2)optionPos;
        SetAnimId((int)ANIM_ID.Idle);

        Vector2 speedVec = Vector2.zero;
        if (IsBackFall())
        {
            speedVec = new Vector2(0f, m_rb2d.linearVelocity.y);
        }
        else
        {
            float distToPlayer = targetPos.x - this.transform.position.x;
            speedVec = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed * -1, m_rb2d.linearVelocity.y);
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
        SetAnimId((int)ANIM_ID.Dead);
        PlayHitBlendShader(false, 0.5f);
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
        m_rb2d.bodyType = RigidbodyType2D.Dynamic;

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
