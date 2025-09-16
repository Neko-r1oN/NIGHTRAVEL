
using MagicOnion;
using MagicOnion.Server;
using NIGHTRAVEL.Server.Model.Context;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using NIGHTRAVEL.Shared.Interfaces.Services;

namespace NIGHTRAVEL.Server.Services
{
    public class RoomService : ServiceBase<IRoomService>, IRoomService
    {
        //ステージをID指定で取得
        public async UnaryResult<Room> GetRoom(string user_name)
        {
            //DBを取得
            using var context = new GameDbContext();

            //ステージのデータ格納変数を定義
            Room room = new Room();

            //テーブルからレコードをidを指定して取得
            room = context.Rooms.Where(room => room.userName == user_name).First();

            //バリデーションチェック
            if (room == null)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument,
                    "ルームが見つかりませんでした");
            }


            //ステージのデータを返す
            return room;
        }

        //ステージの一覧を取得
        public async UnaryResult<Room[]> GetAllRoom()
        {
            //DBを取得
            using var context = new GameDbContext();

            //テーブルからレコードをidを指定して取得
            Room[] rooms = context.Rooms.ToArray();

            //ステージのデータを返す
            return rooms;
        }
    }
}
