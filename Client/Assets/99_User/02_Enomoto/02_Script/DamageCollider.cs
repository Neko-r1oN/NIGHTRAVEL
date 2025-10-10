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
    /// ダメージ計算方法
    /// </summary>
    enum DamageCalculationType
    {
        Percentage,     // 最大HPの割合で与える
        AttackBased,    // ownerの攻撃力を元に計算する
    }
    [SerializeField] DamageCalculationType damageCalculationType = DamageCalculationType.Percentage;

    /// <summary>
    /// 衝突判定方法の種類
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
            // 衝突判定の対象コライダーを設定する
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
    /// パーティクルシステムのトリガーモジュールによる衝突判定
    /// </summary>
    void OnParticleTrigger()
    {
        if (detectionType != TriggerDetectionType.ParticleTrigger) return;
        var player = CharacterManager.Instance.PlayerObjSelf.GetComponent<PlayerBase>();
        List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();

        // Trigger に入ったパーティクルを取得
        int enterCount = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);

        if (enterCount > 0)
        {
            ApplyDamage();
        }
    }

    /// <summary>
    /// プレイヤーにダメージ用適用する
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
