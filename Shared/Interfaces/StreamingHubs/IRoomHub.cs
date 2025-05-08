////////////////////////////////////////////////////////////////////////////
///
///  関数定義スクリプト
///  Author : 川口京佑  2025.01/28
///
////////////////////////////////////////////////////////////////////////////

using MagicOnion;
using Shared.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    public interface IRoomHub : IStreamingHub<IRoomHub, IRoomHubReceiver>
    {
        //ここにクライアント側からサーバー側を呼び出す関数定義を作成

        /// <summary>
        /// ユーザー入室関数
        /// </summary>
        /// <param name="roomName">ルーム名</param>
        /// <param name="userId">ユーザーID</param>
        /// <returns>入室処理</returns>
        Task<JoinedUser[]> JoinAsync(string roomName, int userId);

        Task MastaerCheckAsync(JoinedUser user);

        /// <summary>
        /// ロビー入室処理
        /// </summary>
        /// <param name="userId">入室ユーザーID</param>
        /// <returns></returns>
        Task/*<JoinedUser[]>*/ JoinLobbyAsync(int userId);

        
        /// <summary>
        /// ユーザー退出関数
        /// </summary>
        /// <returns></returns>
        Task LeaveAsync();

        /// <summary>
        /// マッチング成立関数
        /// </summary>
        /// <param name="roomName">ルーム名(4番目に入室したユーザーのID)</param>
        
        /// <returns></returns>
        Task MatchAsync(string roomName);

        /// <summary>
        /// ユーザー移動関数
        /// </summary>
        /// <returns></returns>
        Task MoveAsync(MoveData moveData);


    }
}
