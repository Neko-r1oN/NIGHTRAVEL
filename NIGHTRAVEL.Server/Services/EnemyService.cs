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
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using NIGHTRAVEL.Shared.Interfaces.Services;

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

            //敵のデータ格納変数を定義
            Enemy enemy = new Enemy();

            //バリデーションチェック
            if (context.Enemies.Where(enemy => enemy.id == id).Count() < id || id <= 0)
            {//敵の登録数分を超過、0以下の入力がされた場合

                //400エラー表示
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument,
                    "そのIDの敵は登録されていません");
            }

            //テーブルからレコードをidを指定して取得
            enemy = context.Enemies.Where(enemy => enemy.id == id).First();

            //敵のデータを返す
            return enemy;
        }

        //敵の一覧を取得
        public async UnaryResult<Enemy[]> GetAllEnemy()
        {
            //DBを取得
            using var context = new GameDbContext();

            //テーブルからレコードをidを指定して取得
            Enemy[] enemy = context.Enemies.ToArray();

            //敵のデータを返す
            return enemy;
        }

        //敵をステージIDで取得
        public async UnaryResult<Enemy[]> GetStageEnemy(string stage_id)
        {
            //DBを取得
            using var context = new GameDbContext();

            //検索結果を格納
            List<Enemy> getResult = new List<Enemy>();

            //ステージIDを,で区切る


            //敵を全取得
            Enemy[] enamies = context.Enemies.ToArray();

            foreach(var data in enamies)
            {//dataはenemiesの0番目データからループ

                //ステージIDを,で区切る
                string[] stageId = data.stage_id.ToString().Split(',');

                for (int i = 0; i < stageId.Length; i++) 
                {
                    if (stageId[i] == stage_id)
                    {//そのデータが指定されたIDと一致したら

                        //検索結果に格納
                        getResult.Add(data);break;
                    }
                }
                
            }

            //バリデーションチェック
            if (getResult.Count == 0)
            {//getResultにデータが入っていない場合

                //400エラー表示
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, 
                    "そのステージIDに敵が登録されていないか、無効な入力です。");
            }

            //検索結果を返す
            return getResult.ToArray();
        }

        //敵を名前で取得
        public async UnaryResult<Enemy> GetNameEnemy(string name)
        {
            //検索出来たかどうかの判定
            bool isSerch = false;

            //DBを取得
            using var context = new GameDbContext();

            //検索結果を格納
            Enemy enemy = new Enemy();

            //敵を全取得
            Enemy[] enamies = context.Enemies.ToArray();

            foreach (var data in enamies)
            {//dataはenemiesの0番目データからループ
                if (data.name == name)
                {//そのデータが指定された名前と一致したら

                    //検索出来たことにする
                    isSerch = true;

                    //検索結果に格納
                    enemy =data;
                }
            }

            //バリデーションチェック
            if (isSerch == false)
            {//getResultにデータが入っていない場合

                //400エラー表示
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument,
                    "その名前の敵が登録されていないか、無効な入力です。");
            }

            //検索結果を返す
            return enemy;
        }
    }
}
