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
    protected float baseMoveSpeed = 10f;   // �ړ����x

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected float baseAttackSpeed = 10;    // �U�����x

    [Foldout("�X�e�[�^�X")]
    [SerializeField]
    protected float baseJumpPower = 10;  // ������
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
    /// �ړ����x
    /// </summary>
    public float BaseMoveSpeed { get { return baseMoveSpeed; } }

    /// <summary>
    /// �U�����x
    /// </summary>
    public float BaseAttackSpeed { get { return baseAttackSpeed; } }

    /// <summary>
    /// ������
    /// </summary>
    public float BaseJumpPower { get { return baseJumpPower; } }
    #endregion

    #region �X�e�[�^�X�̏���l�֘A
    protected int maxHp;    // �ő�̗�
    #endregion

    #region ���݂̃X�e�[�^�X�֘A
    public int hp;
    public int defence;
    public int power;
    public float moveSpeed;
    public float attackSpeed;
    public float jumpPower;
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
    /// �ړ����x
    /// </summary>
    public float MoveSpeed { get { return moveSpeed; } set { moveSpeed = value; } }

    /// <summary>
    /// �U�����x
    /// </summary>
    public float AttackSpeed { get { return attackSpeed; } set { attackSpeed = value; } }

    /// <summary>
    /// ������
    /// </summary>
    public float JumpPower { get { return jumpPower; } set { jumpPower = value; } }
    #endregion

    protected virtual void Start()
    {
        RecoverAllStats();
    }

    //protected virtual void Awake()
    //{
    //    RecoverAllStats();
    //}

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
        attackSpeed += addStatusData.attackSpeed;
        jumpPower += addStatusData.jumpPower;
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
        baseAttackSpeed = statusData.attackSpeed;
        baseJumpPower = statusData.jumpPower;

        if (shouldResetToBaseStats) RecoverAllStats();
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
        attackSpeed = baseAttackSpeed;
        jumpPower = baseJumpPower;
    }
}