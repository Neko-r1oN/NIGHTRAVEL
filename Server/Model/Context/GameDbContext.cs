////////////////////////////////////////////////////////////////////////////
///
///  接続先設定スクリプト
///  Author : 川口京佑  2025.01/28
///
////////////////////////////////////////////////////////////////////////////


using Microsoft.EntityFrameworkCore;
using Shared.Model.Entity;

namespace MagicOnionServer.Model.Context
{
    public class GameDbContext : DbContext
    {
        //------------------------------------------------------
        //テーブルを追加したら書き足す

        public DbSet<User> Users { get; set; }



        //------------------------------------------------------

        //接続先を規定

#if DEBUG
        //ローカル用
        readonly string connectionString = "server=localhost;database=realtime_game;user=jobi;password=jobi;";
#else
        //サーバー用
        readonly string connectionString = "server=db-ge-05.mysql.database.azure.com;database=realtime_game;user=student;password=Yoshidajobi2023;";

#endif
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0)));
        }
    }
}
