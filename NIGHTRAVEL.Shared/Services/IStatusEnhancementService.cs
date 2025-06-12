////////////////////////////////////////////////////////////////
///
/// ステータス強化関連の通信を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////
using Grpc.Core;
using MagicOnion;
using NIGHTRAVEL.Server.Model.Entity;
using NIGHTRAVEL.Shared.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NIGHTRAVEL.Shared.Services
{
    /// <summary>
    /// ステータス強化のインターフェースの追加(Shared)
    /// </summary>
    public interface IStatusEnhancementService : IService<IStatusEnhancementService> 
   {
        //ステータス強化をid指定で取得
        UnaryResult<Status_Enhancement> GetStatusEnhancement(int id);
    }
}
