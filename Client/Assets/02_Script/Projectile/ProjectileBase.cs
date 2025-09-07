using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;
public class ProjectileBase : MonoBehaviour
{
    #region データ関連
    [SerializeField]
    protected PROJECTILE_TYPE typeId;

    public PROJECTILE_TYPE TypeId { get { return typeId; } }
    #endregion

    [SerializeField] 
    protected bool isDestructible;

    protected List<DEBUFF_TYPE> debuffs = new List<DEBUFF_TYPE>();
    protected int power;

    public void Init(List<DEBUFF_TYPE> debuffs, int power)
    {
        this.debuffs = debuffs;
        this.power = power;
    }

    public void Shoot(Vector3 shootDirection)
    {
        GetComponent<Rigidbody2D>().AddForce(shootDirection, ForceMode2D.Impulse);
    }

    public void ApplyDamage()
    {
        if(isDestructible) Destroy();
    }

    protected virtual void Destroy()
    {
        Destroy(gameObject);
    }
}
