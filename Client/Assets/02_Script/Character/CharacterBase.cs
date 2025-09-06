//**************************************************
//  キャラクターの抽象クラス
//  Author:r-enomoto
//**************************************************
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;
using Shared.Interfaces.StreamingHubs;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using static UnityEditor.ShaderGraph.Internal.Texture2DShaderProperty;

abstract public class CharacterBase : MonoBehaviour
{
    //--------------------------------------------------------------
    // フィールド

    #region データ関連
    protected Guid uniqueId;    // 生成されたときの識別用ID
    public Guid UniqueId { get { return uniqueId; } set { uniqueId = value; } }
    #endregion

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
    protected float baseAttackSpeedFactor = 1f;    // 攻撃速度(Animatorの係数)

    [Foldout("ステータス")]
    [SerializeField]
    protected float baseHealRate = 0.001f;  // 自動回復倍の倍率
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
    /// 攻撃速度(Animatorの係数)
    /// </summary>
    public float BaseAttackSpeedFactor { get { return baseAttackSpeedFactor; } }

    /// <summary>
    /// 自動回復の倍率
    /// </summary>
    public float BaseHealRate {  get { return baseHealRate; } }
    #endregion
        
    #region 現在のステータスの上限値関連
    public int maxHp;
    public int maxDefense;
    public int maxPower;
    public float maxJumpPower;
    public float maxMoveSpeed;
    public float maxAttackSpeedFactor;
    public float maxHealRate;
    #endregion

    #region 現在のステータスの上限値外部参照用プロパティ
    /// <summary>
    /// 最大体力
    /// </summary>
    public int MaxHP { get { return maxHp; } }

    public int MaxDefence { get { return maxDefense; } }

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
    /// 最大攻撃速度係数(Animator用)
    /// </summary>
    public float MaxAttackSpeedFactor { get { return maxAttackSpeedFactor; } }

    /// <summary>
    /// 最大自動回復の倍率
    /// </summary>
    public float MaxHealRate { get {return maxHealRate; } }
    #endregion

    #region 現在のステータス関連
    // インスペクター上で見るためのpublic
    // 後でprotectedにする
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
    public float attackSpeedFactor;
    [Foldout("[Debug用] 現在のステータス")]
    public float healRate;

    protected DebuffController effectController;
    #endregion

    #region ステータス外部参照用プロパティ
    /// <summary>
    /// 体力
    /// </summary>
    public int HP { get { return hp; } set { hp = value; } }

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
    /// 攻撃速度(Animatorの係数)
    /// </summary>
    public float AttackSpeedFactor { get { return attackSpeedFactor; } set { attackSpeedFactor = value; } }

    /// <summary>
    /// 自動回復の倍率
    /// </summary>
    public float HealRate { get { return healRate; } set { healRate = value; } }

    /// <summary>
    /// 状態異常管理のコンポーネント
    /// </summary>
    public DebuffController EffectController { get { return effectController; } }
    #endregion

    #region テクスチャ・アニメーション
    [Foldout("テクスチャ・アニメーション")]
    [SerializeField] 
    protected Animator animator;
    #endregion

    #region ノックバックパワーID
    /// <summary>
    /// ノックバック強さID
    /// </summary>
    public enum KB_POW
    {
        Small = 1,
        Medium,
        Big,
    }
    #endregion

    #region 定数

    private const float LEVEL_UP_RATE = 0.05f; // レベルアップ時のステータス上昇率

    #endregion

    #region その他
    protected RigidbodyType2D defaultType2D;
    #endregion

    //--------------------------------------------------------------
    // メソッド

    #region 初期処理

    protected virtual void Awake()
    {
        CharacterStatusData baseStatus = new CharacterStatusData(
                hp: baseHp,
                defence: baseDefense,
                power: basePower,
                moveSpeed: baseMoveSpeed,
                attackSpeedFactor: baseAttackSpeedFactor,
                jumpPower: baseJumpPower,
                healRate: baseHealRate
            );
        OverridMaxStatus(baseStatus, STATUS_TYPE.All);
        OverridCurrentStatus(baseStatus, STATUS_TYPE.All);
        if (!animator) animator = GetComponent<Animator>();
        effectController = GetComponent<DebuffController>();
    }

    protected virtual void Start()
    {
        // リアルタイム中&&マスタークライアントではない場合
        if (RoomModel.Instance && RoomModel.Instance.ConnectionId != Guid.Empty && !RoomModel.Instance.IsMaster)
        {
            // 自身が敵 || 操作キャラではない場合はスクリプトを非アクティブにする
            if (gameObject.tag == "Enemy" || CharacterManager.Instance.PlayerObjSelf != this.gameObject)
            {
                var rb2d = GetComponent<Rigidbody2D>();
                defaultType2D = rb2d.bodyType;
                rb2d.bodyType = RigidbodyType2D.Static;
                this.enabled = false;
            }
        }
    }

    #endregion

    /// <summary>
    /// 全てのステータスの種類を取得
    /// </summary>
    /// <returns></returns>
    List<STATUS_TYPE> GetAllStatusType()
    {
        return new List<STATUS_TYPE> { 
            STATUS_TYPE.HP, STATUS_TYPE.Defense, STATUS_TYPE.Power, STATUS_TYPE.JumpPower,
                STATUS_TYPE.MoveSpeed, STATUS_TYPE.AttackSpeedFactor, STATUS_TYPE.HealRate};
    }

    /// <summary>
    /// 現在のステータスを上下限に制限する
    /// </summary>
    void ClampStatus()
    {
        hp = Mathf.Clamp(hp, 0, maxHp);
        defense = Mathf.Clamp(defense, 0, defense);
        power = Mathf.Clamp(power, 0, maxPower);
        moveSpeed = Mathf.Clamp(moveSpeed, 0f, maxMoveSpeed);
        jumpPower = Mathf.Clamp(jumpPower, 0f, maxJumpPower);
        attackSpeedFactor = Mathf.Clamp(attackSpeedFactor, 0f, maxAttackSpeedFactor);
        healRate = Mathf.Clamp(healRate, 0f, maxHealRate);
    }

    #region ステータスを上書きする

    /// <summary>
    /// 現在のステータスを上書きする
    /// </summary>
    public void OverridCurrentStatus(CharacterStatusData statusData, params STATUS_TYPE[] types)
    {
        List<STATUS_TYPE> statusList = new List<STATUS_TYPE>(types);
        if (statusList.Contains(STATUS_TYPE.All)) statusList = GetAllStatusType();

        foreach (STATUS_TYPE type in statusList)
        {
            switch (type)
            {
                case STATUS_TYPE.HP:
                    hp = statusData.hp;
                    break;
                case STATUS_TYPE.Defense:
                    defense = statusData.defence;
                    break;
                case STATUS_TYPE.Power:
                    power = statusData.power;
                    break;
                case STATUS_TYPE.JumpPower:
                    jumpPower = statusData.jumpPower;
                    break;
                case STATUS_TYPE.MoveSpeed:
                    moveSpeed = statusData.moveSpeed;
                    break;
                case STATUS_TYPE.AttackSpeedFactor:
                    attackSpeedFactor = statusData.attackSpeedFactor;
                    break;
                case STATUS_TYPE.HealRate:
                    healRate = statusData.healRate;
                    break;
            }
        }
        ClampStatus();
        OverrideAnimaterParam();
    }

    /// <summary>
    /// 最大値の変更＆それに応じた現在値の変更
    /// </summary>
    /// <param name="changeData">強化後のステータス</param>
    public void ChangeAccordingStatusToMaximumValue(CharacterStatusData changeData)
    {
        // 各ステータスの最大値に対する現在値の割合を計算
        float hpRate = (float)hp / (float)maxHp;
        float defenseRate = (float)defense / (float)maxDefense;
        float powerRate = (float)power / (float)maxPower;
        float jumpPowerRate = jumpPower / maxJumpPower;
        float moveSpeedRate = moveSpeed / maxMoveSpeed;
        float attackSpeedFactorRate = attackSpeedFactor / maxAttackSpeedFactor;

        // 最大値の更新
        maxHp = changeData.hp;
        maxDefense = changeData.defence;
        maxPower = changeData.power;
        maxJumpPower = changeData.jumpPower;
        maxMoveSpeed = changeData.moveSpeed;
        maxAttackSpeedFactor = changeData.attackSpeedFactor;
        maxHealRate = changeData.healRate;

        // 変更後の最大値に応じた現在値の変更
        hp = (int)((float)maxHp * hpRate);
        defense = (int)((float)maxDefense * defenseRate);
        power = (int)((float)maxPower * powerRate);
        jumpPower = maxJumpPower * jumpPowerRate;
        moveSpeed = maxMoveSpeed * moveSpeedRate;
        attackSpeedFactor = maxAttackSpeedFactor * attackSpeedFactorRate;

        OverrideAnimaterParam();
    }

    /// <summary>
    /// 現在の最大ステータスを上書きし、それに応じて現在のステータスを更新する
    /// </summary>
    public void OverridMaxStatus(CharacterStatusData statusData, params STATUS_TYPE[] types)
    {
        List<STATUS_TYPE> statusList = new List<STATUS_TYPE>(types);
        if (statusList.Contains(STATUS_TYPE.All)) statusList = GetAllStatusType();

        foreach (STATUS_TYPE type in statusList)
        {
            switch (type)
            {
                case STATUS_TYPE.HP:
                    maxHp = statusData.hp;
                    break;
                case STATUS_TYPE.Defense:
                    maxDefense = statusData.defence;
                    break;
                case STATUS_TYPE.Power:
                    maxPower = statusData.power;
                    break;
                case STATUS_TYPE.JumpPower:
                    maxJumpPower = statusData.jumpPower;
                    break;
                case STATUS_TYPE.MoveSpeed:
                    maxMoveSpeed = statusData.moveSpeed;
                    break;
                case STATUS_TYPE.AttackSpeedFactor:
                    maxAttackSpeedFactor = statusData.attackSpeedFactor;
                    break;
                case STATUS_TYPE.HealRate:
                    maxHealRate = statusData.healRate;
                    break;
            }
        }

        CharacterStatusData changeData = new CharacterStatusData(
            hp: maxHp,
            defence: maxDefense,
            power: maxPower,
            moveSpeed: maxMoveSpeed,
            attackSpeedFactor: maxAttackSpeedFactor,
            jumpPower: maxJumpPower,
            healRate: maxHealRate
           );

        ChangeAccordingStatusToMaximumValue(changeData);
        OverrideAnimaterParam();
    }

    #endregion

    #region 割合を指定してステータスに加算する

    /// <summary>
    /// 割合を指定して最大ステータスに加算し、それに応じて現在のステータスを更新する
    /// </summary>
    /// <param name="rate"></param>
    /// <param name="canResetHpToMax"></param>
    /// <param name="types"></param>
    public void ApplyMaxStatusModifierByRate(float rate, params STATUS_TYPE[] types)
    {
        List<STATUS_TYPE> statusList = new List<STATUS_TYPE>(types);
        if (statusList.Contains(STATUS_TYPE.All)) statusList = GetAllStatusType();

        foreach (STATUS_TYPE type in statusList)
        {
            switch (type)
            {
                case STATUS_TYPE.HP:
                    maxHp += (int)(baseHp * rate);
                    break;
                case STATUS_TYPE.Defense:
                    maxDefense += (int)(baseDefense * rate);
                    break;
                case STATUS_TYPE.Power:
                    maxPower += (int)(basePower * rate);
                    break;
                case STATUS_TYPE.JumpPower:
                    maxJumpPower = baseJumpPower * rate;
                    break;
                case STATUS_TYPE.MoveSpeed:
                    maxMoveSpeed += baseMoveSpeed * rate;
                    break;
                case STATUS_TYPE.AttackSpeedFactor:
                    maxAttackSpeedFactor += baseAttackSpeedFactor * rate;
                    break;
                case STATUS_TYPE.HealRate:
                    maxHealRate += baseHealRate * rate;
                    break;
            }
        }
        CharacterStatusData changeData = new CharacterStatusData(
            hp: maxHp,
            defence: maxDefense,
            power: maxPower,
            moveSpeed: maxMoveSpeed,
            attackSpeedFactor: maxAttackSpeedFactor,
            jumpPower: maxJumpPower,
            healRate: maxHealRate
            );
        ChangeAccordingStatusToMaximumValue(changeData);
        OverrideAnimaterParam();
    }

    /// <summary>
    /// 割合の値分、現在のステータスに加算する
    /// </summary>
    /// <param name="rate"></param>
    /// <param name="canResetHpToMax"></param>
    /// <param name="types"></param>
    public void ApplyCurrentStatusModifierByRate(float rate, params STATUS_TYPE[] types)
    {
        List<STATUS_TYPE> statusList = new List<STATUS_TYPE>(types);
        if (statusList.Contains(STATUS_TYPE.All)) statusList = GetAllStatusType();

        foreach (STATUS_TYPE type in statusList)
        {
            switch (type)
            {
                case STATUS_TYPE.HP:
                    hp += (int)(baseHp * rate);
                    break;
                case STATUS_TYPE.Defense:
                    defense += (int)(baseDefense * rate);
                    break;
                case STATUS_TYPE.Power:
                    power += (int)(basePower * rate);
                    break;
                case STATUS_TYPE.JumpPower:
                    jumpPower = baseJumpPower * rate;
                    break;
                case STATUS_TYPE.MoveSpeed:
                    moveSpeed += baseMoveSpeed * rate;
                    break;
                case STATUS_TYPE.AttackSpeedFactor:
                    attackSpeedFactor += baseAttackSpeedFactor * rate;
                    break;
                case STATUS_TYPE.HealRate:
                    //healRate += baseHealRate * rate;
                    break;
            }
        }
        ClampStatus();
        OverrideAnimaterParam();
    }

    /// <summary>d
    /// レベルアップ時のステータス変化処理
    /// </summary>
    public void LevelUpStatusChange()
    {
        // 各ステータスの最大値に対する現在値の割合を計算
        float hpRate = (float)hp / (float)maxHp;
        float defenseRate = (float)defense / (float)maxDefense;
        float powerRate = (float)power / (float)maxPower;

        // 最大値の更新
        maxHp = maxHp + (int)(maxHp * LEVEL_UP_RATE);
        maxDefense = maxDefense + (int)(maxDefense * LEVEL_UP_RATE);
        maxPower = maxPower + (int)(maxPower * LEVEL_UP_RATE);

        // 変更後の最大値に応じた現在値の変更
        hp = (int)((float)maxHp * hpRate);
        defense = (int)((float)maxDefense * defenseRate);
        power = (int)((float)maxPower * powerRate);
    }

    #endregion

    #region アニメーション関連

    /// <summary>
    /// アニメーターのパラメーターを上書きする
    /// </summary>
    public void OverrideAnimaterParam()
    {
        if (animator)
        {
            animator.SetFloat("attack_speed", attackSpeedFactor);
            animator.SetFloat("move_speed", maxMoveSpeed / moveSpeed);
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