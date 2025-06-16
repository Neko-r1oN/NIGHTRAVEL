////////////////////////////////////////////////////////////////
///
/// ユーザー関連の通信を管理するスクリプト
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
    /// <summary>
    /// ユーザーのApiを追加(public)
    /// </summary>
    public class UserService:ServiceBase<IUserService>,IUserService
    {
        //ユーザーの登録
        public async UnaryResult<int> RegistUserAsync()
        {
            //データベースを取得
            using var context = new GameDbContext();


            //テーブルにレコードを追加
            User user = new User();             //Userデータ
            user.Created_at = DateTime.Now;     //生成日時
            user.Updated_at = DateTime.Now;     //更新日時
            context.Users.Add(user);            //データベースに格納
            await context.SaveChangesAsync();   //データベースを保存する
            return user.id;                     //ユーザーのidを返す
        }

        //全ユーザーの取得
        public async UnaryResult<User[]> GetAllUsersAsync()
        {
            //データベースを取得
            using var context = new GameDbContext();

            //テーブルからレコードをすべて取得
            User[] users = context.Users.ToArray();
            
            //ユーザーのデータを返す
            return users;
        }

        //ユーザーをidを指定して取得
        public async UnaryResult<User> GetUserAsync(int id)
        {
            //DBを取得
            using var context = new GameDbContext();

            ////バリデーションチェック
            //if (context.Users.Where(user => user.id == id).Count() > context)
            //{
            //    throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "");
            //}

            //テーブルからレコードをidを指定して取得
            User user = context.Users.Where(user=>user.id==id).First();

            //ユーザーのデータを返す
            return user;
        }

    }
}
