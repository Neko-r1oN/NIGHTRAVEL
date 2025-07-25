//--------------------------------------------------------------
// 剣士キャラ [ Sword.cs ]
// Author：Kenta Nakamoto
// 引用：https://assetstore.unity.com/packages/2d/characters/metroidvania-controller-166731
//--------------------------------------------------------------
using UnityEngine;
using System.Collections;
using Pixeye.Unity;
using static StatusEffectController;
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

    #region ソード専用ステータス

    [Foldout("キャラ別ステータス")]
    [SerializeField] private float skillForth = 45f;        // スキルの移動力

    [Foldout("キャラ別ステータス")]
    [SerializeField] private float skillTime = 0.5f;        // スキル効果時間

    [Foldout("キャラ別ステータス")]
    [SerializeField] private float skillCoolDown = 5.0f;    // スキルのクールダウン

    [Foldout("キャラ別ステータス")]
    [SerializeField] private float atkGravityCoefficient = 1.8f;   // 攻撃中の落下速度係数

    [Foldout("スキルエフェクト")]
    [SerializeField] private GameObject skillEffect1;   // キャラに発生するエフェクト

    [Foldout("スキルエフェクト")]
    [SerializeField] private GameObject skillEffect2;   // 剣先に発生するエフェクト

    [Foldout("スキルエフェクト")]
    [SerializeField] private GameObject skillEffect3;   // 追加で発生させるエフェクト

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

    #region 更新関連処理

    /// <summary>
    /// 更新処理
    /// </summary>
    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Attack1"))
        {   // 通常攻撃
            int id = animator.GetInteger("animation_id");

            if (isBlink || isSkill || id == 3 || m_IsZipline) return;

            if (canAttack && !isCombo)
            {   // 攻撃1段目
                canAttack = false;
                animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack1);
            }
            else if (isCombo)
            {   // 攻撃2,3段目
                if (id != (int)S_ANIM_ID.Attack3) isCombo = false;

                if (id == (int)S_ANIM_ID.Attack1)
                {
                    animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack2);
                }
                if (id == (int)S_ANIM_ID.Attack2)
                {
                    animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack3);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.V) || Input.GetButtonDown("Attack2"))
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
    /// 定期更新処理
    /// </summary>
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

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

        if (!canAttack)
        {
            // 攻撃中は落下速度減少
            m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, m_Rigidbody2D.linearVelocity.y / atkGravityCoefficient);
        }
    }

    #endregion

    #region 攻撃・ダメージ関連

    /// <summary>
    /// ダメージを与える処理
    /// </summary>
    //public override void DoDashDamage()
    //{
    //    Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, k_AttackRadius,enemyLayer);

    //    for (int i = 0; i < collidersEnemies.Length; i++)
    //    {
    //        if (collidersEnemies[i].gameObject.tag == "Enemy")
    //        {
    //            //++ GetComponentでEnemyスクリプトを取得し、ApplyDamageを呼び出すように変更
    //            //++ 破壊できるオブジェを作る際にはオブジェの共通被ダメ関数を呼ぶようにする

    //            collidersEnemies[i].gameObject.GetComponent<EnemyBase>().ApplyDamage(Power, playerPos);
    //            cam.GetComponent<CameraFollow>().ShakeCamera();
    //        }
    //        else if (collidersEnemies[i].gameObject.tag == "Object")
    //        {
    //            collidersEnemies[i].gameObject.GetComponent<ObjectBase>().ApplyDamage();
    //        }
    //    }
    //}

    public override void DoDashDamage()
    {
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, k_AttackRadius);
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
                enemyComponent.ApplyDamage(Power, playerPos);
                processedEnemies.Add(enemyComponent); // 処理済みリストに追加

                // カメラのシェイク処理
                cam.GetComponent<CameraFollow>().ShakeCamera();
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
                    objectComponent.ApplyDamage();
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

        yield return new WaitForSeconds(skillCoolDown);
        canSkill = true;
    }

    #endregion

    #region 被ダメ処理

    public override void ApplyDamage(int power, Vector3? position = null, StatusEffectController.EFFECT_TYPE? type = null)
    {
        if (!invincible)
        {
            var damage = Mathf.Abs(CalculationLibrary.CalcDamage(power, Defense));

            UIManager.Instance.PopDamageUI(damage, transform.position, true);
            if (position != null) animator.SetInteger("animation_id", (int)ANIM_ID.Hit);
            hp -= damage;
            Vector2 damageDir = Vector2.zero;

            // ノックバック処理
            if (position != null)
            {
                damageDir = Vector3.Normalize(transform.position - (Vector3)position) * 40f;
                m_Rigidbody2D.linearVelocity = Vector2.zero;
                m_Rigidbody2D.AddForce(damageDir * 15);
            }

            if (type != null)
            {
                effectController.ApplyStatusEffect((StatusEffectController.EFFECT_TYPE)type);
            }

            if (hp <= 0)
            {   // 死亡処理
                m_Rigidbody2D.AddForce(damageDir * 10);
                StartCoroutine(WaitToDead());
            }
            else
            {   // 被ダメ硬直
                if (position != null)
                {
                    StartCoroutine(Stun(0.35f));
                    StartCoroutine(MakeInvincible(0.4f));
                }
            }
        }
    }

    #endregion

    void OnDrawGizmos()
    {
        //　CircleCastのレイを可視化
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(attackCheck.position, k_AttackRadius);
    }
}