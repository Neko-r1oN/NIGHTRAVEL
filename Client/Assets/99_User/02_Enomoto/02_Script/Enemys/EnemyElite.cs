using NUnit.Framework;
using Pixeye.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using static StatusEffectController;

public class EnemyElite : MonoBehaviour
{
    #region プレファブ
    [SerializeField]
    GameObject blazeTrap;
    #endregion

    #region 特殊効果が発動する間隔
    readonly float blazeTickInterval = 0.2f; // ブレイズエリート
    #endregion

    [SerializeField]
    List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    float timer;

    /// <summary>
    /// エリートの種類
    /// </summary>
    public enum ELITE_TYPE
    {
        None,
        Blaze,      // ブレイズエリート
        Frost,      // フロストエリート
        Thunder     // サンダーエリート
    }
    ELITE_TYPE eliteType = ELITE_TYPE.None;
    public ELITE_TYPE EliteType { get { return eliteType; } }

    /// <summary>
    /// 付与する状態異常の種類
    /// </summary>
    public StatusEffectController.EFFECT_TYPE addStatusEffect { get; private set; }

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Init(ELITE_TYPE type)
    {
        eliteType = type;

        // 50%UPしたステータスデータ作成
        CharacterStatusData data = new CharacterStatusData();
        CharacterBase charaBase = GetComponent<CharacterBase>();
        data.hp = charaBase.BaseHP + Mathf.CeilToInt(charaBase.BaseHP * 0.5f);
        data.power = charaBase.BasePower + Mathf.CeilToInt(charaBase.BasePower * 0.5f);
        data.moveSpeed = charaBase.BaseMoveSpeed + Mathf.CeilToInt(charaBase.BaseMoveSpeed * 0.5f);
        data.moveSpeedFactor = charaBase.BaseMoveSpeedFactor + Mathf.CeilToInt(charaBase.BaseMoveSpeedFactor * 0.5f);
        data.attackSpeedFactor = charaBase.BaseAttackSpeedFactor + Mathf.CeilToInt(charaBase.BaseAttackSpeedFactor * 0.5f);

        Color outlineColor = new Color();
        Action action = type switch
        {
            ELITE_TYPE.Blaze => () =>
            {
                // カラーコード FF5D17(赤色)
                outlineColor = new Color32(0xFF, 0x5D, 0x17, 0xFF);
            }
            ,
            ELITE_TYPE.Frost => () =>
            {
                // カラーコード 7BB8CF(青色)
                outlineColor = new Color32(0x7B, 0xB8, 0xCF, 0xFF);
            }
            ,
            ELITE_TYPE.Thunder => () =>
            {
                // カラーコード E492F0(紫色)
                outlineColor = new Color32(0xE4, 0x92, 0xF0, 0xFF);

                data.moveSpeed = charaBase.BaseMoveSpeed * 2;
            }
            ,
            _ => () => { }
        };
        action();

        // ユニーク個体のマテリアルのプロパティ設定
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            Material material = spriteRenderer.material;
            material.SetColor("_OutlineColor", outlineColor);
            material.SetFloat("_OutlineAlpha", 1f);
            material.SetFloat("_OutlineGlow", 1.5f);
            material.SetFloat("_OutlineWidth", 0.2f);
            material.SetFloat("_OutlineDistortAmount", 0.5f);
        }

        // 基礎ステータス&現在のステータスを上書き
        charaBase.OverrideBaseStatus(data, true);
    }

    /// <summary>
    /// 付与させる状態異常のenumを取得する
    /// </summary>
    /// <returns></returns>
    public StatusEffectController.EFFECT_TYPE GetAddStatusEffectEnum()
    {
        return eliteType switch
        {
            ELITE_TYPE.Blaze => StatusEffectController.EFFECT_TYPE.Burn,
            ELITE_TYPE.Frost => StatusEffectController.EFFECT_TYPE.Freeze,
            ELITE_TYPE.Thunder => StatusEffectController.EFFECT_TYPE.Shock,
            _ => StatusEffectController.EFFECT_TYPE.None
        };
    }

    /// <summary>
    /// ブレイズエリートの特殊効果処理
    /// </summary>
    public void ActivateBlazeEffect()
    {
        GameObject trap = Instantiate(blazeTrap, transform.position, Quaternion.identity);
        trap.GetComponent<BlazeTrap>().InitializeBlazeTrap(gameObject);
    }

    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        Action action = eliteType switch
        {
            ELITE_TYPE.Blaze => () =>
            {
                if (timer > blazeTickInterval)
                {
                    timer = 0;
                    ActivateBlazeEffect();
                }
            }
            ,
            _ => () => { }
        };
        action();
    }
}
