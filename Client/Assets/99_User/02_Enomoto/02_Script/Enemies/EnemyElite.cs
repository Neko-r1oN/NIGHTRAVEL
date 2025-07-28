using NUnit.Framework;
using Pixeye.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using static StatusEffectController;
using Shared.Interfaces.StreamingHubs;

public class EnemyElite : MonoBehaviour
{
    #region �v���t�@�u
    [SerializeField]
    GameObject blazeTrap;
    #endregion

    #region ������ʂ���������Ԋu
    readonly float blazeTickInterval = 0.2f; // �u���C�Y�G���[�g
    #endregion

    float timer;

    EnumManager.ENEMY_ELITE_TYPE eliteType = EnumManager.ENEMY_ELITE_TYPE.None;
    public EnumManager.ENEMY_ELITE_TYPE EliteType { get { return eliteType; } }

    /// <summary>
    /// �t�^�����Ԉُ�̎��
    /// </summary>
    public StatusEffectController.EFFECT_TYPE addStatusEffect { get; private set; }

    /// <summary>
    /// ����������
    /// </summary>
    public void Init(EnumManager.ENEMY_ELITE_TYPE type)
    {
        eliteType = type;

        // HP�E�U���͂�50%�����A�h��́E�ړ����x�E�ړ����x�W����25%�����ɂ���
        CharacterBase charaBase = GetComponent<CharacterBase>();
        charaBase.ApplyStatusModifierByRate(0.5f, true, CharacterBase.STATUS_TYPE.HP, CharacterBase.STATUS_TYPE.Power);
        charaBase.ApplyStatusModifierByRate(0.25f, CharacterBase.STATUS_TYPE.Defense, CharacterBase.STATUS_TYPE.MoveSpeed, CharacterBase.STATUS_TYPE.MoveSpeedFactor);

        Color outlineColor = new Color();
        Action action = type switch
        {
            EnumManager.ENEMY_ELITE_TYPE.Blaze => () =>
            {
                // �J���[�R�[�h FF0000(�ԐF)
                outlineColor = new Color32(0xFF, 0x00, 0x00, 0xFF);
            }
            ,
            EnumManager.ENEMY_ELITE_TYPE.Frost => () =>
            {
                // �J���[�R�[�h 00FFEA(���F)
                outlineColor = new Color32(0x00, 0xFF, 0xEF, 0xFF);
            }
            ,
            EnumManager.ENEMY_ELITE_TYPE.Thunder => () =>
            {
                // �J���[�R�[�h E100FF(���F)
                outlineColor = new Color32(0xE1, 0x00, 0xFF, 0xFF);

                // Thunder�݈̂ړ����x�E�ړ����x�W����2�{�ɂȂ�悤�ɂ���
                charaBase.ApplyStatusModifierByRate(0.75f, CharacterBase.STATUS_TYPE.MoveSpeed, CharacterBase.STATUS_TYPE.MoveSpeedFactor);
            }
            ,
            _ => () => { }
        };
        action();

        // ���j�[�N�̂̃}�e���A���̃v���p�e�B�ݒ�
        foreach (SpriteRenderer spriteRenderer in GetComponent<EnemyBase>().SpriteRenderers)
        {
            Material material = spriteRenderer.material;
            material.SetColor("_OutlineColor", outlineColor);
            material.SetFloat("_OutlineAlpha", 1f);
            material.SetFloat("_OutlineGlow", 50f);
            material.SetFloat("_OutlineWidth", 0.2f);
            material.SetFloat("_OutlineDistortAmount", 0.5f);
        }

    }

    /// <summary>
    /// �t�^�������Ԉُ��enum���擾����
    /// </summary>
    /// <returns></returns>
    public StatusEffectController.EFFECT_TYPE? GetAddStatusEffectEnum()
    {
        return eliteType switch
        {
            EnumManager.ENEMY_ELITE_TYPE.Blaze => StatusEffectController.EFFECT_TYPE.Burn,
            EnumManager.ENEMY_ELITE_TYPE.Frost => StatusEffectController.EFFECT_TYPE.Freeze,
            EnumManager.ENEMY_ELITE_TYPE.Thunder => StatusEffectController.EFFECT_TYPE.Shock,
            _ => null
        };
    }

    /// <summary>
    /// �u���C�Y�G���[�g�̓�����ʏ���
    /// </summary>
    public void ActivateBlazeEffect()
    {
        GameObject trap = Instantiate(blazeTrap, transform.position, Quaternion.identity);
        trap.GetComponent<BlazeTrap>().InitializeBlazeTrap(gameObject);
    }

    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        Action action = eliteType switch
        {
            EnumManager.ENEMY_ELITE_TYPE.Blaze => () =>
            {
                if (timer > blazeTickInterval)
                {
                    timer = 0;
                    ActivateBlazeEffect();
                }
            }
            ,
            _ => () => { }
        };
        action();
    }
}
