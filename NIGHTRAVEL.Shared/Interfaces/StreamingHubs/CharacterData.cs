//=============================
// キャラクターのデータクラス
// Author:Enomoto Data:07/25
//=============================
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    public class CharacterData
    {
        /// <summary>
        /// 自身が表示されているかどうか
        /// </summary>
        public bool IsActiveSelf { get; set; } = true;

        /// <summary>
        /// 体力
        /// Author:Nishiura
        /// </summary>
        public int Health { get; set; }

        /// <summary>
        /// 攻撃力
        /// Author:Nishiura
        /// </summary>
        public int AttackPower {  get; set; }

        /// <summary>
        /// 防御力
        /// Author:Nishiura
        /// </summary>
        public int Defense {  get; set; }

        /// <summary>
        /// 跳躍力
        /// </summary>
        public float JumpPower { get; set; }

        /// <summary>
        /// 移動速度
        /// Author:Nishiura
        /// </summary>
        public float MoveSpeed { get; set; }

        /// <summary>
        /// 移動速度係数
        /// </summary>
        public float MoveSpeedFactor { get; set; }

        /// <summary>
        /// 攻撃速度係数
        /// </summary>
        public float AttackSpeedFactor { get; set; }

        /// <summary>
        /// 位置
        /// Author:Nishiura
        /// </summary>
        public Vector2 Position { get; set; } = Vector2.zero;

        /// <summary>
        /// 向き
        /// Author:Nishiura
        /// </summary>
        public Quaternion Rotation { get; set; } = Quaternion.identity;

        /// <summary>
        /// アニメーションID
        /// Author:Nishiura
        /// </summary>
        public int AnimationId { get; set; }

        /// <summary>
        /// 状態異常リスト
        /// Author:Nishiura
        /// </summary>
        public List<int> DebuffList { get; set; } = new List<int>();
    }
}
