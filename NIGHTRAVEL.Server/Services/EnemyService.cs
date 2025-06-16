////////////////////////////////////////////////////////////////
///
/// 敵関連の通信を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////
using MagicOnion;
using MagicOnion.Server;
using NIGHTRAVEL.Server.Model.Context;
using NIGHTRAVEL.Server.Model.Entity;
using NIGHTRAVEL.Shared.Model.Entity;
using NIGHTRAVEL.Shared.Services;

namespace NIGHTRAVEL.Server.Services
{
    /// <summary>
    /// 敵のApiを追加(public)
    /// </summary>
    public class EnemyService : ServiceBase<IEnemyService>, IEnemyService
    {
        //敵をid指定で取得
        public async UnaryResult<Enemy> GetEnemy(int id)
        {
            //DBを取得
            using var context = new GameDbContext();

            //テーブルからレコードをidを指定して取得
            Enemy enemy = context.Enemies.Where(enemy => enemy.id == id).First();

            //ユーザーのデータを返す
            return enemy;
        }

        //敵の一覧を取得
        public async UnaryResult<Enemy[]> GetAllEnemy()
        {
            //DBを取得
            using var context = new GameDbContext();

            //テーブルからレコードをidを指定して取得
            Enemy[] enemy = context.Enemies.ToArray();

            //ユーザーのデータを返す
            return enemy;
        }

        //敵をステージIDで取得
        public async UnaryResult<Enemy[]> GetStageEnemy(int stage_id)
        {
            //DBを取得
            using var context = new GameDbContext();

            //検索結果を格納
            List<Enemy> getResult = new List<Enemy>();

            //敵を全取得
            Enemy[] enamies = context.Enemies.ToArray();

            foreach(var data in enamies)
            {//dataはenemiesの0番目データからループ
                if(data.stage_id==stage_id)
                {//そのデータが指定されたIDと一致したら

                    //検索結果に格納
                    getResult.Add(data);
                }
            }

            //検索結果を返す
            return getResult.ToArray();
        }

        //敵を名前で取得
        public async UnaryResult<Enemy> GetNameEnemy(string name)
        {
            //DBを取得
            using var context = new GameDbContext();

            //検索結果を格納
            Enemy enemy = new Enemy();

            //敵を全取得
            Enemy[] enamies = context.Enemies.ToArray();

            foreach (var data in enamies)
            {//dataはenemiesの0番目データからループ
                if (data.name == name)
                {//そのデータが指定されたIDと一致したら

                    //検索結果に格納
                    enemy=data;
                }
            }

            //検索結果を返す
            return enemy;
        }
    }
}
