////////////////////////////////////////////////////////////////////////////
///
///  ルームデータスクリプト
///  Author : 川口京佑  2025.01/28
///
////////////////////////////////////////////////////////////////////////////

using Shared.Interfaces.StreamingHubs;
using Shared.Model.Entity;

namespace Server.StreamingHubs
{
    //ルーム内に依存するクラス
    public class RoomData
    {
        public JoinedUser JoinedUser { get; set; }      //参加ユーザー情報

        public UserState UserState { get; set; }        //参加ユーザー状態

        public MoveData MoveData { get; set; }          //プレイヤー移動情報

        public MoveData BallMoveData { get; set; }     //ボール座標同期情報
    }  
}
