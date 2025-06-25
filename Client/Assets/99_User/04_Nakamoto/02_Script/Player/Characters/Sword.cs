//--------------------------------------------------------------
// 剣士キャラ [ Sword.cs ]
// Author：Kenta Nakamoto
// 引用：https://assetstore.unity.com/packages/2d/characters/metroidvania-controller-166731
//--------------------------------------------------------------
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;
using Pixeye.Unity;
using System;
using HardLight2DUtil;
using static StatusEffectController;

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

    private bool isCombo = false;   // コンボ可能フラグ
    private bool cantAtk = false;   // 攻撃可能フラグ

    private float plDirection = 0;  // プレイヤーの向き

    [Foldout("キャラ別ステータス")]
    [SerializeField] private float skillForth = 45f;       // スキルの移動力

    [Foldout("キャラ別ステータス")]
    [SerializeField] private float skillTime = 0.5f;        // スキル効果時間

    [Foldout("キャラ別ステータス")]
    [SerializeField] private float skillCoolDown = 5.0f;    // スキルのクールダウン

    [Foldout("スキルエフェクト")]
    [SerializeField] private GameObject skillEffect1;   // キャラに発生するエフェクト

    [Foldout("スキルエフェクト")]
    [SerializeField] private GameObject skillEffect2;   // 剣先に発生するエフェクト

    [Foldout("スキルエフェクト")]
    [SerializeField] private GameObject skillEffect3;   // 追加で発生させるエフェクト

    //--------------------------
    // メソッド

    /// <summary>
    /// 更新処理
    /// </summary>
    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Attack1"))
        {   // 通常攻撃
            int id = animator.GetInteger("animation_id");

            if (isBlink || cantAtk || id == 3) return;

            if (nowAttack && !isCombo)
            {   // 攻撃1段目
                nowAttack = false;
                animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack1);
            }
            else if (isCombo)
            {   // 攻撃2,3段目
                isCombo = false;

                if (id == (int)S_ANIM_ID.Attack1)
                {
                    animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack2);
                }
                if (id == (int)S_ANIM_ID.Attack2)
                {
                    animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack3);
                    StartCoroutine(LastComboAttack());
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.V) || Input.GetButtonDown("Attack2"))
        {   // 攻撃2
            if (canSkill && nowAttack)
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

                collidersEnemies[i].gameObject.GetComponent<EnemyBase>().ApplyDamage(power, playerPos);
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
        nowAttack = true;
        isCombo = false;

        // 移動速度に応じてアニメーション分岐
        if (Mathf.Abs(horizontalMove) >= 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Run);

        if (Mathf.Abs(horizontalMove) < 0.1f)
            animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
    }

    /// <summary>
    /// 最終コンボ処理
    /// </summary>
    IEnumerator LastComboAttack()
    {
        cantAtk = true;

        // コンボ終了時待機  
        yield return new WaitForSeconds(1.5f);
        cantAtk = false;
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
}
