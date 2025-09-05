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
        public float AddExpRate = 0;                // 経験値獲得量上昇率
        [Key(2)]
        public float RegainCodeRate = 0;            // 与ダメージ回復率
        [Key(3)]
        public int ScatterBugCnt = 0;               // ボム所持数
        [Key(4)]
        public float HolographicArmorRate = 0f;     // 回避率
        [Key(5)]
        public float MouseRate = 0;                 // クールダウン短縮率
        [Key(6)]
        public int DigitalMeatCnt = 0;              // 回復肉所持数
        [Key(7)]
        public float FirewallRate = 0;              // 被ダメ軽減率
        [Key(8)]
        public float LifeScavengerRate = 0;         // キル時HP回復率
        [Key(9)]
        public float RugrouterRate = 0;             // DA率
        [Key(10)]
        public int BuckupHDMICnt = 0;               // 自己蘇生回数
        [Key(11)]
        public float IdentificationAIRate = 0;      // デバフ的に対するダメUP率
        [Key(12)]
        public float DanborDollRate = 0;            // 防御貫通率
        [Key(13)]
        public int ChargedCoreCnt = 0;              // 感電オーブ所持数
        [Key(14)]
        public float IllegalScriptRate = 0;         // クリティカルオーバーキル発生率
    }
}
