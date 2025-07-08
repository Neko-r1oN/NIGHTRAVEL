//**************************************************
//  �e�e�𐧌䂷��N���X
//  Author:r-enomoto
//**************************************************
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GunBulletParticle : MonoBehaviour
{
    StatusEffectController.EFFECT_TYPE? effectType = null;
    int damageValue;

    /// <summary>
    /// ����������
    /// </summary>
    /// <param name="damageValue"></param>
    /// <param name="enemyTag"></param>
    public void Initialize(int damageValue, LayerMask collisionLayer, StatusEffectController.EFFECT_TYPE? effectType)
    {
        this.damageValue = damageValue;
        var colision = GetComponent<ParticleSystem>().collision;    // �Փ˂̃��W���[���擾
        colision.collidesWith = collisionLayer;     // �U���Ώۂ̃��C���[��ݒ�
        this.effectType = effectType;
    }

    /// <summary>
    /// �p�[�e�B�N���̓����蔻�菈��
    /// </summary>
    /// <param name="other"></param>
    private void OnParticleCollision(GameObject other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerBase>().ApplyDamage(damageValue, null, effectType);
        }
        else if (other.tag == "Enemy")
        {
            other.GetComponent<EnemyBase>().ApplyDamage(damageValue);
        }
        else if (other.tag == "Object")
        {
            Debug.Log("�j��\�I�u�W�F�N�g�Ƀ_���[�W��^����");
        }
    }
}
