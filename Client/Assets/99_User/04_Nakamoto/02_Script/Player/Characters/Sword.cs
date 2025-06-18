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
        Skill
    }

    private bool isCombo = false;
    private bool cantAtk = false;

    //--------------------------
    // メソッド

    /// <summary>
    /// 更新処理
    /// </summary>
    private void Update()
    {
        Debug.Log("攻撃：" + nowAttack + " コンボ：" + isCombo);

        // キャラの移動
        horizontalMove = Input.GetAxisRaw("Horizontal") * moveSpeed;
        verticalMove = Input.GetAxisRaw("Vertical") * moveSpeed;
        Ladder();

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetButtonDown("Jump"))
        {   // ジャンプ押下時
            if (animator.GetInteger("animation_id") != (int)ANIM_ID.Blink)
                isJump = true;
        }

        if (Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("Blink"))
        {   // ブリンク押下時
            if(nowAttack)
            {
                isBlink = true;
                gameObject.layer = 21;
            }
        }

        if (Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Attack1"))
        {   // 通常攻撃
            if (isBlink || cantAtk) return;

            if (nowAttack && !isCombo)
            {   // 攻撃1段目
                nowAttack = false;
                animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack1);
            }
            else if (isCombo)
            {   // 攻撃2,3段目
                isCombo = false;
                int id = animator.GetInteger("animation_id");

                if (id == (int)S_ANIM_ID.Attack1)
                {
                    animator.SetInteger("animation_id", (int)S_ANIM_ID.Attack2);
                }
                if (id == (int)S_ANIM_ID.Attack2)
                {
                    animator.SetInteger("animation_id", (int)S_ANIM_ID.Skill);
                    StartCoroutine(LastComboAttack());
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.V) || Input.GetButtonDown("Attack2"))
        {   // 攻撃2
            GetComponent<StatusEffectController>().ApplyStatusEffect(StatusEffectController.EFFECT_TYPE.Shock);
        }

        //-----------------------------
        // デバッグ用

        if (Input.GetKeyDown(KeyCode.L))
        {
            GetExp(testExp);
            Debug.Log("獲得経験値：" + testExp + "現レベル：" + nowLv + " 現経験値：" + nowExp + "必要経験値" + nextLvExp);
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

    [ContextMenu("ショック")]
    public void ShockTest()
    {
        this.GetComponent<StatusEffectController>().ApplyStatusEffect(StatusEffectController.EFFECT_TYPE.Shock);
    }
}
