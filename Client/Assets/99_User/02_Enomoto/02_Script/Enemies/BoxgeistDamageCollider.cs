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
            // ���g���G���[�g�̂̏ꍇ�A�t�^�����Ԉُ�̎�ނ��擾����
            DEBUFF_TYPE? applyEffect = m_EnemyBase.GetStatusEffectToApply();
            collision.gameObject.GetComponent<PlayerBase>().ApplyDamage((int)(m_EnemyBase.power * powerRate), transform.position, knockbackPower, applyEffect);
        }
    }
}
