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

        // HP・攻撃力が50%増し、防御力・移動速度・移動速度係数が25%増しにする
        CharacterBase charaBase = GetComponent<CharacterBase>();
        charaBase.ApplyStatusModifierByRate(0.5f, true, CharacterBase.STATUS_TYPE.HP, CharacterBase.STATUS_TYPE.Power);
        charaBase.ApplyStatusModifierByRate(0.25f, CharacterBase.STATUS_TYPE.Defence, CharacterBase.STATUS_TYPE.MoveSpeed, CharacterBase.STATUS_TYPE.MoveSpeedFactor);

        Color outlineColor = new Color();
        Action action = type switch
        {
            ELITE_TYPE.Blaze => () =>
            {
                // カラーコード FF0000(赤色)
                outlineColor = new Color32(0xFF, 0x00, 0x00, 0xFF);
            }
            ,
            ELITE_TYPE.Frost => () =>
            {
                // カラーコード 00FFEA(水色)
                outlineColor = new Color32(0x00, 0xFF, 0xEF, 0xFF);
            }
            ,
            ELITE_TYPE.Thunder => () =>
            {
                // カラーコード E100FF(紫色)
                outlineColor = new Color32(0xE1, 0x00, 0xFF, 0xFF);

                // Thunderのみ移動速度・移動速度係数が2倍になるようにする
                charaBase.ApplyStatusModifierByRate(0.75f, CharacterBase.STATUS_TYPE.MoveSpeed, CharacterBase.STATUS_TYPE.MoveSpeedFactor);
            }
            ,
            _ => () => { }
        };
        action();

        // ユニーク個体のマテリアルのプロパティ設定
        foreach (SpriteRenderer spriteRenderer in GetComponent<EnemyBase>().SpriteRenderers)
        {
            Material material = spriteRenderer.material;
            material.SetColor("_OutlineColor", outlineColor);
            material.SetFloat("_OutlineAlpha", 1f);
            material.SetFloat("_OutlineGlow", 50f);
            material.SetFloat("_OutlineWidth", 0.2f);
            material.SetFloat("_OutlineDistortAmount", 0.5f);
        }

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
