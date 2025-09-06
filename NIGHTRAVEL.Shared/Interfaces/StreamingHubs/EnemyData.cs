//=============================
// エネミーデータスクリプト
// Author:Nishiura Data:07/14
//=============================
using MessagePack;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class EnemyData : CharacterData
    {
        [Key(8)]
        /// <summary>
        /// 識別ID
        /// Author:Nishiura
        /// </summary>
        public Guid UniqueId { get; set; }

        [Key(9)]
        /// <summary>
        /// 敵名称
        /// Author:Nishiura
        /// </summary>
        public string EnemyName { get; set; }

        [Key(10)]
        /// <summary>
        /// ボス判定
        /// Author:Nishiura
        /// </summary>
        public bool isBoss { get; set; } = false;

        [Key(11)]
        /// <summary>
        /// 経験値
        /// Author:Nishiura
        /// </summary>
        public int Exp { get; set; }

        [Key(12)]
        /// <summary>
        /// Quaternionのオプション
        /// Author:Enomoto
        /// </summary>
        public List<Quaternion> Quatarnions { get; set; } = new List<Quaternion>();
    }
}
