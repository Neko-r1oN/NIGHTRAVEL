////////////////////////////////////////////////////////////////////////////
///
///  参加ユーザーデータスクリプト
///  Author : 川口京佑  2025.01/28
///
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;
using Shared.Model.Entity;
using UnityEngine;


namespace Shared.Interfaces.StreamingHubs
{
    /// <summary>
    /// 参加ユーザークラス
    /// </summary>
    [MessagePackObject]
    public class JoinedUser
    {
        [Key(0)]
        public Guid ConnectionId { get; set; }  //接続ID
        [Key(1)]
        public User UserData { get; set; }      //ユーザー情報
        [Key(2)]
        public UserState UserState { get; set; }      //ユーザー状態
        [Key(3)]
        public bool IsSelf { get; set; }        //自分自身かどうか
        [Key(4)]
        public int JoinOrder;                   //参加順番
    }
}
