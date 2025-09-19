//=============================
// プレイヤー死亡APIのレスポンス用データクラス
// Author:Rui Enomoto
//=============================
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace NIGHTRAVEL.Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class PlayerDeathResult
    {
        /// <summary>
        /// 蘇生アイテムの残り所持数
        /// </summary>
        [Key(0)]
        public int BuckupHDMICnt {  get; set; }

        /// <summary>
        /// 実際に死亡したかどうか
        /// </summary>
        [Key(1)]
        public bool IsDead { get; set; }
    }
}
