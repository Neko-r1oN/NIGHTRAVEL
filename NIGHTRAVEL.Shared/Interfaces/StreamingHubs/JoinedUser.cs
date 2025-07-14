////////////////////////////////////////////////////////////////
///
/// ユーザー関連のリアルタイム通信に使うカラムを管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using System;
using MessagePack;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;

namespace Shared.Interfaces.StreamingHubs
{
    [MessagePackObject]
    public class JoinedUser
    {
        [Key(0)]
        public Guid ConnectionId { get; set; } //接続ID
        [Key(1)]
        public User UserData { get; set; }//ユーザー情報
        [Key(2)]
        public int JoinOrder { get; set; } //参加順番

        [Key(3)]
        public bool IsMaster { get; set; } //マスタークライアントを判断
    }

}
