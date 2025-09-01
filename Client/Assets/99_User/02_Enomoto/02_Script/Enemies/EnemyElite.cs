using System;
using UnityEngine;
using Shared.Interfaces.StreamingHubs;
using static Shared.Interfaces.StreamingHubs.EnumManager;

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

    EnumManager.ENEMY_ELITE_TYPE eliteType = EnumManager.ENEMY_ELITE_TYPE.None;
    public EnumManager.ENEMY_ELITE_TYPE EliteType { get { return eliteType; } }

    /// <summary>
    /// 付与する状態異常の種類
    /// </summary>
    public DEBUFF_TYPE addStatusEffect { get; private set; }

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Init(EnumManager.ENEMY_ELITE_TYPE type)
    {
        eliteType = type;

        // HP・攻撃力が50%増し、防御力・移動速度・移動速度係数が25%増しにする
        CharacterBase charaBase = GetComponent<CharacterBase>();
        charaBase.ApplyMaxStatusModifierByRate(0.5f, STATUS_TYPE.HP, STATUS_TYPE.Power);
        charaBase.ApplyMaxStatusModifierByRate(0.25f, STATUS_TYPE.Defense, STATUS_TYPE.MoveSpeed);

        Color outlineColor = new Color();
        Action action = type switch
        {
            EnumManager.ENEMY_ELITE_TYPE.Blaze => () =>
            {
                // カラーコード FF0000(赤色)
                outlineColor = new Color32(0xFF, 0x00, 0x00, 0xFF);
            }
            ,
            EnumManager.ENEMY_ELITE_TYPE.Frost => () =>
            {
                // カラーコード 00FFEA(水色)
                outlineColor = new Color32(0x00, 0xFF, 0xEF, 0xFF);
            }
            ,
            EnumManager.ENEMY_ELITE_TYPE.Thunder => () =>
            {
                // カラーコード E100FF(紫色)
                outlineColor = new Color32(0xE1, 0x00, 0xFF, 0xFF);

                // Thunderのみ移動速度・移動速度係数が2倍になるようにする
                charaBase.ApplyMaxStatusModifierByRate(0.75f, STATUS_TYPE.MoveSpeed);
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
    public DEBUFF_TYPE? GetAddStatusEffectEnum()
    {
        return eliteType switch
        {
            ENEMY_ELITE_TYPE.Blaze => DEBUFF_TYPE.Burn,
            ENEMY_ELITE_TYPE.Frost => DEBUFF_TYPE.Freeze,
            ENEMY_ELITE_TYPE.Thunder => DEBUFF_TYPE.Shock,
            _ => null
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
            EnumManager.ENEMY_ELITE_TYPE.Blaze => () =>
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
