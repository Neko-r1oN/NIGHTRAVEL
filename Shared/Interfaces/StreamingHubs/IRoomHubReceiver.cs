////////////////////////////////////////////////////////////////////////////
///
///  通知関数設定スクリプト
///  Author : 川口京佑  2025.01/28
///
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using MagicOnion;
using Shared.Model.Entity;

namespace Shared.Interfaces.StreamingHubs
{
    public interface IRoomHubReceiver
    {
        //ここからサーバー側からクライアント側を呼び出す
        //関数を定義する

        //サーバーの入室通知
        void OnJoin(JoinedUser user);

        //ロビー入室通知
        void OnJoinLobby(JoinedUser user);

        //マスターチェック通知
        void OnMasterCheck(JoinedUser user);

        //マッチング成立通知
        void OnMatch(string roomName);


        //サーバーの退出通知
        void Leave(Guid connectionId);

        //ユーザーの移動通知
        void OnMove(MoveData moveData);

        void OnStand();


        //全ユーザー準備完了通知
        void Ready(Guid id ,bool isAllUserReady);



    }
}
