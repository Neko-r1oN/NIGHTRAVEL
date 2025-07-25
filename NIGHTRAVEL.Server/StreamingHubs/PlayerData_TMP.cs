//=============================
// プレイヤーデータスクリプト
// Author:木田晃輔
//
//  削除予定のスクリプト
//
//=============================
using Shared.Interfaces.StreamingHubs;
using UnityEngine;

namespace NIGHTRAVEL.Server.StreamingHubs
{
    public class PlayerData_TMP
    {
        /// <summary>
        /// ユーザーのデータ
        /// Author:Nishiura
        /// </summary>
        public JoinedUser JoinedUser { get; set; }

        /// <summary>
        /// プレイヤーID
        /// Author:Nishiura
        /// </summary>
        public int PlayerID { get; set; }

        /// <summary>
        /// 体力
        /// Author:Nishiura
        /// </summary>
        public float Health { get; set; }

        /// <summary>
        /// 攻撃力
        /// Author:Nishiura
        /// </summary>
        public float Attack { get; set; }

        /// <summary>
        /// 攻撃速度
        /// Author:Nishiura
        /// </summary>
        public float AttackSpeed { get; set; }

        /// <summary>
        /// 防御力
        /// Author:Nishiura
        /// </summary>
        public float Defense { get; set; }

        /// <summary>
        /// 移動速度
        /// Author:Nishiura
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// ユーザの位置
        /// Author:Nishiura
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// ユーザの向き
        /// Author:Nishiura
        /// </summary>
        public Quaternion Rotation { get; set; }

        /// <summary>
        /// アニメーション状態
        /// Author:Nishiura
        /// </summary>
        public IRoomHubReceiver.CharacterState State { get; set; }

        /// <summary>
        /// 状態異常リスト
        /// Author:Nishiura
        /// </summary>
        public List<int> DebuffList { get; set; }

        /// <summary>
        /// 死亡判定
        /// </summary>
        public bool IsDead { get; set; }
    }
}
