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


namespace NIGHTRAVEL.Server.Services
{
    public class StatusEnhancementService : ServiceBase<IStatusEnhancementService>, IStatusEnhancementService
    {
        //ステータス強化をid指定で取得
        public async UnaryResult<Status_Enhancement> GetStatusEnhancement(int id)
        {

            //データベースを取得
            using var context = new GameDbContext();

            //テーブルからレコードをidを指定して取得
            Status_Enhancement statusEnhancement = 
                    context.Status_Enhancements.Where(statusEnhancement =>
                                                            statusEnhancement.id == id).First();

                //ステータス強化のデータを返す
                return statusEnhancement;
            
        }
    }
}
