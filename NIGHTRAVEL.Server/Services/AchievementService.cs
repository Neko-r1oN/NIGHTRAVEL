using MagicOnion;
using MagicOnion.Server;
using NIGHTRAVEL.Server.Model.Context;
using NIGHTRAVEL.Server.Model.Entity;
using NIGHTRAVEL.Shared.Model.Entity;
using NIGHTRAVEL.Shared.Services;
using System.Diagnostics;

namespace NIGHTRAVEL.Server.Services
{
    public class AchievementService : ServiceBase<IAchievementService>, IAchievementService
    {
        //実績をIDで取得
        public async UnaryResult<Achievement> GetAchievement(int id)
        {
            //DBを取得
            using var context = new GameDbContext();

            //テーブル情報の変数定義
            Achievement achievement = new Achievement();

            //バリデーションチェック
            if (context.Users.Count() < id || id <= 0)
            {//実績のIDが登録分を超過、無効な入力の場合

                //400エラー表示
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument,
                    "そのIDのユーザーは登録されていません");
            }

            //テーブルからレコードをidを指定して取得
            achievement = context.Achievements.Where(achievement => achievement.id == id).First();


            //実績のデータを返す
            return achievement;
        }

        //実績一覧を取得
        public async UnaryResult<Achievement[]> GetAllAchievement()
        {
            //データベースを取得
            using var context = new GameDbContext();

            //テーブルからレコードをすべて取得
            Achievement[] achievements = context.Achievements.ToArray();

            //実績のデータを返す
            return achievements;
        }

        //実績をタイプで取得
        public async UnaryResult<Achievement[]> GetAchievementType(string type)
        {
            //実績の検索結果を格納
            List<Achievement> getResult = new List<Achievement>();

            //データベースを取得
            using var context = new GameDbContext();

            //実績を全取得
            Achievement[] achievements = context.Achievements.ToArray();

            //識別名を検索して
            foreach (var data in achievements)
            {//dataはachievementsの0番目から順にループする

                //,で区切って格納する
                string[] ehType = data.type.ToString().Split(',');
                for (int i = 0; i < ehType.Length; i++)
                {//識別名分ループ
                    if (ehType[i] == type)
                    {//送られてきた識別名とデータが合致したら

                        //データを格納
                        getResult.Add(data); break;
                    }
                }
            }

            //バリデーションチェック
            if (getResult.Count == 0)
            {//getResultに何も格納されなかったら

                //400エラー表示
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument,
                    "指定タイプのデータが存在しないか、無効な入力です。");
            }

            //実績のデータを返す
            return getResult.ToArray();
        }
    }
}
