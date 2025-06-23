using NUnit.Framework;
using Pixeye.Unity;
using System.Collections.Generic;
using UnityEngine;

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
    /// �����n�߂鏈��
    /// </summary>
    public void StartShooting()
    {
        // �t�^�����Ԉُ���擾
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
    /// ���̂��~�߂鏈��
    /// </summary>
    public void StopShooting()
    {
        gunParticleParent.SetActive(false);
    }
}
