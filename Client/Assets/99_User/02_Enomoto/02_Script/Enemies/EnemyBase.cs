﻿//**************************************************
//  エネミーの抽象クラス
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shared.Interfaces.StreamingHubs;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using System.Linq;

abstract public class EnemyBase : CharacterBase
{
    #region データ関連

    [Foldout("データ関連")]
    [SerializeField]
    [Tooltip("エネミー種類ID")]
    protected ENEMY_TYPE enemyTypeId;    // 自身のエネミー種類ID　(DBのレコードと紐づける)
    
    [Foldout("データ関連")]
    [SerializeField]
    [Tooltip("生成されたときの識別用ID")]
    protected string uniqueId = "";    // 生成されたときの識別用ID  ※他の敵と重複しないよう注意

    [Foldout("データ関連")]
    [SerializeField]
    [Tooltip("生成されたときの識別用ID")]
    protected string selfName = "Enemy Sample";

    #endregion

    #region プレイヤー・ターゲット
    [Header("プレイヤー・ターゲット")]
    public GameObject target;
    public GameObject Target { get { return target; } set { target = value; } }

    protected CharacterManager characterManager;
    #endregion

    #region コンポーネント
    protected EnemyElite enemyElite;
    protected EnemySightChecker sightChecker;
    protected EnemyChaseAI chaseAI;
    protected Rigidbody2D m_rb2d;
    #endregion

    #region テクスチャ・アニメーション関連
    [Foldout("テクスチャ・アニメーション")]
    [SerializeField]
    float destroyWaitSec = 1f;

    [Foldout("テクスチャ・アニメーション")]
    [SerializeField]
    List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    #endregion

    #region チェック判定
    protected LayerMask terrainLayerMask; // 壁と地面のレイヤー
    public LayerMask TerrainLayerMask { get { return terrainLayerMask; } }
    #endregion

    #region ステータス
    [Foldout("ステータス")]
    [SerializeField]
    protected int bulletNum = 3;

    [Foldout("ステータス")]
    [SerializeField]
    [Tooltip("弾の発射間隔")]
    protected float shotsPerSecond = 0.5f;

    [Foldout("ステータス")]
    [SerializeField]
    [Tooltip("攻撃のクールタイム")]
    protected float attackCoolTime = 0.5f;

    [Foldout("ステータス")]
    [SerializeField]
    [Tooltip("攻撃を開始する距離")]
    protected float attackDist = 1.5f;
    
    [Foldout("ステータス")]
    [SerializeField]
    protected float hitTime = 0.5f;

    [Foldout("ステータス")]
    [SerializeField]
    [Tooltip("ターゲットを見失うまでに必要な秒数")]
    protected float obstructionMaxTime = 3f;
    #endregion

    #region オプション
    [Foldout("オプション")]
    [SerializeField]
    protected bool isBoss = false;

    [Foldout("オプション")]
    [SerializeField]
    protected bool isElite = false;

    [Foldout("オプション")]
    [Tooltip("常に動き回ることが可能")]
    [SerializeField] 
    protected bool canPatrol;

    [Foldout("オプション")]
    [Tooltip("ターゲットを追跡可能")]
    [SerializeField] 
    protected bool canChaseTarget;
    
    [Foldout("オプション")]
    [Tooltip("攻撃可能")]
    [SerializeField]
    protected bool canAttack;
    
    [Foldout("オプション")]
    [Tooltip("ジャンプ可能")]
    [SerializeField] 
    protected bool canJump;

    [Foldout("オプション")]
    [Tooltip("攻撃中にヒットした場合、攻撃をキャンセル可能")]
    [SerializeField]
    protected bool canCancelAttackOnHit = false;

    [Foldout("オプション")]
    [Tooltip("DeadZoneとの接触判定を消すことが可能")]
    [SerializeField]
    protected bool canIgnoreDeadZoneCollision = false;

    [Foldout("オプション")]
    [Tooltip("自身のオブジェクトが消えるまでリストから削除されないようにする")]
    [SerializeField]
    protected bool dontRemoveOnDeath = false;

    [Foldout("オプション")]
    [Tooltip("ヒット時にHitBlendをスクリプトから操作する")]
    [SerializeField]
    protected bool isUseHitBlendShader = false;
    #endregion

    #region 状態管理
    protected Dictionary<string,Coroutine> managedCoroutines = new Dictionary<string,Coroutine>();  // 管理しているコルーチン
    protected bool isStun;
    protected bool isInvincible = true;
    protected bool doOnceDecision;
    protected bool isAttacking;
    protected bool isDead;
    protected bool isPatrolPaused;
    protected bool isSpawn = true; // スポーン中かどうか
    protected bool isStartComp;
    #endregion

    #region ターゲットとの距離
    protected float disToTarget;
    protected float disToTargetX;
    #endregion

    #region スポーン関連
    [Foldout("スポーン関連")]
    [Tooltip("生成されるときの地面からの距離")]
    [SerializeField]
    protected float spawnGroundOffset;

    [Foldout("スポーン関連")]
    [SerializeField]
    protected int spawnWeight = 1;  // スポーンの抽選する際の重み

    [Foldout("スポーン関連")]
    [SerializeField]
    protected float checkObstacleRange = 1; // 障害物を検知する範囲
    #endregion

    #region システム

    [Foldout("システム")]
    [Tooltip("判定を描画するかどうか")]
    [SerializeField]
    protected bool canDrawRay = false;
    public bool CanDrawRay { get { return canDrawRay; } }

    [Foldout("システム")]
    [SerializeField]
    protected int baseExp = 100;    // 初期の獲得可能経験値量

    [Foldout("システム")]
    [SerializeField]
    protected int exp = 100;    // 現在の獲得可能経験値量
    #endregion

    #region 外部参照用プロパティ

    public ENEMY_TYPE EnemyTypeId { get { return enemyTypeId; } set { enemyTypeId = value; } }

    public string UniqueId { get { return uniqueId; } set { uniqueId = value; } }

    public string SelfName { get { return selfName; } }

    public int SpawnWeight { get { return spawnWeight; } }

    public int BaseExp { get { return baseExp; } }

    public int Exp { get { return exp; } set { exp = value; } }

    public float AttackDist { get { return attackDist; } }

    public float SpawnGroundOffset { get { return spawnGroundOffset; } }

    public bool IsBoss { get { return isBoss; } set { isBoss = value; } }

    public bool IsElite { get { return isElite; } }

    public bool IsStartComp { get { return isStartComp; } }

    public List<SpriteRenderer> SpriteRenderers { get { return spriteRenderers; } }
    #endregion

    #region 定数
    private const int MAX_DAMAGE = 99999; // 最大ダメージ量
    #endregion

    protected virtual void OnEnable()
    {
        if (!isStartComp || hp <= 0 || isDead) return;
        ResetAllStates();
    }

    private void OnDestroy()
    {
        if (dontRemoveOnDeath)
        {
            RemoveEnemyFromList();
        }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        characterManager = CharacterManager.Instance;
        terrainLayerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("Gimmick") | LayerMask.GetMask("Scaffold");
        m_rb2d = GetComponent<Rigidbody2D>();
        sightChecker = GetComponent<EnemySightChecker>();
        chaseAI = GetComponent<EnemyChaseAI>();
        enemyElite = GetComponent<EnemyElite>();
        isStartComp = true;
        base.Start();
        ApplyDifficultyBasedStatusBoost();
    }

    public void LoadStart()
    {
        if(!isStartComp) Start();
    }

    protected virtual void FixedUpdate()
    {
        CalculateDistanceToTarget();
        if (isSpawn || isStun || isAttacking || isInvincible || hp <= 0 || !sightChecker) return;

        // ターゲットが存在しない || 現在のターゲットが死亡している場合
        if (!target || target && target.GetComponent<PlayerBase>().IsDead)
        {
            // 新しくターゲットを探す
            if (!isBoss) target = sightChecker.GetTargetInSight();
            else SelectNewTargetInBossRoom();
        }

        if (target)
        {
            // 実行中でなければ、ターゲットを監視するコルーチンを開始
            string key = "CheckTargetObstructionCoroutine";
            if (!ContaintsManagedCoroutine(key))
            {
                Coroutine coroutine = StartCoroutine(CheckTargetObstructionCoroutine(() => {
                    RemoveAndStopCoroutineByKey(key);
                }));
                managedCoroutines.Add(key, coroutine);
            }
        }
        else
        {
            if (canPatrol && !isPatrolPaused) Patorol();
            else Idle();
            return;
        }

        // ターゲットのいる方向にテクスチャを反転
        LookAtTarget();
        DecideBehavior();
    }

    /// <summary>
    /// 行動パターン実行処理
    /// </summary>
    abstract protected void DecideBehavior();

    /// <summary>
    /// アイドル処理
    /// </summary>
    protected virtual void Idle() { }

    /// <summary>
    /// ターゲットのいる方向に向かせる
    /// </summary>
    protected void LookAtTarget()
    {
        if (canChaseTarget && target)
        {
            if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
                || target.transform.position.x > transform.position.x && transform.localScale.x < 0) Flip();
        }
    }

    /// <summary>
    /// 方向転換
    /// </summary>
    protected void Flip()
    {
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    /// <summary>
    /// その場に待機する処理
    /// </summary>
    /// <param name="waitingTime"></param>
    /// <returns></returns>
    protected IEnumerator Waiting(float waitingTime, Action onFinished)
    {
        doOnceDecision = false;
        Idle();
        yield return new WaitForSeconds(waitingTime);
        doOnceDecision = true;
        onFinished?.Invoke();
    }

    #region ステータス関連

    /// <summary>
    /// 現在の難易度を基にステータスを上昇させる
    /// </summary>
    public void ApplyDifficultyBasedStatusBoost()
    {
        if (LevelManager.GameLevel == 0) return;

        // 難易度のレベル1毎にHP,Defence,Powerが15%上昇
        const float rate = 0.15f;
        float applyRate = rate * LevelManager.GameLevel;
        int addHp = (int)(baseHp * applyRate) == 0 ? 1 : (int)(baseHp * applyRate);
        int addDef = (int)(baseDefense * applyRate) == 0 ? 1 : (int)(baseDefense * applyRate);
        int addPower = (int)(basePower * applyRate) == 0 ? 1 : (int)(basePower * applyRate);
        float addMoveSpeed = 0;
        int addExp = (int)(baseExp * applyRate) == 0 ? 1 : (int)(baseExp * applyRate);

        // エリートの場合も考慮する
        if (isElite)
        {
            // HP・攻撃力が50%増し、防御力・移動速度・移動速度係数が25%増しにする
            addHp += (int)(baseHp * 0.5f);
            addPower += (int)(basePower * 0.5f);
            addDef += (int)(baseDefense * 0.25f);
            addMoveSpeed += (enemyElite.EliteType == ENEMY_ELITE_TYPE.Thunder) ? baseMoveSpeed : baseMoveSpeed * 0.25f; // エリートタイプがThunderの場合は2倍上がる

            // 二倍経験値増加
            addExp += baseExp;
        }

        Exp = baseExp + addExp;
        CharacterStatusData characterStatusData = new CharacterStatusData()
        {
            hp = baseHp + addHp,
            defence = baseDefense + addDef,
            power = basePower + addPower,
            jumpPower = maxJumpPower,
            moveSpeed = maxMoveSpeed + addMoveSpeed,
            attackSpeedFactor = maxAttackSpeedFactor,
            healRate = maxHealRate,
        };
        ChangeAccordingStatusToMaximumValue(characterStatusData);

        if (isBoss && UIManager.Instance) UIManager.Instance.UpdateBossStatus();
    }

    #endregion

    #region プレイヤー・ターゲット関連

    /// <summary>
    /// ターゲットとの距離を計測する
    /// </summary>
    protected void CalculateDistanceToTarget()
    {
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
    }

    /// <summary>
    /// ボス部屋にいるプレイヤーの中から新しくターゲットを決める
    /// </summary>
    protected bool SelectNewTargetInBossRoom()
    {
        bool isSucsess = false;
        var players = CharacterManager.Instance.PlayerObjs.Values.ToList();
        players = players.OrderBy(a => Guid.NewGuid()).ToList();

        foreach (var player in players)
        {
            var playerBase = player.GetComponent<PlayerBase>();
            if (player && !playerBase.IsDead && playerBase.HP > 0 && playerBase.IsBossArea)
            {
                target = player;
                isSucsess = true;
                break;
            }
        }

        if(!isSucsess) target = null;

        return isSucsess;
    }

    /// <summary>
    /// ターゲットとの間に遮蔽物があるかを監視し続けるコルーチン
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckTargetObstructionCoroutine(Action onFinished)
    {
        float currentTime = 0;
        float waitSec = 0.01f;

        // obstructionMaxTime以上経過でターゲットを見失ったことにする
        while (currentTime < obstructionMaxTime)
        {
            yield return new WaitForSeconds(waitSec);

            if (isBoss)
            {
                // 新しくターゲットを探す必要があるかどうかチェック
                bool needNewTarget = false;
                if (target)
                {
                    var player = target.GetComponent<PlayerBase>();
                    if (player.hp <= 0 || !player.IsBossArea) needNewTarget = true;
                }
                else needNewTarget = true;

                if(needNewTarget) SelectNewTargetInBossRoom();

                // ボス部屋からプレイヤーがいなくなったら見失ったことにする
                if (!target) break;
            }
            else
            {
                if (sightChecker.IsObstructed() || !sightChecker.IsTargetVisible())
                {
                    currentTime += waitSec;
                }
                else
                {
                    currentTime = 0;
                }
            }     
        }

        if (target && currentTime >= obstructionMaxTime)
        {
            target = null;
            if (chaseAI) chaseAI.Stop();
        }

        onFinished?.Invoke();
    }

    /// <summary>
    /// 生存しているプレイヤーの取得処理
    /// </summary>
    /// <returns></returns>
    protected List<GameObject> GetAlivePlayers()
    {
        List<GameObject> alivePlayers = new List<GameObject>();
        foreach (GameObject player in characterManager.PlayerObjs.Values)
        {
            if (player && player.GetComponent<CharacterBase>().HP > 0)
            {
                alivePlayers.Add(player);
            }
        }
        return alivePlayers;
    }

    /// <summary>
    /// 一番近いプレイヤーを取得する
    /// </summary>
    /// <returns></returns>
    public GameObject GetNearPlayer(Vector3? offset = null)
    {
        Vector3 point = transform.position;
        if (offset != null) point += (Vector3)offset;
        GameObject nearPlayer = null;
        float dist = float.MaxValue;
        foreach (GameObject player in GetAlivePlayers())
        {
            if (player != null)
            {
                float distToPlayer = Vector2.Distance(point, player.transform.position);
                if (Mathf.Abs(distToPlayer) < dist)
                {
                    nearPlayer = player;
                    dist = distToPlayer;
                }
            }
        }
        return nearPlayer;
    }

    /// <summary>
    /// 一番近いプレイヤーをターゲットに設定する
    /// </summary>
    public void SetNearTarget(Vector3? offset = null)
    {
        GameObject target = GetNearPlayer(offset);
        if (target != null)
        {
            this.target = target;
        }
    }

    #endregion

    #region エリート個体・状態異常関連
    /// <summary>
    /// エリート個体にする処理
    /// </summary>
    public void PromoteToElite(ENEMY_ELITE_TYPE type)
    {
        if (!isElite && type != ENEMY_ELITE_TYPE.None)
        {
            isElite = true;
            if (!enemyElite) enemyElite = GetComponent<EnemyElite>();
            enemyElite.Init(type);
        }
    }

    /// <summary>
    /// 適用させる状態異常の種類を取得する
    /// </summary>
    public DEBUFF_TYPE? GetStatusEffectToApply()
    {
        bool isElite = this.isElite && enemyElite != null;
        DEBUFF_TYPE? applyEffect = null;
        if (isElite)
        {
            applyEffect = enemyElite.GetAddStatusEffectEnum();
        }
        return applyEffect;
    }
    #endregion

    #region 移動処理関連

    /// <summary>
    /// 追跡する処理
    /// </summary>
    protected virtual void Tracking() { }

    /// <summary>
    /// 巡回する処理
    /// </summary>
    protected virtual void Patorol() { }

    #endregion

    #region ヒット処理関連

    /// <summary>
    /// 死亡時に呼ばれる処理 (専用アニメーションなど)
    /// </summary>
    abstract protected void OnDead();

    /// <summary>
    /// ノックバック処理
    /// </summary>
    /// <param name="damage"></param>
    protected void DoKnokBack()
    {
        float durationX = 130f;
        float durationY = 90f;
        m_rb2d.linearVelocity = Vector2.zero;
        m_rb2d.AddForce(new Vector2(-transform.localScale.x * durationX, durationY));
    }

    /// <summary>
    /// ダメージを受けたときの処理
    /// </summary>
    protected virtual void OnHit()
    {
        isAttacking = false;
        doOnceDecision = true;

        // ヒット時に攻撃処理などを停止する
        if (canCancelAttackOnHit)
        {
            StopAllManagedCoroutines();
        }
    }

    /// <summary>
    /// 被ダメージを表示する
    /// </summary>
    protected void DrawHitDamageUI(int damage, Vector3? attackerPos = null)
    {
        // 被ダメージ量のUIを表示する
        var attackerPoint = attackerPos != null ? (Vector3)attackerPos : transform.position;
        var hitPoint = TransformUtils.GetHitPointToTarget(transform, attackerPoint);
        if (hitPoint == null) hitPoint = transform.position;
        UIManager.Instance.PopDamageUI(damage, (Vector2)hitPoint, false);   // ダメージ表記
    }

    /// <summary>
    /// ダメージ適用リクエスト
    /// </summary>
    /// <param name="damage"></param>
    async public virtual void ApplyDamageRequest(int power, GameObject attacker = null, bool isKnokBack = true, bool drawDmgText = true, params DEBUFF_TYPE[] debuffList)
    {
        GameObject playerSelf = CharacterManager.Instance.PlayerObjSelf;
        if (isInvincible || isDead || attacker && attacker != playerSelf) return;

        #region リアルタイム同期用
        if (RoomModel.Instance)
        {
            if (attacker && attacker == playerSelf)
            {
                // プレイヤーによるダメージ適用
                await RoomModel.Instance.EnemyHealthAsync(uniqueId, power, new List<DEBUFF_TYPE>(debuffList));
            }
            else if (RoomModel.Instance.IsMaster)
            {
                // ギミックや状態異常によるダメージ適用(マスタクライアントのみリクエスト可能)
                await RoomModel.Instance.ApplyDamageToEnemyAsync(uniqueId, power);
            }
            return;
        }
        #endregion

        // 以降はローカル || ギミック用

        PlayerBase plBase = null;
        int damage = 0;

        if (attacker != null && attacker.tag == "Player")
        {
            plBase = attacker.GetComponent<PlayerBase>();

            damage = CalculationLibrary.CalcDamage(power, Defense - (int)(Defense * plBase.PierceRate));   // 貫通率適用

            //----------------
            // レリック適用

            // 自身がデバフを受けている + レリック「識別AI」所有時、ダメージが増加
            var debuffController = GetComponent<DebuffController>();
            if (debuffController.GetAppliedStatusEffects().Count != 0) damage = (int)(damage * plBase.DebuffDmgRate);

            // レリック「イリーガルスクリプト」適用時、ダメージを99999にする
            damage = (plBase.LotteryRelic(RELIC_TYPE.IllegalScript)) ? MAX_DAMAGE : damage;
        }
        else
        {
            // ギミックなどによるダメージ
            damage = power;
        }

        // ダメージ適用
        int remainingHp = this.hp - Mathf.Abs(damage);
        ApplyDamage(damage, remainingHp, attacker, isKnokBack, drawDmgText, debuffList);
    }

    /// <summary>
    /// ダメージ適用処理
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="remainingHP"></param>
    /// <param name="attacker"></param>
    /// <param name="drawDmgText"></param>
    /// <param name="debuffList"></param>
    public void ApplyDamage(int damage, int remainingHP, GameObject attacker = null, bool isKnokBack = true, bool drawDmgText = true, params DEBUFF_TYPE[] debuffList)
    {
        if (isInvincible || isDead) return;
        var charaManager = CharacterManager.Instance;

        // ダメージ適用、ダメージ表記
        Vector3? attackerPos = null;
        if (attacker != null) attackerPos = attacker.transform.position;
        if (drawDmgText && !isInvincible) DrawHitDamageUI(damage, attackerPos);
        hp = remainingHP;

        if(isBoss && UIManager.Instance) UIManager.Instance.UpdateBossStatus();

        // レリック「リゲインコード」所有時、与ダメージの一部をHP回復
        if (attacker != null && attacker.tag == "Player")
        {
            var plBase = attacker.GetComponent<PlayerBase>();

            if (plBase.DmgHealRate > 0)
            {
                plBase.HP += (int)(damage * plBase.DmgHealRate);

                if (plBase.HP > plBase.MaxHP) plBase.HP = plBase.MaxHP;
            }
        }

        // 状態異常を付与する
        if (debuffList.Length > 0) effectController.ApplyStatusEffect(debuffList);

        // アタッカーが居る方向にテクスチャを反転させ、ノックバックをさせる
        if (isKnokBack && attackerPos != null)
        {
            Vector3 pos = (Vector3)attackerPos;
            if (pos.x < transform.position.x && transform.localScale.x > 0
            || pos.x > transform.position.x && transform.localScale.x < 0) Flip();

            if (hp > 0 && canCancelAttackOnHit) ApplyStun(hitTime);

            if(canCancelAttackOnHit) DoKnokBack();
        }

        if (hp <= 0)
        {
            // 全てのコルーチン(攻撃処理やスタン処理など)を停止する
            StopAllCoroutines();

            PlayerBase player = attacker ? attacker.gameObject.GetComponent<PlayerBase>() : null;
            StartCoroutine(DestroyEnemy(player));
        }
        else
        {
            if (isUseHitBlendShader) PlayHitBlendShader(true, 0.1f);

            // 現在のターゲットを視認できていない&&アタッカーが存在する場合、ターゲットをアタッカーに更新する
            bool isTargetVisible = true;
            if (target && sightChecker) isTargetVisible = !sightChecker.IsObstructed() || sightChecker.IsTargetVisible();

            if(!isTargetVisible && attacker != null)
            {
                target = attacker;
            }
        }
    }

    /// <summary>
    /// 死亡処理
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator DestroyEnemy(PlayerBase player)
    {
        if (!isDead)
        {
            isDead = true;
            OnDead();
            if (player)
            {
                player.KilledHPRegain();
                if (!RoomModel.Instance)
                {
                    player.GetExp(exp);
                }
            }

            if (!dontRemoveOnDeath)
            {
                RemoveEnemyFromList();
            }

            m_rb2d.excludeLayers = LayerMask.GetMask("BlinkPlayer") | LayerMask.GetMask("Player"); ;  // プレイヤーとの判定を消す
            yield return new WaitForSeconds(destroyWaitSec);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 自身を外部のリストから削除する
    /// </summary>
    void RemoveEnemyFromList()
    {
        if (CharacterManager.Instance.Enemies[uniqueId].SpawnType == SPAWN_ENEMY_TYPE.ByTerminal)
        {   // 生成タイプがターミナルなら
            var termId = CharacterManager.Instance.Enemies[uniqueId].TerminalID;    // 端末IDを保管
            CharacterManager.Instance.RemoveEnemyFromList(uniqueId);                // リストから削除

            if (CharacterManager.Instance.GetEnemysByTerminalID(termId).Count == 0)
            {   // 生成端末の敵が全員倒されたら報酬出す
                if (RoomModel.Instance.IsMaster)
                    TerminalManager.Instance.TerminalObjs[termId].GetComponent<TerminalBase>().SuccessRequest();
            }
        }
        else
        {
            // Instanceがあるなら敵撃破関数を呼ぶ
            if (GameManager.Instance) GameManager.Instance.CrushEnemy(this);

            CharacterManager.Instance.RemoveEnemyFromList(uniqueId);
        }
    }

    /// <summary>
    /// トリガー接触判定
    /// </summary>
    /// <param name="collision"></param>
    private async void OnTriggerEnter2D(Collider2D collision)
    {
        // 範囲外にでたら破棄する
        if (!canIgnoreDeadZoneCollision && collision.gameObject.tag == "Gimmick/Abyss")
        {
            if (RoomModel.Instance && RoomModel.Instance.IsMaster)
            {
                await RoomModel.Instance.DeleteEnemyAsync(uniqueId);
            }
            else
            {
                CharacterManager.Instance.RemoveEnemyFromList(uniqueId);
                Destroy(gameObject);
            }
        }
    }

    #endregion

    #region スタン処理関連

    /// <summary>
    /// スタン処理
    /// </summary>
    /// <param name="time"></param>
    public void ApplyStun(float time, bool isHit = true)
    {
        if (isHit) OnHit();

        isStun = true;

        // スタンのコルーチンを再開始
        string cooldownKey = "StunTime";
        RemoveAndStopCoroutineByKey(cooldownKey);
        Coroutine coroutine = StartCoroutine(StunTime(time));
        managedCoroutines.Add(cooldownKey, coroutine);
    }

    /// <summary>
    /// 一定時間スタンさせる処理
    /// </summary>
    /// <returns></returns>
    IEnumerator StunTime(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);
        isStun = false;
    }

    #endregion

    #region テクスチャ・アニメーション関連

    #region 攻撃パターン１

    /// <summary>
    /// 攻撃アニメーションのイベント通知処理
    /// </summary>
    public virtual void OnAttackAnimEvent() { }

    /// <summary>
    /// 攻撃のアニメーション終了時のイベント通知処理
    /// </summary>
    public virtual void OnEndAttackAnimEvent() { }

    #endregion

    #region 攻撃パターン２

    /// <summary>
    /// 攻撃アニメーションのイベント通知処理
    /// </summary>
    public virtual void OnAttackAnim2Event() { }

    /// <summary>
    /// 攻撃のアニメーション終了時のイベント通知処理
    /// </summary>
    public virtual void OnEndAttackAnim2Event() { }

    #endregion

    #region 攻撃パターン３

    /// <summary>
    /// 攻撃アニメーションのイベント通知処理
    /// </summary>
    public virtual void OnAttackAnim3Event() { }

    /// <summary>
    /// 攻撃のアニメーション終了時のイベント通知処理
    /// </summary>
    public virtual void OnEndAttackAnim3Event() { }

    #endregion

    #region 攻撃パターン４

    /// <summary>
    /// 攻撃アニメーションのイベント通知処理
    /// </summary>
    public virtual void OnAttackAnim4Event() { }

    /// <summary>
    /// 攻撃のアニメーション終了時のイベント通知処理
    /// </summary>
    public virtual void OnEndAttackAnim4Event() { }

    #endregion

    #region オプションのアニメーションイベント

    /// <summary>
    /// アニメーションイベント オプション１
    /// </summary>
    public virtual void OnAnimEventOption1() { }

    /// <summary>
    /// アニメーションイベント オプション２
    /// </summary>
    public virtual void OnAnimEventOption2() { }

    #endregion

    #region 移動アニメーションイベント

    /// <summary>
    /// 移動するアニメーションイベント通知
    /// </summary>
    public virtual void OnMoveAnimEvent() { }

    /// <summary>
    /// 移動終了アニメーションイベント通知
    /// </summary>
    public virtual void OnEndMoveAnimEvent() { }

    #endregion

    #region スポーンアニメーションイベント

    /// <summary>
    /// スポーンアニメメーション開始時
    /// </summary>
    public virtual void OnSpawnAnimEvent()
    {
        if (!isStartComp) Start();
        isSpawn = false;
        isInvincible = true;    // 無敵状態にする & 本来の行動不可
    }

    /// <summary>
    /// スポーンアニメーションが終了したとき
    /// </summary>
    public virtual void OnEndSpawnAnimEvent()
    {
        isInvincible = false;
        isSpawn = false;

        // ターゲットを探す
        if (!isBoss)
        {
            var nearPlayer = GetNearPlayer();
            if (nearPlayer != null && sightChecker)
            {
                target = nearPlayer;    // 一時的にターゲットに設定
                bool isTargetVisible = !sightChecker.IsObstructed() || sightChecker.IsTargetVisible();
                if (!isTargetVisible) target = null;
            }
        }
        else SelectNewTargetInBossRoom();

        if (target) LookAtTarget();
    }
    #endregion

    #region ヒット演出関連

    /// <summary>
    /// 指定したアニメーションIDがヒットアニメーションかどうか
    /// </summary>
    /// <param name="animationId"></param>
    /// <returns></returns>
    public virtual bool IsHitAnimIdFrom(int animationId)
    {
        return false;
    }

    /// <summary>
    /// HitBlendの演出開始
    /// </summary>
    /// <param name="loopTime">演出時間</param>
    /// <param name="isStartFromOne">Blendの値を1から開始するかどうか</param>
    protected void PlayHitBlendShader(bool isStartFromOne, float loopTime, float delay = 0)
    {
        // 実行していなければ、クールダウンのコルーチンを開始
        string cooldownKey = "HitBlend";

        // HitBlendリセット
        RemoveAndStopCoroutineByKey(cooldownKey);
        float blend = isStartFromOne ? 1f : 0f;
        foreach (var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.material.SetFloat("_HitEffectBlend", blend);
        }

        Coroutine coroutine = StartCoroutine(HitBlendShaderCoroutine(loopTime, delay, isStartFromOne ,() =>
        {
            RemoveAndStopCoroutineByKey(cooldownKey);
        }));
        managedCoroutines.Add(cooldownKey, coroutine);
    }

    /// <summary>
    /// 指定した秒数にかけてBlendを徐々に1に変更する
    /// </summary>
    /// <param name="loopTime"></param>
    /// <param name="onFinished"></param>
    /// <returns></returns>
    protected IEnumerator HitBlendShaderCoroutine(float loopTime, float delay, bool isStartFromOne, Action onFinished)
    {
        yield return new WaitForSeconds(delay);

        float currentTime = 0;
        while (currentTime < loopTime)
        {
            currentTime += Time.deltaTime;

            // 進行度（0-1または1-0）
            float nextBlend = Mathf.Clamp01(currentTime / loopTime);
            nextBlend = isStartFromOne ? 1 - nextBlend : nextBlend;

            // 各スプライトのマテリアルに反映
            foreach (var spriteRenderer in spriteRenderers)
            {
                Material material = spriteRenderer.material;
                material.SetFloat("_HitEffectBlend", nextBlend);
            }

            yield return null;
        }

        // 最終的に値を『0か1』に固定
        float blend = isStartFromOne ? 0 : 1f;
        foreach (var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.material.SetFloat("_HitEffectBlend", blend);
        }

        onFinished?.Invoke();
    }

    #endregion

    #endregion

    #region コルーチン管理関連

    /// <summary>
    /// 管理しているものの中で、起動中のコルーチンを検索する
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected bool ContaintsManagedCoroutine(string key)
    {
        bool isHit = false;
        if (managedCoroutines.Count > 0 && managedCoroutines.ContainsKey(key))
        {
            if (managedCoroutines[key] == null)
            {
                managedCoroutines.Remove(key);
            }
            else
            {
                isHit = true;
            }            
        }
        return isHit;
    }

    /// <summary>
    /// 指定したkeyのコルーチンの要素を削除
    /// </summary>
    /// <param name="key"></param>
    protected void RemoveAndStopCoroutineByKey(string key)
    {
        if (ContaintsManagedCoroutine(key))
        {
            if (managedCoroutines[key] != null) StopCoroutine(managedCoroutines[key]);
            managedCoroutines.Remove(key);
        }
    }

    /// <summary>
    /// 管理している全てのコルーチンを停止する
    /// </summary>
    protected void StopAllManagedCoroutines()
    {
        if (managedCoroutines.Count > 0)
        {
            foreach (var coroutine in managedCoroutines.Values)
            {
                if(coroutine != null) StopCoroutine(coroutine);
            }
        }

        managedCoroutines.Clear();
    }
    #endregion

    #region リアルタイム同期関連

    /// <summary>
    /// マスタクライアント切り替え時に状態をリセットする
    /// </summary>
    public virtual void ResetAllStates()
    {
        StopAllManagedCoroutines();
        isAttacking = false;
        isStun = false;
        isInvincible = false;
        doOnceDecision = true;
        isPatrolPaused = false;
        m_rb2d.bodyType = defaultType2D;
    }

    /// <summary>
    /// EnemyDataを作成する
    /// </summary>
    /// <param name="enemyData">型を指定</param>
    /// <returns></returns>
    protected EnemyData SetEnemyData(EnemyData enemyData)
    {
        var debuffController = GetComponent<DebuffController>();

        enemyData.IsActiveSelf = this.gameObject.activeInHierarchy;
        enemyData.Status = new CharacterStatusData(
            hp: this.MaxHP,
            defence: this.MaxDefence,
            power: this.MaxPower,
            moveSpeed: this.MaxMoveSpeed,
            attackSpeedFactor: this.MaxAttackSpeedFactor,
            jumpPower: this.MaxJumpPower
        );
        enemyData.State = new CharacterStatusData(
            hp: this.HP,
            defence: this.defense,
            power: this.power,
            moveSpeed: this.moveSpeed,
            attackSpeedFactor: this.attackSpeedFactor,
            jumpPower: this.jumpPower
        );
        enemyData.Position = this.transform.position;
        enemyData.Scale = this.transform.localScale;
        enemyData.Rotation = this.transform.rotation;
        enemyData.AnimationId = this.GetAnimId();
        enemyData.DebuffList = debuffController.GetAppliedStatusEffects();

        // 以下は専用プロパティ
        enemyData.UniqueId = this.UniqueId;
        enemyData.EnemyName = this.gameObject.name;
        enemyData.isBoss = this.IsBoss;
        enemyData.IsInvincible = this.isInvincible;
        enemyData.TypeId = this.EnemyTypeId;
        enemyData.Exp = this.Exp;

        return enemyData;
    }

    /// <summary>
    /// EnemyData取得処理
    /// </summary>
    /// <returns></returns>
    public virtual EnemyData GetEnemyData()
    {
        return SetEnemyData(new EnemyData());
    }

    /// <summary>
    /// 敵の同期情報を更新する
    /// </summary>
    /// <param name="enemyData"></param>
    public virtual void UpdateEnemy(EnemyData enemyData)
    {
        uniqueId = enemyData.UniqueId;
        gameObject.name = enemyData.EnemyName;
        isBoss = enemyData.isBoss;
        isInvincible = enemyData.IsInvincible;
        exp = enemyData.Exp;
    }

    #endregion

    #region チェック関連

    /// <summary>
    /// 指定した座標にスポーンする場合、障害物と重なるかどうかチェックする
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool IsOverlappingObstacle(Vector2 point)
    {
        terrainLayerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("Gimmick") | LayerMask.GetMask("Scaffold");
        return Physics2D.OverlapCircle(point, checkObstacleRange, terrainLayerMask);
    }

    #endregion

    #region Debug描画処理関連

    /// <summary>
    /// 他の検知範囲の描画処理
    /// </summary>
    abstract protected void DrawDetectionGizmos();

    /// <summary>
    /// [ デバック用 ] Gizmosを使用して検出範囲を描画
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!canDrawRay) return;

        // 視線描画
        if (sightChecker != null)
        {
            sightChecker.DrawSightLine(canChaseTarget, target);
        }

        DrawDetectionGizmos();

        Gizmos.color = Color.azure;
        Gizmos.DrawWireSphere(transform.position, checkObstacleRange);
    }

    #endregion
}