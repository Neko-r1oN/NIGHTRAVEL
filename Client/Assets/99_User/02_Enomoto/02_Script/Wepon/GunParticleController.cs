using NUnit.Framework;
using Pixeye.Unity;
using System.Collections.Generic;
using UnityEngine;

public class GunParticleController : MonoBehaviour
{
    #region パーティクル関連
    [Foldout("パーティクル関連")]
    [SerializeField]
    GameObject gunParticleParent;

    [Foldout("パーティクル関連")]
    [SerializeField]
    GunBulletParticle gunBulletParticle;
    #endregion

    #region 攻撃対象関連
    [Foldout("攻撃対象の設定")]
    [SerializeField]
    bool canAttackEnemy;

    [Foldout("攻撃対象の設定")]
    [SerializeField]
    bool canAttackPlayer;

    [Foldout("攻撃対象の設定")]
    [SerializeField]
    bool canAttackObject;
    LayerMask attackTargetLayerMask;
    #endregion

    [SerializeField] GameObject owner;
    [SerializeField] int damageValue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (canAttackEnemy) attackTargetLayerMask += LayerMask.GetMask("Enemy");
        if (canAttackPlayer) attackTargetLayerMask += LayerMask.GetMask("BlinkPlayer") | LayerMask.GetMask("Player");
        attackTargetLayerMask += LayerMask.GetMask("Default");
    }

    /// <summary>
    /// 撃ち始める処理
    /// </summary>
    public void StartShooting()
    {
        // 付与する状態異常を取得
        StatusEffectController.EFFECT_TYPE? effectType = null;
        if (owner.tag == "Enemy")
        {
            if (owner.GetComponent<EnemyBase>().IsElite)
            {
                effectType = owner.GetComponent<EnemyElite>().GetAddStatusEffectEnum();
            }
        }

        gunBulletParticle.Initialize(damageValue, attackTargetLayerMask, effectType);
        gunParticleParent.SetActive(true);
    }

    /// <summary>
    /// 撃つのを止める処理
    /// </summary>
    public void StopShooting()
    {
        gunParticleParent.SetActive(false);
    }
}
