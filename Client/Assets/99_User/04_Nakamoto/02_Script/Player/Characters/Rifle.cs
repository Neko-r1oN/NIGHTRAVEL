//--------------------------------------------------------------
// ライフルキャラ [ Rifle.cs ]
// Author：Kenta Nakamoto
// 引用：https://assetstore.unity.com/packages/2d/characters/metroidvania-controller-166731
//--------------------------------------------------------------
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;   // HashSet 用
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using static Sword;

public class Rifle : PlayerBase
{
    //--------------------------
    // フィールド

    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum GS_ANIM_ID
    {
        Attack = 10,
        Skill,
        BeamReady,
        SkillAfter
    }

    private bool isFiring = false;      // ビーム照射中フラグ
    private bool isRailgun = false;     // 銃変形フラグ
    private const float BEAM_MAG = 1.3f;// ビームの攻撃力倍率

    [Foldout("ビーム関連")]
    [SerializeField] private Transform firePoint;           // 発射地点
    [Foldout("ビーム関連")]
    [SerializeField] private float maxDistance = 20f;       // ビームの長さ
    [Foldout("ビーム関連")]
    [SerializeField] private float beamRadius = 0.15f;      // ビーム半径
    [Foldout("ビーム関連")]
    [SerializeField] private LayerMask targetLayers;        // 敵レイヤー
    [Foldout("ビーム関連")]
    [SerializeField] private float duration = 2.5f;         // 照射時間
    [Foldout("ビーム関連")]
    [SerializeField] private float damageInterval = 0.3f;   // ダメージ間隔
    [Foldout("ビーム関連")]
    [SerializeField] private GameObject beamEffect;         // ビームエフェクト

    [Foldout("通常攻撃")]
    [SerializeField] private float bulletSpeed;
    [Foldout("通常攻撃")]
    [SerializeField] private GameObject bulletPrefab;
    [Foldout("通常攻撃")]
    [SerializeField] private GameObject bulletSpawnObj;

    //--------------------------
    // メソッド

    /// <summary>
    /// 動作フラグをリセット
    /// </summary>
    public override void ResetFlag()
    {
        canAttack = true;
        isFiring = false;
        isRailgun = false;
        isSkill = false;
    }

    /// <summary>
    /// 初期処理
    /// </summary>
    protected override void Start()
    {
        base.Start();

        playerType = Player_Type.Gunner;
    }

    #region 更新関連処理
    /// <summary>
    /// 更新処理
    /// </summary>
    protected override void Update()
    {
        int id = animator.GetInteger("animation_id");

        if (Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Attack1"))
        {   // 通常攻撃
            if (isBlink || isSkill || id == 3 || m_IsZipline) return;

            if (canAttack)
            {
                if (isRailgun)
                {
                    canAttack = false;
                    isRailgun = false;
                    FireLaser(new Vector2(transform.localScale.x,0));
                }
                else
                {
                    canAttack = false;
                    animator.SetInteger("animation_id", (int)GS_ANIM_ID.Attack);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.V) || Input.GetButtonDown("Attack2"))
        {   // スキル
            if (m_IsZipline) return;

            isSkill = true;
            canAttack = true;
            animator.SetInteger("animation_id", (int)GS_ANIM_ID.Skill);
        }

        if (id == (int)GS_ANIM_ID.BeamReady || id == (int)GS_ANIM_ID.Skill) return;

        base.Update();
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    /// <param name="move">移動量</param>
    /// <param name="jump">ジャンプ入力</param>
    /// <param name="blink">ダッシュ入力</param>
    protected override void Move(float move, bool jump, bool blink)
    {
        if (isSkill)
        {   // 銃変形中は動けないように
            m_Rigidbody2D.linearVelocity = new Vector2(0,m_Rigidbody2D.linearVelocityY);
            return;
        }

        base.Move(move, jump, blink);

        // ダッシュ中の場合
        if (isBlinking)
        {   // クールダウンに入るまで加速
            m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * m_BlinkForce, 0);
        }

        // 銃変形時の移動制限
        if (isRailgun)
        {
            m_Rigidbody2D.linearVelocity = new Vector2(0, m_Rigidbody2D.linearVelocityY);
        }

        // 発射時後ろに少しだけ後ろに
        if (isFiring)
        {
            m_Rigidbody2D.linearVelocity = new Vector2(-transform.localScale.x * 0.3f, m_Rigidbody2D.linearVelocityY);
        }
    }

    #endregion

    #region 攻撃・ダメージ関連

    /// <summary>
    /// ダメージを与える処理
    /// </summary>
    public override void DoDashDamage()
    {

    }

    /// <summary>
    /// 攻撃終了時
    /// </summary>
    public void AttackEnd()
    {
        canAttack = true;

        // Idleに戻る
        animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
    }

    /// <summary>
    /// 弾の発射
    /// </summary>
    public void FireBullet()
    {
        var bullet = Instantiate(bulletPrefab, bulletSpawnObj.transform.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().SetPlayer(m_Player);
        bullet.GetComponent<Bullet>().Speed = bulletSpeed;
        bullet.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(transform.localScale.x * bulletSpeed, 0);
    }

    #endregion

    #region 被ダメ処理

    public override void ApplyDamage(int power, Vector3? position = null, KB_POW? kbPow = null, DEBUFF_TYPE? type = null)
    {
        if (!invincible)
        {
            // ダメージ計算
            var damage = Mathf.Abs(CalculationLibrary.CalcDamage(power, Defense));

            // ダメージ表記
            UIManager.Instance.PopDamageUI(damage, transform.position, true);

            // アニメーション変更
            var id = animator.GetInteger("animation_id");
            if (position != null && id != (int)GS_ANIM_ID.Skill && id != (int)GS_ANIM_ID.BeamReady) animator.SetInteger("animation_id", (int)ANIM_ID.Hit);

            hp -= damage;
            Vector2 damageDir = Vector2.zero;

            // ノックバック処理
            if (position != null && id != (int)GS_ANIM_ID.Skill || position != null && id != (int)GS_ANIM_ID.BeamReady)
            {
                damageDir = Vector3.Normalize(transform.position - (Vector3)position) * KNOCKBACK_DIR;
                m_Rigidbody2D.linearVelocity = Vector2.zero;

                // 引数に応じてノックバック力を変更
                switch(kbPow)
                {
                    case KB_POW.Small:
                        m_Rigidbody2D.AddForce(damageDir * KB_SMALL);
                        break;

                    case KB_POW.Medium:
                        m_Rigidbody2D.AddForce(damageDir * KB_MEDIUM);
                        break;

                    case KB_POW.Big:
                        m_Rigidbody2D.AddForce(damageDir * KB_BIG);
                        break;

                    default:
                        break;
                }
            }

            // 状態異常付与
            if (type != null)
            {
                effectController.ApplyStatusEffect((DEBUFF_TYPE)type);
            }

            if (hp <= 0)
            {   // 死亡処理
                m_Rigidbody2D.AddForce(damageDir * KB_MEDIUM);
                StartCoroutine(WaitToDead());
            }
            else
            {   // 被ダメ硬直
                if (position != null)
                {
                    StartCoroutine(Stun(STUN_TIME));
                    StartCoroutine(MakeInvincible(INVINCIBLE_TIME));
                }
            }
        }
    }

    #endregion

    #region ビーム関連

    /// <summary>
    /// スキル演出終了時
    /// </summary>
    public void SkillEnd()
    {
        // 発射準備へ
        animator.SetInteger("animation_id", (int)GS_ANIM_ID.BeamReady);
    }

    /// <summary>
    /// 発射準備完了
    /// </summary>
    public void ReadyToFire()
    {
        isSkill = false;
        isRailgun = true;
    }

    /// <summary>
    /// 照射処理
    /// </summary>
    /// <param name="direction">プレイヤーの向き</param>
    public void FireLaser(Vector2 direction)
    {
        if (isFiring) return;             // 多重発射防止
        StartCoroutine(LaserRoutine(direction.normalized));
    }

    /// <summary>
    /// 照射中処理
    /// </summary>
    /// <param name="dir">向き</param>
    /// <returns></returns>
    private IEnumerator LaserRoutine(Vector2 dir)
    {
        isFiring = true;

        // ビームエフェクト表示
        beamEffect.SetActive(true);

        float laserTimer = 0f;   // 全体の照射時間
        float tickTimer = 0f;    // ダメージ間隔計測

        while (laserTimer < duration)
        {
            laserTimer += Time.deltaTime;
            tickTimer += Time.deltaTime;

            // CircleCastAll で「太さ」を持った線判定
            RaycastHit2D[] hits = Physics2D.CircleCastAll(
                origin: firePoint.position,
                radius: beamRadius,
                direction: dir,
                distance: maxDistance,
                layerMask: targetLayers);

            // レーザー可視長：最初に衝突した位置 or 最大距離
            Vector3 endPos = firePoint.position + (Vector3)(dir * maxDistance);
            if (hits.Length > 0) endPos = hits[0].point;

            // カメラのシェイク処理
            cam.GetComponent<CameraFollow>().ShakeCamera();

            // 指定ダメージ間隔毎にダメージ
            if (tickTimer >= damageInterval && hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.collider == null) continue;

                    hit.collider.gameObject.GetComponent<EnemyBase>().ApplyDamage((int)(Power * BEAM_MAG), gameObject);
                }

                tickTimer = 0f;
            }

            yield return null;
        }

        // ビームエフェクト非表示
        beamEffect.SetActive(false);
        isFiring = false;
        animator.SetInteger("animation_id", (int)GS_ANIM_ID.SkillAfter);
    }

    /// <summary>
    /// スキル終了処理
    /// </summary>
    public void EndSkill()
    {
        ResetFlag();
        animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
    }

    #endregion
}