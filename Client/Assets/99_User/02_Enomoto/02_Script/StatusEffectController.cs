//**************************************************
//  ��Ԉُ���Ǘ�(����E�K�p�E�X�V�Ȃ�)����N���X
//  Author:r-enomoto
//**************************************************
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectController : MonoBehaviour
{
    /// <summary>
    /// ��Ԉُ�̎��
    /// </summary>
    public enum EFFECT_TYPE
    {
        Burn,       // ������
        Freeze,     // ���Ă����
        Shock       // ���d���
    }
    Dictionary<EFFECT_TYPE, float> currentEffects = new Dictionary<EFFECT_TYPE, float>();

    #region �e��Ԉُ�̌��ʎ���
    readonly float maxBurnDuration = 5f;    // ����
    readonly float maxFreezeDuration = 10f; // ���Ă�
    readonly float maxShockDuration = 5f; // ���d
    #endregion

    #region ��Ԉُ�̌��ʂ���������Ԋu
    readonly float burnTickInterval = 0.5f; // ����
    #endregion

    #region �X�e�[�^�X�ɉ������鍇�v����
    public int MoveSpeedOffset { get; private set; }    // �ړ����x
    public int AttackSpeedOffset { get; private set; }  // �U�����x
    #endregion

    #region  �e��Ԉُ��K�p�������Ƃ��̌��ʒl
    Dictionary<EFFECT_TYPE, int> tmpMoveSpeedValue = new Dictionary<EFFECT_TYPE, int>();    // �ړ����x
    Dictionary<EFFECT_TYPE, int> tmpAttackSpeedValue = new Dictionary<EFFECT_TYPE, int>();  // �U�����x
    #endregion

    // �e��Ԉُ��K�p�������Ƃ��̌��ʒl
    Player player;
    EnemyBase enemy;

    private void Start()
    {
        if (this.gameObject.tag == "Player") player = GetComponent<Player>();
        else if (this.gameObject.tag == "EnemyBase") enemy = GetComponent<EnemyBase>();


        Invoke("ApplyStatusEffect", 2f);
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
    /// ��Ԉُ��t�^���鏈��
    /// </summary>
    /// <param name="effectType"></param>
    public void ApplyStatusEffect()
    {
        EFFECT_TYPE effectType = EFFECT_TYPE.Shock;
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
                EFFECT_TYPE.Burn => () => {
                    Debug.Log("������ʓK�p");
                    InvokeRepeating("ActivateBurnEffect", 0, burnTickInterval);
                }
                ,
                EFFECT_TYPE.Freeze => () => {
                    Debug.Log("���Ă����ʓK�p");

                }
                ,
                EFFECT_TYPE.Shock => () => {
                    Debug.Log("���d���ʓK�p");
                    if (this.gameObject.tag == "EnemyBase")
                    {
                        GetComponent<EnemyBase>().ApplyStun(maxShockDuration);
                    }
                }
                ,
                _ => null
            };
            action();
        }
        else
        {
            currentEffects[effectType] = effectDuration;    // ���ʎ��Ԃ����Z�b�g
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
                Debug.Log("������ʉ���");
                CancelInvoke("ActivateBurnEffect");
            }
            ,
            EFFECT_TYPE.Freeze => () => { 
                Debug.Log($"���Ă��E�������ʉ����F*�ړ����x�F{tmpMoveSpeedValue[EFFECT_TYPE.Freeze]}* , *�U�����x�F{tmpAttackSpeedValue[EFFECT_TYPE.Freeze]}*"); 
            }
            ,
            EFFECT_TYPE.Shock => () => { 
                Debug.Log("���d���ʉ���");
            }
            ,
            _ => null
        };
        action();
        currentEffects.Remove(effectType);
    }

    /// <summary>
    /// �������
    /// </summary>
    void ActivateBurnEffect()
    {
        // �ő�HP��5%�̃_���[�W��^����
        Debug.Log("������ʔ�����");
    }
}
