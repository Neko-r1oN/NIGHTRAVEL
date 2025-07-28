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

        [Key(13)]
        /// <summary>
        /// 接続ID(識別用)
        /// </summary>
        public Guid ConnectionId {  get; set; } = Guid.Empty;

        [Key(14)]
        /// <summary>
        /// プレイヤーID
        /// Author:Nishiura
        /// </summary>
        public int PlayerID { get; set; }

        [Key(15)]
        /// <summary>
        /// 死亡判定
        /// </summary>
        public bool IsDead { get; set; } = false;
    }
}
