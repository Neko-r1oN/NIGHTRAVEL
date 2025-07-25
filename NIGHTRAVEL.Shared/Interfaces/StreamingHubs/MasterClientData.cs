//=============================
// マスタクライアントが同期する際に使用
// Author:Enomoto Data:07/25
//=============================
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    /// <summary>
    /// マスタークライアント用のデータクラス
    /// </summary>
    public class MasterClientData
    {
        /// <summary>
        /// 自身の操作キャラを同期させる情報
        /// </summary>
        public PlayerData PlayerData { get; set; }

        /// <summary>
        /// 全ての敵を同期させる情報
        /// </summary>
        public List<EnemyData> EnemyDatas { get; set; }
    }
}
