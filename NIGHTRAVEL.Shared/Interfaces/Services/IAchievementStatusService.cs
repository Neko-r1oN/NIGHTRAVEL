////////////////////////////////////////////////////////////////
///
/// ユーザーの実績進捗関連の通信を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using MagicOnion;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NIGHTRAVEL.Shared.Interfaces.Services
{
    public interface IAchievementStatusService : IService<IAchievementStatusService>
    {
        //実績進捗をIDで取得
        UnaryResult<AchievementStatus> GetAchievementStatus(int id);

        //実績進捗一覧を取得
        UnaryResult<AchievementStatus[]> GetAllAchievementStatus();

        //実績進捗をユーザーIDで取得
        UnaryResult<AchievementStatus[]> GetUserAchievementStatus(int user_id);

        //実績進捗を実績IDで取得
        UnaryResult<AchievementStatus[]> GetAchievementIdStatus(int achievement_id);
    }
}
