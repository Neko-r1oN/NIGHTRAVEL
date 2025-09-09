//=============================
// プレイヤーデータスクリプト
// Author:木田晃輔
//=============================
using MessagePack;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class PlayerData : CharacterData
    {
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

        [Key(11)]
        /// <summary>
        /// キャラクターのクラス
        /// </summary>
        public EnumManager.Player_Type Class { get; set; }
    }
}