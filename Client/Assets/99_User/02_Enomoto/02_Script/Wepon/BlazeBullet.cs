using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class BlazeBullet : MonoBehaviour
{
    [SerializeField]
    EnemyBase owner;

    int power;
    DEBUFF_TYPE? effectType = null;

    private void Start()
    {
        power = (int)(owner.power * 1.5f);
        effectType = owner.GetStatusEffectToApply();
    }

    /// <summary>
    /// パーティクルの当たり判定処理
    /// </summary>
    /// <param name="other"></param>
    private void OnParticleCollision(GameObject other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerBase>().ApplyDamage(power, owner.transform.position, CharacterBase.KB_POW.Big, effectType);
        }
    }
}
