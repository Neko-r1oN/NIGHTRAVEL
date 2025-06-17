////////////////////////////////////////////////////////////////
///
/// ステージ関連の通信を管理するスクリプト
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
    /// ステージのApiを追加(public)
    /// </summary>
    public class StageService : ServiceBase<IStageService>, IStageService
    {
        //ステージをID指定で取得
        public async UnaryResult<Stage> GetStage(int id)
        {
            //DBを取得
            using var context = new GameDbContext();

            //ステージのデータ格納変数を定義
            Stage stage = new Stage();

            //バリデーションチェック
            if (context.Stages.Where(stage => stage.id == id).Count() < id)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument,
                    "そのIDのステージは登録されていません");
            }

            //テーブルからレコードをidを指定して取得
            stage = context.Stages.Where(stage => stage.id == id).First();

            //ステージのデータを返す
            return stage;
        }

        //ステージの一覧を取得
        public async UnaryResult<Stage[]> GetAllStage()
        {
            //DBを取得
            using var context = new GameDbContext();

            //テーブルからレコードをidを指定して取得
            Stage[] stage = context.Stages.ToArray();

            //ステージのデータを返す
            return stage;
        }
    }
}
