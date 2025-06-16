////////////////////////////////////////////////////////////////
///
/// データベースとの接続を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////
using Microsoft.EntityFrameworkCore;
using NIGHTRAVEL.Server.Model.Entity;
using NIGHTRAVEL.Server.Services;
using NIGHTRAVEL.Shared.Model.Entity;


namespace NIGHTRAVEL.Server.Model.Context
{
    /// <summary>
    /// データベースの設定、SQLとの接続を行う
    /// </summary>
    public class GameDbContext : DbContext
    {
        //ユーザーのデータベース設定
        public DbSet<User> Users { get; set; }

        //ステータス強化のデータベース設定
        public DbSet<Status_Enhancement> Status_Enhancements {  get; set; }        
        
        //敵のデータベース設定
        public DbSet<Enemy> Enemies {  get; set; }
        
        //難易度のデータベース設定
        public DbSet<Difficulty> Difficulties {  get; set; }

        //ステージのデータベース設定
        public DbSet<Stage> Stages { get; set; }

        //server名;ユーザー名;パスワード指定
        readonly string connectionString = "server=localhost;database=admin_console;user=jobi;password=jobi;";

        //SQLと接続
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString,
                                                new MySqlServerVersion(new Version(8, 0)));
        }
    }
}
