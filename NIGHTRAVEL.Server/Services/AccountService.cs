using MagicOnion;
using MagicOnion.Server;
using NIGHTRAVEL.Server.Model.Context;
using NIGHTRAVEL.Server.Model.Entity;
using NIGHTRAVEL.Shared.Model.Entity;
using NIGHTRAVEL.Shared.Services;
using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace NIGHTRAVEL.Server.Services
{
    public class AccountService : ServiceBase<IAccountService>, IAccountService
    {
        

        //アカウントを登録
        public async UnaryResult<int> RegistAccountAsync(string name, string password)
        {
            //データベースを取得
            using var context = new GameDbContext();


            //テーブルにレコードを追加
            Account account = new Account();        //Accountデータ7
            account.account_name = name;            //アカウント名
            account.password = password;            //パスワード
            account.Created_at = DateTime.Now;      //生成日時
            account.Updated_at = DateTime.Now;      //更新日時
            context.Accounts.Add(account);          //データベースに格納
            await context.SaveChangesAsync();       //データベースを保存する
            return account.id;                      //アカウントのidを返す
        }

        //アカウント一覧を取得
        public async UnaryResult<Account[]> GetAllAccountsAsync()
        {
            //データベースを取得
            using var context = new GameDbContext();

            //テーブルからレコードをすべて取得
            Account[] accounts = context.Accounts.ToArray();

            //ユーザーのデータを返す
            return accounts;
        }

        //アカウントをidから取得
        public async UnaryResult<Account> GetAccountAsync(int id)
        {
            //DBを取得
            using var context = new GameDbContext();

            //テーブル情報の変数定義
            Account account = new Account();

            //バリデーションチェック
            if (context.Accounts.Where(account => account.id == id).Count() < id || id <= 0)
            {//アカウントのIDが登録分を超過、無効な入力の場合

                //400エラー表示
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument,
                    "そのIDのアカウントは登録されていません");
            }

            //テーブルからレコードをidを指定して取得
            account = context.Accounts.Where(account => account.id == id).First();


            //ユーザーのデータを返す
            return account;
        }
    }
}
