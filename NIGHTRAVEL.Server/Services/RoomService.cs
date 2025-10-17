
using MagicOnion;
using MagicOnion.Server;
using NIGHTRAVEL.Server.Model.Context;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using NIGHTRAVEL.Shared.Interfaces.Services;
using System.Security.Principal;
using System.Xml.Linq;

namespace NIGHTRAVEL.Server.Services
{
    public class RoomService : ServiceBase<IRoomService>, IRoomService
    {
        //ルームをユーザーの名前で取得
        public async UnaryResult<Room> GetRoom(string user_name)
        {
            bool isSerch = false;

            //DBを取得
            using var context = new GameDbContext();

            //ステージのデータ格納変数を定義
            Room room = new Room();

            Room[] rooms = context.Rooms.ToArray();

            foreach (var data in rooms)
            {//dataはenemiesの0番目データからループ
                if (data.userName == user_name)
                {//そのデータが指定された名前と一致したら

                    //検索出来たことにする
                    isSerch = true;
                }
            }


            //バリデーションチェック
            if (isSerch == false)
            {
                return null;
                //400エラー表示
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument,
                    "ルームが存在しません");
            }

            //テーブルからレコードをidを指定して取得
            room = context.Rooms.Where(room => room.userName == user_name).First();

            //ステージのデータを返す
            return room;
        }

        //ルームの一覧を取得
        public async UnaryResult<Room[]> GetAllRoom()
        {
            //DBを取得
            using var context = new GameDbContext();

            //テーブルからレコードをidを指定して取得
            Room[] rooms = context.Rooms.ToArray();

            //ステージのデータを返す
            return rooms;
        }

        //ルームを生成
        public async UnaryResult<Room> RegistRoom(string room_name,string user_name, string pass, int game_mode)
        {
            Room room = new Room();
            room.userName = user_name;
            room.roomName = room_name;
            room.password = pass;
            room.Created_at = DateTime.Now;      //生成日時
            room.Updated_at = DateTime.Now;      //更新日時
            if (game_mode == 0) 
            {
                room.is_started = true;
            }


            //DBを取得
            using var context = new GameDbContext();

            //ルームを追加
            context.Add(room);

            await context.SaveChangesAsync();   //データベースを保存する


            //ステージのデータを返す
            return room;
        }

        //ルームを開始状態に
        public async UnaryResult<Room> StartRoom(string userName)
        {
            // DBを取得
            using var context = new GameDbContext();

            // ステージのデータ格納変数を定義
            Room room = new Room();

            // テーブルからレコードをuserNameを指定して取得
            room = context.Rooms.Where(room => room.userName == userName).First();

            // バリデーションチェック
            if (room == null)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument,
                    "ルームが見つかりませんでした");
            }

            room.is_started = true;
            context.Update(room);
            await context.SaveChangesAsync();   //データベースを保存する

            //ステージのデータを返す
            return room;
        }

        //ルームを削除
        public async UnaryResult<Room> RemoveRoom(string room_name)
        {
            //DBを取得
            using var context = new GameDbContext();

            var room = context.Rooms.Where(room => room.roomName == room_name).First();

            context.Rooms.Remove(room);

            await context.SaveChangesAsync();   //データベースを保存する

            //ステージのデータを返す
            return room;
        }
    }
}
