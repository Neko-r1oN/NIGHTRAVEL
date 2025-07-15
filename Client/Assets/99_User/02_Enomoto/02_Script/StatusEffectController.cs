//**************************************************
//  状態異常を管理(制御・適用・更新など)するクラス
//  Author:r-enomoto
//**************************************************
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class StatusEffectController : MonoBehaviour
{
    /// <summary>
    /// 状態異常の種類
    /// </summary>
    public enum EFFECT_TYPE
    {
        None,
        Burn,       // 炎上状態
        Freeze,     // 霜焼け状態
        Shock       // 感電状態
    }
    Dictionary<EFFECT_TYPE, float> currentEffects = new Dictionary<EFFECT_TYPE, float>();

    #region 各状態異常の効果時間
    readonly float maxBurnDuration = 6f;    // 炎上
    readonly float maxFreezeDuration = 5f; // 霜焼け
    readonly float maxShockDuration = 1f; // 感電
    #endregion

    #region 各状態異常の効果値
    readonly float burnEffect = 0.05f;
    readonly float freezeEffect = 0.2f;
    readonly float shockEffect = 0.5f;
    #endregion

    #region 状態異常の効果が発動する間隔
    readonly float burnTickInterval = 0.5f; // 炎上
    #endregion

    #region  各状態異常を適用させた効果割合
    Dictionary<EFFECT_TYPE, float> tmpMoveSpeedRates = new Dictionary<EFFECT_TYPE, float>();    // 移動速度
    Dictionary<EFFECT_TYPE, float> tmpMoveSpeedFactorRates = new Dictionary<EFFECT_TYPE, float>();    // 移動速度(Animatorの係数)
    Dictionary<EFFECT_TYPE, float> tmpAttackSpeedFactorRates = new Dictionary<EFFECT_TYPE, float>();  // 攻撃速度(Animatorの係数)
    #endregion

    #region 状態異常のパーティクル
    Dictionary<EFFECT_TYPE, GameObject> appliedParticles = new Dictionary<EFFECT_TYPE, GameObject>();   // 適用中のパーティクル
    [SerializeField] GameObject burnPS;
    [SerializeField] GameObject freezePS;
    [SerializeField] GameObject shockPS;
    #endregion

    // 各状態異常を適用させたときの効果値
    CapsuleCollider2D capsule2D;
    CharacterBase characterBase;
    PlayerBase playerBase;
    EnemyBase enemyBase;

    private void Start()
    {
        if (this.gameObject.tag == "Player") playerBase = GetComponent<PlayerBase>();
        else if (this.gameObject.tag == "Enemy") enemyBase = GetComponent<EnemyBase>();
        capsule2D = GetComponent<CapsuleCollider2D>();
        characterBase = GetComponent<CharacterBase>();
    }

    private void FixedUpdate()
    {
        List<EFFECT_TYPE> keyList = new List<EFFECT_TYPE>(currentEffects.Keys);
        List<EFFECT_TYPE> effectsToRemove = new List<EFFECT_TYPE>();
        foreach (var key in keyList)
        {
            currentEffects[key] -= Time.fixedDeltaTime;
            if (currentEffects[key] <= 0)
            {
                effectsToRemove.Add(key);
            }
        }

        // 終了した効果を削除
        foreach (var key in effectsToRemove)
        {
            ClearStatusEffect(key);
        }
    }

    /// <summary>
    /// 状態異常を付与する処理
    /// </summary>
    /// <param name="effectType"></param>
    public void ApplyStatusEffect(params EFFECT_TYPE[] effectTypes)
    {
        foreach (EFFECT_TYPE effectType in effectTypes)
        {
            float effectDuration = effectType switch
            {
                EFFECT_TYPE.Burn => maxBurnDuration,
                EFFECT_TYPE.Freeze => maxFreezeDuration,
                EFFECT_TYPE.Shock => maxShockDuration,
                _ => 0
            };

            if (!currentEffects.ContainsKey(effectType))
            {
                currentEffects.Add(effectType, effectDuration);

                // 状態異常の効果を適用させる
                Action action = effectType switch
                {
                    EFFECT_TYPE.Burn => () =>
                    {
                        InvokeRepeating("ActivateBurnEffect", 0, burnTickInterval);
                    }
                    ,
                    EFFECT_TYPE.Freeze => () =>
                    {
                        // 移動速度・攻撃速度の20%分を現在のステータスから減算する
                        float freezeRatio = -freezeEffect;
                        tmpMoveSpeedRates.Add(effectType, freezeRatio);
                        tmpMoveSpeedFactorRates.Add(effectType, freezeRatio);
                        tmpAttackSpeedFactorRates.Add(effectType, freezeRatio);
                        characterBase.ApplyStatusModifierByRate(
                            freezeRatio,
                            CharacterBase.STATUS_TYPE.MoveSpeed,
                            CharacterBase.STATUS_TYPE.MoveSpeedFactor,
                            CharacterBase.STATUS_TYPE.AttackSpeedFactor
                        );
                    }
                    ,
                    EFFECT_TYPE.Shock => () =>
                    {
                        if (this.gameObject.tag == "Player") StartCoroutine(playerBase.AbnormalityStun(shockEffect));
                        else if (this.gameObject.tag == "Enemy") enemyBase.ApplyStun(shockEffect);
                    }
                    ,
                    _ => null
                };
                action();
                ApplyStatusEffectParticle(effectType);
            }
            else
            {
                if (effectType != EFFECT_TYPE.Shock)
                {
                    currentEffects[effectType] = effectDuration;    // 効果時間をリセット
                }
            }
        }
    }

    /// <summary>
    /// 状態異常解除
    /// </summary>
    /// <param name="effectType"></param>
    void ClearStatusEffect(EFFECT_TYPE effectType)
    {
        Action action = effectType switch
        {
            EFFECT_TYPE.Burn => () => { 
                CancelInvoke("ActivateBurnEffect");
            }
            ,
            EFFECT_TYPE.Freeze => () => {
                // 移動速度・攻撃速度の減算されていた分の値をステータスに加算する
                float freezeRatio = freezeEffect;
                characterBase.ApplyStatusModifierByRate(
                        freezeRatio,
                        CharacterBase.STATUS_TYPE.MoveSpeed,
                        CharacterBase.STATUS_TYPE.MoveSpeedFactor,
                        CharacterBase.STATUS_TYPE.AttackSpeedFactor
                    );
                tmpMoveSpeedRates.Remove(effectType);
                tmpMoveSpeedFactorRates.Remove(effectType);
                tmpAttackSpeedFactorRates.Remove(effectType);
            }
            ,
            EFFECT_TYPE.Shock => () => {
            }
            ,
            _ => null
        };
        action();
        currentEffects.Remove(effectType);
        DestroyStatusEffectParticle(effectType);
    }

    /// <summary>
    /// 状態異常のパーティクルを適用させる
    /// </summary>
    /// <param name="type"></param>
    /// <param name="particlePrefab"></param>
    void ApplyStatusEffectParticle(EFFECT_TYPE type)
    {
        GameObject psObj = null;
        ParticleSystem ps;

        // 各パーティクルの詳細設定
        Action action = type switch
        {
            EFFECT_TYPE.Burn => () =>
            {
                psObj = Instantiate(burnPS, this.transform);
                psObj.transform.position = capsule2D.bounds.center;
                ps = psObj.GetComponent<ParticleSystem>();
                ParticleHelper.MatchRadiusToSpriteWidth(capsule2D, ps);
            }
            ,
            EFFECT_TYPE.Freeze => () =>
            {
                psObj = Instantiate(freezePS, this.transform);
                psObj.transform.position = capsule2D.bounds.center;
                ps = psObj.GetComponent<ParticleSystem>();
                ParticleHelper.MatchRadiusToSpriteWidth(capsule2D, ps);
            }
            ,
            EFFECT_TYPE.Shock => () =>
            {
                psObj = Instantiate(shockPS, this.transform);
                psObj.transform.position = capsule2D.bounds.center;
                ps = psObj.GetComponent<ParticleSystem>();
                float baseRateOverTime = 15f;
                ParticleHelper.MatchRadiusToSpriteWidth(capsule2D, ps);
                ParticleHelper.AdjustRateOverTimeBySpriteWidth(capsule2D, ps, baseRateOverTime);
            }
            ,
            _ => () => { }
        };
        action();
        if (psObj != null) appliedParticles.Add(type, psObj);
    }

    /// <summary>
    /// 適用されていた状態異常のパーティクルを破棄する
    /// </summary>
    /// <param name="type"></param>
    void DestroyStatusEffectParticle(EFFECT_TYPE type)
    {
        if (appliedParticles.ContainsKey(type))
        {
            GameObject psObj = appliedParticles[type];
            appliedParticles.Remove(type);
            Destroy(psObj);
        }
    }

    /// <summary>
    /// 炎上効果
    /// </summary>
    void ActivateBurnEffect()
    {
        // 最大HPの5%のダメージを与える
        int dmgValue = Mathf.CeilToInt((float)characterBase.MaxHP * burnEffect);
        if (this.gameObject.tag == "Player") playerBase.ApplyDamage(dmgValue);
        else if (this.gameObject.tag == "Enemy") enemyBase.ApplyDamage(dmgValue);
    }
}
