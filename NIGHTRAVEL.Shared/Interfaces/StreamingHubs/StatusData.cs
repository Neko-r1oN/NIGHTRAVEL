//===============================
// キャラクターのデータクラス
// Author:Kenta Nakamoto
//===============================
using MessagePack;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Text;
using static Shared.Interfaces.StreamingHubs.EnumManager;

namespace NIGHTRAVEL.Shared.Interfaces.StreamingHubs
{
    public class PlayerRelicData : CharacterStatusData
    {
        [Key(7)]
        public Dictionary<DEBUFF_TYPE, float> GiveDebuffRates = new Dictionary<DEBUFF_TYPE, float>()
        {
            { DEBUFF_TYPE.Burn, 0f },
            { DEBUFF_TYPE.Freeze, 0f },
            { DEBUFF_TYPE.Shock, 0f },
        };  // 状態異常付与率

        [Key(8)]
        public float DebuffDmgRate = 0;      // 状態異常ダメージ倍率

        [Key(9)]
        public float PierceRate = 0;         // 防御貫通率

        [Key(10)]
        public float DmgHealRate = 0f;       // 与ダメ回復率

        [Key(11)]
        public float DodgeRate = 0;          // 回避率

        [Key(12)]
        public float DmgResistRate = 0;      // 被ダメージ軽減率

        [Key(13)]
        public float KillHpReward = 0;       // キル時HP回復率

        [Key(14)]
        public float DARate = 0;             // ダブルアタック率

        [Key(15)]
        public int BombCnt = 2;              // ボム所持数

        [Key(16)]
        public int HealMeatCnt = 0;          // 回復肉所持数

        [Key(17)]
        public int ReviveCnt = 0;            // リバイブ回数

        [Key(18)]
        public int ElecOrbCnt = 0;           // 感電オーブ所持数
    }
}
