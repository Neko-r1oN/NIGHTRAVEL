////////////////////////////////////////////////////////////////
///
/// ユーザーのインターフェースを管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////using MagicOnion;
using MagicOnion;
using NIGHTRAVEL.Server.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NIGHTRAVEL.Shared.Services
{
    /// <summary>
    /// ユーザーのインターフェースの追加(Shared)
    /// </summary>
    public interface IUserService : IService<IUserService>
    {
        //ユーザーの登録
        UnaryResult<int> RegistUserAsync();

        ////ユーザーの実績の取得
        //UnaryResult<int> GetUserAchievementsAsync();

        ////ユーザーの実績の更新
        //UnaryResult<int> UpdateUserAchievementsAsync();

        //ユーザーの全取得
        UnaryResult<User[]> GetAllUsersAsync(); 
        
        //ユーザーのid指定取得
        UnaryResult<User> GetUserAsync(int id);
    }
}
