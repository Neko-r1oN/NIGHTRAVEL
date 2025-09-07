//**************************************************
//  [敵] カルカスのクラス
//  Author:r-enomoto
//**************************************************
using DG.Tweening;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Carcass_Old : EnemyBase
{
    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        RotateAttack,
        RotateAttack_Rotate,
        RotateAttack_Finish,
        MeleeAttack,
        Run,
        Hit,
        Dead,
    }

    /// <summary>
    /// 行動パターン
    /// </summary>
    public enum DECIDE_TYPE
    {
        Waiting = 1,
        RotateAttack,
        MeleeAttack,
        Tracking,
        AirMovement
    }
    DECIDE_TYPE nextDecide = DECIDE_TYPE.Waiting;

    /// <summary>
    /// 管理するコルーチンの種類
    /// </summary>
    public enum COROUTINE
    {
        NextDecision,
        RotateAnim,
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
    [Foldout("チェック関連")]
    [SerializeField]
    Transform groundCheck;
    [Foldout("チェック関連")]
    [SerializeField]
    Vector2 groundCheckRadius = new Vector2(0.5f, 0.2f);

    // 落下チェック
    [Foldout("チェック関連")]
    [SerializeField]
    Transform fallCheck;
    [Foldout("チェック関連")]
    [SerializeField]
    Vector2 fallCheckRange;
    #endregion

    #region 抽選関連
    float decisionTimeMax = 2f;
    float randomDecision;
    bool endDecision;
    #endregion

    #region ターゲットと離す距離
    readonly float disToTargetMin = 0.25f;
    #endregion

    #region オリジナル
    [Foldout("オリジナル")]
    [SerializeField]
    GameObject selfCurledObj;

    [Foldout("オリジナル")]
    [SerializeField]
    float rotateSpeed;
    #endregion

    protected override void Start()
    {
        isAttacking = false;
        doOnceDecision = true;
        NextDecision();
        base.Start();
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
                    NextDecision(1f);
                    break;
                case DECIDE_TYPE.RotateAttack:
                    Attack(nextDecide);
                    break;
                case DECIDE_TYPE.MeleeAttack:
                    Attack(nextDecide);
                    break;
                case DECIDE_TYPE.Tracking:
                    Tracking();
                    NextDecision(0);
                    break;
                case DECIDE_TYPE.AirMovement:
                    AirMovement();
                    NextDecision(0);
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
    void NextDecision(float? time = null)
    {
        if (time == null) time = UnityEngine.Random.Range(0.1f, decisionTimeMax);

        // 実行していなければ、行動の抽選のコルーチンを開始
        string key = COROUTINE.NextDecision.ToString();
        if (!ContaintsManagedCoroutine(key))
        {
            Coroutine coroutine = StartCoroutine(NextDecisionCoroutine((float)time, () => { RemoveAndStopCoroutineByKey(key); }));
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
        if (isSpawn)
        {
            weights[DECIDE_TYPE.Waiting] = 100;
        }
        else if (canChaseTarget && !IsGround())
        {
            weights[DECIDE_TYPE.AirMovement] = 100;
        }
        else if (canAttack && !sightChecker.IsObstructed() && disToTarget <= attackDist)
        {
            weights[DECIDE_TYPE.Waiting] = 15;

            if (disToTarget <= attackDist / 4) 
            {
                weights[DECIDE_TYPE.RotateAttack] = 25;
                weights[DECIDE_TYPE.MeleeAttack] = 50;
            }
            else if (disToTarget <= attackDist)
            {
                weights[DECIDE_TYPE.RotateAttack] = 50;
                weights[DECIDE_TYPE.MeleeAttack] = 25;
            }
        }
        else if (moveSpeed > 0 && canChaseTarget && Mathf.Abs(disToTargetX) > disToTargetMin)
        {
            if (canChaseTarget && IsWall() && !canJump)
            {
                weights[DECIDE_TYPE.Waiting] = 50;
            }
            else if (canChaseTarget && target)
            {
                weights[DECIDE_TYPE.Tracking] = 50;
            }
        }
        else weights[DECIDE_TYPE.Waiting] = 100;

        weights.Values.OrderBy(x => x); ;  // 昇順に並び替え
        #endregion

        // 全体の長さを使って抽選
        int totalWeight = weights.Values.Sum();
        randomDecision = UnityEngine.Random.Range(1, totalWeight + 1);

        // 抽選した値で次の行動パターンを決定する
        int currentWeight = 0;
        foreach(var weight in weights)
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
    public void Attack(DECIDE_TYPE attackType)
    {
        if(attackType == DECIDE_TYPE.RotateAttack || attackType == DECIDE_TYPE.MeleeAttack)
        {
            isAttacking = true;
            m_rb2d.linearVelocity = Vector2.zero;

            switch (attackType)
            {
                case DECIDE_TYPE.RotateAttack:
                    SetAnimId((int)ANIM_ID.RotateAttack);
                    break;
                case DECIDE_TYPE.MeleeAttack:
                    SetAnimId((int)ANIM_ID.MeleeAttack);
                    break;
            }
        }
        else
        {
            NextDecision(0);
        }
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] 回転攻撃処理
    /// </summary>

    public override void OnAttackAnimEvent()
    {
        m_rb2d.linearVelocity = Vector3.zero;
        m_rb2d.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);

        // 実行していなければ、回転攻撃のコルーチンを開始
        string cooldownKey = COROUTINE.RotateAnim.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(RotateAttackCoroutine(() =>
            {
                RemoveAndStopCoroutineByKey(cooldownKey);
            }));
            managedCoroutines.Add(cooldownKey, coroutine);
        }
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] 近接攻撃処理
    /// </summary>

    public override void OnAttackAnim2Event()
    {
        // 自身がエリート個体の場合、付与する状態異常の種類を取得する
        DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();

        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Player")
            {
                collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position, KB_POW.Medium, applyEffect);
            }
        }

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
    /// 回転攻撃のコルーチン
    /// </summary>
    /// <returns></returns>
    IEnumerator RotateAttackCoroutine(Action onFinished)
    {
        bool isAttacked = false;
        float startAttackSec = 2f;
        float currentSec = 0;

        while (true)
        {
            currentSec += Time.deltaTime;
            //selfCurledObj.transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);

            if (currentSec >= startAttackSec && !isAttacked)
            {
                isAttacked = true;

                // ターゲットのいる方向に突進
                m_rb2d.bodyType = RigidbodyType2D.Dynamic;
                m_rb2d.linearVelocity = Vector3.zero;
                m_rb2d.AddForce((target.transform.position - transform.position).normalized * jumpPower * 1.2f, ForceMode2D.Impulse);
            }
            else if (currentSec >= startAttackSec / 2 && !isAttacked)
            {
                // 空中で静止
                m_rb2d.bodyType = RigidbodyType2D.Static;
            }

            if (IsGround() && isAttacked)
            {
                SetAnimId((int)ANIM_ID.RotateAttack_Finish);
                break;
            }

            yield return null;
        }

        // 実行していなければ、クールダウンのコルーチンを開始
        string cooldownKey = COROUTINE.AttackCooldown.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(AttackCooldown(attackCoolTime, () => {
                RemoveAndStopCoroutineByKey(cooldownKey);
            }));
            managedCoroutines.Add(cooldownKey, coroutine);
        }

        onFinished?.Invoke();
    }

    /// <summary>
    /// 攻撃時のクールダウン処理
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time, Action onFinished)
    {
        isAttacking = true;
        yield return new WaitForSeconds(time);
        isAttacking = false;
        Idle();
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
        SetAnimId((int)ANIM_ID.Run);
        Vector2 speedVec = Vector2.zero;
        if (IsFall() || IsWall())
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
    /// 空中状態での移動処理
    /// </summary>
    void AirMovement()
    {
        SetAnimId((int)ANIM_ID.Run);

        // ジャンプ(落下)中にプレイヤーに向かって移動する
        float distToPlayer = target.transform.position.x - this.transform.position.x;
        Vector3 targetVelocity = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * moveSpeed, m_rb2d.linearVelocity.y);
        Vector3 velocity = Vector3.zero;
        m_rb2d.linearVelocity = Vector3.SmoothDamp(m_rb2d.linearVelocity, targetVelocity, ref velocity, 0.05f);
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
        m_rb2d.bodyType = RigidbodyType2D.Dynamic;
    }

    /// <summary>
    /// 死亡するときに呼ばれる処理
    /// </summary>
    /// <returns></returns>
    protected override void OnDead()
    {
        SetAnimId((int)ANIM_ID.Dead);
    }

    #endregion

    #region テクスチャ・アニメーション関連

    /// <summary>
    /// スポーンアニメメーション開始時
    /// </summary>
    public override void OnSpawnAnimEvent()
    {
        base.OnSpawnAnimEvent();
        SetAnimId((int)ANIM_ID.Spawn);

        // 前に飛び込む
        Vector2 jumpVec = new Vector2(moveSpeed * 1.2f * TransformUtils.GetFacingDirection(transform), jumpPower);
        m_rb2d.linearVelocity = jumpVec;
    }

    /// <summary>
    /// スポーンアニメーションが終了したとき
    /// </summary>
    public override void OnEndSpawnAnimEvent()
    {
        base.OnEndSpawnAnimEvent();
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
                animator.Play("Carcass_Damage_Animation", 0, 0);
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
    /// 落下中かどうか
    /// </summary>
    /// <returns></returns>
    bool IsFall()
    {
        return !Physics2D.OverlapBox(fallCheck.position, fallCheckRange, 0, terrainLayerMask);
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
        if (fallCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(fallCheck.position, fallCheckRange);
        }
    }

    #endregion
}
