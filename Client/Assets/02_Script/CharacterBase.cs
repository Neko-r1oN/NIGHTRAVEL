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
    public int BaseDefence { get { return baseDefence; } }

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

    #region ステータスの上限値関連
    protected int maxHp;    // 最大体力
    #endregion

    #region 現在のステータス関連
    // インスペクター上で見るためのpublic
    // 後でpublicを消す
    public int hp;
    public int defence;
    public int power;
    public float jumpPower;
    public float moveSpeed;
    public float moveSpeedFactor;
    public float attackSpeedFactor;

    protected StatusEffectController effectController;
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

    #region コンポーネント
    [Foldout("コンポーネント")]
    [SerializeField] 
    protected Animator animator;
    #endregion

    protected virtual void Awake()
    {
        RecoverAllStats();
        if (!animator) animator = GetComponent<Animator>();
        effectController = GetComponent<StatusEffectController>();
    }

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
        moveSpeedFactor += addStatusData.moveSpeedFactor;
        attackSpeedFactor += addStatusData.attackSpeedFactor;
        jumpPower += addStatusData.jumpPower;
        OverrideAnimaterParam();
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
        baseMoveSpeedFactor = statusData.moveSpeedFactor;
        baseAttackSpeedFactor = statusData.attackSpeedFactor;
        baseJumpPower = statusData.jumpPower;
        OverrideAnimaterParam();

        if (shouldResetToBaseStats)
        {
            RecoverAllStats();
        }
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
        jumpPower = baseJumpPower;
        moveSpeedFactor = baseMoveSpeedFactor;
        attackSpeedFactor = baseAttackSpeedFactor;
        OverrideAnimaterParam();
    }

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
}