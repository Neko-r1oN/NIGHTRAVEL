//**************************************************
//  [敵] スレイドのクラス
//  Author:r-enomoto
//**************************************************
using KanKikuchi.AudioManager;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Slade : EnemyBase
{
    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        Attack_Rounding_Up,
        Attack_Rounding_Down,
        Attack_Charge,
        Attack_Combo,
        Teleport,
        Hit,
        Dead,
    }

    /// <summary>
    /// 行動パターン
    /// </summary>
    public enum DECIDE_TYPE
    {
        Waiting = 1,
        Attack_Upward_Slash,
        Attack_Charge,
        Attack_Combo_Slash,
        Teleport,
    }
    DECIDE_TYPE nextDecide = DECIDE_TYPE.Waiting;

    /// <summary>
    /// 管理するコルーチンの種類
    /// </summary>
    public enum COROUTINE
    {
        NextDecision,
        AttackChargeCoroutine,
        AttackCooldown,
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
    #endregion

    #region 抽選関連
    float decisionTimeMax = 2f;
    float randomDecision;
    bool endDecision;
    #endregion

    #region オリジナル
    [SerializeField]
    CapsuleCollider2D terrainCollider;
    List<GameObject> hitPlayers = new List<GameObject>();
    #endregion

    #region オーディオ関連

    [SerializeField]
    [Foldout("オーディオ")]
    AudioSource audioAttack;

    [SerializeField]
    [Foldout("オーディオ")]
    AudioSource audioChargeAttack;

    [SerializeField]
    [Foldout("オーディオ")]
    AudioSource audioTeleport;

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
                case DECIDE_TYPE.Attack_Upward_Slash:
                    AttackUpwardSlash();
                    break;
                case DECIDE_TYPE.Attack_Charge:
                    AttackCharge();
                    break;
                case DECIDE_TYPE.Attack_Combo_Slash:
                    AttackComboSlash();
                    break;
                case DECIDE_TYPE.Teleport:
                    Tracking();
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

        const float attackChargeDist = 10f;

        // 各攻撃の条件
        // -----------------
        // 切り上げ, 切り下げ・・・：ターゲットが攻撃範囲内にいる場合
        // 突進・・・・・・・・・・：切り上げ, 切り下げの範囲外かつ、ターゲットが攻撃範囲内にいる場合
        // コンボ技・・・・・・・・：現在HPが半分以下かつ、ターゲットが攻撃範囲内
        bool canAttackSlash = canAttack && !sightChecker.IsObstructed() && disToTarget <= attackDist;
        bool canAttackCharge = canAttack && !sightChecker.IsObstructed() && disToTarget > attackDist && disToTarget <= attackChargeDist;
        bool canAttackCombo = hp <= MaxHP / 2 && canAttack && !sightChecker.IsObstructed() && disToTarget <= attackDist;
        Dictionary<DECIDE_TYPE, int> weights = new Dictionary<DECIDE_TYPE, int>();

        if (canAttackSlash)
        {
            weights[DECIDE_TYPE.Attack_Upward_Slash] = nextDecide == DECIDE_TYPE.Attack_Upward_Slash ? 5 : 20;
        }
        if (canAttackCharge)
        {
            weights[DECIDE_TYPE.Attack_Charge] = nextDecide == DECIDE_TYPE.Attack_Charge ? 5 : 20;
        }
        if (canAttackCombo)
        {
            weights[DECIDE_TYPE.Attack_Combo_Slash] = nextDecide == DECIDE_TYPE.Attack_Combo_Slash ? 5 : 30;
        }
        if (canChaseTarget && target && disToTarget > attackDist)
        {
            weights[DECIDE_TYPE.Teleport] = nextDecide == DECIDE_TYPE.Attack_Charge ? 30 : 10;
        }
        if(!target)
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

    #region 攻撃：切り上げ・切り下げ

    /// <summary>
    /// 攻撃：切り上げ
    /// </summary>
    void AttackUpwardSlash()
    {
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Rounding_Up);

        SEManager.Instance.Play(
               audioPath: SEPath.SWORD_ATK, //再生したいオーディオのパス
               volumeRate: 1.0f,                //音量の倍率
               delay: 0.0f,                //再生されるまでの遅延時間
               pitch: 1.0f,                //ピッチ
               isLoop: false,             //ループ再生するか
               callback: null              //再生終了後の処理
               );
    }

    /// <summary>
    /// 攻撃：切り下げ
    /// </summary>
    void AttackDownwardSlash()
    {
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Rounding_Down);

        SEManager.Instance.Play(
               audioPath: SEPath.SWORD_ATK, //再生したいオーディオのパス
               volumeRate: 1.0f,                //音量の倍率
               delay: 0.0f,                //再生されるまでの遅延時間
               pitch: 1.0f,                //ピッチ
               isLoop: false,             //ループ再生するか
               callback: null              //再生終了後の処理
               );
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] 切り上げ・切り下げの攻撃判定処理
    /// </summary>
    public override void OnAttackAnimEvent()
    {
        audioAttack.Play();

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

    #region 攻撃：突進

    /// <summary>
    /// 攻撃：突進
    /// </summary>
    void AttackCharge()
    {
        terrainCollider.enabled = true;
        hitPlayers.Clear();
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Charge);

        SEManager.Instance.Play(
               audioPath: SEPath.DANBO_ATK, //再生したいオーディオのパス
               volumeRate: 1.0f,                //音量の倍率
               delay: 0.0f,                //再生されるまでの遅延時間
               pitch: 1.0f,                //ピッチ
               isLoop: false,             //ループ再生するか
               callback: null              //再生終了後の処理
               );
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] 突進の攻撃判定開始
    /// </summary>
    public override void OnAttackAnim2Event()
    {
        terrainCollider.enabled = false;

        // 実行していなければ、クールダウンのコルーチンを開始
        string cooldownKey = COROUTINE.AttackChargeCoroutine.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(AttackChargeCoroutine());
            managedCoroutines.Add(cooldownKey, coroutine);
        }
    }

    /// <summary>
    /// 突進の攻撃判定
    /// </summary>
    /// <param name="onFinished"></param>
    /// <returns></returns>
    IEnumerator AttackChargeCoroutine()
    {
        // 自身がエリート個体の場合、付与する状態異常の種類を取得する
        DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();

        audioChargeAttack.Play();

        while (true)
        {
            // 移動処理
            Vector2 speedVec = Vector2.zero;
            if (IsWall())
            {
                speedVec = new Vector2(0f, m_rb2d.linearVelocity.y);
            }
            else
            {
                speedVec = new Vector2(TransformUtils.GetFacingDirection(transform) * moveSpeed, m_rb2d.linearVelocity.y);
            }
            m_rb2d.linearVelocity = speedVec;

            Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
            for (int i = 0; i < collidersEnemies.Length; i++)
            {
                if (collidersEnemies[i].gameObject.tag == "Player" && !hitPlayers.Contains(collidersEnemies[i].gameObject))
                {
                    hitPlayers.Add(collidersEnemies[i].gameObject);
                    collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position, KB_POW.Big, applyEffect);
                }
            }
            yield return null;
        }
    }

    #endregion

    #region 攻撃：コンボ技

    /// <summary>
    /// 攻撃：コンボ技
    /// </summary>
    void AttackComboSlash()
    {
        hitPlayers.Clear();
        canCancelAttackOnHit = false;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack_Combo);

        SEManager.Instance.Play(
               audioPath: SEPath.SWORD_HIT, //再生したいオーディオのパス
               volumeRate: 1.0f,                //音量の倍率
               delay: 0.0f,                //再生されるまでの遅延時間
               pitch: 1.0f,                //ピッチ
               isLoop: false,             //ループ再生するか
               callback: null              //再生終了後の処理
               );
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] 大きく切り上げたときの攻撃判定
    /// 攻撃が命中しなかった場合は攻撃を中断する
    /// </summary>
    public override void OnAttackAnim3Event()
    {
        const float addForcePower = 40f;
        bool isHitTarget = false;

        audioAttack.Play();

        // 自身がエリート個体の場合、付与する状態異常の種類を取得する
        DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();

        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Player")
            {
                PlayerBase player = collidersEnemies[i].gameObject.GetComponent<PlayerBase>();
                Rigidbody2D rigidbody2D = player.GetComponent<Rigidbody2D>();

                if (player.gameObject == target) isHitTarget = true;
                hitPlayers.Add(player.gameObject);

                player.ApplyDamage((int)(power * 1.5f), transform.position, null, applyEffect);
                player.StartCoroutine(player.Stun(10f));
                rigidbody2D.gravityScale = 0;
                rigidbody2D.linearVelocity = Vector2.zero;
                rigidbody2D.AddForce(Vector2.up * addForcePower, ForceMode2D.Impulse);
            }
        }

        if(!isHitTarget && hitPlayers.Count > 0)
        {
            target = hitPlayers[UnityEngine.Random.Range(0, hitPlayers.Count)];
        }

        // 失敗したら攻撃を中断
        if (hitPlayers.Count == 0)
        {
            SetAnimId((int)ANIM_ID.Idle);
            OnEndAttackAnimEvent();
        }
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] 大きく切り下げたときの攻撃判定
    /// </summary>
    public override void OnAttackAnim4Event()
    {
        const float addForcePower = 100f;

        audioAttack.Play();

        // 自身がエリート個体の場合、付与する状態異常の種類を取得する
        DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();

        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange * 1.5f);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Player")
            {
                PlayerBase player = collidersEnemies[i].gameObject.GetComponent<PlayerBase>();
                Rigidbody2D rigidbody2D = player.GetComponent<Rigidbody2D>();

                player.ApplyDamage((int)(power * 1.5f), transform.position, null, applyEffect);
                rigidbody2D.gravityScale = 0;
                rigidbody2D.linearVelocity = Vector2.zero;
                rigidbody2D.AddForce(new Vector2(TransformUtils.GetFacingDirection(transform) * addForcePower, 0), ForceMode2D.Impulse);
            }
        }
    }

    #endregion

    #region 攻撃クールダウン

    /// <summary>
    /// [Animationイベントからの呼び出し] 攻撃クールダウン処理
    /// </summary>
    public override void OnEndAttackAnimEvent()
    {
        RemoveAndStopCoroutineByKey(COROUTINE.AttackChargeCoroutine.ToString());

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
        terrainCollider.enabled = true;
        canCancelAttackOnHit = true;
        isAttacking = true;
        Idle();
        yield return new WaitForSeconds(time);
        isAttacking = false;
        NextDecision();
        onFinished?.Invoke();
    }

    #endregion

    #endregion

    #region 移動処理関連

    /// <summary>
    /// テレポートして追跡する処理を開始
    /// </summary>
    protected override void Tracking()
    {
        SetAnimId((int)ANIM_ID.Teleport);
        m_rb2d.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// [アニメーションイベントから呼び出し] テレポートのアニメーションイベント
    /// </summary>
    public override void OnMoveAnimEvent()
    {
        isInvincible = true;
        m_rb2d.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// [アニメーションイベントから呼び出し] テレポートのアニメーション終了時
    /// </summary>
    public override void OnEndMoveAnimEvent()
    {
        audioTeleport.Play();

        Vector2 teleportPos = transform.position;

        // プレイヤーの背後にテレポート
        const float offsetX = 1.6f;
        if (target) teleportPos = (Vector2)target.transform.position + Vector2.right * offsetX * -TransformUtils.GetFacingDirection(target.transform);
        transform.position = teleportPos;

        // プレイヤーの方向を向く
        LookAtTarget();

        isInvincible = false;
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

        SEManager.Instance.Play(
               audioPath: SEPath.MOB_HIT, //再生したいオーディオのパス
               volumeRate: 1.0f,                //音量の倍率
               delay: 0.0f,                //再生されるまでの遅延時間
               pitch: 1.0f,                //ピッチ
               isLoop: false,             //ループ再生するか
               callback: null              //再生終了後の処理
               );
    }

    /// <summary>
    /// 死亡するときに呼ばれる処理
    /// </summary>
    /// <returns></returns>
    protected override void OnDead()
    {
        SetAnimId((int)ANIM_ID.Dead);

        SEManager.Instance.Play(
               audioPath: SEPath.MOB_DEATH, //再生したいオーディオのパス
               volumeRate: 1.0f,                //音量の倍率
               delay: 0.0f,                //再生されるまでの遅延時間
               pitch: 1.0f,                //ピッチ
               isLoop: false,             //ループ再生するか
               callback: null              //再生終了後の処理
               );
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
                animator.Play("Hit_Knight", 0, 0);
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

        terrainCollider.enabled = true;
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
    }

    #endregion
}
