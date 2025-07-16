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
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using NIGHTRAVEL.Shared.Interfaces.Services;
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
            return user.Id;                     //ユーザーのidを返す
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

            //テーブル情報の変数定義
            User user = new User();

            //バリデーションチェック
            if (context.Users.Count() < id || id <= 0)
            {//ユーザーのIDが登録分を超過、無効な入力の場合

                //400エラー表示
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, 
                    "そのIDのユーザーは登録されていません");
            }

            //テーブルからレコードをidを指定して取得
            user = context.Users.Where(user=>user.Id==id).First();


            //ユーザーのデータを返す
            return user;
        }

    }
}
