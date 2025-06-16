using MagicOnion;
using MagicOnion.Server;
using NIGHTRAVEL.Server.Model.Context;
using NIGHTRAVEL.Server.Model.Entity;
using NIGHTRAVEL.Shared.Model.Entity;
using NIGHTRAVEL.Shared.Services;
using System.Diagnostics;

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

            //テーブルからレコードをidを指定して取得
            Difficulty difficulty = context.Difficulties.Where(difficulty => difficulty.id == id).First();

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
