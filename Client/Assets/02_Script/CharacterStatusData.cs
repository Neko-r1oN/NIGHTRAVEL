//**************************************************
//  �L�����N�^�[�̃X�e�[�^�X�̃f�[�^�N���X
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using UnityEngine;

public class CharacterStatusData
{
    public int hp;              // HP
    public int defence;         // �h���
    public int power;           // �U����
    public float jumpPower;     // ������
    public float moveSpeed;     // �ړ����x
    public float moveSpeedFactor;     // �ړ����x(Animator�̌W��)
    public float attackSpeedFactor;   // �U�����x(Animator�̌W��)

    public CharacterStatusData(int hp = 0, int defence = 0, int power = 0, float moveSpeed = 0, float moveSpeedFactor = 0, float attackSpeedFactor = 0, float jumpPower = 0)
    {
        this.hp = hp;
        this.defence = defence;
        this.power = power;
        this.jumpPower = jumpPower;
        this.moveSpeed = moveSpeed;
        this.moveSpeedFactor = moveSpeedFactor;
        this.attackSpeedFactor = attackSpeedFactor;
    }
}
