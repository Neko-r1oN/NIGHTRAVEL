using Rewired;
using UnityEngine;
using static CharacterBase;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class BoxgeistDamageCollider : MonoBehaviour
{
    [SerializeField]
    EnemyBase m_EnemyBase;

    [SerializeField]
    float powerRate;

    [SerializeField]
    KB_POW knockbackPower;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player" && CharacterManager.Instance.PlayerObjSelf == collision.gameObject)
        {
            // 自身がエリート個体の場合、付与する状態異常の種類を取得する
            DEBUFF_TYPE? applyEffect = m_EnemyBase.GetStatusEffectToApply();
            collision.gameObject.GetComponent<PlayerBase>().ApplyDamage((int)(m_EnemyBase.power * powerRate), transform.position, knockbackPower, applyEffect);
        }
    }
}
