using System;
using UnityEngine;
using Shared.Interfaces.StreamingHubs;
using static Shared.Interfaces.StreamingHubs.EnumManager;

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
    public DEBUFF_TYPE addStatusEffect { get; private set; }

    /// <summary>
    /// ����������
    /// </summary>
    public void Init(EnumManager.ENEMY_ELITE_TYPE type)
    {
        eliteType = type;

        // HP�E�U���͂�50%�����A�h��́E�ړ����x�E�ړ����x�W����25%�����ɂ���
        CharacterBase charaBase = GetComponent<CharacterBase>();
        charaBase.ApplyMaxStatusModifierByRate(0.5f, STATUS_TYPE.HP, STATUS_TYPE.Power);
        charaBase.ApplyMaxStatusModifierByRate(0.25f, STATUS_TYPE.Defense, STATUS_TYPE.MoveSpeed);

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
                charaBase.ApplyMaxStatusModifierByRate(0.75f, STATUS_TYPE.MoveSpeed);
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
    public DEBUFF_TYPE? GetAddStatusEffectEnum()
    {
        return eliteType switch
        {
            ENEMY_ELITE_TYPE.Blaze => DEBUFF_TYPE.Burn,
            ENEMY_ELITE_TYPE.Frost => DEBUFF_TYPE.Freeze,
            ENEMY_ELITE_TYPE.Thunder => DEBUFF_TYPE.Shock,
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
