//=============================
// マスタクライアントが同期する際に使用
// Author:Enomoto Data:07/25
//=============================
using MessagePack;
using NIGHTRAVEL.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    /// <summary>
    /// マスタークライアント用のデータクラス
    /// </summary>
    public class MasterClientData
    {
        [Key(0)]
        /// <summary>
        /// 自身の操作キャラを同期させる情報
        /// </summary>
        public PlayerData PlayerData { get; set; }

        [Key(1)]
        /// <summary>
        /// 全ての敵を同期させる情報
        /// </summary>
        public List<EnemyData> EnemyDatas { get; set; }

        [Key(2)]
        /// <summary>
        /// 全ての存在し続けるギミックを同期させる情報
        /// </summary>
        public List<GimmickData> GimmickDatas { get; set; }

        /// <summary>
        /// ボスが出現するまでのゲームタイマー
        /// </summary>
        [Key(3)]
        public float GameTimer { get; set; }

        /// <summary>
        /// 端末情報
        /// </summary>
        [Key(4)]
        public List<TerminalData> TerminalDatas { get; set; }
    }
}
