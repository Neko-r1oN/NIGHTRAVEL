//=============================
// 端末のデータクラス
// Author:Kenta Nakamoto
//=============================
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using static Shared.Interfaces.StreamingHubs.EnumManager;

namespace NIGHTRAVEL.Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class TerminalData
    {
        /// <summary>
        /// 識別ID
        /// </summary>
        [Key(0)]
        public int ID { get; set; } = 0;

        /// <summary>
        /// 種類
        /// </summary>
        [Key(1)]
        public TERMINAL_TYPE Type { get; set; } = TERMINAL_TYPE.None;

        /// <summary>
        /// 状態
        /// </summary>
        [Key(2)]
        public TERMINAL_STATE State { get; set; } = TERMINAL_STATE.None;

        /// <summary>
        /// 残り起動時間
        /// </summary>
        [Key(3)]
        public int Time { get; set; } = 0;

        /// <summary>
        /// 生成された敵のリスト
        /// </summary>
        [Key(4)]
        public List<string> EnemyList { get; set; } = new List<string>();
    }
}
