using System;
using System.Collections.Generic;
using System.Text;
using MagicOnion;
using Shared.Model.Entity;

namespace Shared.Interfaces.RPCServices
{
    public interface IUserService : IService<IUserService>
    {//ここにどのようなAPIを作るか、関数形式で定義を作成

        /// <summary>
        /// ユーザー登録処理
        /// </summary>
        /// <param name="name">ユーザー名</param>
        /// <returns>ユーザーID</returns>
        UnaryResult<User> RegistUserAsync(string name);

        /// <summary>
        /// 指定ユーザー情報取得処理
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>ユーザー情報</returns>
        UnaryResult<User> GetUserInfoAsync(int userId);

        /// <summary>
        /// 全ユーザー情報取得処理
        /// </summary>
        /// <returns>ユーザー情報</returns>
        UnaryResult<User[]> GetAllUserInfoAsync();

        /// <summary>
        /// ユーザー名変更処理
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="userName">変更後のユーザー名</param>
        /// <returns></returns>
        UnaryResult<bool> UpdateUserInfoAsync(int userId, string userName);
    }

}
