using System.Collections;
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
        Skill
    }

    private bool isChanging = false;    // 変形中フラグ
    private bool isRailgun = false;     // 銃変形フラグ
    private bool isCooldown = false;    // 攻撃のクールダウンフラグ

    /// <summary>
    /// 更新処理
    /// </summary>
    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Attack1"))
        {   // 通常攻撃
            int id = animator.GetInteger("animation_id");

            if (isBlink || isSkill || isCooldown || id == 3 || m_IsZipline) return;

            if (canAttack)
            {
                if(isRailgun)
                {
                    canAttack = false;
                    isRailgun = false;
                    animator.SetInteger("animation_id", (int)GS_ANIM_ID.Attack);
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
        if (isSkill)
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
        if (isRailgun)
        {
            var targetVelocity = new Vector2(move, m_Rigidbody2D.linearVelocity.y);
            m_Rigidbody2D.linearVelocity = targetVelocity;
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

        // Idleに戻る
        animator.SetInteger("animation_id", (int)ANIM_ID.Idle);
    }
}
