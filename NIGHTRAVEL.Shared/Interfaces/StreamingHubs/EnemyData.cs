//=============================
// エネミーデータスクリプト
// Author:Nishiura Data:07/14
//=============================
using Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    public class EnemyData
    {
        /// <summary>
        /// 識別ID
        /// Author:Nishiura
        /// </summary>
        public int EnemyID { get; set; }

        /// <summary>
        /// 敵名称
        /// Author:Nishiura
        /// </summary>
        public string EnemyName { get; set; }

        /// <summary>
        /// ボス判定
        /// Author:Nishiura
        /// </summary>
        public bool isBoss { get; set; }

        /// <summary>
        /// 体力
        /// Author:Nishiura
        /// </summary>
        public float Health { get; set; }

        /// <summary>
        /// 攻撃力
        /// Author:Nishiura
        /// </summary>
        public float Attack {  get; set; }

        /// <summary>
        /// 攻撃速度
        /// Author:Nishiura
        /// </summary>
        public float AttackSpeed { get; set; }

        /// <summary>
        /// 防御力
        /// Author:Nishiura
        /// </summary>
        public float Defense {  get; set; }

        /// <summary>
        /// 移動速度
        /// Author:Nishiura
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// 敵の位置
        /// Author:Nishiura
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// 敵の向き
        /// Author:Nishiura
        /// </summary>
        public Quaternion Rotation { get; set; }

        /// <summary>
        /// アニメーション状態
        /// Author:Nishiura
        /// </summary>
        public IRoomHubReceiver.EnemyAnimState State { get; set; }

        /// <summary>
        /// 状態異常リスト
        /// Author:Nishiura
        /// </summary>
        public List<int> DebuffList { get; set; }
    }
}
