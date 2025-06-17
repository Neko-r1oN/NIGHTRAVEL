////////////////////////////////////////////////////////////////
///
/// ステータス強化関連の通信を管理するスクリプト
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
using System.Diagnostics;


namespace NIGHTRAVEL.Server.Services
{
    /// <summary>
    /// ステータス強化のApiを追加(public)
    /// </summary>
    public class StatusEnhancementService : ServiceBase<IStatusEnhancementService>, IStatusEnhancementService
    {
        //ステータス強化を識別名で取得
        public async UnaryResult<Status_Enhancement[]> GetStatusEnhancement(string enhancementType)
        {
            //ステータス強化の検索結果を格納
            List<Status_Enhancement>  getResult = new List<Status_Enhancement>();

            //データベースを取得
            using var context = new GameDbContext();

            //ステータス強化を全取得
            Status_Enhancement[] statusEnhancements = context.Status_Enhancements.ToArray();

            //識別名を検索して
            foreach (var data in statusEnhancements)
            {//dataはstatusEnhancementsの0番目から順にループする

                //,で区切って格納する
                string[] ehType = data.enhancement_type.ToString().Split(',');
                for(int i = 0; i < ehType.Length; i++) 
                {//識別名分ループ
                    if (ehType[i] == enhancementType)
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
                    "識別名のデータが存在しないか、無効な入力です。");
            }

            //ステータス強化のデータを返す
            return getResult.ToArray();
        }

        //ステータス強化をすべて取得
        public async UnaryResult<Status_Enhancement[]> GetAllStatusEnhancement()
        {
            //データベースを取得
            using var context = new GameDbContext();

            //ステータス強化を全取得
            Status_Enhancement[] statusEnhancements = context.Status_Enhancements.ToArray();

            //ステータス強化のデータを返す
            return statusEnhancements;
        }
    }
}
