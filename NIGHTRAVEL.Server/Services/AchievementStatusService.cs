////////////////////////////////////////////////////////////////
///
/// 実績の進捗関連の通信を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using MagicOnion;
using MagicOnion.Server;
using NIGHTRAVEL.Server.Model.Context;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using NIGHTRAVEL.Shared.Interfaces.Services;
using System.Diagnostics;

namespace NIGHTRAVEL.Server.Services
{
    /// <summary>
    /// 実績の進捗API追加
    /// </summary>
    public class AchievementStatusService : ServiceBase<IAchievementStatusService>, IAchievementStatusService
    {
        //実績進捗をIDで取得
        public async UnaryResult<AchievementStatus> GetAchievementStatus(int id)
        {
            //DBを取得
            using var context = new GameDbContext();

            //テーブル情報の変数定義
            AchievementStatus achievementStatus = new AchievementStatus();

            //バリデーションチェック
            if (context.Achievement_Statuses.Count() < id || id <= 0)
            {//実績のIDが登録分を超過、無効な入力の場合

                //400エラー表示
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument,
                    "そのIDの実績進捗は登録されていません");
            }

            //テーブルからレコードをidを指定して取得
            achievementStatus = context.Achievement_Statuses.Where(achievementStatus => achievementStatus.id == id).First();


            //実績進捗のデータを返す
            return achievementStatus;
        }

        //実績進捗一覧を取得
        public async UnaryResult<AchievementStatus[]> GetAllAchievementStatus()
        {
            //データベースを取得
            using var context = new GameDbContext();

            //テーブルからレコードをすべて取得
            AchievementStatus[] achievementStatuses = context.Achievement_Statuses.ToArray();

            //実績進捗のデータを返す
            return achievementStatuses;
        }

        //実績進捗をユーザーのIDで取得
        public async UnaryResult<AchievementStatus[]> GetUserAchievementStatus(int user_id)
        {
            //実績進捗の検索結果を格納
            List<AchievementStatus> getResult = new List<AchievementStatus>();

            //データベースを取得
            using var context = new GameDbContext();

            //実績進捗を全取得
            AchievementStatus[] achievementStatuses = context.Achievement_Statuses.ToArray();

            //識別名を検索して
            foreach (var data in achievementStatuses)
            {//dataはachievementStatusesの0番目から順にループする

                    if (data.achievement_id== user_id)
                    {//送られてきたユーザーのIDとデータが合致したら

                        //データを格納
                        getResult.Add(data); break;
                    }
            }

            //バリデーションチェック
            if (getResult.Count == 0)
            {//getResultに何も格納されなかったら

                //400エラー表示
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument,
                    "指定タイプのデータが存在しないか、無効な入力です。");
            }

            //実績進捗のデータを返す
            return getResult.ToArray();
        }

        //実績進捗を実績IDで取得
        public async UnaryResult<AchievementStatus[]> GetAchievementIdStatus(int achievement_id)
        {
            //実績進捗の検索結果を格納
            List<AchievementStatus> getResult = new List<AchievementStatus>();

            //データベースを取得
            using var context = new GameDbContext();

            //実績進捗を全取得
            AchievementStatus[] achievementStatuses = context.Achievement_Statuses.ToArray();

            //識別名を検索して
            foreach (var data in achievementStatuses)
            {//dataはachievementStatusesの0番目から順にループする

                if (data.achievement_id == achievement_id)
                {//送られてきたユーザーのIDとデータが合致したら

                    //データを格納
                    getResult.Add(data); break;
                }
            }

            //バリデーションチェック
            if (getResult.Count == 0)
            {//getResultに何も格納されなかったら

                //400エラー表示
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument,
                    "指定タイプのデータが存在しないか、無効な入力です。");
            }

            //実績進捗のデータを返す
            return getResult.ToArray();
        }
    }
}
