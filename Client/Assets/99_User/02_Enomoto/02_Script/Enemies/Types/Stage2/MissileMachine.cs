//**************************************************
//  [敵] ミサイルマシンのクラス
//  Author:r-enomoto
//**************************************************
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Pixeye.Unity;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class MissileMachine : EnemyBase
{
    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum ANIM_ID
    {
        Spawn = 0,
        Idle,
        Attack,
        Run,
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

    #region コンポーネント
    EnemyProjectileChecker projectileChecker;
    #endregion

    #region 攻撃関連
    [Foldout("攻撃関連")]
    [SerializeField]
    Transform aimTransform; // 弾発射部分

    [Foldout("攻撃関連")]
    [SerializeField]
    GameObject missileBulletPrefab;

    [Foldout("攻撃関連")]
    [SerializeField]
    float shootSpeed;
    #endregion

    #region チェック関連
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

    #region ターゲットと離す距離
    readonly float disToTargetMin = 0.25f;
    #endregion

    protected override void Start()
    {
        base.Start();
        isAttacking = false;
        projectileChecker = aimTransform.GetComponent<EnemyProjectileChecker>();
    }

    /// <summary>
    /// 行動パターン実行処理
    /// </summary>
    protected override void DecideBehavior()
    {
        // 行動パターン
        if (canChaseTarget && !IsGround())
        {
            AirMovement();
        }
        else if (canAttack && projectileChecker.CanFireProjectile(target) && !sightChecker.IsObstructed())
        {
            Attack();
        }
        else if (moveSpeed > 0 && canPatrol && Mathf.Abs(disToTargetX) > disToTargetMin
            || moveSpeed > 0 && canChaseTarget && Mathf.Abs(disToTargetX) > disToTargetMin)
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
    }

    #region 攻撃処理関連

    /// <summary>
    /// 攻撃処理
    /// </summary>
    public void Attack()
    {
        isAttacking = true;
        SetAnimId((int)ANIM_ID.Attack);
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] 後方に飛ぶだけ
    /// </summary>

    public override void OnAttackAnimEvent()
    {
        // 後方に飛ぶ
        Vector2 jumpVec = new Vector2(moveSpeed * 3f * -TransformUtils.GetFacingDirection(transform), jumpPower);
        m_rb2d.linearVelocity = jumpVec;
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] ミサイル発射
    /// </summary>
    public override async void OnAttackAnim2Event()
    {
        // 自身がエリート個体の場合、付与する状態異常の種類を取得する
        DEBUFF_TYPE? applyEffect = GetStatusEffectToApply();
        List<DEBUFF_TYPE> debuffs = new List<DEBUFF_TYPE>();
        if (applyEffect != null) debuffs.Add((DEBUFF_TYPE)applyEffect);
        Vector2 shootVec = aimTransform.up * shootSpeed;

        if (RoomModel.Instance && RoomModel.Instance.IsMaster)
        {
            // 弾の生成リクエスト
            ShootBulletData shootBulletData = new ShootBulletData()
            {
                Type = PROJECTILE_TYPE.MissileBullet,
                Debuffs = debuffs,
                Power = power,
                SpawnPos = aimTransform.position,
                ShootVec = shootVec,
                Rotation = aimTransform.rotation
            };
            await RoomModel.Instance.ShootBulletAsync(shootBulletData);
        }
        else if(!RoomModel.Instance)
        {
            var missile = Instantiate(missileBulletPrefab, aimTransform.position, aimTransform.rotation);
            missile.GetComponent<ProjectileBase>().Init(debuffs, power);
            missile.GetComponent<ProjectileBase>().Shoot(shootVec);
        }
    }

    /// <summary>
    /// [Animationイベントからの呼び出し] 攻撃終了
    /// </summary>
    public override void OnEndAttackAnimEvent()
    {
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
        Idle();
        yield return new WaitForSeconds(time);
        isAttacking = false;
        onFinished?.Invoke();
    }

    #endregion

    #region 移動処理関連

    /// <summary>
    /// 後ろに下がる処理
    /// </summary>
    void MoveBackward()
    {
        SetAnimId((int)ANIM_ID.Run);
    }

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
                animator.Play("Hit", 0, 0);
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
