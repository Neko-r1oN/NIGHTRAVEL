//**************************************************
//  ��Ԉُ���Ǘ�(����E�K�p�E�X�V�Ȃ�)����N���X
//  Author:r-enomoto
//**************************************************
using System;
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class DebuffController : MonoBehaviour
{
    Dictionary<DEBUFF_TYPE, float> currentEffects = new Dictionary<DEBUFF_TYPE, float>();

    #region �e��Ԉُ�̌��ʎ���
    readonly float maxBurnDuration = 6f;    // ����
    readonly float maxFreezeDuration = 5f; // ���Ă�
    readonly float maxShockDuration = 1f; // ���d
    #endregion

    #region �e��Ԉُ�̌��ʒl
    readonly float burnEffect = 0.05f;
    readonly float freezeEffect = 0.2f;
    readonly float shockEffect = 0.5f;
    #endregion

    #region ��Ԉُ�̌��ʂ���������Ԋu
    readonly float burnTickInterval = 0.5f; // ����
    #endregion

    #region  �e��Ԉُ��K�p���������ʊ���
    Dictionary<DEBUFF_TYPE, float> tmpMoveSpeedRates = new Dictionary<DEBUFF_TYPE, float>();    // �ړ����x
    Dictionary<DEBUFF_TYPE, float> tmpMoveSpeedFactorRates = new Dictionary<DEBUFF_TYPE, float>();    // �ړ����x(Animator�̌W��)
    Dictionary<DEBUFF_TYPE, float> tmpAttackSpeedFactorRates = new Dictionary<DEBUFF_TYPE, float>();  // �U�����x(Animator�̌W��)
    #endregion

    #region ��Ԉُ�̃p�[�e�B�N��
    Dictionary<DEBUFF_TYPE, GameObject> appliedParticles = new Dictionary<DEBUFF_TYPE, GameObject>();   // �K�p���̃p�[�e�B�N��
    [SerializeField] GameObject burnPS;
    [SerializeField] GameObject freezePS;
    [SerializeField] GameObject shockPS;
    #endregion

    CapsuleCollider2D capsule2D;
    CharacterBase characterBase;
    PlayerBase playerBase;
    EnemyBase enemyBase;

    private void Start()
    {
        if (this.gameObject.tag == "Player") playerBase = GetComponent<PlayerBase>();
        else if (this.gameObject.tag == "Enemy") enemyBase = GetComponent<EnemyBase>();
        capsule2D = GetComponent<CapsuleCollider2D>();
        characterBase = GetComponent<CharacterBase>();
    }

    private void FixedUpdate()
    {
        List<DEBUFF_TYPE> keyList = new List<DEBUFF_TYPE>(currentEffects.Keys);
        List<DEBUFF_TYPE> effectsToRemove = new List<DEBUFF_TYPE>();
        foreach (var key in keyList)
        {
            currentEffects[key] -= Time.fixedDeltaTime;
            if (currentEffects[key] <= 0)
            {
                effectsToRemove.Add(key);
            }
        }

        // �I���������ʂ��폜
        foreach (var key in effectsToRemove)
        {
            ClearStatusEffect(key);
        }
    }

    /// <summary>
    /// �K�p�ς݂̏�Ԉُ���擾����
    /// </summary>
    /// <returns></returns>
    public List<DEBUFF_TYPE> GetAppliedStatusEffects()
    {
        List<DEBUFF_TYPE> result = new List<DEBUFF_TYPE>(currentEffects.Keys);
        return result;
    }

    /// <summary>
    /// ��Ԉُ��t�^���鏈�� (�t���O�ȗ�)
    /// </summary>
    /// <param name="effectTypes"></param>
    public void ApplyStatusEffect(params DEBUFF_TYPE[] effectTypes)
    {
        ApplyStatusEffect(true, effectTypes);
    }

    /// <summary>
    /// ��Ԉُ��t�^���鏈��
    /// </summary>
    /// <param name="effectTypes"></param>
    public void ApplyStatusEffect(bool canUpdateDuration, params DEBUFF_TYPE[] effectTypes)
    {
        foreach (DEBUFF_TYPE effectType in effectTypes)
        {
            float effectDuration = effectType switch
            {
                DEBUFF_TYPE.Burn => maxBurnDuration,
                DEBUFF_TYPE.Freeze => maxFreezeDuration,
                DEBUFF_TYPE.Shock => maxShockDuration,
                _ => 0
            };

            if (!currentEffects.ContainsKey(effectType))
            {
                currentEffects.Add(effectType, effectDuration);

                // ��Ԉُ�̌��ʂ�K�p������
                Action action = effectType switch
                {
                    DEBUFF_TYPE.Burn => () =>
                    {
                        InvokeRepeating("ActivateBurnEffect", 0, burnTickInterval);
                    }
                    ,
                    DEBUFF_TYPE.Freeze => () =>
                    {
                        // �ړ����x�E�U�����x��20%�������݂̃X�e�[�^�X���猸�Z����
                        float freezeRatio = -freezeEffect;
                        tmpMoveSpeedRates.Add(effectType, freezeRatio);
                        tmpMoveSpeedFactorRates.Add(effectType, freezeRatio);
                        tmpAttackSpeedFactorRates.Add(effectType, freezeRatio);
                        characterBase.ApplyMaxStatusModifierByRate(
                            freezeRatio,
                            STATUS_TYPE.MoveSpeed,
                            STATUS_TYPE.AttackSpeedFactor
                        );
                    }
                    ,
                    DEBUFF_TYPE.Shock => () =>
                    {
                        if (this.gameObject.tag == "Player") StartCoroutine(playerBase.AbnormalityStun(shockEffect));
                        else if (this.gameObject.tag == "Enemy") enemyBase.ApplyStun(shockEffect);
                    }
                    ,
                    _ => () => { }
                };
                action();
                ApplyStatusEffectParticle(effectType);
            }
            else
            {
                if (effectType != DEBUFF_TYPE.Shock && canUpdateDuration)
                {
                    currentEffects[effectType] = effectDuration;    // ���ʎ��Ԃ����Z�b�g
                }
            }
        }
    }

    /// <summary>
    /// ��Ԉُ����
    /// </summary>
    /// <param name="effectType"></param>
    void ClearStatusEffect(DEBUFF_TYPE effectType)
    {
        Action action = effectType switch
        {
            DEBUFF_TYPE.Burn => () => { 
                CancelInvoke("ActivateBurnEffect");
            }
            ,
            DEBUFF_TYPE.Freeze => () => {
                // �ړ����x�E�U�����x�̌��Z����Ă������̒l���X�e�[�^�X�ɉ��Z����
                float freezeRatio = freezeEffect;
                characterBase.ApplyMaxStatusModifierByRate(
                        freezeRatio,
                        STATUS_TYPE.MoveSpeed,
                        STATUS_TYPE.AttackSpeedFactor
                    );
                tmpMoveSpeedRates.Remove(effectType);
                tmpMoveSpeedFactorRates.Remove(effectType);
                tmpAttackSpeedFactorRates.Remove(effectType);
            }
            ,
            DEBUFF_TYPE.Shock => () => {
            }
            ,
            _ => null
        };
        action();
        currentEffects.Remove(effectType);
        DestroyStatusEffectParticle(effectType);
    }

    /// <summary>
    /// ��Ԉُ�̃p�[�e�B�N����K�p������
    /// </summary>
    /// <param name="type"></param>
    /// <param name="particlePrefab"></param>
    void ApplyStatusEffectParticle(DEBUFF_TYPE type)
    {
        GameObject psObj = null;
        ParticleSystem ps;

        // �e�p�[�e�B�N���̏ڍאݒ�
        Action action = type switch
        {
            DEBUFF_TYPE.Burn => () =>
            {
                psObj = Instantiate(burnPS, this.transform);
                psObj.transform.position = capsule2D.bounds.center;
                ps = psObj.GetComponent<ParticleSystem>();
                ParticleHelper.MatchRadiusToSpriteWidth(capsule2D, ps);
            }
            ,
            DEBUFF_TYPE.Freeze => () =>
            {
                psObj = Instantiate(freezePS, this.transform);
                psObj.transform.position = capsule2D.bounds.center;
                ps = psObj.GetComponent<ParticleSystem>();
                ParticleHelper.MatchRadiusToSpriteWidth(capsule2D, ps);
            }
            ,
            DEBUFF_TYPE.Shock => () =>
            {
                psObj = Instantiate(shockPS, this.transform);
                psObj.transform.position = capsule2D.bounds.center;
                ps = psObj.GetComponent<ParticleSystem>();
                float baseRateOverTime = 15f;
                ParticleHelper.MatchRadiusToSpriteWidth(capsule2D, ps);
                ParticleHelper.AdjustRateOverTimeBySpriteWidth(capsule2D, ps, baseRateOverTime);
            }
            ,
            _ => () => { }
        };
        action();
        if (psObj != null) appliedParticles.Add(type, psObj);
    }

    /// <summary>
    /// �K�p����Ă�����Ԉُ�̃p�[�e�B�N����j������
    /// </summary>
    /// <param name="type"></param>
    void DestroyStatusEffectParticle(DEBUFF_TYPE type)
    {
        if (appliedParticles.ContainsKey(type))
        {
            GameObject psObj = appliedParticles[type];
            appliedParticles.Remove(type);
            Destroy(psObj);
        }
    }

    /// <summary>
    /// �������
    /// </summary>
    void ActivateBurnEffect()
    {
        // �ő�HP��5%�̃_���[�W��^����
        int dmgValue = Mathf.CeilToInt((float)characterBase.MaxHP * burnEffect);
        if (this.gameObject.tag == "Player") playerBase.ApplyDamage(dmgValue);
        else if (this.gameObject.tag == "Enemy") enemyBase.ApplyDamageRequest(dmgValue);
    }
}
