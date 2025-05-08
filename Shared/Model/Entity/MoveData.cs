////////////////////////////////////////////////////////////////////////////
///
///  移動データスクリプト
///  Author : 川口京佑  2025.01/28
///
////////////////////////////////////////////////////////////////////////////

using MessagePack;
using System;
using UnityEngine;

namespace Shared.Model.Entity
{
    /// <summary>
    /// 移動データクラス
    /// </summary>
    [MessagePackObject]
    public class MoveData
    {
        [Key(0)]
        public Guid ConnectionId { get; set; }               //接続ID
        [Key(1)]
        public Vector3 Pos { get; set; }                     //位置
        [Key(2)]
        public Vector3 Rotate { get; set; }                  //角度
        [Key(3)]
        public bool Angle { get; set; }                      //キャラの向き
        [Key(4)]
        public int AnimId { get; set; }                      //アニメID

    }
}
