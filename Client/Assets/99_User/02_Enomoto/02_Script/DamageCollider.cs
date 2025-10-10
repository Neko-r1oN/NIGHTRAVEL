using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    [SerializeField]
    EnemyBase owner;

    [SerializeField]
    float damageRate = 0;

    [SerializeField]
    EnumManager.DEBUFF_TYPE applyDebuffType = EnumManager.DEBUFF_TYPE.None;

    [SerializeField]
    CharacterBase.KB_POW knockBackPower = CharacterBase.KB_POW.Small;

    /// <summary>
    /// �_���[�W�v�Z���@
    /// </summary>
    enum DamageCalculationType
    {
        Percentage,     // �ő�HP�̊����ŗ^����
        AttackBased,    // owner�̍U���͂����Ɍv�Z����
    }
    [SerializeField] DamageCalculationType damageCalculationType = DamageCalculationType.Percentage;

    /// <summary>
    /// �Փ˔�����@�̎��
    /// </summary>
    enum TriggerDetectionType
    {
        TriggerEnter,
        TriggerStay,
        ParticleTrigger
    }
    [SerializeField] TriggerDetectionType detectionType = TriggerDetectionType.TriggerEnter;

    ParticleSystem ps;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        if (ps != null)
        {
            // �Փ˔���̑ΏۃR���C�_�[��ݒ肷��
            var trigger = ps.trigger;
            trigger.enabled = true;
            trigger.SetCollider(0, CharacterManager.Instance.PlayerObjSelf.GetComponent<CapsuleCollider2D>());
        }  
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (detectionType != TriggerDetectionType.TriggerEnter) return;
        var player = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>();
        if (collision.gameObject == player.gameObject)
        {
            ApplyDamage();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (detectionType != TriggerDetectionType.TriggerStay) return;
        var player = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>();
        if (collision.gameObject == player.gameObject)
        {
            ApplyDamage();
        }
    }

    /// <summary>
    /// �p�[�e�B�N���V�X�e���̃g���K�[���W���[���ɂ��Փ˔���
    /// </summary>
    void OnParticleTrigger()
    {
        if (detectionType != TriggerDetectionType.ParticleTrigger) return;
        var player = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>();
        List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();

        // Trigger �ɓ������p�[�e�B�N�����擾
        int enterCount = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);

        if (enterCount > 0)
        {
            ApplyDamage();
        }
    }

    /// <summary>
    /// �v���C���[�Ƀ_���[�W�p�K�p����
    /// </summary>
    void ApplyDamage()
    {
        var player = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>();
        if(player != null && !player.IsDead)
        {
            int power = 0;
            if (damageCalculationType == DamageCalculationType.Percentage)
            {
                power = (int)(player.MaxHP * damageRate);
            }
            else
            {
                power = owner.Power + (int)(owner.Power * damageRate);
            }

            EnumManager.DEBUFF_TYPE? debuff = applyDebuffType == EnumManager.DEBUFF_TYPE.None ? null : applyDebuffType;
            player.ApplyDamage(power, transform.position, knockBackPower, debuff);
        }
    }
}
