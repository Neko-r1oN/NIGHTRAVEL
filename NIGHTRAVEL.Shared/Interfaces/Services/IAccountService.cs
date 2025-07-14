////////////////////////////////////////////////////////////////
///
/// アカウント関連の通信を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using MagicOnion;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;

namespace NIGHTRAVEL.Shared.Interfaces.Services
{
    /// <summary>
    /// アカウントのAPI追加(public)
    /// </summary>
    public interface IAccountService : IService<IAccountService>
    {
        //アカウントを登録
        UnaryResult<int> RegistAccountAsync(string name, string password);

        //アカウント一覧を取得
        UnaryResult<Account[]> GetAllAccountsAsync();

        //アカウントをidから取得
        UnaryResult<Account> GetAccountAsync(int id);
    }
}
