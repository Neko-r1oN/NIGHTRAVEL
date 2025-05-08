////////////////////////////////////////////////////////////////////////////
///
///  API内部処理(ユーザー関連)スクリプト
///  Author : 川口京佑  2025.02/14
///
////////////////////////////////////////////////////////////////////////////

using MagicOnion;
using MagicOnion.Server;
using MagicOnionServer.Model.Context;
using MessagePack.Resolvers;
using Server.StreamingHubs;
using Shared.Interfaces.RPCServices;
using Shared.Model.Entity;
using System.Data;
using System.Linq;
using System;
using System.Security.Cryptography;

namespace Server.RPCServices
{
    /// <summary>
    /// API内部処理クラス
    /// </summary>
    public class UserService:ServiceBase<IUserService>, IUserService
    {        
        /// <summary>
        /// ユーザー登録API
        /// </summary>
        /// <param name="name">登録名</param>
        /// <returns>作成したユーザー識別ID</returns>
        public async UnaryResult<User>RegistUserAsync(string name) 
        {
            using var context = new GameDbContext();   //DB接続

            bool isSucces = false;   //成功判定
            int id = 0;              //ID変数

            while (!isSucces)
            {
                id = generateRandId(100000000, 1000000000);   //九桁のID生成

                //バリデーションチェック
                if (context.Users.Where(user => user.UserId == id).Count() > 0)
                {//DBに同じIDが登録されていた場合
                    Console.WriteLine("ID重複");   //ループ継続
                }
                else
                {//重複が確認できなかった場合
                    isSucces = true;    //ループを抜ける
                }
            }
            //entitiyFrameWorkCore

            //テーブルにレコードを追加
            User user = new User();
            user.Name = name;      //ユーザー名
            user.UserId = id;      //ユーザーID
            user.Token = Guid.NewGuid().ToString();
            user.Created_at = DateTime.Now;
            user.Updated_at = DateTime.Now;
            context.Users.Add(user);        //DB登録

            //DBにデータを保存
            await context.SaveChangesAsync();

            //作成したユーザーのIDを返す
            return user;
        }

        /// <summary>
        /// ユーザーID生成生成関数
        /// </summary>
        /// <param name="minValue">ID最小値(九桁)</param>
        /// <param name="maxValue">ID最大値(九桁)</param>
        /// <returns>範囲内で生成した数字列</returns>
        private static int generateRandId(int minValue, int maxValue)
        {
            Random r = new Random();
            return r.Next(minValue, maxValue);
        }

        /// <summary>
        /// ユーザー情報取得API(ID指定)
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>該当ユーザー情報</returns>
        public async UnaryResult<User> GetUserInfoAsync(int userId)
        {
            using var context = new GameDbContext();

            //バリデーションチェック
            if (context.Users.Where(user => user.UserId == userId).Count() == 0)
            {//DBに指定したユーザーIDが無かった場合

                //例外を返す
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "そのユーザーIDは存在しません。");
            }

            //テーブルからユーザー情報を取得
            User userInfo = context.Users.Where(user => user.UserId == userId).First();

            //作成したユーザーの情報を返す
            return userInfo;
        }


        /// <summary>
        /// 全ユーザー情報取得API
        /// </summary>
        /// <returns></returns>
        public async UnaryResult<User[]> GetAllUserInfoAsync()
        {
            using var context = new GameDbContext();

            //テーブルからユーザー情報を取得
            User[] usersInfo = context.Users.ToArray();

            //全ユーザーの情報を返す
            return usersInfo;
        }


        /// <summary>
        /// ユーザー情報更新API
        /// </summary>
        /// <param name="userId">変更するユーザーID</param>
        /// <param name="userName">変更後のユーザーID</param>
        /// <returns>更新完了</returns>
        public async UnaryResult<bool> UpdateUserInfoAsync(int userId, string userName)
        {
            using var context = new GameDbContext();

            //テーブルからユーザー情報を取得・名前上書き
            User user = context.Users.Where(user => user.Id == userId).First();
            user.Name = userName;

            //DBにデータを保存
            await context.SaveChangesAsync();

            return true;
        }

    }
}
