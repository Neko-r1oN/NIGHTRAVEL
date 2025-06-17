////////////////////////////////////////////////////////////////
///
/// レリック関連の通信を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using MagicOnion;
using MagicOnion.Server;
using NIGHTRAVEL.Server.Model.Context;
using NIGHTRAVEL.Shared.Model.Entity;
using NIGHTRAVEL.Shared.Services;

namespace NIGHTRAVEL.Server.Services
{
    public class RelicService : ServiceBase<IRelicService>,IRelicService
    {
        //レリックをIDで取得
        public async UnaryResult<Relic> GetRelic(int id)
        {
            //DBを取得
            using var context = new GameDbContext();

            //レリックのデータ格納変数を定義
            Relic relic = new Relic();

            //バリデーションチェック
            if (context.Enemies.Where(relic => relic.id == id).Count() < id || id <= 0)
            {//レリックの登録数分を超過、0以下の入力がされた場合

                //400エラー表示
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument,
                    "そのIDのレリックは登録されていません");
            }

            //テーブルからレコードをidを指定して取得
            relic = context.Relics.Where(relic => relic.id == id).First();

            //レリックのデータを返す
            return relic;
        }

        //レリックの一覧を取得
        public async UnaryResult<Relic[]> GetAllRelic()
        {
            //DBを取得
            using var context = new GameDbContext();

            //テーブルからレコードをidを指定して取得
            Relic[] relics = context.Relics.ToArray();

            //敵のデータを返す
            return relics;
        }

        //レリックをレア度で取得
        public async UnaryResult<Relic[]> GetRarityRelic(int rarity)
        {
            //DBを取得
            using var context = new GameDbContext();

            //検索結果を格納
            List<Relic> getResult = new List<Relic>();

            //レリックを全取得
            Relic[] relics = context.Relics.ToArray();

            foreach (var data in relics)
            {//dataはenemiesの0番目データからループ
                if (data.rarity == rarity)
                {//そのデータが指定されたIDと一致したら

                    //検索結果に格納
                    getResult.Add(data);
                }
            }

            //バリデーションチェック
            if (getResult.Count == 0)
            {//getResultにデータが入っていない場合

                //400エラー表示
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument,
                    "そのレア度のレリックが登録されていないか、無効な入力です。");
            }

            //検索結果を返す
            return getResult.ToArray();
        }

        //レリックを名前で取得
        public async UnaryResult<Relic> GetNameRelic(string name)
        {
            //検索出来たかどうかの判定
            bool isSerch = false;

            //DBを取得
            using var context = new GameDbContext();

            //検索結果を格納
            Relic relic = new Relic();

            //レリックを全取得
            Relic[] relics = context.Relics.ToArray();

            foreach (var data in relics)
            {//dataはenemiesの0番目データからループ
                if (data.name == name)
                {//そのデータが指定された名前と一致したら

                    //検索出来たことにする
                    isSerch = true;

                    //検索結果に格納
                    relic = data;
                }
            }

            //バリデーションチェック
            if (isSerch == false)
            {//getResultにデータが入っていない場合

                //400エラー表示
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument,
                    "その名前のレリックが登録されていないか、無効な入力です。");
            }

            //検索結果を返す
            return relic;
        }
    }
}
