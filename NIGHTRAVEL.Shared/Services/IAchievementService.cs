using MagicOnion;
using NIGHTRAVEL.Shared.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NIGHTRAVEL.Shared.Services
{
    public interface IAchievementService : IService<IAchievementService>
    {

        //実績をIDで取得
        UnaryResult<Achievement> GetAchievement(int id);

        //実績一覧を取得
        UnaryResult<Achievement[]> GetAllAchievement();

        //実績をタイプで取得
        UnaryResult<Achievement[]> GetAchievementType(string type );
    }
}
