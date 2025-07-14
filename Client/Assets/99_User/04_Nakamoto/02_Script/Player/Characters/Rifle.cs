using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;   // HashSet 用
using UnityEngine;
using static StatusEffectController;
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
        BeamReady
    }

    private bool isFiring = false;      // ビーム照射中フラグ
    private bool isRailgun = false;     // 銃変形フラグ

    [Foldout("キャラ別ステータス")]
    [SerializeField] private Transform firePoint;           // 発射地点
    [Foldout("キャラ別ステータス")]
    [SerializeField] private float maxDistance = 20f;       // ビームの長さ
    [Foldout("キャラ別ステータス")]
    [SerializeField] private float beamRadius = 0.15f;      // ビーム半径
    [Foldout("キャラ別ステータス")]
    [SerializeField] private float beamWidthScale = 1f;     // LineRenderer に乗算して見た目を調整したい場合
    [Foldout("キャラ別ステータス")]
    [SerializeField] private LayerMask targetLayers;        // 敵レイヤー
    [Foldout("キャラ別ステータス")]
    [SerializeField] private float duration = 2.5f;         // 照射時間
    [Foldout("キャラ別ステータス")]
    [SerializeField] private float damageInterval = 0.3f;   // ダメージ間隔
    [Foldout("キャラ別ステータス")]
    [SerializeField] private GameObject beamEffect;         // ビームエフェクト
    [Foldout("キャラ別ステータス")]
    [SerializeField] private LineRenderer lr;

    //--------------------------
    // メソッド

    /// <summary>
    /// 被ダメ時各フラグをリセット
    /// </summary>
    public override void HitReset()
    {
        canAttack = true;
        isFiring = false;
        isRailgun = false;
        isSkill = false;
    }

    #region 更新関連処理

    /// <summary>
    /// 更新処理
    /// </summary>
    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Attack1"))
        {   // 通常攻撃
            int id = animator.GetInteger("animation_id");

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
            isSkill = true;
            animator.SetInteger("animation_id", (int)GS_ANIM_ID.Skill);
        }

        base.Update();

        //-----------------------------
        // デバッグ用

        if (Input.GetKeyDown(KeyCode.L))
        {
            GetComponent<StatusEffectController>().ApplyStatusEffect(EFFECT_TYPE.Burn);
        }

        //Escが押された時
        if (Input.GetKey(KeyCode.Escape))
        {

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
    Application.Quit();//ゲームプレイ終了
#endif
        }
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    /// <param name="move">移動量</param>
    /// <param name="jump">ジャンプ入力</param>
    /// <param name="blink">ダッシュ入力</param>
    protected override void Move(float move, bool jump, bool blink)
    {
        if (isSkill || isRailgun)
        {   // 銃変形中は動けないように
            m_Rigidbody2D.linearVelocity = Vector3.zero;
            return;
        }

        base.Move(move, jump, blink);

        // ダッシュ中の場合
        if (isBlinking)
        {   // クールダウンに入るまで加速
            m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * m_BlinkForce, 0);
        }

        // 銃変形時の移動制限
        if (isFiring)
        {
            m_Rigidbody2D.linearVelocity = Vector2.zero;
        }
    }

    #endregion

    #region 攻撃・ダメージ関連

    /// <summary>
    /// ダメージを与える処理
    /// </summary>
    public override void DoDashDamage()
    {
        power = Mathf.Abs(power);
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, k_AttackRadius);

        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Enemy")
            {
                if (collidersEnemies[i].transform.position.x - transform.position.x < 0)
                {
                    power = -power;
                }
                //++ GetComponentでEnemyスクリプトを取得し、ApplyDamageを呼び出すように変更
                //++ 破壊できるオブジェを作る際にはオブジェの共通被ダメ関数を呼ぶようにする

                collidersEnemies[i].gameObject.GetComponent<EnemyBase>().ApplyDamage(1, playerPos);
                cam.GetComponent<CameraFollow>().ShakeCamera();
            }
            else if (collidersEnemies[i].gameObject.tag == "Object")
            {
                collidersEnemies[i].gameObject.GetComponent<ObjectBase>().ApplyDamage();
            }
        }
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
    /// スキル演出終了時
    /// </summary>
    public void SkillEnd()
    {
        isSkill = false;
        isRailgun = true;

        // 発射準備へ
        animator.SetInteger("animation_id", (int)GS_ANIM_ID.BeamReady);
    }

    #endregion

    #region ビーム関連

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

#if UNITY_EDITOR
        lr.enabled = true;

        // LineRenderer の太さを当たり判定と合わせる
        float lrWidth = beamRadius * 2f * beamWidthScale;
        lr.startWidth = lrWidth;
        lr.endWidth = lrWidth;
#endif

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

            // 指定ダメージ間隔毎にダメージ
            if (tickTimer >= damageInterval && hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.collider == null) continue;

                    hit.collider.gameObject.GetComponent<EnemyBase>().ApplyDamage(this.Power, this.transform);
                }
                tickTimer = 0f;
            }

#if UNITY_EDITOR
            // LineRenderer 更新
            lr.SetPosition(0, firePoint.position);
            lr.SetPosition(1, endPos);
#endif

            yield return null;
        }

        // ビームエフェクト非表示
        beamEffect.SetActive(false);
        animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
        isFiring = false;
        canAttack = true;
    }

    #endregion
}