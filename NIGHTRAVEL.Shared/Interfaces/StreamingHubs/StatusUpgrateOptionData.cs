//=============================
// ステータス強化のデータクラス
// Author:Rui Enomoto Data:09/18
//=============================
using MessagePack;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Text;

namespace NIGHTRAVEL.Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class StatusUpgrateOptionData
    {
        /// <summary>
        /// 種類ID
        /// </summary>
        [Key(0)]
        public EnumManager.STAT_UPGRADE_OPTION TypeId { get; set; }

        /// <summary>
        /// 名前
        /// </summary>
        [Key(1)]
        public string Name { get; set; }

        /// <summary>
        /// レアリティ
        /// </summary>
        [Key(2)]
        public EnumManager.RARITY_TYPE Rarity { get; set; }

        /// <summary>
        /// 説明文
        /// </summary>
        [Key(3)]
        public string Explanation { get; set; }

        /// <summary>
        /// 強化するステータスのタイプ
        /// </summary>
        [Key(4)]
        public EnumManager.STATUS_TYPE StatusType { get; set; }

    }
}
