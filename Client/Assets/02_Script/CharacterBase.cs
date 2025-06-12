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

    #region �X�e�[�^�X�̏���l�֘A
    protected int maxHp;    // �ő�̗�
    #endregion

    #region ���݂̃X�e�[�^�X�֘A
    protected int hp;
    protected int defence;
    protected int power;
    protected float moveSpeed;
    protected float attackSpeed;
    protected float jumpPower;
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
    /// ���x�W��
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
        maxHp = baseHp;
        hp = baseHp;
        defence = baseDefence;
        power = basePower;
        moveSpeed = baseMoveSpeed;
        attackSpeed = baseAttackSpeed;
        jumpPower = baseJumpPower;
    }
}