//=============================
// ギミックデータスクリプト
// Author:Nishiura Data:07/16
//=============================
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    public class GimmickData
    {
        /// <summary>
        /// ギミックID
        /// Author:Nishiura
        /// </summary>
        public int GimmickID {  get; set; }

        /// <summary>
        /// ギミック名称
        /// Author:Nishiura
        /// </summary>
        public string GimmickName { get; set; }

        /// <summary>
        /// 位置
        /// Author:Nishiura
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// 動作ID
        /// Author:Nishiura
        /// </summary>
        public int TriggerID { get; set; }

        /// <summary>
        /// 起動可能判定
        /// Author:Nishiura
        /// </summary>
        public bool CanBoot { get; set; }
    }
}
