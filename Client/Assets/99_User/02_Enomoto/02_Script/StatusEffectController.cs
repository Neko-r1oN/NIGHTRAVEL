//**************************************************
//  状態異常を管理(制御・適用・更新など)するクラス
//  Author:r-enomoto
//**************************************************
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectController : MonoBehaviour
{
    /// <summary>
    /// 状態異常の種類
    /// </summary>
    public enum EFFECT_TYPE
    {
        Burn,       // 炎上状態
        Freeze,     // 霜焼け状態
        Shock       // 感電状態
    }
    Dictionary<EFFECT_TYPE, float> currentEffects = new Dictionary<EFFECT_TYPE, float>();

    #region 各状態異常の効果時間
    readonly float maxBurnDuration = 5f;    // 炎上
    readonly float maxFreezeDuration = 10f; // 霜焼け
    readonly float maxShockDuration = 5f; // 感電
    #endregion

    #region 状態異常の効果が発動する間隔
    readonly float burnTickInterval = 0.5f; // 炎上
    #endregion

    #region ステータスに加減する合計差分
    public int MoveSpeedOffset { get; private set; }    // 移動速度
    public int AttackSpeedOffset { get; private set; }  // 攻撃速度
    #endregion

    #region  各状態異常を適用させたときの効果値
    Dictionary<EFFECT_TYPE, int> tmpMoveSpeedValue = new Dictionary<EFFECT_TYPE, int>();    // 移動速度
    Dictionary<EFFECT_TYPE, int> tmpAttackSpeedValue = new Dictionary<EFFECT_TYPE, int>();  // 攻撃速度
    #endregion

    // 各状態異常を適用させたときの効果値
    Player player;
    EnemyBase enemy;

    private void Start()
    {
        if (this.gameObject.tag == "Player") player = GetComponent<Player>();
        else if (this.gameObject.tag == "EnemyBase") enemy = GetComponent<EnemyBase>();


        Invoke("ApplyStatusEffect", 2f);
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
    public void ApplyStatusEffect()
    {
        EFFECT_TYPE effectType = EFFECT_TYPE.Shock;
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
                EFFECT_TYPE.Burn => () => {
                    Debug.Log("炎上効果適用");
                    InvokeRepeating("ActivateBurnEffect", 0, burnTickInterval);
                }
                ,
                EFFECT_TYPE.Freeze => () => {
                    Debug.Log("霜焼け効果適用");

                }
                ,
                EFFECT_TYPE.Shock => () => {
                    Debug.Log("感電効果適用");
                    if (this.gameObject.tag == "EnemyBase")
                    {
                        GetComponent<EnemyBase>().ApplyStun(maxShockDuration);
                    }
                }
                ,
                _ => null
            };
            action();
        }
        else
        {
            currentEffects[effectType] = effectDuration;    // 効果時間をリセット
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
                Debug.Log("炎上効果解除");
                CancelInvoke("ActivateBurnEffect");
            }
            ,
            EFFECT_TYPE.Freeze => () => { 
                Debug.Log($"霜焼け・凍結効果解除：*移動速度：{tmpMoveSpeedValue[EFFECT_TYPE.Freeze]}* , *攻撃速度：{tmpAttackSpeedValue[EFFECT_TYPE.Freeze]}*"); 
            }
            ,
            EFFECT_TYPE.Shock => () => { 
                Debug.Log("感電効果解除");
            }
            ,
            _ => null
        };
        action();
        currentEffects.Remove(effectType);
    }

    /// <summary>
    /// 炎上効果
    /// </summary>
    void ActivateBurnEffect()
    {
        // 最大HPの5%のダメージを与える
        Debug.Log("炎上効果発動中");
    }
}
