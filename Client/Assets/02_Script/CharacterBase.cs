//**************************************************
//  �L�����N�^�[�̒��ۃN���X
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CharacterBase : MonoBehaviour
{
    #region �����X�e�[�^�X�֘A
    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected int baseHp = 10;   // HP

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected int baseDefence = 10;  // �h���

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
    public int BaseDefence { get { return baseDefence; } }

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

    #region �X�e�[�^�X�̏���l�֘A
    protected int maxHp;    // �ő�̗�
    #endregion

    #region ���݂̃X�e�[�^�X�֘A
    // �C���X�y�N�^�[��Ō��邽�߂�public
    // ���public������
    public int hp;
    public int defence;
    public int power;
    public float jumpPower;
    public float moveSpeed;
    public float moveSpeedFactor;
    public float attackSpeedFactor;

    protected StatusEffectController effectController;
    #endregion

    #region �X�e�[�^�X�O���Q�Ɨp�v���p�e�B
    /// <summary>
    /// �ő�̗�
    /// </summary>
    public int MaxHP { get { return maxHp; } }

    /// <summary>
    /// �̗�
    /// </summary>
    public int HP { get { return hp; } }

    /// <summary>
    /// �h���
    /// </summary>
    public int Defence { get { return defence; } set { defence = value; } }

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

    #region �R���|�[�l���g
    [Foldout("�R���|�[�l���g")]
    [SerializeField] 
    protected Animator animator;
    #endregion

    protected virtual void Awake()
    {
        RecoverAllStats();
        if (!animator) animator = GetComponent<Animator>();
        effectController = GetComponent<StatusEffectController>();
    }

    /// <summary>
    /// �ꊇ�ŃX�e�[�^�X�ɉ������鏈��
    /// </summary>
    public void ApplyStatusBonus(CharacterStatusData addStatusData)
    {
        maxHp += addStatusData.hp;
        hp += addStatusData.hp;
        defence += addStatusData.defence;
        power += addStatusData.power;
        moveSpeed += addStatusData.moveSpeed;
        moveSpeedFactor += addStatusData.moveSpeedFactor;
        attackSpeedFactor += addStatusData.attackSpeedFactor;
        jumpPower += addStatusData.jumpPower;
        OverrideAnimaterParam();
    }

    /// <summary>
    /// ��b�X�e�[�^�X���㏑�����鏈��
    /// </summary>
    /// <param name="statusData"></param>
    /// <param name="shouldResetToBaseStats">���݂̃X�e�[�^�X����b�X�e�[�^�X�Ƀ��Z�b�g���邩�ǂ���</param>
    public void OverrideBaseStatus(CharacterStatusData statusData, bool shouldResetToBaseStats = false)
    {
        baseHp = statusData.hp;
        baseDefence = statusData.defence;
        basePower = statusData.power;
        baseMoveSpeed = statusData.moveSpeed;
        baseMoveSpeedFactor = statusData.moveSpeedFactor;
        baseAttackSpeedFactor = statusData.attackSpeedFactor;
        baseJumpPower = statusData.jumpPower;
        OverrideAnimaterParam();

        if (shouldResetToBaseStats)
        {
            RecoverAllStats();
        }
    }

    /// <summary>
    /// ���݂̃X�e�[�^�X��S�Č��ɖ߂�
    /// </summary>
    public void RecoverAllStats()
    {
        maxHp = baseHp;
        hp = baseHp;
        defence = baseDefence;
        power = basePower;
        moveSpeed = baseMoveSpeed;
        jumpPower = baseJumpPower;
        moveSpeedFactor = baseMoveSpeedFactor;
        attackSpeedFactor = baseAttackSpeedFactor;
        OverrideAnimaterParam();
    }

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
}