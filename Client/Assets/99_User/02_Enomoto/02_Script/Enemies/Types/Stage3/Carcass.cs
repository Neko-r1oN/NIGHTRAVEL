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

public class Carcass : EnemyBase
{
    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        RotationAttack,
        RotationAttack_Rotate,
        RotationAttack_Finish,
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
        RotationAttack,
        MeleeAttack,
        Tracking,
        AirMovement
    }

    /// <summary>
    /// 管理するコルーチンの種類
    /// </summary>
    public enum COROUTINE
    {
        RotationAttack,
        MeleeAttack,
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

    #region プレイヤー関連
    List<GameObject> hitPlayers = new List<GameObject>();   // 攻撃を受けたプレイヤーのリスト
    readonly float disToTargetMin = 0.25f;
    #endregion

    #region オリジナル
    [Foldout("オリジナル")]
    [Tooltip("回転攻撃の際にpowerにかける倍率")]
    float rotationPowerRate = 1.5f;

    bool isRotationAttacking;
    #endregion

    protected override void Start()
    {
        isRotationAttacking = false;
        isAttacking = false;
        base.Start();
    }

    /// <summary>
    /// 行動パターン実行処理
    /// </summary>
    protected override void DecideBehavior()
    {
        if (canChaseTarget && !IsGround())
        {
            AirMovement();
        }
        else if (canAttack && !sightChecker.IsObstructed() && disToTarget <= attackDist
            || canAttack && !sightChecker.IsObstructed() && disToTarget <= attackDist * 3)
        {
            if (disToTarget <= attackDist)
            {
                Attack(DECIDE_TYPE.MeleeAttack);
            }
            else if (disToTarget <= attackDist * 3)
            {
                Attack(DECIDE_TYPE.RotationAttack);
            }
        }
        else if (moveSpeed > 0 && canChaseTarget && Mathf.Abs(disToTargetX) > disToTargetMin)
        {
            if (canChaseTarget && IsWall() && !canJump)
            {
                Idle();
            }
            else if (canChaseTarget && target)
            {
                Tracking();
            }
        }
        else Idle();
    }

    /// <summary>
    /// アイドル処理
    /// </summary>
    protected override void Idle()
    {
        SetAnimId((int)ANIM_ID.Idle);
        m_rb2d.linearVelocity = new Vector2(0f, m_rb2d.linearVelocity.y);
    }

    #region 攻撃処理関連

    /// <summary>
    /// 回転攻撃時の攻撃判定用
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isRotationAttacking && collision.gameObject.tag == "Player" && !hitPlayers.Contains(collision.gameObject))
        {
            // 自身がエリート個体の場合、付与する状態異常の種類を取得する
            DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();

            hitPlayers.Add(collision.gameObject);
            collision.gameObject.GetComponent<PlayerBase>().ApplyDamage((int)(power * rotationPowerRate), transform.position, KB_POW.Medium, applyEffect);

        }
    }

    /// <summary>
    /// 攻撃処理
    /// </summary>
    public void Attack(DECIDE_TYPE attackType)
    {
        isAttacking = true;
        m_rb2d.linearVelocity = Vector2.zero;

        switch (attackType)
        {
            case DECIDE_TYPE.RotationAttack:
                SetAnimId((int)ANIM_ID.RotationAttack);
                break;
            case DECIDE_TYPE.MeleeAttack:
                SetAnimId((int)ANIM_ID.MeleeAttack);
                break;
        }
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] 回転攻撃処理
    /// </summary>

    public override void OnAttackAnimEvent()
    {
        if (isStun)
        {
            isAttacking = false;
            return;
        }

        hitPlayers.Clear();

        SetAnimId((int)ANIM_ID.RotationAttack_Rotate);
        m_rb2d.linearVelocity = Vector3.zero;
        m_rb2d.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);

        // 実行していなければ、回転攻撃のコルーチンを開始
        string cooldownKey = COROUTINE.RotationAttack.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(RotationAttackCoroutine(() =>
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
        // 前に飛び出す
        Vector2 jumpVec = new Vector2(moveSpeed * 1.5f * TransformUtils.GetFacingDirection(transform), jumpPower / 3);
        m_rb2d.linearVelocity = jumpVec;
        hitPlayers.Clear();

        // 実行していなければ、近接攻撃のコルーチンを開始
        string cooldownKey = COROUTINE.MeleeAttack.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(MeleeAttack());
            managedCoroutines.Add(cooldownKey, coroutine);
        }
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] 近接攻撃の判定処理を終了
    /// </summary>
    public override void OnEndAttackAnim2Event()
    {
        RemoveAndStopCoroutineByKey(COROUTINE.MeleeAttack.ToString());

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
    IEnumerator RotationAttackCoroutine(Action onFinished)
    {
        bool isAttacked = false;
        float startAttackSec = 2f;
        float currentSec = 0;

        while (true)
        {
            if (isStun)
            {
                m_rb2d.bodyType = RigidbodyType2D.Dynamic;
                isAttacking = false;
                yield break;
            }

            if(target == null)
            {
                var nearPlayer = GetNearPlayer(transform.position);
                if (nearPlayer != null) target = nearPlayer;
            }
            if (target)
            {
                // ターゲットのいる方向を向く
                LookAtTarget();
            }

            currentSec += Time.deltaTime;

            if (currentSec >= startAttackSec && !isAttacked)
            {
                isRotationAttacking = true;
                isAttacked = true;
                Vector3 targetPos = target ? target.transform.position : this.transform.position + Vector3.down;

                // 目的地に向かって突進
                m_rb2d.bodyType = RigidbodyType2D.Dynamic;
                m_rb2d.linearVelocity = Vector3.zero;
                m_rb2d.AddForce((targetPos - transform.position).normalized * jumpPower * 1.5f, ForceMode2D.Impulse);
            }
            else if (m_rb2d.linearVelocity.y <= 0 && !isAttacked)
            {
                // 空中で静止
                m_rb2d.bodyType = RigidbodyType2D.Static;
            }

            if (IsGround() && isAttacked)
            {
                isRotationAttacking = false;
                SetAnimId((int)ANIM_ID.RotationAttack_Finish);
                break;
            }

            yield return null;
        }

        // 実行していなければ、クールダウンのコルーチンを開始
        string cooldownKey = COROUTINE.AttackCooldown.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(AttackCooldown(attackCoolTime * 1.5f, () => {
                RemoveAndStopCoroutineByKey(cooldownKey);
            }));
            managedCoroutines.Add(cooldownKey, coroutine);
        }

        onFinished?.Invoke();
    }

    /// <summary>
    /// 近接攻撃の判定を繰り返す
    /// </summary>
    /// <returns></returns>
    IEnumerator MeleeAttack()
    {
        while (true)
        {
            // 自身がエリート個体の場合、付与する状態異常の種類を取得する
            DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();

            Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(meleeAttackCheck.position, meleeAttackRange);
            for (int i = 0; i < collidersEnemies.Length; i++)
            {
                if (collidersEnemies[i].gameObject.tag == "Player" && !hitPlayers.Contains(collidersEnemies[i].gameObject))
                {
                    hitPlayers.Add(collidersEnemies[i].gameObject);
                    collidersEnemies[i].gameObject.GetComponent<PlayerBase>().ApplyDamage(power, transform.position, KB_POW.Medium, applyEffect);
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// 攻撃時のクールダウン処理
    /// </summary>
    /// <returns></returns>
    IEnumerator AttackCooldown(float time, Action onFinished)
    {
        Idle();
        yield return new WaitForSeconds(time);
        isAttacking = false;
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
        isRotationAttacking = false;
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
        isRotationAttacking = false;
        m_rb2d.bodyType = RigidbodyType2D.Dynamic;
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
