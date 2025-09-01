//**************************************************
//  銃のパーティクルを制御する
//  Author:r-enomoto
//**************************************************
using NUnit.Framework;
using Pixeye.Unity;
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

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
    #endregion

    [SerializeField] CharacterBase owner;

    /// <summary>
    /// 撃ち始める処理
    /// </summary>
    public void StartShooting()
    {
        // 付与する状態異常を取得
        DEBUFF_TYPE? effectType = null;
        if (owner.tag == "Enemy")
        {
            effectType = owner.GetComponent<EnemyBase>().GetStatusEffectToApply();
        }

        gunBulletParticle.Initialize(owner, effectType);
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
