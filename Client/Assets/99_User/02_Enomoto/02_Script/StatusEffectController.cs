//**************************************************
//  ��Ԉُ���Ǘ�(����E�K�p�E�X�V�Ȃ�)����N���X
//  Author:r-enomoto
//**************************************************
using System;
using System.Collections.Generic;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

public class StatusEffectController : MonoBehaviour
{
    Dictionary<EFFECT_TYPE, float> currentEffects = new Dictionary<EFFECT_TYPE, float>();

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
    Dictionary<EFFECT_TYPE, float> tmpMoveSpeedRates = new Dictionary<EFFECT_TYPE, float>();    // �ړ����x
    Dictionary<EFFECT_TYPE, float> tmpMoveSpeedFactorRates = new Dictionary<EFFECT_TYPE, float>();    // �ړ����x(Animator�̌W��)
    Dictionary<EFFECT_TYPE, float> tmpAttackSpeedFactorRates = new Dictionary<EFFECT_TYPE, float>();  // �U�����x(Animator�̌W��)
    #endregion

    #region ��Ԉُ�̃p�[�e�B�N��
    Dictionary<EFFECT_TYPE, GameObject> appliedParticles = new Dictionary<EFFECT_TYPE, GameObject>();   // �K�p���̃p�[�e�B�N��
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
        List<EFFECT_TYPE> keyList = new List<EFFECT_TYPE>(currentEffects.Keys);
        List<EFFECT_TYPE> effectsToRemove = new List<EFFECT_TYPE>();
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
    public List<int> GetAppliedStatusEffects()
    {
        List<int> result = new List<int>();
        foreach (EFFECT_TYPE effectType in currentEffects.Keys)
        {
            result.Add((int)effectType);
        }
        return result;
    }

    /// <summary>
    /// ��Ԉُ��t�^���鏈�� (�t���O�ȗ�)
    /// </summary>
    /// <param name="effectTypes"></param>
    public void ApplyStatusEffect(params EFFECT_TYPE[] effectTypes)
    {
        ApplyStatusEffect(true, effectTypes);
    }

    /// <summary>
    /// ��Ԉُ��t�^���鏈��
    /// </summary>
    /// <param name="effectTypes"></param>
    public void ApplyStatusEffect(bool canUpdateDuration, List<int> effectTypeIds)
    {
        List<EFFECT_TYPE> effectTypes = new List<EFFECT_TYPE>();
        foreach(int id in effectTypeIds)
        {
            foreach (EFFECT_TYPE type in Enum.GetValues(typeof(EFFECT_TYPE)))
            {
                if ((int)type == id)
                {
                    effectTypes.Add(type);
                    break;
                }
            }
        }
        ApplyStatusEffect(canUpdateDuration, effectTypes.ToArray());
    }

    /// <summary>
    /// ��Ԉُ��t�^���鏈��
    /// </summary>
    /// <param name="effectTypes"></param>
    public void ApplyStatusEffect(bool canUpdateDuration, params EFFECT_TYPE[] effectTypes)
    {
        foreach (EFFECT_TYPE effectType in effectTypes)
        {
            float effectDuration = effectType switch
            {
                EFFECT_TYPE.Burn => maxBurnDuration,
                EFFECT_TYPE.Freeze => maxFreezeDuration,
                EFFECT_TYPE.Shock => maxShockDuration,
                _ => 0
            };

            if (!currentEffects.ContainsKey(effectType))
            {
                currentEffects.Add(effectType, effectDuration);

                // ��Ԉُ�̌��ʂ�K�p������
                Action action = effectType switch
                {
                    EFFECT_TYPE.Burn => () =>
                    {
                        InvokeRepeating("ActivateBurnEffect", 0, burnTickInterval);
                    }
                    ,
                    EFFECT_TYPE.Freeze => () =>
                    {
                        // �ړ����x�E�U�����x��20%�������݂̃X�e�[�^�X���猸�Z����
                        float freezeRatio = -freezeEffect;
                        tmpMoveSpeedRates.Add(effectType, freezeRatio);
                        tmpMoveSpeedFactorRates.Add(effectType, freezeRatio);
                        tmpAttackSpeedFactorRates.Add(effectType, freezeRatio);
                        characterBase.ApplyStatusModifierByRate(
                            freezeRatio,
                            STATUS_TYPE.MoveSpeed,
                            STATUS_TYPE.MoveSpeedFactor,
                            STATUS_TYPE.AttackSpeedFactor
                        );
                    }
                    ,
                    EFFECT_TYPE.Shock => () =>
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
                if (effectType != EFFECT_TYPE.Shock && canUpdateDuration)
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
    void ClearStatusEffect(EFFECT_TYPE effectType)
    {
        Action action = effectType switch
        {
            EFFECT_TYPE.Burn => () => { 
                CancelInvoke("ActivateBurnEffect");
            }
            ,
            EFFECT_TYPE.Freeze => () => {
                // �ړ����x�E�U�����x�̌��Z����Ă������̒l���X�e�[�^�X�ɉ��Z����
                float freezeRatio = freezeEffect;
                characterBase.ApplyStatusModifierByRate(
                        freezeRatio,
                        STATUS_TYPE.MoveSpeed,
                        STATUS_TYPE.MoveSpeedFactor,
                        STATUS_TYPE.AttackSpeedFactor
                    );
                tmpMoveSpeedRates.Remove(effectType);
                tmpMoveSpeedFactorRates.Remove(effectType);
                tmpAttackSpeedFactorRates.Remove(effectType);
            }
            ,
            EFFECT_TYPE.Shock => () => {
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
    void ApplyStatusEffectParticle(EFFECT_TYPE type)
    {
        GameObject psObj = null;
        ParticleSystem ps;

        // �e�p�[�e�B�N���̏ڍאݒ�
        Action action = type switch
        {
            EFFECT_TYPE.Burn => () =>
            {
                psObj = Instantiate(burnPS, this.transform);
                psObj.transform.position = capsule2D.bounds.center;
                ps = psObj.GetComponent<ParticleSystem>();
                ParticleHelper.MatchRadiusToSpriteWidth(capsule2D, ps);
            }
            ,
            EFFECT_TYPE.Freeze => () =>
            {
                psObj = Instantiate(freezePS, this.transform);
                psObj.transform.position = capsule2D.bounds.center;
                ps = psObj.GetComponent<ParticleSystem>();
                ParticleHelper.MatchRadiusToSpriteWidth(capsule2D, ps);
            }
            ,
            EFFECT_TYPE.Shock => () =>
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
    void DestroyStatusEffectParticle(EFFECT_TYPE type)
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
        else if (this.gameObject.tag == "Enemy") enemyBase.ApplyDamage(dmgValue);
    }
}
