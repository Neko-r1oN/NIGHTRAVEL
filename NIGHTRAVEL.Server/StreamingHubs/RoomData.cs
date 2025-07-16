using Shared.Interfaces.StreamingHubs;
using UnityEngine;

namespace NIGHTRAVEL.Server.StreamingHubs
{
    public class RoomData
    {
        /// <summary>
        /// ユーザーのデータ
        /// </summary>
        public JoinedUser JoinedUser { get; set; }

        /// <summary>
        /// ユーザの位置
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// ユーザの向き
        /// </summary>
        public Quaternion Rotation { get; set; }

        /// <summary>
        /// アニメーション状態
        /// Author:Nishiura
        /// </summary>
        public IRoomHubReceiver.CharacterState State { get; set; }
    }
}
