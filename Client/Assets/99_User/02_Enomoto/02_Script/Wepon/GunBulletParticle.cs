//**************************************************
//  銃弾を制御するクラス
//  Author:r-enomoto
//**************************************************
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class GunBulletParticle : MonoBehaviour
{
    DEBUFF_TYPE? effectType = null;
    CharacterBase owner;
    string ownerTag;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <param name="damageValue"></param>
    /// <param name="enemyTag"></param>
    public void Initialize(CharacterBase owner, DEBUFF_TYPE? effectType)
    {
        this.ownerTag = owner.gameObject.tag;
        this.owner = owner;
        this.effectType = effectType;
    }

    /// <summary>
    /// パーティクルの当たり判定処理
    /// </summary>
    /// <param name="other"></param>
    private void OnParticleCollision(GameObject other)
    {
        if (other.tag == ownerTag) return;

        if (other.tag == "Player")
        {
            other.GetComponent<PlayerBase>().ApplyDamage(owner.Power, null,null, effectType);
        }
        else if (other.tag == "Enemy")
        {
            Debug.Log(other.gameObject.name);
            other.GetComponent<EnemyBase>().ApplyDamageRequest(owner.Power);
        }
        else if (other.tag == "Object")
        {
            Debug.Log("破壊可能オブジェクトにダメージを与える");
        }
    }
}
