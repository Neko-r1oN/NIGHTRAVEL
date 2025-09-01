//**************************************************
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
using UnityEditor.Experimental.GraphView;

abstract public class EnemyBase : CharacterBase
{
    #region プレイヤー・ターゲット
    [Header("プレイヤー・ターゲット")]
    protected GameObject target;
    public GameObject Target { get { return target; } set { target = value; } }

    public List<GameObject> players = new List<GameObject>();
    public List<GameObject> Players { get { return players; } set { players = value; } }
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
    private EnumManager.SPAWN_ENEMY_TYPE spawnEnemyType;

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

    #region システム
    [Foldout("システム")]
    [Tooltip("生成されるときの地面からの距離")]
    [SerializeField]
    float spawnGroundOffset;

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

    [Foldout("システム")]
    [SerializeField]
    protected int spawnWeight = 1;  // スポーンの抽選する際の重み

    [Foldout("システム")]
    private Terminal terminalManager;
    #endregion

    #region 外部参照用プロパティ

    public int SpawnWeight { get { return spawnWeight; } }

    public int BaseExp { get { return baseExp; } }

    public int Exp { get { return exp; } set { exp = value; } }

    public float AttackDist { get { return attackDist; } }

    public float SpawnGroundOffset { get { return spawnGroundOffset; } }

    public bool IsBoss { get { return isBoss; } set { isBoss = value; } }

    public bool IsElite { get { return isElite; } }

    public List<SpriteRenderer> SpriteRenderers { get { return spriteRenderers; } }

    public EnumManager.SPAWN_ENEMY_TYPE SpawnEnemyType { get { return spawnEnemyType; } set { spawnEnemyType = value; } }

    public Terminal TerminalManager { get { return terminalManager; } set { terminalManager = value; } }
    #endregion

    #region ID
    int selfID;

    /// <summary>
    /// 自身のユニークなID
    /// </summary>
    public int SelfID { get { return selfID; } set { selfID = value; } }
    #endregion

    protected override void Start()
    {
        base.Start();
        players = new List<GameObject>(CharacterManager.Instance.PlayerObjs.Values);
        terrainLayerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("Gimmick");
        m_rb2d = GetComponent<Rigidbody2D>();
        sightChecker = GetComponent<EnemySightChecker>();
        chaseAI = GetComponent<EnemyChaseAI>();
        enemyElite = GetComponent<EnemyElite>();
        isStartComp = true;
    }

    protected virtual void FixedUpdate()
    {
        if (isSpawn || isStun || isAttacking || isInvincible || hp <= 0 || !sightChecker) return;

        // ターゲットが存在しない || 現在のターゲットが死亡している場合
        if (Players.Count > 0 && !target || target && target.GetComponent<CharacterBase>().HP <= 0)
        {
            // 新しくターゲットを探す
            target = sightChecker.GetTargetInSight();
        }

        if (target)
        {
            // 実行中でなければ、ターゲットを監視するコルーチンを開始
            string key = "CheckTargetObstructionCoroutine";
            if (!ContaintsManagedCoroutine(key))
            {
                Coroutine coroutine = StartCoroutine(CheckTargetObstructionCoroutine(() => {
                    RemoveCoroutineByKey(key);
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

        // ターゲットとの距離を取得する
        disToTarget = Vector3.Distance(target.transform.position, this.transform.position);
        disToTargetX = target.transform.position.x - transform.position.x;

        // ターゲットのいる方向にテクスチャを反転
        if (canChaseTarget)
        {
            if (target.transform.position.x < transform.position.x && transform.localScale.x > 0
                || target.transform.position.x > transform.position.x && transform.localScale.x < 0) Flip();
        }

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
    /// 方向転換
    /// </summary>
    public void Flip()
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

    #region プレイヤー・ターゲット関連

    /// <summary>
    /// ターゲットとの間に遮蔽物があるかを監視し続けるコルーチン
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckTargetObstructionCoroutine(Action onFinished)
    {
        float obstructionMaxTime = 3f;
        float currentTime = 0;
        float waitSec = 0.01f;

        // obstructionMaxTime以上経過でターゲットを見失ったことにする
        while (currentTime < obstructionMaxTime)
        {
            yield return new WaitForSeconds(waitSec);

            if (sightChecker.IsObstructed() || !sightChecker.IsTargetVisible())
            {
                currentTime += waitSec;
            }
            else
            {
                currentTime = 0;
            }        
        }

        if (target && currentTime >= obstructionMaxTime)
        {
            target = null;
            if (chaseAI) chaseAI.Stop();
        }

        // 実行中でなければ、その場に待機するコルーチンを開始
        string key = "Waiting";
        float waitTime = 2f;
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine waitCoroutine = StartCoroutine(Waiting(waitTime, () => { RemoveCoroutineByKey(key); }));
            managedCoroutines.Add(key, waitCoroutine);
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
        foreach (GameObject player in Players)
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
    public void PromoteToElite(EnumManager.ENEMY_ELITE_TYPE type)
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
        transform.gameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        transform.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(-transform.localScale.x * durationX, durationY));
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
    public virtual void ApplyDamageRequest(int power, GameObject attacker = null, bool drawDmgText = true, params DEBUFF_TYPE[] debuffList)
    {
        GameObject playerSelf = CharacterManager.Instance.PlayerObjSelf;
        if (isInvincible || isDead || attacker && attacker != playerSelf) return;

        // リアルタイム中、自分による攻撃の場合はダメージ同期をリクエスト
        if (RoomModel.Instance && attacker && attacker == playerSelf)
        {
            return;
        }
        // ギミックや状態異常によるダメージはマスタクライアントのみ処理させる
        else if (RoomModel.Instance && !RoomModel.Instance.IsMaster)
        {
            return;
        }


        // 以降はローカル || ギミック用
        int damage = CalculationLibrary.CalcDamage(power, Defense);
        int remainingHp = this.hp - Mathf.Abs(damage);
        ApplyDamage(damage, remainingHp, attacker, drawDmgText, debuffList);
    }

    /// <summary>
    /// ダメージ適用処理
    /// </summary>
    /// <param name="damegeData"></param>
    public void ApplyDamage(int damage, int remainingHP, GameObject attacker = null, bool drawDmgText = true, params DEBUFF_TYPE[] debuffList)
    {
        if (isInvincible || isDead) return;
        var charaManager = CharacterManager.Instance;

        // ダメージ適用、ダメージ表記
        Vector3? attackerPos = null;
        if (attacker != null) attackerPos = attacker.transform.position;
        if (drawDmgText && !isInvincible) DrawHitDamageUI(damage, attackerPos);
        hp -= Mathf.Abs(damage);

        // 状態異常を付与する
        if (debuffList.Length > 0) effectController.ApplyStatusEffect(debuffList);

        // アタッカーが居る方向にテクスチャを反転させ、ノックバックをさせる
        if (attackerPos != null)
        {
            Vector3 pos = (Vector3)attackerPos;
            if (pos.x < transform.position.x && transform.localScale.x > 0
            || pos.x > transform.position.x && transform.localScale.x < 0) Flip();

            DoKnokBack();

            if (hp > 0 && canCancelAttackOnHit) ApplyStun(hitTime);
        }

        if (hp <= 0)
        {
            // 全てのコルーチン(攻撃処理やスタン処理など)を停止する
            StopAllCoroutines();

            PlayerBase player = attacker ? attacker.gameObject.GetComponent<PlayerBase>() : null;
            StartCoroutine(DestroyEnemy(player));
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
                // 倒したのが自分自身の場合は経験値を与える
                if (!RoomModel.Instance || RoomModel.Instance && CharacterManager.Instance.PlayerObjSelf == player)
                    player.GetExp(exp);
            }

            if (spawnEnemyType == SPAWN_ENEMY_TYPE.ByTerminal)
            {// 生成タイプがターミナルなら
                // 死んだ敵をリストから削除
                terminalManager.TerminalSpawnList.Remove(this.gameObject);
                if(terminalManager.TerminalSpawnList.Count <= 0)
                {// リストのカウントが0なら
                    // レリックの生成
                    RelicManager.Instance.GenerateRelicTest();
                }
            }
            else
            {
                // Instanceがあるなら敵撃破関数を呼ぶ
                if (GameManager.Instance) GameManager.Instance.CrushEnemy(this);
            }

            m_rb2d.excludeLayers = LayerMask.GetMask("BlinkPlayer") | LayerMask.GetMask("Player"); ;  // プレイヤーとの判定を消す
            yield return new WaitForSeconds(destroyWaitSec);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// トリガー接触判定
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 範囲外にでたら破棄する
        if (!canIgnoreDeadZoneCollision && collision.gameObject.tag == "Gimmick/Abyss")
        {
            Destroy(gameObject);
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
        isStun = true;
        if(isHit) OnHit();
        Coroutine coroutine = StartCoroutine(StunTime(time));
        managedCoroutines.Add("StunTime", coroutine);
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

    /// <summary>
    /// 攻撃アニメーションのイベント通知処理
    /// </summary>
    public virtual void OnAttackAnimEvent() { }

    /// <summary>
    /// 攻撃のアニメーション終了時のイベント通知処理
    /// </summary>
    public virtual void OnEndAttackAnimEvent() { }

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
    }
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
    protected void RemoveCoroutineByKey(string key)
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
                if (coroutine != null) StopCoroutine(coroutine);
            }
        }

        managedCoroutines.Clear();
    }
    #endregion

    #region リアルタイム同期関連

    /// <summary>
    /// マスタクライアント切り替え時に状態をリセットする
    /// </summary>
    abstract public void ResetAllStates();

    /// <summary>
    /// EnemyDataを作成する
    /// </summary>
    /// <param name="enemyData">型を指定</param>
    /// <returns></returns>
    protected EnemyData CreateEnemyData(EnemyData enemyData)
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
        enemyData.EnemyID = this.SelfID;
        enemyData.EnemyName = this.gameObject.name;
        enemyData.isBoss = this.IsBoss;
        Exp = this.Exp;

        return enemyData;
    }

    /// <summary>
    /// EnemyData取得処理
    /// </summary>
    /// <returns></returns>
    public virtual EnemyData GetEnemyData()
    {
        return CreateEnemyData(new EnemyData());
    }

    /// <summary>
    /// 敵の同期情報を更新する
    /// </summary>
    /// <param name="enemyData"></param>
    public virtual void UpdateEnemy(EnemyData enemyData)
    {
        selfID = enemyData.EnemyID;
        gameObject.name = enemyData.EnemyName;
        isBoss = enemyData.isBoss;
        exp = enemyData.Exp;
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
            sightChecker.DrawSightLine(canChaseTarget, target, players);
        }

        DrawDetectionGizmos();
    }

    #endregion
}