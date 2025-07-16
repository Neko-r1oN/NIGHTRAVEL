//**************************************************
//  [ボス] フルメタルワーム(本体)のの管理クラス
//  Author:r-enomoto
//**************************************************
using DG.Tweening;
using NUnit.Framework;
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
    #endregion

    #region 状態管理
    readonly float disToTargetPosMin = 3f;
    float randomDecision;
    bool endDecision;
    #endregion

    #region 敵の生成関連
    [Foldout("敵の生成関連")]
    [SerializeField]
    List<GameObject> enemyPrefabs = new List<GameObject>();
    public List<GameObject> EnemyPrefabs { get { return enemyPrefabs; } }

    [Foldout("敵の生成関連")]
    [SerializeField]
    int generatedMax = 15;
    public int GeneratedMax { get { return generatedMax; } }

    [Foldout("敵の生成関連")]
    [SerializeField]
    float distToPlayerMin = 6;
    public float DistToPlayerMin { get { return distToPlayerMin; } }

    // 生成済みの敵
    List<GameObject> generatedEnemies = new List<GameObject>();
    public List<GameObject> GeneratedEnemies { get { return generatedEnemies; } set { generatedEnemies = value; } }
    #endregion

    #region その他メンバ変数
    Vector2 targetPos;
    #endregion

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
        doOnceDecision = false;
        endDecision = true;
        isSpawn = false;
        isInvincible = false;
        bodys.AddRange(GetComponentsInChildren<FullMetalBody>(true));   // 全ての子オブジェクトが持つFullMetalBodyを取得
        MeleeAttack();

        // 実行していなければ、行動の抽選のコルーチンを開始
        if (!ContaintsManagedCoroutine(COROUTINE.NextDecision.ToString()))
        {
            Coroutine coroutine = StartCoroutine(NextDecision());
            managedCoroutines.Add(COROUTINE.NextDecision.ToString(), coroutine);
        }
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
            if (!isAttacking && randomDecision <= 0.4f)
            {
                // 既に死亡している生成済みの敵の要素を削除
                generatedEnemies.RemoveAll(item => item == null);

                // 範囲内にPlayerがいるかチェック
                int layerNumber = 1 << LayerMask.NameToLayer("Player");
                Collider2D hit = Physics2D.OverlapCircle(playerCheck.position, playerCheckRange, layerNumber);
                if (hit)
                {
                    // 全ての部位の行動を実行
                    isAttacking = true;
                    ExecuteAllPartActions();

                    // 実行していなければ、クールダウンのコルーチンを開始
                    string key = COROUTINE.AttackCooldown.ToString();
                    if (!ContaintsManagedCoroutine(key))
                    {
                        Coroutine coroutine = StartCoroutine(AttackCooldown(attackCoolTime, () => { RemoveCoroutineByKey(key); }));
                        managedCoroutines.Add(key, coroutine);
                    }
                }
                else
                {
                    // 実行していなければ、行動の抽選のコルーチンを開始
                    string key = COROUTINE.NextDecision.ToString();
                    if (!ContaintsManagedCoroutine(key))
                    {
                        Coroutine coroutine = StartCoroutine(NextDecision(0.5f));
                        managedCoroutines.Add(key, coroutine);
                    }
                }
            }
            else if (randomDecision <= 0.8f)
            {
                RemoveCoroutineByKey(COROUTINE.MoveGraduallyCoroutine.ToString());

                // 実行していなければ、移動のコルーチンを開始
                string key = COROUTINE.MoveCoroutine.ToString();
                if (!ContaintsManagedCoroutine(key))
                {
                    Coroutine coroutine = StartCoroutine(MoveCoroutine(() => { RemoveCoroutineByKey(key); }));
                    managedCoroutines.Add(key, coroutine);
                }
            }
            else
            {
                // 実行していなければ、行動の抽選のコルーチンを開始
                string key = COROUTINE.NextDecision.ToString();
                if (!ContaintsManagedCoroutine(key))
                {
                    Coroutine coroutine = StartCoroutine(NextDecision());
                    managedCoroutines.Add(key, coroutine);
                }
            }
        }
    }

    /// <summary>
    /// 次の行動パターン決定処理
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator NextDecision(float? time = null, Action onFinished = null)
    {
        if (time == null) time = Mathf.Floor(UnityEngine.Random.Range(decisionTimeMin, decisionTimeMax));
        doOnceDecision = false;

        // 実行していなければ、ゆっくり移動するコルーチンを開始
        string key = COROUTINE.MoveGraduallyCoroutine.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(MoveGraduallyCoroutine());
            managedCoroutines.Add(key, coroutine);
        }

        yield return new WaitForSeconds((float)time);
        randomDecision = UnityEngine.Random.Range(0f, 1f);
        doOnceDecision = true;
        onFinished?.Invoke();
    }

    #region 攻撃処理関連

    [ContextMenu("ExecuteAllPartActions")]
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
    /// 近接攻撃処理
    /// </summary>
    public void MeleeAttack()
    {
        // 実行していなければ、ゆっくり移動するコルーチンを開始
        string key = COROUTINE.MeleeAttackCoroutine.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(MeleeAttackCoroutine(() => { RemoveCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
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
            StatusEffectController.EFFECT_TYPE? applyEffect = GetStatusEffectToApply();

            Collider2D[] collidersEnemies = Physics2D.OverlapBoxAll(meleeAttackCheck.position, meleeAttackRange, 0);
            for (int i = 0; i < collidersEnemies.Length; i++)
            {
                if (collidersEnemies[i].gameObject.tag == "Player")
                {
                    collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position, applyEffect);
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
    IEnumerator AttackCooldown(float time, Action onFinished)
    {
        yield return new WaitForSeconds(time);
        isAttacking = false;

        // 実行していなければ、行動の抽選するコルーチンを開始
        string key = COROUTINE.NextDecision.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(NextDecision(null, () => { RemoveCoroutineByKey(key); }));
            managedCoroutines.Add(key, coroutine);
        }
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

        // 実行していなければ、行動の抽選するコルーチンを開始
        if (!ContaintsManagedCoroutine(COROUTINE.NextDecision.ToString()))
        {
            Coroutine coroutine = StartCoroutine(NextDecision());
            managedCoroutines.Add(COROUTINE.NextDecision.ToString(), coroutine);
        }
        onFinished?.Invoke();
    }

    /// <summary>
    /// 外部から停止されるまでゆっくりと移動し続ける処理
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveGraduallyCoroutine()
    {
        SetNextTargetPosition(false);
        while (true)
        {
            RotateTowardsMovementDirection(targetPos, rotationSpeedMin);
            MoveTowardsTarget(moveSpeedMin);
            float disToTargetPos = Mathf.Abs(Vector3.Distance(targetPos, this.transform.position));
            if (disToTargetPos <= disToTargetPosMin / 2)
            {
                SetNextTargetPosition(false);
            }
            yield return null;
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
    }

    /// <summary>
    /// ダメージ適用処理
    /// </summary>
    /// <param name="power"></param>
    /// <param name="attacker"></param>
    /// <param name="effectTypes"></param>
    public override void ApplyDamage(int power, Transform attacker = null, bool drawDmgText = true, params StatusEffectController.EFFECT_TYPE[] effectTypes)
    {
        attacker = null;
        base.ApplyDamage(power, attacker, true, effectTypes);
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
        //Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(transform.position, terrainCheckRange);
    }

    #endregion

}
