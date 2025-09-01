//**************************************************
//  �e�̃p�[�e�B�N���𐧌䂷��
//  Author:r-enomoto
//**************************************************
using NUnit.Framework;
using Pixeye.Unity;
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class GunParticleController : MonoBehaviour
{
    #region �p�[�e�B�N���֘A
    [Foldout("�p�[�e�B�N���֘A")]
    [SerializeField]
    GameObject gunParticleParent;

    [Foldout("�p�[�e�B�N���֘A")]
    [SerializeField]
    GunBulletParticle gunBulletParticle;
    #endregion

    #region �U���Ώۊ֘A
    [Foldout("�U���Ώۂ̐ݒ�")]
    [SerializeField]
    bool canAttackEnemy;

    [Foldout("�U���Ώۂ̐ݒ�")]
    [SerializeField]
    bool canAttackPlayer;

    [Foldout("�U���Ώۂ̐ݒ�")]
    [SerializeField]
    bool canAttackObject;
    #endregion

    [SerializeField] CharacterBase owner;

    /// <summary>
    /// �����n�߂鏈��
    /// </summary>
    public void StartShooting()
    {
        // �t�^�����Ԉُ���擾
        DEBUFF_TYPE? effectType = null;
        if (owner.tag == "Enemy")
        {
            effectType = owner.GetComponent<EnemyBase>().GetStatusEffectToApply();
        }

        gunBulletParticle.Initialize(owner, effectType);
        gunParticleParent.SetActive(true);
    }

    /// <summary>
    /// ���̂��~�߂鏈��
    /// </summary>
    public void StopShooting()
    {
        gunParticleParent.SetActive(false);
    }
}
