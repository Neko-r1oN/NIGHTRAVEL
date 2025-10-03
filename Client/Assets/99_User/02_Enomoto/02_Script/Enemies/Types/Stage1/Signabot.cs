//**************************************************
//  [敵] シグナボットのクラス
//  Author:r-enomoto
//**************************************************
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class Signabot : EnemyBase
{
    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Attack,
        Hit,
        Dead,
    }

    /// <summary>
    /// 管理するコルーチンの種類
    /// </summary>
    public enum COROUTINE
    {
        AttackCooldown,
        MeleeAttack
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

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
    }

    protected override void FixedUpdate()
    {
        if (isSpawn || isStun || isInvincible || hp <= 0 || !sightChecker) return;
        SetAnimId((int)ANIM_ID.Attack);
        DecideBehavior();
    }


    /// <summary>
    /// 行動パターン実行処理
    /// </summary>
    protected override void DecideBehavior()
    {
        if (!isAttacking) return;

        if (m_rb2d.linearVelocityY < 0 && !IsGround())
        {
            AirMovement();
        }
        else if (canPatrol)
        {
            Patorol();
        }
    }

    #region 攻撃処理関連

    /// <summary>
    /// [Animationイベントからの呼び出し] 近接攻撃処理
    /// </summary>

    public override void OnAttackAnimEvent()
    {
        isAttacking = true;

        // 実行していなければ、近接攻撃のコルーチンを開始
        string cooldownKey = COROUTINE.MeleeAttack.ToString();
        if (!ContaintsManagedCoroutine(cooldownKey))
        {
            Coroutine coroutine = StartCoroutine(MeleeAttack());
            managedCoroutines.Add(cooldownKey, coroutine);
        }
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] 攻撃の判定処理を終了
    /// </summary>
    public override void OnEndAttackAnimEvent()
    {
        isAttacking = false;
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
                if (collidersEnemies[i].gameObject.tag == "Player")
                {
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
        yield return new WaitForSeconds(time);
        isAttacking = false;
        Idle();
        onFinished?.Invoke();
    }

    #endregion

    #region 移動処理関連

    /// <summary>
    /// 巡回する処理
    /// </summary>
    protected override void Patorol()
    {
        if (IsFall() || IsWall()) Flip();
        Vector2 speedVec = new Vector2(TransformUtils.GetFacingDirection(transform) * moveSpeed, m_rb2d.linearVelocity.y);
        m_rb2d.linearVelocity = speedVec;
    }

    /// <summary>
    /// 空中状態での移動処理
    /// </summary>
    void AirMovement()
    {
        // 現在向いている方向に移動する
        Vector2 speedVec = new Vector2(TransformUtils.GetFacingDirection(transform) * moveSpeed, m_rb2d.linearVelocity.y);
        m_rb2d.linearVelocity = speedVec;
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
                animator.Play("Hit_Signabot", 0, 0);
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
