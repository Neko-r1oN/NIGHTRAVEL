//**************************************************
//  キャラクターのステータスのデータクラス
//  Author:r-enomoto
//**************************************************
using Pixeye.Unity;
using UnityEngine;

public class CharacterStatusData
{
    public int hp = 10;   // HP
    public int defence = 10;  // 防御力
    public int power = 10;    // 攻撃力
    public float moveSpeed = 10f;   // 移動速度
    public float attackSpeed = 10;    // 攻撃速度
    public float jumpPower = 10;  // 跳躍力

    public CharacterStatusData(int hp = 0, int defence = 0, int power = 0, float moveSpeed = 0, float attackSpeed = 0, float jumpPower = 0)
    {
        this.hp = hp;
        this.defence = defence;
        this.power = power;
        this.moveSpeed = moveSpeed;
        this.attackSpeed = attackSpeed;
        this.jumpPower = jumpPower;
    }
}
