//**************************************************
//  [ボス] フルメタルワーム(本体)のの管理クラス
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class FullMetalWorm : EnemyBase
{
    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        Idle = 0,
        Attack,
        Dead,
    }

    /// <summary>
    /// 管理するコルーチンの種類
    /// </summary>
    public enum COROUTINE
    {
        NextDecision,
        MeleeAttackCoroutine,
        AttackCooldown,
        MoveCoroutine,
        MoveGraduallyCoroutine,
    }

    /// <summary>
    /// 行動パターン
    /// </summary>
    public enum DECIDE_TYPE
    {
        None = 0,
        Move,
        Attack,
    }
    DECIDE_TYPE nextDecide = DECIDE_TYPE.None;

    #region コンポーネント
    List<FullMetalBody> bodys = new List<FullMetalBody>();
    #endregion

    #region ステータス
    [Foldout("ステータス")]
    [SerializeField]
    float rotationSpeed = 50f; // 頭の回転速度

    [Foldout("ステータス")]
    [SerializeField]
    float rotationSpeedMin = 5f; // 最小回転速度

    [Foldout("ステータス")]
    [SerializeField]
    float moveSpeedMin = 2.5f;    // 最小移動速度
    #endregion

    #region チェック判定
    // 近くにプレイヤーがいるかチェックする範囲
    [Foldout("チェック関連")]
    [SerializeField]
    Transform playerCheck;
    [Foldout("チェック関連")]
    [SerializeField]
    float playerCheckRange;

    // 近距離攻撃の範囲
    [Foldout("チェック関連")]
    [SerializeField]
    Transform meleeAttackCheck;
    [Foldout("チェック関連")]
    [SerializeField]
    Vector2 meleeAttackRange = Vector2.zero;

    // 移動可能範囲関連
    [Foldout("チェック関連")]
    [SerializeField]
    float moveRange;

    [Foldout("チェック関連")]
    [SerializeField]
    float terrainCheckRange;    // 地形に埋まっていないかチェックする範囲
    public float TerrainCheckRane { get { return terrainCheckRange; } }
    #endregion

    #region 抽選処理関連
    [Foldout("抽選関連")]
    [SerializeField] 
    float decisionTimeMax = 6f;

    [Foldout("抽選関連")]
    [SerializeField]
    float decisionTimeMin = 2f;

    float randomDecision;
    #endregion

    #region 状態管理
    readonly float disToTargetPosMin = 3f;
    #endregion

    #region 敵の生成関連
    [Foldout("敵の生成関連")]
    [SerializeField]
    int generatedMax = 15;
    public int GeneratedMax { get { return generatedMax; } }

    [Foldout("敵の生成関連")]
    [SerializeField]
    float distToPlayerMin = 10;
    public float DistToPlayerMin { get { return distToPlayerMin; } }

    // 生成している敵の数
    int generatedEnemyCnt = 0;
    public int GeneratedEnemyCnt { get { return generatedEnemyCnt; } set { generatedEnemyCnt = value; } }
    #endregion

    #region その他メンバ変数
    Vector2 targetPos;
    #endregion

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
        doOnceDecision = false;
        bodys.AddRange(GetComponentsInChildren<FullMetalBody>(true));   // 全ての子オブジェクトが持つFullMetalBodyを取得
    }

    protected override void FixedUpdate()
    {
        if (isSpawn || isStun || isAttacking || isInvincible || hp <= 0) return;

        if (!target && Players.Count > 0)
        {
            // ターゲットを探す
            SetRandomTargetPlayer();
        }

        if (target)
        {
            // ターゲットとの距離を取得する
            disToTarget = Vector3.Distance(target.transform.position, this.transform.position);
            disToTargetX = target.transform.position.x - transform.position.x;
        }

        DecideBehavior();
    }

    /// <summary>
    /// 行動パターン実行処理
    /// </summary>
    protected override void DecideBehavior()
    {
        if (doOnceDecision)
        {
            doOnceDecision = false;
            RemoveAndStopCoroutineByKey(COROUTINE.MoveGraduallyCoroutine.ToString());

            if (nextDecide == DECIDE_TYPE.Attack)
            {
                generatedEnemyCnt = CharacterManager.Instance.GetEnemiesBySpawnType(SPAWN_ENEMY_TYPE.ByWorm).Count;
                // 全ての部位の行動を実行
                isAttacking = true;
                ExecuteAllPartActions();
                MoveGradually(true);
                AttackCooldown();
            }        
            else if (nextDecide == DECIDE_TYPE.Move)
            {
                Move();
            }
            else
            {
                NextDecision();
            }
        }
    }

    #region コルーチン呼び出し

    /// <summary>
    /// 次の行動パターン抽選コルーチン呼び出し
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    void NextDecision(float? time = null)
    {
        // 実行していなければ、行動の抽選のコルーチンを開始
        string key = COROUTINE.NextDecision.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(NextDecisionCoroutine(time, () => { RemoveAndStopCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// 近接攻撃のコルーチン呼び出し
    /// </summary>
    public void MeleeAttack()
    {
        // 実行していなければ、ゆっくり移動するコルーチンを開始
        string key = COROUTINE.MeleeAttackCoroutine.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(MeleeAttackCoroutine(() => { RemoveAndStopCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// 攻撃クールダウンのコルーチン呼び出し
    /// </summary>
    void AttackCooldown()
    {
        // 実行していなければ、クールダウンのコルーチンを開始
        string key = COROUTINE.AttackCooldown.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(AttackCooldownCoroutine(attackCoolTime, () => { RemoveAndStopCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// 移動のコルーチン呼び出し
    /// </summary>
    void Move()
    {
        // 実行していなければ、移動のコルーチンを開始
        string key = COROUTINE.MoveCoroutine.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(MoveCoroutine(() => { RemoveAndStopCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
    }

    /// <summary>
    /// ゆっくり移動し続けるコルーチン呼び出し
    /// </summary>
    void MoveGradually(bool isTargetLottery = false)
    {
        // 実行していなければ、ゆっくり移動するコルーチンを開始
        string key = COROUTINE.MoveGraduallyCoroutine.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(MoveGraduallyCoroutine(isTargetLottery));
            managedCoroutines.Add(key, coroutine);
        }
    }

    #endregion

    #region 抽選処理関連
    /// <summary>
    /// 次の行動パターン決定処理
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator NextDecisionCoroutine(float? time = null, Action onFinished = null)
    {
        if (time == null) time = Mathf.Floor(UnityEngine.Random.Range(decisionTimeMin, decisionTimeMax));
        doOnceDecision = false;
        MoveGradually();
        yield return new WaitForSeconds((float)time);

        // 各行動パターンの重み
        int attackWeight = 0, moveWeight = 5;

        if (!isAttacking)
        {
            // 範囲内にPlayerがいるかチェック
            int layerNumber = 1 << LayerMask.NameToLayer("Player");
            Collider2D hit = Physics2D.OverlapCircle(playerCheck.position, playerCheckRange, layerNumber);
            if (hit)
            {
                attackWeight = 10;
            }
        }

        // 全体の長さを使って抽選
        int totalWeight = attackWeight + moveWeight;
        randomDecision = UnityEngine.Random.Range(1, totalWeight + 1);

        // 抽選した値で次の行動パターンを決定する
        if (randomDecision <= attackWeight) nextDecide = DECIDE_TYPE.Attack;
        else nextDecide = DECIDE_TYPE.Move;

        doOnceDecision = true;
        onFinished?.Invoke();
    }

    #endregion

    #region 攻撃処理関連

    /// <summary>
    /// 全ての部位の行動を実行
    /// </summary>
    public void ExecuteAllPartActions()
    {
        foreach (var body in bodys)
        {
            if (body.HP > 0)
            {
                body.ActByRoleType();
            }
        }
    }

    /// <summary>
    /// 近接攻撃コルーチン
    /// </summary>
    /// <returns></returns>
    IEnumerator MeleeAttackCoroutine(Action onFinished)
    {
        SetAnimId((int)ANIM_ID.Attack);
        while (!isDead)
        {
            // 自身がエリート個体の場合、付与する状態異常の種類を取得する
            DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();

            Collider2D[] collidersEnemies = Physics2D.OverlapBoxAll(meleeAttackCheck.position, meleeAttackRange, 0);
            for (int i = 0; i < collidersEnemies.Length; i++)
            {
                if (collidersEnemies[i].gameObject.tag == "Player")
                {
                    collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position,KB_POW.Big, applyEffect);
                }
            }
            yield return null;
        }
        onFinished?.Invoke();
    }

    /// <summary>
    /// クールダウン処理
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldownCoroutine(float time, Action onFinished)
    {
        yield return new WaitForSeconds(time);
        isAttacking = false;
        NextDecision();
        onFinished?.Invoke();
    }

    #endregion

    #region 移動処理関連

    /// <summary>
    /// 進行方向に向かって移動する処理
    /// </summary>
    /// <param name="targetPos"></param>
    /// <param name="direction"></param>
    void MoveTowardsTarget(float speed)
    {
        m_rb2d.linearVelocity = transform.up.normalized * speed;
    }

    /// <summary>
    /// 現在の進行方向に向かって回転させる
    /// </summary>
    void RotateTowardsMovementDirection(Vector3 targetPos, float rotationSpeed)
    {
        Vector2 direction = (targetPos - this.transform.position).normalized;
        // 動いていない場合は回転させない
        if (direction == Vector2.zero) return;

        // ベクトルから角度を計算（ラジアンから度数に変換し、-90度で真上を0度にする）
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        // 現在の回転角度から目標角度へ滑らかに補間し、Rigidbody2Dの回転を更新
        float newAngle = Mathf.LerpAngle(m_rb2d.rotation, targetAngle, Time.fixedDeltaTime * rotationSpeed);
        m_rb2d.MoveRotation(newAngle);
    }

    /// <summary>
    /// 目標地点まで移動する処理
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveCoroutine(Action onFinished)
    {
        SetNextTargetPosition(true);
        float currentSpeed = moveSpeed;
        bool isTargetPos = false;

        while (true)
        {
            // 目標地点に到達するまで、角度を追従する
            float disToTargetPos = Mathf.Abs(Vector3.Distance(targetPos, this.transform.position));
            if (!isTargetPos && disToTargetPos <= disToTargetPosMin) isTargetPos = true;
            if (!isTargetPos) RotateTowardsMovementDirection(targetPos, rotationSpeed);

            // 目標地点到達後、徐々に減速する
            if (isTargetPos)
            {
                if (currentSpeed <= moveSpeedMin) break;
                else currentSpeed -= Time.deltaTime * moveSpeed;
            }

            MoveTowardsTarget(currentSpeed);
            yield return null;
        }

        NextDecision();
        onFinished?.Invoke();
    }

    /// <summary>
    /// 外部から停止されるまでゆっくりと移動し続ける処理
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveGraduallyCoroutine(bool isTargetLottery = false)
    {
        Vector2 maxPos = SpawnManager.Instance.StageMaxPoint.position;
        Vector2 minPos = SpawnManager.Instance.StageMinPoint.position;
        SetNextTargetPosition(isTargetLottery);
        while (true)
        {
            if (!isTargetLottery)
            {
                // プレイヤーとの距離がかけ離れている場合は、プレイヤーの座標に向かって移動
                float distToNearPlayer = Vector2.Distance(GetNearPlayer().transform.position, transform.position);
                if (Mathf.Abs(distToNearPlayer) > playerCheckRange * 1.5f)
                {
                    SetNextTargetPosition(true);
                }
            }

            RotateTowardsMovementDirection(targetPos, rotationSpeedMin);
            MoveTowardsTarget(moveSpeedMin);
            float disToTargetPos = Mathf.Abs(Vector3.Distance(targetPos, this.transform.position));
            if (disToTargetPos <= disToTargetPosMin)
            {
                SetNextTargetPosition(isTargetLottery);
            }
            yield return null;

            // 現在ステージ範囲外にいる場合は現在のコルーチンを終了し、MoveCoroutineを実行
            if (transform.position.x < minPos.x || transform.position.x > maxPos.x
                || transform.position.y < minPos.y || transform.position.y > maxPos.y)
            {
                Move();
                RemoveAndStopCoroutineByKey(COROUTINE.NextDecision.ToString());
                RemoveAndStopCoroutineByKey(COROUTINE.MoveGraduallyCoroutine.ToString());
            }
        }
    }

    #endregion

    #region ヒット処理関連

    /// <summary>
    /// 死亡するときに呼ばれる処理処理
    /// </summary>
    /// <returns></returns>
    protected override void OnDead()
    {
        SetAnimId((int)ANIM_ID.Dead);

        // 全ての部位の死亡処理を呼び出し
        foreach (var body in bodys)
        {
            StartCoroutine(body.DestroyEnemy(null));
        }

        // 生成したザコ敵を破棄する
        foreach (var enemy in CharacterManager.Instance.GetEnemiesBySpawnType(SPAWN_ENEMY_TYPE.ByWorm))
        {
            StartCoroutine(enemy.GetComponent<EnemyBase>().DestroyEnemy(null));
        }
    }

    /// <summary>
    /// ダメージ適用処理
    /// </summary>
    /// <param name="power"></param>
    /// <param name="attacker"></param>
    /// <param name="effectTypes"></param>
    public override void ApplyDamageRequest(int power, GameObject attacker = null, bool isKnokBack = true, bool drawDmgText = true, params DEBUFF_TYPE[] effectTypes)
    {
        base.ApplyDamageRequest(power, attacker, false, drawDmgText, effectTypes);
    }

    #endregion

    #region アニメーションイベント関連

    /// <summary>
    /// 死亡アニメーション再生中に呼ばれる
    /// </summary>
    public void OnDeathAnimEvent()
    {
        // 全ての部位のデスポーンアニメーション再生
        foreach (var body in bodys)
        {
            body.Despown();
        }
    }

    /// <summary>
    /// スポーンアニメメーション開始時
    /// </summary>
    public void OnSpawnAnimEventByHead()
    {
        OnSpawnAnimEvent();
    }

    /// <summary>
    /// スポーンアニメーションが終了したとき
    /// </summary>
    public void OnEndSpawnAnimEventByHead()
    {
        OnEndSpawnAnimEvent();

        MeleeAttack();
        NextDecision();
    }

    #endregion

    #region リアルタイム同期関連

    /// <summary>
    /// マスタクライアント切り替え時に状態をリセットする
    /// </summary>
    public override void ResetAllStates()
    {
        base.ResetAllStates();

        bool isAttacking = IsBodyAttacking();
        foreach (var body in bodys)
        {
            body.ResetAllStates();
        }

        nextDecide = isAttacking ? DECIDE_TYPE.Attack : DECIDE_TYPE.None;
        MeleeAttack();
        DecideBehavior();
    }

    /// <summary>
    /// 一つでもbodyが攻撃中かどうか
    /// </summary>
    /// <returns></returns>
    bool IsBodyAttacking()
    {
        bool isAttacking = false;
        foreach (var body in bodys)
        {
            FullMetalBody.ANIM_ID animId = (FullMetalBody.ANIM_ID)body.GetAnimId();
            if (animId == FullMetalBody.ANIM_ID.Attack)
            {
                isAttacking = true; break;
            }
        }
        return isAttacking;
    }

    #endregion

    #region チェック処理関連

    /// <summary>
    /// playerリストからランダムにターゲットを決める処理
    /// </summary>
    bool SetRandomTargetPlayer()
    {
        List<GameObject> alivePlayers = GetAlivePlayers();
        if (alivePlayers.Count > 0) target = alivePlayers[UnityEngine.Random.Range(0, alivePlayers.Count)];
        else target = null;
        return target;
    }

    /// <summary>
    /// 次の目標地点を設定する
    /// </summary>
    /// <returns></returns>
    void SetNextTargetPosition(bool isTargetLottery = true)
    {
        if (isTargetLottery)
        {
            SetRandomTargetPlayer();
            float rnd = UnityEngine.Random.Range(0f, 1f);
            if (target) targetPos = target.transform.position;
        }

        if (!target || !isTargetLottery)
        {
            for (int i = 0; i < 100; i++)
            {
                // ランダムな地点を抽選
                Vector2 maxPos = SpawnManager.Instance.StageMaxPoint.position;
                Vector2 minPos = SpawnManager.Instance.StageMinPoint.position;
                targetPos = new Vector2(UnityEngine.Random.Range(minPos.x, maxPos.x + 1), UnityEngine.Random.Range(minPos.y, maxPos.y + 1));

                // ランダムな目標地点との距離が一定以上離れていれば確定する
                float distance = Mathf.Abs(Vector3.Distance(targetPos, this.transform.position));
                if (distance >= moveRange)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// ステージの範囲を描画する
    /// </summary>
    void DrawRectGizmos()
    {
        if (SpawnManager.Instance == null) return;
        Vector2 min = SpawnManager.Instance.StageMinPoint.position;
        Vector2 max = SpawnManager.Instance.StageMaxPoint.position;
        Vector2 bl = new Vector2(min.x, min.y);
        Vector2 br = new Vector2(max.x, min.y);
        Vector2 tr = new Vector2(max.x, max.y);
        Vector2 tl = new Vector2(min.x, max.y);

        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);
    }

    /// <summary>
    /// 検出範囲の描画処理
    /// </summary>
    protected override void DrawDetectionGizmos()
    {
        // Player検知範囲
        if (playerCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerCheck.transform.position, playerCheckRange);
        }

        // 攻撃範囲
        if (meleeAttackCheck)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(meleeAttackCheck.transform.position, meleeAttackRange);
        }

        // 移動範囲
        DrawRectGizmos();

        // 障害物を検知する範囲
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, terrainCheckRange);
    }

    #endregion

}
