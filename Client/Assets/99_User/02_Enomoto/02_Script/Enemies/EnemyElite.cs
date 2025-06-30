using NUnit.Framework;
using Pixeye.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using static StatusEffectController;

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

    /// <summary>
    /// �G���[�g�̎��
    /// </summary>
    public enum ELITE_TYPE
    {
        None,
        Blaze,      // �u���C�Y�G���[�g
        Frost,      // �t���X�g�G���[�g
        Thunder     // �T���_�[�G���[�g
    }
    ELITE_TYPE eliteType = ELITE_TYPE.None;
    public ELITE_TYPE EliteType { get { return eliteType; } }

    /// <summary>
    /// �t�^�����Ԉُ�̎��
    /// </summary>
    public StatusEffectController.EFFECT_TYPE addStatusEffect { get; private set; }

    /// <summary>
    /// ����������
    /// </summary>
    public void Init(ELITE_TYPE type)
    {
        eliteType = type;

        // HP�E�U���͂�50%�����A�h��́E�ړ����x��25%�����̃X�e�[�^�X�f�[�^�쐬
        CharacterBase charaBase = GetComponent<CharacterBase>();
        CharacterStatusData data = new CharacterStatusData(
            hp: charaBase.BaseHP + Mathf.CeilToInt(charaBase.BaseHP * 0.5f),
            defence: charaBase.BaseDefence + Mathf.CeilToInt(charaBase.BaseDefence * 0.25f),
            power: charaBase.BasePower + Mathf.CeilToInt(charaBase.BasePower * 0.5f),
            moveSpeed: charaBase.BaseMoveSpeed + Mathf.CeilToInt(charaBase.BaseMoveSpeed * 0.25f),
            moveSpeedFactor: charaBase.BaseMoveSpeedFactor + Mathf.CeilToInt(charaBase.BaseMoveSpeedFactor * 0.25f),
            attackSpeedFactor: charaBase.BaseAttackSpeedFactor + Mathf.CeilToInt(charaBase.BaseAttackSpeedFactor * 0.25f)
         );

        Color outlineColor = new Color();
        Action action = type switch
        {
            ELITE_TYPE.Blaze => () =>
            {
                // �J���[�R�[�h FF5D17(�ԐF)
                outlineColor = new Color32(0xFF, 0x5D, 0x17, 0xFF);
            }
            ,
            ELITE_TYPE.Frost => () =>
            {
                // �J���[�R�[�h 7BB8CF(�F)
                outlineColor = new Color32(0x7B, 0xB8, 0xCF, 0xFF);
            }
            ,
            ELITE_TYPE.Thunder => () =>
            {
                // �J���[�R�[�h E492F0(���F)
                outlineColor = new Color32(0xE4, 0x92, 0xF0, 0xFF);

                data.moveSpeed = charaBase.BaseMoveSpeed * 2;
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
            material.SetFloat("_OutlineGlow", 1.5f);
            material.SetFloat("_OutlineWidth", 0.2f);
            material.SetFloat("_OutlineDistortAmount", 0.5f);
        }

        // ��b�X�e�[�^�X&���݂̃X�e�[�^�X���㏑��
        charaBase.OverrideBaseStatus(data, true);
    }

    /// <summary>
    /// �t�^�������Ԉُ��enum���擾����
    /// </summary>
    /// <returns></returns>
    public StatusEffectController.EFFECT_TYPE GetAddStatusEffectEnum()
    {
        return eliteType switch
        {
            ELITE_TYPE.Blaze => StatusEffectController.EFFECT_TYPE.Burn,
            ELITE_TYPE.Frost => StatusEffectController.EFFECT_TYPE.Freeze,
            ELITE_TYPE.Thunder => StatusEffectController.EFFECT_TYPE.Shock,
            _ => StatusEffectController.EFFECT_TYPE.None
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
            ELITE_TYPE.Blaze => () =>
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
