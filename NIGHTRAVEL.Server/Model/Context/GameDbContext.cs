////////////////////////////////////////////////////////////////
///
/// データベースとの接続を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using Microsoft.EntityFrameworkCore;
using NIGHTRAVEL.Server.Services;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;

namespace NIGHTRAVEL.Server.Model.Context
{
    /// <summary>
    /// データベースの設定、SQLとの接続を行う
    /// </summary>
    public class GameDbContext : DbContext
    {

        #region データベース設定一覧
        //アカウントのデータベース設定
        public DbSet<Account> Accounts { get; set; }

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

        //レリックのデータベース設定
        public DbSet<Relic> Relics { get; set; }

        //実績のデータベース設定
        public DbSet<Achievement> Achievements { get; set; }

        //ユーザーの実績進捗のデータベース設定
        public DbSet<AchievementStatus> Achievement_Statuses { get; set; }

        //ルーム
        public DbSet<Room> Rooms { get; set; }
        #endregion

#if DEBUG
        //server名;ユーザー名;パスワード指定
        readonly string connectionString = "server=localhost;database=admin_console;user=jobi;password=jobi;";
#else
        readonly string connectionString = "server=db-ge-07.mysql.database.azure.com;database=nightraveldb;user=student;password=Yoshidajobi2023;";
#endif

        //SQLと接続
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString,
                                                new MySqlServerVersion(new Version(8, 0)));
        }
    }
}
