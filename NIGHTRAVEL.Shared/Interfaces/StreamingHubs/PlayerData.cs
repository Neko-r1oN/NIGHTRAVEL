//=============================
// プレイヤーデータスクリプト
// Author:木田晃輔
//=============================
using MessagePack;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class PlayerData : CharacterData
    {
        ///// <summary>
        ///// ユーザーのデータ
        ///// Author:Nishiura
        ///// </summary>
        //public JoinedUser JoinedUser { get; set; }

        [Key(8)]
        /// <summary>
        /// 接続ID(識別用)
        /// </summary>
        public Guid ConnectionId {  get; set; } = Guid.Empty;

        [Key(9)]
        /// <summary>
        /// プレイヤーID
        /// Author:Nishiura
        /// </summary>
        public int PlayerID { get; set; }

        [Key(10)]
        /// <summary>
        /// 死亡判定
        /// </summary>
        public bool IsDead { get; set; } = false;

        ///---------------------------
        /// レリック関連
        /// Author:中本健太
        /// --------------------------

        [Key(11)]
        public Dictionary<EnumManager.DEBUFF_TYPE, float> GiveDebuffRates { get; set; } = new Dictionary<EnumManager.DEBUFF_TYPE, float>()
        {
            { EnumManager.DEBUFF_TYPE.Burn, 0f },
            { EnumManager.DEBUFF_TYPE.Freeze, 0f },
            { EnumManager.DEBUFF_TYPE.Shock, 0f },
        };

        [Key(12)]
        /// <summary>
        /// 防御無視率
        /// </summary>
        public float PierceRate { get; set; } = 0;

        [Key(13)]
        /// <summary>
        /// 与ダメ回復率
        /// <summary>
        public float DmgHealRate { get; set; } = 0f;

        [Key(14)]
        /// <summary>
        /// ボムの個数
        /// <summary>
        public int BombCnt { get; set; } = 2;

        [Key(15)]
        /// <summary>
        /// ボムダメージ倍率
        /// <summary>
        public float BombDmgRate { get; set; } = 0.2f;

        [Key(16)]
        /// <summary>
        /// 回避率
        /// <summary>
        public float DodgeRate { get; set; } = 0;

        [Key(17)]
        /// <summary>
        /// 回復肉の個数
        /// <summary>
        public int HealMeatCnt { get; set; } = 0;

        [Key(18)]
        /// <summary>
        /// ダメージ軽減率
        /// <summary>
        public float DmgResistRate { get; set; } = 0;

        [Key(19)]
        /// <summary>
        /// 撃破時のHP回復量
        /// <summary>
        public float KillHpReward { get; set; } = 0;

        [Key(20)]
        /// <summary>
        /// ダブルアタック率
        /// <summary>
        public float DARate { get; set; } = 0;

        [Key(21)]
        /// <summary>
        /// 復活回数
        /// <summary>
        public int ReviveCnt { get; set; } = 0;

        [Key(22)]
        /// <summary>
        /// 状態異常特攻率
        /// <summary>
        public float DebuffDmgRate { get; set; } = 0;

        [Key(23)]
        /// <summary>
        /// 電気玉個数
        /// <summary>
        public int ElecOrbCnt { get; set; } = 0;
    }
}