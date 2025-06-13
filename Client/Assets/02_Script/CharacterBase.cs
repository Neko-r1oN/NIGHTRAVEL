//**************************************************
//  キャラクターの抽象クラス
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CharacterBase : MonoBehaviour
{
    #region 初期ステータス関連
    [Foldout("ステータス")]
    [SerializeField]
    protected int baseHp = 10;   // HP

    [Foldout("ステータス")]
    [SerializeField]
    protected int baseDefence = 10;  // 防御力

    [Foldout("ステータス")]
    [SerializeField]
    protected int basePower = 10;    // 攻撃力

    [Foldout("ステータス")]
    [SerializeField]
    protected float baseMoveSpeed = 10f;   // 移動速度

    [Foldout("ステータス")]
    [SerializeField]
    protected float baseAttackSpeed = 10;    // 攻撃速度

    [Foldout("ステータス")]
    [SerializeField]
    protected float baseJumpPower = 10;  // 跳躍力
    #endregion

    #region 初期ステータス外部参照用プロパティ
    /// <summary>
    /// 体力
    /// </summary>
    public int BaseHP { get { return baseHp; } }

    /// <summary>
    /// 防御力
    /// </summary>
    public int BaseDefence { get { return baseDefence; } }

    /// <summary>
    /// 攻撃力
    /// </summary>
    public int BasePower { get { return basePower; } }

    /// <summary>
    /// 移動速度
    /// </summary>
    public float BaseMoveSpeed { get { return baseMoveSpeed; } }

    /// <summary>
    /// 攻撃速度
    /// </summary>
    public float BaseAttackSpeed { get { return baseAttackSpeed; } }

    /// <summary>
    /// 跳躍力
    /// </summary>
    public float BaseJumpPower { get { return baseJumpPower; } }
    #endregion

    #region ステータスの上限値関連
    protected int maxHp;    // 最大体力
    #endregion

    #region 現在のステータス関連
    public int hp;
    public int defence;
    public int power;
    public float moveSpeed;
    public float attackSpeed;
    public float jumpPower;
    #endregion

    #region ステータス外部参照用プロパティ
    /// <summary>
    /// 最大体力
    /// </summary>
    public int MaxHP { get { return maxHp; } }

    /// <summary>
    /// 体力
    /// </summary>
    public int HP { get { return hp; } }

    /// <summary>
    /// 防御力
    /// </summary>
    public int Defence { get { return defence; } set { defence = value; } }

    /// <summary>
    /// 攻撃力
    /// </summary>
    public int Power { get { return power; } set { power = value; } }

    /// <summary>
    /// 移動速度
    /// </summary>
    public float MoveSpeed { get { return moveSpeed; } set { moveSpeed = value; } }

    /// <summary>
    /// 攻撃速度
    /// </summary>
    public float AttackSpeed { get { return attackSpeed; } set { attackSpeed = value; } }

    /// <summary>
    /// 跳躍力
    /// </summary>
    public float JumpPower { get { return jumpPower; } set { jumpPower = value; } }
    #endregion

    protected virtual void Start()
    {
        RecoverAllStats();
    }

    //protected virtual void Awake()
    //{
    //    RecoverAllStats();
    //}

    /// <summary>
    /// 一括でステータスに加減する処理
    /// </summary>
    public void ApplyStatusBonus(CharacterStatusData addStatusData)
    {
        maxHp += addStatusData.hp;
        hp += addStatusData.hp;
        defence += addStatusData.defence;
        power += addStatusData.power;
        moveSpeed += addStatusData.moveSpeed;
        attackSpeed += addStatusData.attackSpeed;
        jumpPower += addStatusData.jumpPower;
    }

    /// <summary>
    /// 基礎ステータスを上書きする処理
    /// </summary>
    /// <param name="statusData"></param>
    /// <param name="shouldResetToBaseStats">現在のステータスを基礎ステータスにリセットするかどうか</param>
    public void OverrideBaseStatus(CharacterStatusData statusData, bool shouldResetToBaseStats = false)
    {
        baseHp = statusData.hp;
        baseDefence = statusData.defence;
        basePower = statusData.power;
        baseMoveSpeed = statusData.moveSpeed;
        baseAttackSpeed = statusData.attackSpeed;
        baseJumpPower = statusData.jumpPower;

        if (shouldResetToBaseStats) RecoverAllStats();
    }

    /// <summary>
    /// 現在のステータスを全て元に戻す
    /// </summary>
    public void RecoverAllStats()
    {
        maxHp = baseHp;
        hp = baseHp;
        defence = baseDefence;
        power = basePower;
        moveSpeed = baseMoveSpeed;
        attackSpeed = baseAttackSpeed;
        jumpPower = baseJumpPower;
    }
}