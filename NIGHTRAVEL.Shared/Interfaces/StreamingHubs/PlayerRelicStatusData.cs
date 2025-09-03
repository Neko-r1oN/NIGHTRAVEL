using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using static Shared.Interfaces.StreamingHubs.EnumManager;

namespace NIGHTRAVEL.Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class PlayerRelicStatusData
    {
        [Key(0)]
        public Dictionary<DEBUFF_TYPE, float> GiveDebuffRates = new Dictionary<DEBUFF_TYPE, float>()
        {
            { DEBUFF_TYPE.Burn, 0f },
            { DEBUFF_TYPE.Freeze, 0f },
            { DEBUFF_TYPE.Shock, 0f },
        };  // 状態異常付与率

        [Key(1)]
        public float DebuffDmgRate = 0;      // 状態異常ダメージ倍率

        [Key(2)]
        public float PierceRate = 0;         // 防御貫通率

        [Key(3)]
        public float DmgHealRate = 0f;       // 与ダメ回復率

        [Key(4)]
        public float DodgeRate = 0;          // 回避率

        [Key(5)]
        public float DmgResistRate = 0;      // 被ダメージ軽減率

        [Key(6)]
        public float KillHpReward = 0;       // キル時HP回復率

        [Key(7)]
        public float DARate = 0;             // ダブルアタック率

        [Key(8)]
        public int BombCnt = 2;              // ボム所持数

        [Key(9)]
        public int HealMeatCnt = 0;          // 回復肉所持数

        [Key(10)]
        public int ReviveCnt = 0;            // リバイブ回数

        [Key(11)]
        public int ElecOrbCnt = 0;           // 感電オーブ所持数
    }
}
