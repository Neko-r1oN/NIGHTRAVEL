//**************************************************
//  キャラクターの抽象クラス
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

abstract public class CharacterBase : MonoBehaviour
{
    #region 初期ステータス関連
    [Foldout("ステータス")]
    [SerializeField]
    protected int baseHp = 10;   // HP

    [Foldout("ステータス")]
    [SerializeField]
    protected int baseDefense = 10;  // 防御力

    [Foldout("ステータス")]
    [SerializeField]
    protected int basePower = 10;    // 攻撃力

    [Foldout("ステータス")]
    [SerializeField]
    protected float baseJumpPower = 10;  // 跳躍力

    [Foldout("ステータス")]
    [SerializeField]
    protected float baseMoveSpeed = 1f;   // 移動速度

    [Foldout("ステータス")]
    protected float baseMoveSpeedFactor = 1f;    // 移動速度(Animatorの係数)

    [Foldout("ステータス")]
    protected float baseAttackSpeedFactor = 1f;    // 攻撃速度(Animatorの係数)
    #endregion

    #region 初期ステータス外部参照用プロパティ
    /// <summary>
    /// 体力
    /// </summary>
    public int BaseHP { get { return baseHp; } }

    /// <summary>
    /// 防御力
    /// </summary>
    public int BaseDefense { get { return baseDefense; } }

    /// <summary>
    /// 攻撃力
    /// </summary>
    public int BasePower { get { return basePower; } }

    /// <summary>
    /// 跳躍力
    /// </summary>
    public float BaseJumpPower { get { return baseJumpPower; } }

    /// <summary>
    /// 移動速度
    /// </summary>
    public float BaseMoveSpeed { get { return baseMoveSpeed; } }

    /// <summary>
    /// 移動速度(Animatorの係数)
    /// </summary>
    public float BaseMoveSpeedFactor {  get { return baseMoveSpeedFactor; } }

    /// <summary>
    /// 攻撃速度(Animatorの係数)
    /// </summary>
    public float BaseAttackSpeedFactor { get { return baseAttackSpeedFactor; } }
    #endregion

    #region 現在のステータスの上限値関連(マイナスの値を許容しないもののみ)
    protected int maxHp;
    protected int maxPower;
    protected float maxJumpPower;
    protected float maxMoveSpeed;
    protected float maxMoveSpeedFactor;
    protected float maxAttackSpeedFactor;
    #endregion

    #region 現在のステータスの上限値外部参照用プロパティ
    /// <summary>
    /// 最大体力
    /// </summary>
    public int MaxHP { get { return maxHp; } }

    /// <summary>
    /// 最大攻撃力
    /// </summary>
    public int MaxPower { get { return maxPower; } }

    /// <summary>
    /// 最大跳躍力
    /// </summary>
    public float MaxJumpPower { get { return maxJumpPower; } }

    /// <summary>
    /// 最大移動速度
    /// </summary>
    public float MaxMoveSpeed { get { return maxMoveSpeed; } }

    /// <summary>
    /// 最大移動速度係数(Animator用)
    /// </summary>
    public float MaxMoveSpeedFactor { get { return maxMoveSpeedFactor; } }

    /// <summary>
    /// 最大攻撃速度係数(Animator用)
    /// </summary>
    public float MaxAttackSpeedFactor { get { return maxAttackSpeedFactor; } }
    #endregion

    #region 現在のステータス関連
    // インスペクター上で見るためのpublic
    // 後でpublicを消す
    [Foldout("[Debug用] 現在のステータス")]
    public int hp;
    [Foldout("[Debug用] 現在のステータス")]
    public int defense;
    [Foldout("[Debug用] 現在のステータス")]
    public int power;
    [Foldout("[Debug用] 現在のステータス")]
    public float jumpPower;
    [Foldout("[Debug用] 現在のステータス")]
    public float moveSpeed;
    [Foldout("[Debug用] 現在のステータス")]
    public float moveSpeedFactor;
    [Foldout("[Debug用] 現在のステータス")]
    public float attackSpeedFactor;

    protected StatusEffectController effectController;
    #endregion

    #region ステータス外部参照用プロパティ
    /// <summary>
    /// 体力
    /// </summary>
    public int HP { get { return hp; } }

    /// <summary>
    /// 防御力
    /// </summary>
    public int Defense { get { return defense; } set { defense = value; } }

    /// <summary>
    /// 攻撃力
    /// </summary>
    public int Power { get { return power; } set { power = value; } }

    /// <summary>
    /// 跳躍力
    /// </summary>
    public float JumpPower { get { return jumpPower; } set { jumpPower = value; } }

    /// <summary>
    /// 移動速度
    /// </summary>
    public float MoveSpeed { get { return moveSpeed; } set { moveSpeed = value; } }

    /// <summary>
    /// 移動速度(Animatorの係数)
    /// </summary>
    public float MoveSpeedFactor { get { return moveSpeedFactor; }  set { moveSpeedFactor = value; } }

    /// <summary>
    /// 攻撃速度(Animatorの係数)
    /// </summary>
    public float AttackSpeedFactor { get { return attackSpeedFactor; } set { attackSpeedFactor = value; } }

    /// <summary>
    /// 状態異常管理のコンポーネント
    /// </summary>
    public StatusEffectController EffectController { get { return effectController; } }
    #endregion

    #region テクスチャ・アニメーション
    [Foldout("テクスチャ・アニメーション")]
    [SerializeField] 
    protected Animator animator;
    #endregion

    public enum STATUS_TYPE
    {
        All,
        HP,
        Defense,
        Power,
        JumpPower,
        MoveSpeed,
        MoveSpeedFactor,
        AttackSpeedFactor
    }

    protected virtual void Awake()
    {
        ApplyStatusModifierByRate(1f, true, STATUS_TYPE.All);
        if (!animator) animator = GetComponent<Animator>();
        effectController = GetComponent<StatusEffectController>();
    }

    /// <summary>
    /// 一括で現在のステータスに加減する処理
    /// </summary>
    public void ApplyStatusBonus(CharacterStatusData addStatusData)
    {
        var applyHp = hp + addStatusData.hp;
        hp = applyHp < maxHp ? applyHp : maxHp;

        defense += addStatusData.defence;

        var applyPower = power + addStatusData.power;
        power = applyPower < maxPower ? applyPower : maxPower;

        var applyMoveSpeed = moveSpeed + addStatusData.moveSpeed;
        moveSpeed = applyMoveSpeed < maxMoveSpeed ? applyMoveSpeed : maxMoveSpeed;

        var applyJumpPower = jumpPower + addStatusData.jumpPower;
        jumpPower = applyJumpPower < maxJumpPower ? applyJumpPower : maxJumpPower;

        var applyMoveSpeedFactor = moveSpeedFactor + addStatusData.moveSpeedFactor;
        moveSpeedFactor = applyMoveSpeedFactor < maxMoveSpeedFactor ? applyMoveSpeedFactor : maxMoveSpeedFactor;

        var applyAttackSpeedFactor = attackSpeedFactor + addStatusData.attackSpeedFactor;
        attackSpeedFactor = applyAttackSpeedFactor < maxAttackSpeedFactor ? applyAttackSpeedFactor : maxAttackSpeedFactor;

        OverrideAnimaterParam();
    }

    /// <summary>
    /// 割合を指定して、ステータスに変化を適用させる
    /// </summary>
    /// <param name="rate"></param>
    /// <param name="types"></param>
    public void ApplyStatusModifierByRate(float rate, params STATUS_TYPE[] types)
    {
        ApplyStatusModifierByRate(rate, false, types);
    }

    /// <summary>
    /// 割合を指定して、ステータスに変化を適用させる
    /// </summary>
    /// <param name="rate"></param>
    /// <param name="canResetHpToMax"></param>
    /// <param name="types"></param>
    public void ApplyStatusModifierByRate(float rate, bool canResetHpToMax, params STATUS_TYPE[] types)
    {
        List<STATUS_TYPE> statusList = new List<STATUS_TYPE>(types);
        if (statusList.Contains(STATUS_TYPE.All)) 
        {
            statusList = new List<STATUS_TYPE> {
                STATUS_TYPE.HP, STATUS_TYPE.Defense, STATUS_TYPE.Power, STATUS_TYPE.JumpPower,
                STATUS_TYPE.MoveSpeed, STATUS_TYPE.MoveSpeedFactor, STATUS_TYPE.AttackSpeedFactor
            };
        }

        foreach (STATUS_TYPE type in statusList)
        {
            switch (type)
            {
                case STATUS_TYPE.HP:
                    maxHp += (int)(baseHp * rate);
                    int applyHp = maxHp < 0 ? 0 : maxHp;
                    if (canResetHpToMax) hp = maxHp;
                    else hp = applyHp < hp ? applyHp : hp;
                    break;
                case STATUS_TYPE.Defense:
                    defense += (int)(baseDefense * rate);
                    break;
                case STATUS_TYPE.Power:
                    maxPower += (int)(basePower * rate);
                    power = maxPower < 0 ? 0 : maxPower;
                    break;
                case STATUS_TYPE.JumpPower:
                    maxJumpPower += baseJumpPower * rate;
                    jumpPower = maxJumpPower < 0 ? 0 : maxJumpPower;
                    break;
                case STATUS_TYPE.MoveSpeed:
                    maxMoveSpeed += baseMoveSpeed * rate;
                    moveSpeed = maxMoveSpeed < 0 ? 0 : maxMoveSpeed;
                    break;
                case STATUS_TYPE.MoveSpeedFactor:
                    maxMoveSpeedFactor += baseMoveSpeedFactor * rate;
                    moveSpeedFactor = maxMoveSpeedFactor < 0 ? 0 : maxMoveSpeedFactor;
                    break;
                case STATUS_TYPE.AttackSpeedFactor:
                    maxAttackSpeedFactor += baseAttackSpeedFactor * rate;
                    attackSpeedFactor = maxAttackSpeedFactor < 0 ? 0 : maxAttackSpeedFactor;
                    break;
            }
        }
        OverrideAnimaterParam();
    }

    /// <summary>
    /// 現在のステータスを最大値の大きさに戻す
    /// </summary>
    public void RecoverAllStats()
    {
        hp = maxHp;
        power = maxPower;
        moveSpeed = maxMoveSpeed;
        jumpPower = maxJumpPower;
        moveSpeedFactor = maxMoveSpeedFactor;
        attackSpeedFactor = maxAttackSpeedFactor;
        OverrideAnimaterParam();
    }

    #region アニメーション関連

    /// <summary>
    /// アニメーターのパラメーターを上書きする
    /// </summary>
    public void OverrideAnimaterParam()
    {
        if (animator)
        {
            animator.SetFloat("attack_speed", attackSpeedFactor);
            animator.SetFloat("move_speed", moveSpeedFactor);
        }
    }

    /// <summary>
    /// アニメーション設定処理
    /// </summary>
    /// <param name="id"></param>
    public virtual void SetAnimId(int id)
    {
        if (animator == null) return;
        animator.SetInteger("animation_id", id);
    }

    /// <summary>
    /// アニメーションID取得処理
    /// </summary>
    /// <returns></returns>
    public int GetAnimId()
    {
        return animator != null ? animator.GetInteger("animation_id") : 0;
    }

    #endregion
}