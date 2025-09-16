using MagicOnion;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NIGHTRAVEL.Shared.Interfaces.Services
{
    public interface IRoomService : IService<IRoomService>
    {
        //ステージをID指定で取得
        UnaryResult<Room> GetRoom(string user_name);

        //ステージ一覧を取得
        UnaryResult<Room[]> GetAllRoom();

    }
}
