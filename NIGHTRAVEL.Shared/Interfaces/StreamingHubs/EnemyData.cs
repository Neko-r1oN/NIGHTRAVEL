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
        public string UniqueId { get; set; }

        [Key(9)]
        /// <summary>
        /// 敵の種類ID
        /// Author:Nishiura
        /// </summary>
        public EnumManager.ENEMY_TYPE TypeId { get; set; }

        [Key(10)]
        /// <summary>
        /// 敵名称
        /// Author:Nishiura
        /// </summary>
        public string EnemyName { get; set; }

        [Key(11)]
        /// <summary>
        /// ボス判定
        /// Author:Nishiura
        /// </summary>
        public bool isBoss { get; set; } = false;

        [Key(12)]
        /// <summary>
        /// 無敵状態かどうか
        /// Author:Enomoto
        /// </summary>
        public bool IsInvincible { get; set; }

        [Key(13)]
        /// <summary>
        /// Quaternionのオプション
        /// Author:Enomoto
        /// </summary>
        public List<Quaternion> Quatarnions { get; set; } = new List<Quaternion>();
    }
}
