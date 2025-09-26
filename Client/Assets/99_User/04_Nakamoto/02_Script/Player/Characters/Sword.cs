//--------------------------------------------------------------
// 剣士キャラ [ Sword.cs ]
// Author：Kenta Nakamoto
// 引用：https://assetstore.unity.com/packages/2d/characters/metroidvania-controller-166731
//--------------------------------------------------------------
using UnityEngine;
using System.Collections;
using Pixeye.Unity;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class Sword : PlayerBase
{
    //--------------------------
    // フィールド

    /// <summary>
    /// アニメーションID
    /// </summary>
    public enum S_ANIM_ID
    {
        Attack1 = 10,
        Attack2,
        Attack3,
        Skill
    }
    private bool isCombo = false;       // コンボ可能フラグ
    private float plDirection = 0;      // プレイヤーの向き

    #region 剣士専用ステータス

    [Foldout("キャラ別ステータス")]
    [SerializeField] private float skillForth = 45f;        // スキルの移動力

    [Foldout("キャラ別ステータス")]
    [SerializeField] private float skillTime = 0.5f;        // スキル効果時間

    [Foldout("キャラ別ステータス")]
    [SerializeField] private float skillCoolDown = 5.0f;    // スキルのクールダウン

    [Foldout("アタックエフェクト")]
    [SerializeField] private ParticleSystem normalEffect1;   // 通常攻撃1

    [Foldout("アタックエフェクト")]
    [SerializeField] private ParticleSystem normalEffect2;   // 通常攻撃2

    [Foldout("アタックエフェクト")]
    [SerializeField] private GameObject skillEffect1;   // キャラに発生するエフェクト

    [Foldout("アタックエフェクト")]
    [SerializeField] private GameObject skillEffect2;   // 剣先に発生するエフェクト

    [Foldout("アタックエフェクト")]
    [SerializeField] private GameObject skillEffect3;   // 追加で発生させるエフェクト

    private bool isAirAttack = false;   // 空中攻撃をしたかどうか

    // 各攻撃の攻撃力倍率
    private const float ATTACK2_MAG = 1.1f;
    private const float ATTACK3_MAG = 1.3f;
    private const float SKILL_MAG = 1.65f;
    #endregion

    //--------------------------
    // メソッド

    /// <summary>
    /// 動作フラグをリセット
    /// </summary>
    public override void ResetFlag()
    {
        canAttack = true;
        isCombo = false;
        isSkill = false;
    }

    /// <summary>
    /// 初期処理
    /// </summary>
    protected override void Start()
    {
        base.Start();

        playerType = Player_Type.Sword;
    }

    #region 更新関連処理

    /// <summary>
    /// 更新処理
    /// </summary>
    protected override void Update()
    {
        base.Update();

        // キャラの移動
        if (!canMove) return;

        if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Attack1"))
        {   // 通常攻撃
            int id = animator.GetInteger("animation_id");

            if (isBlink || isSkill || id == (int)ANIM_ID.Hit || m_IsZipline || isAirAttack) return;

            if (canAttack && !isCombo)
            {   // 攻撃1段目
                canAttack = false;
                normalEffect1.Play();
                animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack1);
            }
            else if (isCombo)
            {   // 攻撃2,3段目
                if (id != (int)S_ANIM_ID.Attack3) isCombo = false;

                if (id == (int)S_ANIM_ID.Attack1)
                {
                    normalEffect2.Play();
                    animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack2);
                }
                if (id == (int)S_ANIM_ID.Attack2)
                {
                    animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack3);
                }
            }
        }

        if (Input.GetMouseButtonDown(1) || Input.GetButtonDown("Attack2"))
        {   // 攻撃2
            if (canSkill && canAttack)
            {
                //gameObject.layer = 21;
                animator.SetInteger("animation_id", (int)S_ANIM_ID.Skill);
                canSkill = false;
                plDirection = transform.localScale.x;
                StartCoroutine(SkillCoolDown());
            }
        }
    }

    /// <summary>
    /// 定期更新処理
    /// </summary>
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if(m_Grounded) isAirAttack = false;

        if (isSkill)
        {
            // クールダウンに入るまで加速
            m_Rigidbody2D.linearVelocity = new Vector2(plDirection * skillForth, 0);
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
        base.Move(move, jump, blink);

        // ダッシュ中の場合
        if (isBlinking)
        {   // クールダウンに入るまで加速
            m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * m_BlinkForce, 0);
        }

        if (!m_Grounded && !canAttack)
        {
            // 攻撃中は落下速度減少
            m_Rigidbody2D.gravityScale = 0;
            m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, 0);
        }
        else
        {
            m_Rigidbody2D.gravityScale = gravity;
        }
    }

    void OnDrawGizmos()
    {
        //　CircleCastのレイを可視化
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(attackCheck.position, ATTACK_RADIUS);
    }

    #endregion

    #region 攻撃・ダメージ関連
    public override void DoDashDamage()
    {
        // 攻撃範囲に居る敵を取得
        int animID = animator.GetInteger("animation_id");
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, ATTACK_RADIUS);
        HashSet<EnemyBase> processedEnemies = new HashSet<EnemyBase>();

        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if(collidersEnemies[i].gameObject.tag == "Enemy")
            {
                var enemyComponent = collidersEnemies[i].gameObject.GetComponent<EnemyBase>();
                if (enemyComponent == null) continue; // EnemyBaseが付いていないオブジェクトをスキップ

                // 既にダメージ処理済みの敵かどうかチェック
                if (processedEnemies.Contains(enemyComponent))
                    continue;

                // 敵にダメージを与える
                int attackPower = 0;    // 最終攻撃力

                // 攻撃力を設定
                switch (animID)
                {
                    case (int)S_ANIM_ID.Attack1:
                        attackPower = Power;
                        break;

                    case (int)S_ANIM_ID.Attack2:
                        attackPower = (int)(Power * ATTACK2_MAG);
                        break;

                    case (int)S_ANIM_ID.Attack3:
                        attackPower = (int)(Power * ATTACK3_MAG);
                        break;

                    case (int)S_ANIM_ID.Skill:
                        attackPower = (int)(Power * SKILL_MAG);
                        break;

                    default:
                        attackPower = Power;
                        break;
                }

                // ボスかどうか判定
                if(!collidersEnemies[i].gameObject.GetComponent<EnemyBase>().IsBoss)
                {
                    // 二回攻撃の抽選
                    if (LotteryRelic(RELIC_TYPE.Rugrouter))
                    {
                        enemyComponent.ApplyDamageRequest(attackPower, gameObject, true, true, LotteryDebuff());
                        enemyComponent.ApplyDamageRequest(attackPower / 2, gameObject, true, true, LotteryDebuff());
                    }
                    else
                    {
                        enemyComponent.ApplyDamageRequest(attackPower, gameObject, true, true, LotteryDebuff());
                    }
                }
                else
                {
                    // ボスの場合はボスエリアに入っているか確認
                    if(isBossArea)
                    {
                        // 二回攻撃の抽選
                        if (LotteryRelic(RELIC_TYPE.Rugrouter))
                        {
                            enemyComponent.ApplyDamageRequest(attackPower, gameObject, true, true, LotteryDebuff());
                            enemyComponent.ApplyDamageRequest(attackPower / 2, gameObject, true, true, LotteryDebuff());
                        }
                        else
                        {
                            enemyComponent.ApplyDamageRequest(attackPower, gameObject, true, true, LotteryDebuff());
                        }
                    }
                }

                    processedEnemies.Add(enemyComponent); // 処理済みリストに追加
            }
        }

        // Objectタグの処理
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Object")
            {
                var objectComponent = collidersEnemies[i].gameObject.GetComponent<ObjectBase>();
                if (objectComponent != null)
                {
                    objectComponent.TurnOnPowerRequest(gameObject);
                }
            }
        }
    }

    /// <summary>
    /// コンボ判定開始
    /// </summary>
    public void HitAttack()
    {
        isCombo = true;
    }

    /// <summary>
    /// 攻撃終了時
    /// </summary>
    public void AttackEnd()
    {
        if (!isCombo) return;

        // フラグの初期化
        canAttack = true;
        isCombo = false;

        if (!m_Grounded) isAirAttack = true;

        // Idleに戻る
        animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
    }

    /// <summary>
    /// スキルクールダウン処理
    /// </summary>
    /// <returns></returns>
    IEnumerator SkillCoolDown()
    {
        isSkill = true;

        // Effect再生
        skillEffect1.SetActive(true);
        skillEffect2.SetActive(true);
        skillEffect3.SetActive(true);

        yield return new WaitForSeconds(skillTime);
        isSkill = false;
        gameObject.layer = 20;

        // Effect非表示
        skillEffect1.SetActive(false);
        skillEffect2.SetActive(false);
        skillEffect3.SetActive(false);

        // 移動速度に応じてアニメーション分岐
        if (Mathf.Abs(horizontalMove) >= 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Run);

        if (Mathf.Abs(horizontalMove) < 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Idle);

        // レリック「マウス」発動時はクールダウン無し
        if (LotteryRelic(RELIC_TYPE.Mouse))
        {
            canSkill = true;
            yield break;
        }

        UIManager.Instance.DisplayCoolDown(true, skillCoolDown);
        yield return new WaitForSeconds(skillCoolDown);
        canSkill = true;
    }

    #endregion

    #region 被ダメ処理

    public override void ApplyDamage(int power, Vector3? position = null, KB_POW? kbPow = null, DEBUFF_TYPE? type = null)
    {
        // 無敵以外の時
        if (!invincible)
        {
            // 自動回復停止
            StartCoroutine(RegeneStop());

            // ダメージ計算
            int damage = (type == null) ? Mathf.Abs(CalculationLibrary.CalcDamage(power, Defense)): power;
            damage = (kbPow == null) ? power : Mathf.Abs(CalculationLibrary.CalcDamage(power, Defense));

            // ダメージ表記
            UIManager.Instance.PopDamageUI(damage, transform.position, true);

            // アニメーション変更
            var id = animator.GetInteger("animation_id");
            if (position != null && id != (int)S_ANIM_ID.Skill) animator.SetInteger("animation_id", (int)ANIM_ID.Hit);

            // 回避判定
            if (LotteryRelic(RELIC_TYPE.HolographicArmor))
            {
                // 回避成功表示
            }
            else
            {
                // HP減少
                hp -= damage - (int)(damage * firewallRate);
            }

            Vector2 damageDir = Vector2.zero;

            // ノックバック処理
            if (position != null)
            {
                damageDir = Vector3.Normalize(transform.position - (Vector3)position) * KNOCKBACK_DIR;
                m_Rigidbody2D.linearVelocity = Vector2.zero;

                // 引数に応じてノックバック力を変更
                switch (kbPow)
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
                hp = 0;
                canMove = false;
                isRegene = false;
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
}