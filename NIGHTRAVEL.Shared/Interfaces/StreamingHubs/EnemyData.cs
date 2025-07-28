//=============================
// エネミーデータスクリプト
// Author:Nishiura Data:07/14
//=============================
using MessagePack;
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class EnemyData : CharacterData
    {
        [Key(13)]
        /// <summary>
        /// 識別ID
        /// Author:Nishiura
        /// </summary>
        public int EnemyID { get; set; }

        [Key(14)]
        /// <summary>
        /// 敵名称
        /// Author:Nishiura
        /// </summary>
        public string EnemyName { get; set; }

        [Key(15)]
        /// <summary>
        /// ボス判定
        /// Author:Nishiura
        /// </summary>
        public bool isBoss { get; set; } = false;
    }
}
