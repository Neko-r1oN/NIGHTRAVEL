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
    public class SpawnEnemyData
    {
        [Key(0)]
        /// <summary>
        /// 敵の種類ID
        /// </summary>
        public EnumManager.ENEMY_TYPE TypeId { get; set; }
        
        [Key(1)]
        /// <summary>
        /// 識別用ID
        /// </summary>
        public string UniqueId { get; set; }

        [Key(2)]
        /// <summary>
        /// 生成する座標
        /// </summary>
        public Vector2 Position { get; set; }

        [Key(3)]
        /// <summary>
        /// 向き
        /// </summary>
        public Vector3 Scale { get; set; }

        [Key(4)]
        /// <summary>
        /// エリート個体のタイプ
        /// </summary>
        public EnumManager.ENEMY_ELITE_TYPE EliteType { get; set; }

        [Key(5)]
        /// <summary>
        /// 生成タイプ
        /// </summary>
        public EnumManager.SPAWN_ENEMY_TYPE SpawnType { get; set; }

        /// <summary>
        /// 端末ID
        /// </summary>
        [Key(6)]
        public int TerminalID { get; set; }
    }
}
