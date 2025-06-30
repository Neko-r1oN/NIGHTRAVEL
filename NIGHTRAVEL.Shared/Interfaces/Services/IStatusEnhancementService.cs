////////////////////////////////////////////////////////////////
///
/// ステータス強化関連の通信を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using MagicOnion;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;

namespace NIGHTRAVEL.Shared.Interfaces.Services
{
    /// <summary>
    /// ステータス強化のインターフェースの追加(Shared)
    /// </summary>
    public interface IStatusEnhancementService : IService<IStatusEnhancementService>
    {
        //ステータス強化を識別名で取得
        UnaryResult<Status_Enhancement[]> GetStatusEnhancement(string enhancementType);

        //ステータス強化をすべて取得
        UnaryResult<Status_Enhancement[]> GetAllStatusEnhancement();
    }
}
