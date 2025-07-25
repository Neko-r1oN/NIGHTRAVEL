//=============================
// プレイヤーデータスクリプト
// Author:木田晃輔
//=============================
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    public class PlayerData : CharacterData
    {
        ///// <summary>
        ///// ユーザーのデータ
        ///// Author:Nishiura
        ///// </summary>
        //public JoinedUser JoinedUser { get; set; }

        /// <summary>
        /// 接続ID(識別用)
        /// </summary>
        public Guid ConnectionId {  get; set; } = Guid.Empty;

        /// <summary>
        /// プレイヤーID
        /// Author:Nishiura
        /// </summary>
        public int PlayerID { get; set; }

        /// <summary>
        /// 死亡判定
        /// </summary>
        public bool IsDead { get; set; } = false;
    }
}
