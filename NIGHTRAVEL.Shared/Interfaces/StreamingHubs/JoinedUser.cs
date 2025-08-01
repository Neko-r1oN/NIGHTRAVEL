//=============================
// ユーザー関連のリアルタイム通信に使うカラムを管理するスクリプト
// Author:木田晃輔
//=============================

using System;
using MessagePack;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;

namespace Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class JoinedUser
    {
        /// <summary>
        /// 接続ID
        /// </summary>
        [Key(0)]
        public Guid ConnectionId { get; set; }
        /// <summary>
        /// ユーザ情報
        /// </summary>
        [Key(1)]
        public User UserData { get; set; }
    
        /// <summary>
        /// 参加順
        /// </summary>
        [Key(2)]
        public int JoinOrder { get; set; }

        /// <summary>
        /// マスタクライアント判定
        /// </summary>
        [Key(3)]
        public bool IsMaster { get; set; }

        /// <summary>
        /// 準備完了判定
        /// </summary>
        [Key(4)]
        public bool IsReady { get; set; }

        /// <summary>
        /// ステージ進行完了判定
        /// </summary>
        [Key(5)]
        public bool IsAdvance { get; set; }
    }

}
