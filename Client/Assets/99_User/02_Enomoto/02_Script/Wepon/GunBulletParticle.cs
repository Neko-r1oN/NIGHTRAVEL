//**************************************************
//  �e�e�𐧌䂷��N���X
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
    string ownerTag;
    int damageValue;

    /// <summary>
    /// ����������
    /// </summary>
    /// <param name="damageValue"></param>
    /// <param name="enemyTag"></param>
    public void Initialize(string ownerTag, int damageValue, DEBUFF_TYPE? effectType)
    {
        this.ownerTag = ownerTag;
        this.damageValue = damageValue;
        this.effectType = effectType;
    }

    /// <summary>
    /// �p�[�e�B�N���̓����蔻�菈��
    /// </summary>
    /// <param name="other"></param>
    private void OnParticleCollision(GameObject other)
    {
        if (other.tag == ownerTag) return;

        if (other.tag == "Player")
        {
            other.GetComponent<PlayerBase>().ApplyDamage(damageValue, null, effectType);
        }
        else if (other.tag == "Enemy")
        {
            Debug.Log(other.gameObject.name);
            other.GetComponent<EnemyBase>().ApplyDamage(damageValue);
        }
        else if (other.tag == "Object")
        {
            Debug.Log("�j��\�I�u�W�F�N�g�Ƀ_���[�W��^����");
        }
    }
}
