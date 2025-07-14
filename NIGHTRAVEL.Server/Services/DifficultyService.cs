////////////////////////////////////////////////////////////////
///
/// 難易度関連の通信を管理するスクリプト
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
    /// 難易度のApiを追加(public)
    /// </summary>
    public class DifficultyService : ServiceBase<IDifficultyService>, IDifficultyService
    {
        //難易度をID指定で取得
        public async UnaryResult<Difficulty> GetDifficulty(int id)
        {
            //DBを取得
            using var context = new GameDbContext();

            //難易度のデータ格納変数を定義
            Difficulty difficulty = new Difficulty();

            //バリデーションチェック
            if (context.Difficulties.Count() < id || id <= 0)
            {//難易度の登録数分を超過、0以下の入力がされた場合

                //400エラー表示
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument,
                    "そのIDの難易度は登録されていません");
            }

            //テーブルからレコードをidを指定して取得
            difficulty = context.Difficulties.Where(difficulty => difficulty.id == id).First();

            //難易度のデータを返す
            return difficulty;
        }

        //難易度一覧を取得
        public async UnaryResult<Difficulty[]> GetAllDifficulty()
        {
            //DBを取得
            using var context = new GameDbContext();

            //テーブルからレコード全部取得
            Difficulty[] difficulty = context.Difficulties.ToArray();

            //難易度のデータを返す
            return difficulty;
        }
    }
}
