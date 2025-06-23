using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GunParticleController : MonoBehaviour
{
    [SerializeField] GameObject gunParticleParent;
    [SerializeField] GunBulletParticle gunBulletParticle;
    [SerializeField] List<ParticleSystem> particles = new List<ParticleSystem>();

    [SerializeField] int damageValue;

    // �U���Ώ�
    [SerializeField] bool canAttackEnemy;
    [SerializeField] bool canAttackPlayer;
    [SerializeField] bool canAttackObject;
    LayerMask attackTargetLayerMask;

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
        gunBulletParticle.Initialize(damageValue, attackTargetLayerMask);
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
