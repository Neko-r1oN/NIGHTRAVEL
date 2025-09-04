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
        /// 起動済みかどうか
        /// Author:Nishiura
        /// </summary>
        public bool IsActivated { get; set; }

        [Key(4)]
        /// <summary>
        /// 位置
        /// Author:Nishiura
        /// </summary>
        public Vector2 Position { get; set; }

        [Key(5)]
        /// <summary>
        /// 回転
        /// Author:Nishiura
        /// </summary>
        public Quaternion Rotation { get; set; }
    }
}
