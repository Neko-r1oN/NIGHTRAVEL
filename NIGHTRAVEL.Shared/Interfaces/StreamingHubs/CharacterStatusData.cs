//=============================
// キャラクターのデータクラス
// Author:Enomoto Data:08/01
//=============================
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Interfaces.StreamingHubs
{
    public class CharacterStatusData
    {
        public int hp;              // HP
        public int defence;         // 防御力
        public int power;           // 攻撃力
        public float jumpPower;     // 跳躍力
        public float moveSpeed;     // 移動速度
        public float moveSpeedFactor;     // 移動速度(Animatorの係数)
        public float attackSpeedFactor;   // 攻撃速度(Animatorの係数)

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
}
