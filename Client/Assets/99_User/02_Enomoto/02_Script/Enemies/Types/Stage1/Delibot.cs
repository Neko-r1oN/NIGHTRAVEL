//**************************************************
//  [敵] デリボットを制御するクラス
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;
public class Delibot : EnemyBase
{
    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        Attack,
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
    GameObject boxBulletPrefab;
    #endregion

    #region チェック判定
    // 壁チェック
    [Foldout("チェック関連")]
    [SerializeField]
    Transform wallCheck;
    [Foldout("チェック関連")]
    [SerializeField]
    Vector2 wallCheckRadius = new Vector2(0, 1.5f);
    #endregion

    #region 抽選関連
    float decisionTimeMax = 2f;
    float randomDecision;
    bool endDecision;
    #endregion

    #region オリジナル
    Vector3 targetPos;
    #endregion

    protected override void Start()
    {
        base.Start();
        projectileChecker = aimTransform.GetComponent<EnemyProjectileChecker>();
        isAttacking = false;
        doOnceDecision = true;
        targetPos = Vector3.zero;
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
                case DECIDE_TYPE.Tracking:
                    Tracking();
                    NextDecision();
                    break;
                case DECIDE_TYPE.RndMove:
                    chaseAI.DoRndMove();
                    NextDecision(2f);
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
    void NextDecision(float? rndMaxTime = null)
    {
        if (rndMaxTime == null) rndMaxTime = decisionTimeMax;
        float time = UnityEngine.Random.Range(0.1f, (float)rndMaxTime);

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

        // 攻撃が可能な場合
        if (canAttack && projectileChecker.CanFireProjectile(target) && !sightChecker.IsObstructed())
        {
            int weightRate = nextDecide == DECIDE_TYPE.Attack ? 3 : 1;
            weights[DECIDE_TYPE.Attack] = 10 / weightRate;
            weights[DECIDE_TYPE.RndMove] = 5 * weightRate;
        }
        else if (target)
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
                animator.Play("Haitatu_Damage_Animation");
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
        if (target == null)
        {
            targetPos = Vector3.zero;
            NextDecision();
            return;
        }

        targetPos = target.transform.position;
        doOnceDecision = false;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        chaseAI.Stop();
        SetAnimId((int)ANIM_ID.Attack);
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] ダンボール弾を発射
    /// </summary>
    public override async void OnAttackAnimEvent()
    {
        if(targetPos != Vector3.zero)
        {
            // 自身がエリート個体の場合、付与する状態異常の種類を取得する
            DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();
            List<DEBUFF_TYPE> debuffs = new List<DEBUFF_TYPE>();
            if (applyEffect != null) debuffs.Add((DEBUFF_TYPE)applyEffect);
            var shootVec = (targetPos - aimTransform.position).normalized * 20;

            if (RoomModel.Instance && RoomModel.Instance.IsMaster)
            {
                // 弾の生成リクエスト
                await RoomModel.Instance.ShootBulletAsync(PROJECTILE_TYPE.BoxBullet, debuffs, power, aimTransform.position, shootVec);
            }
            else
            {
                var bulletObj = Instantiate(boxBulletPrefab, aimTransform.position, Quaternion.identity);
                List<DEBUFF_TYPE> dEBUFFs = new List<DEBUFF_TYPE>();
                bulletObj.GetComponent<ProjectileBase>().Init(debuffs, power);
                bulletObj.GetComponent<ProjectileBase>().Shoot(shootVec);
            }
        }

        // 実行していなければ、クールダウンのコルーチンを開始
        string key = COROUTINE.AttackCooldown.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(AttackCooldown(attackCoolTime, () => { RemoveAndStopCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// 攻撃時のクールダウン処理
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time, Action onFinished)
    {
        isAttacking = true;
        yield return new WaitForSeconds(time);
        Idle();
        isAttacking = false;
        doOnceDecision = true;
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
        chaseAI.DoChase(target);
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
        if (hp > 0) NextDecision();
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

    #region リアルタイム同期関連

    /// <summary>
    /// マスタクライアント切り替え時に状態をリセットする
    /// </summary>
    public override void ResetAllStates()
    {
        base.ResetAllStates();

        ANIM_ID id = (ANIM_ID)GetAnimId();
        nextDecide = id switch
        {
            ANIM_ID.Attack => DECIDE_TYPE.Attack,
            _ => DECIDE_TYPE.Waiting,
        };

        DecideBehavior();
    }

    /// <summary>
    /// オリジナルのEnemyData取得処理
    /// </summary>
    /// <returns></returns>
    public override EnemyData GetEnemyData()
    {
        EnemyData enemyData = new EnemyData();
        enemyData.Quatarnions.Add(aimTransform.localRotation);
        return SetEnemyData(enemyData);
    }

    /// <summary>
    /// ドローンの同期情報を更新する
    /// </summary>
    /// <param name="enemyData"></param>
    public override void UpdateEnemy(EnemyData enemyData)
    {
        base.UpdateEnemy(enemyData);
        aimTransform.localRotation = enemyData.Quatarnions[0];
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
