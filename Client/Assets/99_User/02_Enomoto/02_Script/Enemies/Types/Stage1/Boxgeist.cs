////**************************************************
////  [ボス] ボックスガイストのクラス
////  Author:r-enomoto
////**************************************************
//using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
//using Pixeye.Unity;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using Unity.VisualScripting;
//using UnityEngine;
//using static Shared.Interfaces.StreamingHubs.EnumManager;

//public class Boxgeist : EnemyBase
//{
//    /// <summary>
//    /// アニメーションID
//    /// </summary>
//    public enum ANIM_ID
//    {
//        Spawn = 0,
//        Idle,
//        Attack,
//        Dead,
//    }

//    /// <summary>
//    /// 行動パターン
//    /// </summary>
//    public enum DECIDE_TYPE
//    {
//        Waiting = 1,
//        Attack_Range,
//        Attack_Shotgun,
//        Attack_Golem,
//        Attack_FallBlock,
//        Tracking,
//        RndMove,
//    }
//    DECIDE_TYPE nextDecide = DECIDE_TYPE.Waiting;

//    /// <summary>
//    /// 管理するコルーチンの種類
//    /// </summary>
//    public enum COROUTINE
//    {
//        NextDecision,
//        PatorolCoroutine,
//        AttackCooldown,
//    }

//    #region 攻撃関連
//    [Foldout("攻撃関連")]
//    [SerializeField]
//    Transform aimTransform; // 銃のAIM部分

//    [Foldout("攻撃関連")]
//    [SerializeField]
//    List<GameObject> nodeBulletPrefabs = new List<GameObject>();
//    int lastNodeBulletIndex;    // 前回射出した弾のインデックス番号
//    #endregion

//    #region チェック関連

//    // 壁・地面チェック
//    [Foldout("チェック関連")]
//    [SerializeField]
//    Transform wallCheck;
//    [Foldout("チェック関連")]
//    [SerializeField]
//    Vector2 wallCheckRadius = new Vector2(0, 1.5f);
//    #endregion

//    #region 抽選関連
//    float decisionTimeMax = 2f;
//    float randomDecision;
//    bool endDecision;
//    #endregion

//    #region オリジナル

//    [Foldout("オリジナル")]
//    [SerializeField]
//    float patorolRange = 10f;

//    EnemyProjectileChecker projectileChecker;
//    Vector3 targetPos;
//    Vector2? startPatorolPoint = null;
//    #endregion

//    protected override void Start()
//    {
//        base.Start();
//        lastNodeBulletIndex = nodeBulletPrefabs.Count - 1;
//        projectileChecker = aimTransform.GetComponent<EnemyProjectileChecker>();
//        isAttacking = false;
//        doOnceDecision = true;
//        targetPos = Vector3.zero;
//        NextDecision();
//    }

//    /// <summary>
//    /// 行動パターン実行処理
//    /// </summary>
//    protected override void DecideBehavior()
//    {
//        if (doOnceDecision)
//        {
//            doOnceDecision = false;

//            switch (nextDecide)
//            {
//                case DECIDE_TYPE.Waiting:
//                    chaseAI.Stop();
//                    Idle();
//                    NextDecision();
//                    break;
//                case DECIDE_TYPE.Attack:
//                    chaseAI.Stop();
//                    Attack();
//                    break;
//                case DECIDE_TYPE.Tracking:
//                    Tracking();
//                    NextDecision();
//                    break;
//                case DECIDE_TYPE.Patrol:
//                    Patorol();
//                    break;
//                case DECIDE_TYPE.RndMove:
//                    chaseAI.DoRndMove();
//                    NextDecision(2f);
//                    break;
//            }
//        }
//    }


//    /// <summary>
//    /// アイドル処理
//    /// </summary>
//    protected override void Idle()
//    {
//        SetAnimId((int)ANIM_ID.Idle);
//        m_rb2d.linearVelocity = new Vector2(0f, m_rb2d.linearVelocity.y);
//    }

//    #region 抽選処理関連

//    /// <summary>
//    /// 抽選処理を呼ぶ
//    /// </summary>
//    /// <param name="time"></param>
//    void NextDecision(float? rndMaxTime = null)
//    {
//        if (rndMaxTime == null) rndMaxTime = decisionTimeMax;
//        float time = UnityEngine.Random.Range(0.1f, (float)rndMaxTime);

//        // 実行していなければ、行動の抽選のコルーチンを開始
//        string key = COROUTINE.NextDecision.ToString();
//        if (!ContaintsManagedCoroutine(key))
//        {
//            Coroutine coroutine = StartCoroutine(NextDecisionCoroutine(time, () => { RemoveAndStopCoroutineByKey(key); }));
//            managedCoroutines.Add(key, coroutine);
//        }
//    }

//    /// <summary>
//    /// 次の行動パターン決定処理
//    /// </summary>
//    /// <param name="time"></param>
//    /// <returns></returns>
//    IEnumerator NextDecisionCoroutine(float time, Action onFinished)
//    {
//        yield return new WaitForSeconds(time);

//        #region 各行動パターンの重み付け

//        Dictionary<DECIDE_TYPE, int> weights = new Dictionary<DECIDE_TYPE, int>();

//        // 攻撃が可能な場合
//        if (canAttack && projectileChecker.CanFireProjectile(target) && !sightChecker.IsObstructed())
//        {
//            int weightRate = nextDecide == DECIDE_TYPE.Attack ? 3 : 1;
//            if (!isAttacking) weights[DECIDE_TYPE.Attack] = 10 / weightRate;
//            weights[DECIDE_TYPE.RndMove] = 5 * weightRate;
//        }
//        else if (canChaseTarget && target)
//        {
//            weights[DECIDE_TYPE.Tracking] = 10;
//        }
//        else if (canPatrol && !isPatrolPaused)
//        {
//            weights[DECIDE_TYPE.Patrol] = 10;
//        }
//        else
//        {
//            weights[DECIDE_TYPE.Waiting] = 10;
//        }

//        // valueを基準に昇順で並べ替え
//        var sortedWeights = weights.OrderBy(x => x.Value);
//        #endregion

//        // 全体の長さを使って抽選
//        int totalWeight = weights.Values.Sum();
//        randomDecision = UnityEngine.Random.Range(1, totalWeight + 1);

//        // 抽選した値で次の行動パターンを決定する
//        int currentWeight = 0;
//        foreach (var weight in sortedWeights)
//        {
//            currentWeight += weight.Value;
//            if (currentWeight >= randomDecision)
//            {
//                nextDecide = weight.Key;
//                break;
//            }
//        }

//        doOnceDecision = true;
//        onFinished?.Invoke();
//    }

//    #endregion

//    #region 攻撃処理関連

//    /// <summary>
//    /// 攻撃処理
//    /// </summary>
//    public void Attack()
//    {
//        if (target == null)
//        {
//            targetPos = Vector3.zero;
//            NextDecision();
//            return;
//        }

//        targetPos = target.transform.position;
//        doOnceDecision = false;
//        isAttacking = true;
//        m_rb2d.linearVelocity = Vector2.zero;
//        chaseAI.Stop();
//        StopPatorol();
//        SetAnimId((int)ANIM_ID.Attack);
//    }

//    /// <summary>
//    /// [Animationイベントからの呼び出し] 弾発射処理
//    /// </summary>

//    public async override void OnAttackAnimEvent()
//    {
//        if (targetPos != Vector3.zero)
//        {
//            if (!target || target && target.GetComponent<CharacterBase>().HP <= 0) target = sightChecker.GetTargetInSight();
//            if (target) targetPos = target.transform.position;

//            // 自身がエリート個体の場合、付与する状態異常の種類を取得する
//            DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();
//            List<DEBUFF_TYPE> debuffs = new List<DEBUFF_TYPE>();
//            if (applyEffect != null) debuffs.Add((DEBUFF_TYPE)applyEffect);
//            var shootVec = (targetPos - aimTransform.position).normalized * 20;

//            // 射出する弾を取得
//            lastNodeBulletIndex = lastNodeBulletIndex == nodeBulletPrefabs.Count - 1 ? 0 : lastNodeBulletIndex + 1;
//            GameObject bullet = nodeBulletPrefabs[lastNodeBulletIndex];
//            PROJECTILE_TYPE bulletType = bullet.GetComponent<ProjectileBase>().TypeId;

//            if (RoomModel.Instance && RoomModel.Instance.IsMaster)
//            {
//                // 弾の生成リクエスト
//                await RoomModel.Instance.ShootBulletAsync(bulletType, debuffs, power, aimTransform.position, shootVec, Quaternion.identity);
//            }
//            else if (!RoomModel.Instance)
//            {
//                var bulletObj = Instantiate(bullet, aimTransform.position, Quaternion.identity);
//                bulletObj.GetComponent<ProjectileBase>().Init(debuffs, power);
//                bulletObj.GetComponent<ProjectileBase>().Shoot(shootVec);
//            }
//        }
//    }

//    /// <summary>
//    /// [Animationイベントからの呼び出し] 攻撃クールダウン処理
//    /// </summary>
//    public override void OnEndAttackAnimEvent()
//    {
//        // 実行していなければ、クールダウンのコルーチンを開始
//        string cooldownKey = COROUTINE.AttackCooldown.ToString();
//        if (!ContaintsManagedCoroutine(cooldownKey))
//        {
//            Coroutine coroutine = StartCoroutine(AttackCooldown(attackCoolTime, () => {
//                RemoveAndStopCoroutineByKey(cooldownKey);
//            }));
//            managedCoroutines.Add(cooldownKey, coroutine);
//        }
//    }

//    /// <summary>
//    /// 攻撃時のクールダウン処理
//    /// </summary>
//    /// <returns></returns>
//    IEnumerator AttackCooldown(float time, Action onFinished)
//    {
//        isAttacking = true;
//        Idle();
//        yield return new WaitForSeconds(time);
//        isAttacking = false;
//        NextDecision();
//        onFinished?.Invoke();
//    }

//    #endregion

//    #region 移動処理関連

//    /// <summary>
//    /// 追跡する処理
//    /// </summary>
//    protected override void Tracking()
//    {
//        SetAnimId((int)ANIM_ID.Idle);

//        aimTransform.localEulerAngles = Vector3.back * 90f; // 銃の向きを初期化
//        StopPatorol();
//        chaseAI.DoChase(target);
//    }

//    /// <summary>
//    /// 巡回処理
//    /// </summary>
//    protected override void Patorol()
//    {
//        SetAnimId((int)ANIM_ID.Idle);

//        aimTransform.localEulerAngles = Vector3.back * 90f; // 銃の向きを初期化

//        // 実行していなければ、パトロールのコルーチンを開始
//        string key = COROUTINE.PatorolCoroutine.ToString();
//        if (!ContaintsManagedCoroutine(key))
//        {
//            Coroutine coroutine = StartCoroutine(PatorolCoroutine(() => { RemoveAndStopCoroutineByKey(key); }));
//            managedCoroutines.Add(key, coroutine);
//        }
//    }

//    /// <summary>
//    /// 巡回する処理
//    /// </summary>
//    IEnumerator PatorolCoroutine(Action onFinished)
//    {
//        float pauseTime = 2f;
//        if (startPatorolPoint == null)
//        {
//            startPatorolPoint = transform.position;
//        }

//        if (IsWall()) Flip();

//        if (TransformUtils.GetFacingDirection(transform) > 0)
//        {
//            if (transform.position.x >= startPatorolPoint.Value.x + patorolRange)
//            {
//                isPatrolPaused = true;
//                Idle();
//                yield return new WaitForSeconds(pauseTime);
//                isPatrolPaused = false;
//                Flip();
//            }
//        }
//        else if (TransformUtils.GetFacingDirection(transform) < 0)
//        {
//            if (transform.position.x <= startPatorolPoint.Value.x - patorolRange)
//            {
//                isPatrolPaused = true;
//                Idle();
//                yield return new WaitForSeconds(pauseTime);
//                isPatrolPaused = false;
//                Flip();
//            }
//        }

//        Vector2 speedVec = Vector2.zero;
//        speedVec = new Vector2(TransformUtils.GetFacingDirection(transform) * moveSpeed / 2, m_rb2d.linearVelocity.y);
//        m_rb2d.linearVelocity = speedVec;
//        NextDecision();
//        onFinished?.Invoke();
//    }

//    /// <summary>
//    /// 巡回処理を停止する
//    /// </summary>
//    void StopPatorol()
//    {
//        startPatorolPoint = null;
//    }

//    #endregion

//    #region ヒット処理関連

//    /// <summary>
//    /// ダメージを受けたときの処理
//    /// </summary>
//    protected override void OnHit()
//    {
//        base.OnHit();
//        chaseAI.Stop();
//        StopPatorol();
//        SetAnimId((int)ANIM_ID.Hit);
//    }

//    /// <summary>
//    /// 死亡するときに呼ばれる処理
//    /// </summary>
//    /// <returns></returns>
//    protected override void OnDead()
//    {
//        SetAnimId((int)ANIM_ID.Dead);
//    }

//    #endregion

//    #region テクスチャ・アニメーション関連

//    /// <summary>
//    /// スポーンアニメーションが終了したとき
//    /// </summary>
//    public override void OnEndSpawnAnimEvent()
//    {
//        base.OnEndSpawnAnimEvent();
//        chaseAI.Stop();
//        ApplyStun(0.5f, false);
//    }

//    /// <summary>
//    /// アニメーション設定処理
//    /// </summary>
//    /// <param name="id"></param>
//    public override void SetAnimId(int id)
//    {
//        if (animator == null) return;
//        animator.SetInteger("animation_id", id);

//        switch (id)
//        {
//            case (int)ANIM_ID.Hit:
//                animator.Play("Hit_NodeCode", 0, 0);
//                break;
//            default:
//                break;
//        }
//    }
//    #endregion

//    #region リアルタイム同期関連

//    /// <summary>
//    /// マスタクライアント切り替え時に状態をリセットする
//    /// </summary>
//    public override void ResetAllStates()
//    {
//        base.ResetAllStates();

//        if (target == null)
//        {
//            target = sightChecker.GetTargetInSight();
//        }

//        DecideBehavior();
//    }

//    #endregion

//    #region チェック処理関連

//    /// <summary>
//    /// 壁があるかどうか
//    /// </summary>
//    /// <returns></returns>
//    bool IsWall()
//    {
//        return Physics2D.OverlapBox(wallCheck.position, wallCheckRadius, 0f, terrainLayerMask);
//    }

//    /// <summary>
//    /// 検出範囲の描画処理
//    /// </summary>
//    protected override void DrawDetectionGizmos()
//    {
//        // 攻撃開始距離
//        Gizmos.color = Color.blue;
//        Gizmos.DrawWireSphere(transform.position, attackDist);

//        // 壁の判定
//        if (wallCheck)
//        {
//            Gizmos.color = Color.green;
//            Gizmos.DrawWireCube(wallCheck.transform.position, wallCheckRadius);
//        }
//    }

//    #endregion

//}
