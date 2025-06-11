////////////////////////////////////////////////////////////////
///
/// ユーザーの通信を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using MagicOnion;
using MagicOnion.Server;
using NIGHTRAVEL.Server.Model.Context;
using NIGHTRAVEL.Server.Model.Entity;
using NIGHTRAVEL.Shared.Services;
using System.Linq.Expressions;

namespace NIGHTRAVEL.Server.Services
{
    public class UserService:ServiceBase<IUserService>,IUserService
    {
        public async UnaryResult<int> RegistUserAsync(string name)
        {
            using var context = new GameDbContext();

            //テーブルにレコードを追加
            User user = new User();
            user.Created_at = DateTime.Now;
            user.Updated_at = DateTime.Now;
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user.id;
        }

    }
}
