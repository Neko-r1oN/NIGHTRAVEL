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

    #region ステータスの上限値関連
    protected int maxHp;    // 最大体力
    #endregion

    #region 現在のステータス関連
    protected int hp;
    protected int defence;
    protected int power;
    protected float moveSpeed;
    protected float attackSpeed;
    protected float jumpPower;
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
    /// 速度係数
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
        maxHp = baseHp;
        hp = baseHp;
        defence = baseDefence;
        power = basePower;
        moveSpeed = baseMoveSpeed;
        attackSpeed = baseAttackSpeed;
        jumpPower = baseJumpPower;
    }
}