//=============================
// キャラクターのデータクラス
// Author:Enomoto Data:08/01
//=============================
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class CharacterStatusData
    {
        [Key(0)]
        public int hp;              // HP
        [Key(1)]
        public int defence;         // 防御力
        [Key(2)]
        public int power;           // 攻撃力
        [Key(3)]
        public float jumpPower;     // 跳躍力
        [Key(4)]
        public float moveSpeed;     // 移動速度
        [Key(5)]
        public float attackSpeedFactor;   // 攻撃速度(Animatorの係数)
        [Key(6)]
        public float healRate;  // 自動回復の倍率

        public CharacterStatusData(int hp = 0, int defence = 0, int power = 0, float moveSpeed = 0, float attackSpeedFactor = 0, float jumpPower = 0, float healRate = 0)
        {
            this.hp = hp;
            this.defence = defence;
            this.power = power;
            this.jumpPower = jumpPower;
            this.moveSpeed = moveSpeed;
            this.attackSpeedFactor = attackSpeedFactor;
            this.healRate = healRate;
        }

    }
}
