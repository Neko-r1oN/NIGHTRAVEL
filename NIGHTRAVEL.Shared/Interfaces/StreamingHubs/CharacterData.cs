//=============================
// キャラクターのデータクラス
// Author:Enomoto Data:07/25
//=============================
using MessagePack;
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using static Shared.Interfaces.StreamingHubs.EnumManager;

namespace Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class CharacterData
    {
        [Key(0)]
        /// <summary>
        /// 自身が表示されているかどうか
        /// </summary>
        public bool IsActiveSelf { get; set; } = true;

        [Key(1)]
        /// <summary>
        /// 最大ステータス
        /// </summary>
        public CharacterStatusData Status { get; set; } = new CharacterStatusData();

        [Key(2)]
        /// <summary>
        /// 現在のステータス
        /// </summary>
        public CharacterStatusData State { get; set; } = new CharacterStatusData();

        [Key(3)]
        /// <summary>
        /// 位置
        /// Author:Nishiura
        /// </summary>
        public Vector2 Position { get; set; } = Vector2.zero;

        [Key(4)]
        /// <summary>
        /// スケール
        /// Author:木田晃輔
        /// </summary>
        public Vector3 Scale { get; set; }

        [Key(5)]
        /// <summary>
        /// 向き
        /// Author:Nishiura
        /// </summary>
        public Quaternion Rotation { get; set; } = Quaternion.identity;

        [Key(6)]
        /// <summary>
        /// アニメーションID
        /// Author:Nishiura
        /// </summary>
        public int AnimationId { get; set; }

        [Key(7)]
        /// <summary>
        /// 状態異常リスト
        /// Author:Nishiura
        /// </summary>
        public List<DEBUFF_TYPE> DebuffList { get; set; } = new List<DEBUFF_TYPE>();
    }
}
