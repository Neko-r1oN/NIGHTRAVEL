//=============================
// ドロップしたレリックのデータクラス
// Author:Nishiura Data:08/05
//=============================
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    //[MessagePackObject]
    public class DropRelicData
    {
        [Key(0)]
        /// <summary>
        /// ID
        /// Author:Nishiura
        /// </summary>
        public string uniqueId { get; set; }

        [Key(1)]
        /// <summary>
        /// 名前
        /// Author:Nishiura
        /// </summary>  
        public string Name { get; set; }

        [Key(2)]
        /// <summary>
        /// 説明文
        /// Author:Nishiura
        /// </summary>  
        public string ExplanationText { get; set; }

        [Key(3)]
        /// <summary>
        /// レリックの種類
        /// Author:Nishiura
        /// </summary>  
        public EnumManager.RELIC_TYPE RelicType { get; set; }

        [Key(4)]
        /// <summary>
        /// レアリティ
        /// </summary>
        public EnumManager.RARITY_TYPE RarityType { get; set; }

        [Key(5)]
        /// <summary>
        /// 生成位置
        /// Author:Nishiura
        /// </summary>  
        public Vector2 SpawnPos { get; set; }
    }
}
