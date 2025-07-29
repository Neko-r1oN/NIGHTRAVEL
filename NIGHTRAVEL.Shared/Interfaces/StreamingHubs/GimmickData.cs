//=============================
// ギミックデータスクリプト
// Author:Nishiura Data:07/16
//=============================
using MessagePack;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class GimmickData
    {
        [Key(1)]
        /// <summary>
        /// ギミックID
        /// Author:Nishiura
        /// </summary>
        public int GimmickID {  get; set; }

        [Key(2)]
        /// <summary>
        /// ギミック名称
        /// Author:Nishiura
        /// </summary>
        public string GimmickName { get; set; }

        [Key(3)]
        /// <summary>
        /// 位置
        /// Author:Nishiura
        /// </summary>
        public Vector2 Position { get; set; }

        [Key(4)]
        /// <summary>
        /// 動作ID
        /// Author:Nishiura
        /// </summary>
        public int TriggerID { get; set; }

        [Key(5)]
        /// <summary>
        /// 起動可能判定
        /// Author:Nishiura
        /// </summary>
        public bool CanBoot { get; set; }
    }
}
