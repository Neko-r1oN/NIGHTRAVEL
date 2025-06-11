////////////////////////////////////////////////////////////////
///
/// データベースとの接続を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////
using Microsoft.EntityFrameworkCore;
using NIGHTRAVEL.Server.Model.Entity;


namespace NIGHTRAVEL.Server.Model.Context
{
    public class GameDbContext : DbContext
    {
        //ユーザーのデータベース設定
        public DbSet<User> Users { get; set; }

        //server名;ユーザー名;パスワード
        readonly string connectionString = "";

        //SQLと接続
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString,
                                                new MySqlServerVersion(new Version(8, 0)));
        }
    }
}
