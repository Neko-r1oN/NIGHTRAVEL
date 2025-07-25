//=============================
// エネミーデータスクリプト
// Author:Nishiura Data:07/14
//=============================
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    public class EnemyData : CharacterData
    {
        /// <summary>
        /// 識別ID
        /// Author:Nishiura
        /// </summary>
        public int EnemyID { get; set; }

        /// <summary>
        /// 敵名称
        /// Author:Nishiura
        /// </summary>
        public string EnemyName { get; set; }

        /// <summary>
        /// ボス判定
        /// Author:Nishiura
        /// </summary>
        public bool isBoss { get; set; } = false;
    }
}
