//**************************************************
//  �L�����N�^�[�̒��ۃN���X
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

abstract public class CharacterBase : MonoBehaviour
{
    #region �����X�e�[�^�X�֘A
    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected int baseHp = 10;   // HP

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected int baseDefense = 10;  // �h���

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected int basePower = 10;    // �U����

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected float baseJumpPower = 10;  // ������

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected float baseMoveSpeed = 1f;   // �ړ����x

    [Foldout("�X�e�[�^�X")]
    protected float baseMoveSpeedFactor = 1f;    // �ړ����x(Animator�̌W��)

    [Foldout("�X�e�[�^�X")]
    protected float baseAttackSpeedFactor = 1f;    // �U�����x(Animator�̌W��)
    #endregion

    #region �����X�e�[�^�X�O���Q�Ɨp�v���p�e�B
    /// <summary>
    /// �̗�
    /// </summary>
    public int BaseHP { get { return baseHp; } }

    /// <summary>
    /// �h���
    /// </summary>
    public int BaseDefense { get { return baseDefense; } }

    /// <summary>
    /// �U����
    /// </summary>
    public int BasePower { get { return basePower; } }

    /// <summary>
    /// ������
    /// </summary>
    public float BaseJumpPower { get { return baseJumpPower; } }

    /// <summary>
    /// �ړ����x
    /// </summary>
    public float BaseMoveSpeed { get { return baseMoveSpeed; } }

    /// <summary>
    /// �ړ����x(Animator�̌W��)
    /// </summary>
    public float BaseMoveSpeedFactor {  get { return baseMoveSpeedFactor; } }

    /// <summary>
    /// �U�����x(Animator�̌W��)
    /// </summary>
    public float BaseAttackSpeedFactor { get { return baseAttackSpeedFactor; } }
    #endregion

    #region ���݂̃X�e�[�^�X�̏���l�֘A(�}�C�i�X�̒l�����e���Ȃ����̂̂�)
    protected int maxHp;
    protected int maxPower;
    protected float maxJumpPower;
    protected float maxMoveSpeed;
    protected float maxMoveSpeedFactor;
    protected float maxAttackSpeedFactor;
    #endregion

    #region ���݂̃X�e�[�^�X�̏���l�O���Q�Ɨp�v���p�e�B
    /// <summary>
    /// �ő�̗�
    /// </summary>
    public int MaxHP { get { return maxHp; } }

    /// <summary>
    /// �ő�U����
    /// </summary>
    public int MaxPower { get { return maxPower; } }

    /// <summary>
    /// �ő咵����
    /// </summary>
    public float MaxJumpPower { get { return maxJumpPower; } }

    /// <summary>
    /// �ő�ړ����x
    /// </summary>
    public float MaxMoveSpeed { get { return maxMoveSpeed; } }

    /// <summary>
    /// �ő�ړ����x�W��(Animator�p)
    /// </summary>
    public float MaxMoveSpeedFactor { get { return maxMoveSpeedFactor; } }

    /// <summary>
    /// �ő�U�����x�W��(Animator�p)
    /// </summary>
    public float MaxAttackSpeedFactor { get { return maxAttackSpeedFactor; } }
    #endregion

    #region ���݂̃X�e�[�^�X�֘A
    // �C���X�y�N�^�[��Ō��邽�߂�public
    // ���public������
    [Foldout("[Debug�p] ���݂̃X�e�[�^�X")]
    public int hp;
    [Foldout("[Debug�p] ���݂̃X�e�[�^�X")]
    public int defense;
    [Foldout("[Debug�p] ���݂̃X�e�[�^�X")]
    public int power;
    [Foldout("[Debug�p] ���݂̃X�e�[�^�X")]
    public float jumpPower;
    [Foldout("[Debug�p] ���݂̃X�e�[�^�X")]
    public float moveSpeed;
    [Foldout("[Debug�p] ���݂̃X�e�[�^�X")]
    public float moveSpeedFactor;
    [Foldout("[Debug�p] ���݂̃X�e�[�^�X")]
    public float attackSpeedFactor;

    protected StatusEffectController effectController;
    #endregion

    #region �X�e�[�^�X�O���Q�Ɨp�v���p�e�B
    /// <summary>
    /// �̗�
    /// </summary>
    public int HP { get { return hp; } }

    /// <summary>
    /// �h���
    /// </summary>
    public int Defense { get { return defense; } set { defense = value; } }

    /// <summary>
    /// �U����
    /// </summary>
    public int Power { get { return power; } set { power = value; } }

    /// <summary>
    /// ������
    /// </summary>
    public float JumpPower { get { return jumpPower; } set { jumpPower = value; } }

    /// <summary>
    /// �ړ����x
    /// </summary>
    public float MoveSpeed { get { return moveSpeed; } set { moveSpeed = value; } }

    /// <summary>
    /// �ړ����x(Animator�̌W��)
    /// </summary>
    public float MoveSpeedFactor { get { return moveSpeedFactor; }  set { moveSpeedFactor = value; } }

    /// <summary>
    /// �U�����x(Animator�̌W��)
    /// </summary>
    public float AttackSpeedFactor { get { return attackSpeedFactor; } set { attackSpeedFactor = value; } }

    /// <summary>
    /// ��Ԉُ�Ǘ��̃R���|�[�l���g
    /// </summary>
    public StatusEffectController EffectController { get { return effectController; } }
    #endregion

    #region �e�N�X�`���E�A�j���[�V����
    [Foldout("�e�N�X�`���E�A�j���[�V����")]
    [SerializeField] 
    protected Animator animator;
    #endregion

    public enum STATUS_TYPE
    {
        All,
        HP,
        Defense,
        Power,
        JumpPower,
        MoveSpeed,
        MoveSpeedFactor,
        AttackSpeedFactor
    }

    protected virtual void Awake()
    {
        ApplyStatusModifierByRate(1f, true, STATUS_TYPE.All);
        if (!animator) animator = GetComponent<Animator>();
        effectController = GetComponent<StatusEffectController>();
    }

    /// <summary>
    /// �ꊇ�Ō��݂̃X�e�[�^�X�ɉ������鏈��
    /// </summary>
    public void ApplyStatusBonus(CharacterStatusData addStatusData)
    {
        var applyHp = hp + addStatusData.hp;
        hp = applyHp < maxHp ? applyHp : maxHp;

        defense += addStatusData.defence;

        var applyPower = power + addStatusData.power;
        power = applyPower < maxPower ? applyPower : maxPower;

        var applyMoveSpeed = moveSpeed + addStatusData.moveSpeed;
        moveSpeed = applyMoveSpeed < maxMoveSpeed ? applyMoveSpeed : maxMoveSpeed;

        var applyJumpPower = jumpPower + addStatusData.jumpPower;
        jumpPower = applyJumpPower < maxJumpPower ? applyJumpPower : maxJumpPower;

        var applyMoveSpeedFactor = moveSpeedFactor + addStatusData.moveSpeedFactor;
        moveSpeedFactor = applyMoveSpeedFactor < maxMoveSpeedFactor ? applyMoveSpeedFactor : maxMoveSpeedFactor;

        var applyAttackSpeedFactor = attackSpeedFactor + addStatusData.attackSpeedFactor;
        attackSpeedFactor = applyAttackSpeedFactor < maxAttackSpeedFactor ? applyAttackSpeedFactor : maxAttackSpeedFactor;

        OverrideAnimaterParam();
    }

    /// <summary>
    /// �������w�肵�āA�X�e�[�^�X�ɕω���K�p������
    /// </summary>
    /// <param name="rate"></param>
    /// <param name="types"></param>
    public void ApplyStatusModifierByRate(float rate, params STATUS_TYPE[] types)
    {
        ApplyStatusModifierByRate(rate, false, types);
    }

    /// <summary>
    /// �������w�肵�āA�X�e�[�^�X�ɕω���K�p������
    /// </summary>
    /// <param name="rate"></param>
    /// <param name="canResetHpToMax"></param>
    /// <param name="types"></param>
    public void ApplyStatusModifierByRate(float rate, bool canResetHpToMax, params STATUS_TYPE[] types)
    {
        List<STATUS_TYPE> statusList = new List<STATUS_TYPE>(types);
        if (statusList.Contains(STATUS_TYPE.All)) 
        {
            statusList = new List<STATUS_TYPE> {
                STATUS_TYPE.HP, STATUS_TYPE.Defense, STATUS_TYPE.Power, STATUS_TYPE.JumpPower,
                STATUS_TYPE.MoveSpeed, STATUS_TYPE.MoveSpeedFactor, STATUS_TYPE.AttackSpeedFactor
            };
        }

        foreach (STATUS_TYPE type in statusList)
        {
            switch (type)
            {
                case STATUS_TYPE.HP:
                    maxHp += (int)(baseHp * rate);
                    int applyHp = maxHp < 0 ? 0 : maxHp;
                    if (canResetHpToMax) hp = maxHp;
                    else hp = applyHp < hp ? applyHp : hp;
                    break;
                case STATUS_TYPE.Defense:
                    defense += (int)(baseDefense * rate);
                    break;
                case STATUS_TYPE.Power:
                    maxPower += (int)(basePower * rate);
                    power = maxPower < 0 ? 0 : maxPower;
                    break;
                case STATUS_TYPE.JumpPower:
                    maxJumpPower += baseJumpPower * rate;
                    jumpPower = maxJumpPower < 0 ? 0 : maxJumpPower;
                    break;
                case STATUS_TYPE.MoveSpeed:
                    maxMoveSpeed += baseMoveSpeed * rate;
                    moveSpeed = maxMoveSpeed < 0 ? 0 : maxMoveSpeed;
                    break;
                case STATUS_TYPE.MoveSpeedFactor:
                    maxMoveSpeedFactor += baseMoveSpeedFactor * rate;
                    moveSpeedFactor = maxMoveSpeedFactor < 0 ? 0 : maxMoveSpeedFactor;
                    break;
                case STATUS_TYPE.AttackSpeedFactor:
                    maxAttackSpeedFactor += baseAttackSpeedFactor * rate;
                    attackSpeedFactor = maxAttackSpeedFactor < 0 ? 0 : maxAttackSpeedFactor;
                    break;
            }
        }
        OverrideAnimaterParam();
    }

    /// <summary>
    /// ���݂̃X�e�[�^�X���ő�l�̑傫���ɖ߂�
    /// </summary>
    public void RecoverAllStats()
    {
        hp = maxHp;
        power = maxPower;
        moveSpeed = maxMoveSpeed;
        jumpPower = maxJumpPower;
        moveSpeedFactor = maxMoveSpeedFactor;
        attackSpeedFactor = maxAttackSpeedFactor;
        OverrideAnimaterParam();
    }

    #region �A�j���[�V�����֘A

    /// <summary>
    /// �A�j���[�^�[�̃p�����[�^�[���㏑������
    /// </summary>
    public void OverrideAnimaterParam()
    {
        if (animator)
        {
            animator.SetFloat("attack_speed", attackSpeedFactor);
            animator.SetFloat("move_speed", moveSpeedFactor);
        }
    }

    /// <summary>
    /// �A�j���[�V�����ݒ菈��
    /// </summary>
    /// <param name="id"></param>
    public virtual void SetAnimId(int id)
    {
        if (animator == null) return;
        animator.SetInteger("animation_id", id);
    }

    /// <summary>
    /// �A�j���[�V����ID�擾����
    /// </summary>
    /// <returns></returns>
    public int GetAnimId()
    {
        return animator != null ? animator.GetInteger("animation_id") : 0;
    }

    #endregion
}