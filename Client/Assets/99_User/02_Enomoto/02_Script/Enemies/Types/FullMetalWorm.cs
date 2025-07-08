//**************************************************
//  [ボス] フルメタルワームのクラス
//  Author:r-enomoto
//**************************************************
using DG.Tweening;
using NUnit.Framework;
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullMetalWorm : EnemyBase
{
    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        None = 0,
        Idle,
        Attack,
        Run,
        Hit,
        Dead,
    }

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
    // 近距離攻撃の範囲
    [Foldout("チェック関連")]
    [SerializeField]
    Transform meleeAttackCheck;
    [Foldout("チェック関連")]
    [SerializeField] 
    float meleeAttackRange = 0.9f;

    [Foldout("チェック関連")]
    [SerializeField]
    Transform stageCenter;
    [Foldout("チェック関連")]
    [SerializeField]
    float moveRange;

    [Foldout("チェック関連")]
    [SerializeField]
    float terrainCheckRange;    // 地形に埋まっていないかチェックする範囲
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
    List<Transform> enemySpawnPoints = new List<Transform>();   // 敵を生成する部位

    [Foldout("敵の生成関連")]
    [SerializeField]
    List<GameObject> enemyPrefabs = new List<GameObject>();

    [Foldout("敵の生成関連")]
    [SerializeField]
    int generatedMax = 15;

    [Foldout("敵の生成関連")]
    [SerializeField]
    float distToPlayerMin = 6;

    List<GameObject> generatedEnemies = new List<GameObject>();

    // あとで消す
    [SerializeField] Transform minRange;
    [SerializeField] Transform maxRange;

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
        cancellCoroutines.Add(StartCoroutine(NextDecision()));
    }

    protected override void FixedUpdate()
    {
        if (isSpawn || isStun || isAttacking || isInvincible || hp <= 0) return;

        if (!target && Players.Count > 0)
        {
            // ターゲットを探す
            SetRandomTargetPlayer();
        }

        if (!target)
        {
            //if (canPatrol && !isPatrolPaused) Patorol();
            //else Idle();
            //return;

            // ランダムな座標に向かって移動する
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
            if (!isAttacking && randomDecision <= 0.5f)
            {
                // 既に死亡している敵の要素を削除
                generatedEnemies.RemoveAll(s => s == null);

                if (generatedEnemies.Count < generatedMax)
                {
                    RunEnemySpawnLoops();
                    return;
                }
            }
            else
            {
                StopCoroutine("MoveGraduallyCoroutine");
                cancellCoroutines.Add(StartCoroutine(MoveCoroutine()));
            }
        }
    }

    /// <summary>
    /// 次の行動パターン決定処理
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator NextDecision()
    {
        SetNextTargetPosition();
        float time = Mathf.Floor(Random.Range(decisionTimeMin, decisionTimeMax));
        doOnceDecision = false;
        StartCoroutine("MoveGraduallyCoroutine");
        yield return new WaitForSeconds(time);
        randomDecision = Random.Range(0f, 1f);
        doOnceDecision = true;
    }

    #region 攻撃処理関連

    /// <summary>
    /// ザコ敵を複数生成するコルーチンの実行
    /// </summary>
    void RunEnemySpawnLoops()
    {
        isAttacking = true;
        bool isGenerateSucsess = false;
        int generatedEnemiesCnt = generatedEnemies.Count;
        var alivePlayers = GetAlivePlayers();

        foreach (Transform point in enemySpawnPoints)
        {
            if (generatedEnemiesCnt >= generatedMax) continue;

            bool isPlayerNearby = false;
            foreach (var playerObj in alivePlayers)
            {
                float distToPlayer = Vector2.Distance(point.position, playerObj.transform.position);
                if (distToPlayer <= distToPlayerMin) isPlayerNearby = true;
            }

            // 生成位置の近くにプレイヤーがいる && 生成位置がステージの範囲内 && 生成位置が壁に埋まっていない場合
            if (isPlayerNearby
                && TransformUtils.IsWithinBounds(point, minRange, maxRange)
                && !Physics2D.OverlapCircle(point.position, terrainCheckRange, terrainLayerMask))
            {
                isGenerateSucsess = true;
                int maxEnemies = Random.Range(1, 4);
                if (generatedEnemiesCnt + maxEnemies > generatedMax) maxEnemies = generatedMax - generatedEnemiesCnt;

                cancellCoroutines.Add(StartCoroutine(GenerateEnemeiesCoroutine(point, maxEnemies)));
            }
        }

        // 一度でも敵の生成が成功した場合はクールタイムをしっかり実行する
        float time = isGenerateSucsess ? attackCoolTime : 0f;
        cancellCoroutines.Add(StartCoroutine(AttackCooldown(time)));
    }

    /// <summary>
    /// ザコ敵を複数生成するコルーチン
    /// </summary>
    IEnumerator GenerateEnemeiesCoroutine(Transform point, int maxEnemies)
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            Random.InitState(System.DateTime.Now.Millisecond);  // 乱数のシード値を更新
            float time = Random.Range(0f, 0.5f);
            yield return new WaitForSeconds(time);

            // ここに生成する処理 && ハッチが開くアニメーション####################################
            Random.InitState(System.DateTime.Now.Millisecond);  // 乱数のシード値を更新
            generatedEnemies.Add(GenerateEnemy(point.transform.position));
        }
    }

    /// <summary>
    /// ザコ敵を生成する処理
    /// </summary>
    GameObject GenerateEnemy(Vector2 point)
    {
        var enemyObj = Instantiate(enemyPrefabs[(int)Random.Range(0, enemyPrefabs.Count)], point, Quaternion.identity).gameObject;
        EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();

        if ((int)Random.Range(0, 2) == 0) enemy.Flip();    // 確率で向きが変わる
        enemy.TransparentSprites();
        return enemyObj;
    }

    /// <summary>
    /// 攻撃処理
    /// </summary>
    public void Attack()
    {
        doOnceDecision = false;
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;
        SetAnimId((int)ANIM_ID.Attack);
    }

    /// <summary>
    /// 一定時間、タレットで攻撃する処理
    /// </summary>

    public override void OnAttackAnimEvent()
    {
        //// 前に飛び込む
        //Vector2 jumpVec = new Vector2(18 * TransformUtils.GetFacingDirection(transform), 10);
        //m_rb2d.linearVelocity = jumpVec;

        //// 自身がエリート個体の場合、付与する状態異常の種類を取得する
        //bool isElite = this.isElite && enemyElite != null;
        //StatusEffectController.EFFECT_TYPE? applyEffect = null;
        //if (isElite)
        //{
        //    applyEffect = enemyElite.GetAddStatusEffectEnum();
        //}

        //Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
        //for (int i = 0; i < collidersEnemies.Length; i++)
        //{
        //    if (collidersEnemies[i].gameObject.tag == "Player")
        //    {
        //        collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position, applyEffect);
        //    }
        //}

        cancellCoroutines.Add(StartCoroutine(AttackCooldown(attackCoolTime)));
    }

    /// <summary>
    /// クールダウン処理
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time)
    {
        yield return new WaitForSeconds(time);
        isAttacking = false;
        cancellCoroutines.Add(StartCoroutine(NextDecision()));
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
        //Vector2 direction = (targetPos - transform.position).normalized;
        //m_rb2d.linearVelocity = direction * speed;
        //RotateTowardsMovementDirection();

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
    IEnumerator MoveCoroutine()
    {
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
                else currentSpeed -= Time.deltaTime * moveSpeed * 0.7f;
            }

            MoveTowardsTarget(currentSpeed);
            yield return null;
        }
        StartCoroutine(NextDecision());
    }

    /// <summary>
    /// 外部から停止されるまでゆっくりと移動し続ける処理
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveGraduallyCoroutine()
    {
        while (true)
        {
            RotateTowardsMovementDirection(targetPos, rotationSpeedMin);
            MoveTowardsTarget(moveSpeedMin);
            float disToTargetPos = Mathf.Abs(Vector3.Distance(targetPos, this.transform.position));
            if (disToTargetPos <= disToTargetPosMin / 2)
            {
                SetNextTargetPosition();
            }
            yield return null;
        }
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

    #region チェック処理関連

    /// <summary>
    /// playerリストからランダムにターゲットを決める処理
    /// </summary>
    bool SetRandomTargetPlayer()
    {
        List<GameObject> alivePlayers = GetAlivePlayers();
        if (alivePlayers.Count > 0) target = alivePlayers[Random.Range(0, alivePlayers.Count)];
        else target = null;
        return target;
    }

    /// <summary>
    /// ステージの中央の座標を設定
    /// </summary>
    /// <param name="stageCenter"></param>
    public void SetStageCenterParam(Transform stageCenter)
    {
        this.stageCenter = stageCenter;
    }

    /// <summary>
    /// 次の目標地点を設定する
    /// </summary>
    /// <returns></returns>
    void SetNextTargetPosition(bool isTargetLottery = true)
    {
        if(isTargetLottery) SetRandomTargetPlayer();

        if (target)
        {
            targetPos = target.transform.position;
        }
        else
        {
            for (int i = 0; i < 100; i++)
            {
                // ターゲットのプレイヤーが存在しない場合、ランダムな地点に移動する
                Vector2 maxPos = (Vector2)stageCenter.position + Vector2.one * moveRange;
                Vector2 minPos = (Vector2)stageCenter.position + Vector2.one * -moveRange;
                targetPos = new Vector2(Random.Range(minPos.x, maxPos.x + 1), Random.Range(minPos.y, maxPos.y + 1));

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

        // 移動範囲
        if (stageCenter != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere((Vector2)stageCenter.position, moveRange);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, terrainCheckRange);
    }

    #endregion

}
