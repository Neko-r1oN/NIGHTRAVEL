//=============================
// エネミーデータスクリプト
// Author:Nishiura Data:07/14
//=============================
using Shared.Interfaces.StreamingHubs;
using UnityEngine;

namespace NIGHTRAVEL.Server.StreamingHubs
{
    public class EnemyData
    {
        /// <summary>
        /// 識別ID
        /// Author:Nishiura
        /// </summary>
        public int EnemyID { get; set; }
        /// <summary>
        /// 敵の位置
        /// Author:Nishiura
        /// </summary>
        public Vector3 Position { get; set; }

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
    }
}
