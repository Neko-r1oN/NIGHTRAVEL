////////////////////////////////////////////////////////////////
///
/// レリック関連の通信を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using MagicOnion;
////////////////////////////////////////////////////////////////
///
/// レリック関連の通信を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NIGHTRAVEL.Shared.Interfaces.Services
{
    /// <summary>
    /// レリックのインターフェースの追加(Shared)
    /// </summary>
    public interface IRelicService : IService<IRelicService>
    {
        //レリックをIDで取得
        UnaryResult<Relic> GetRelic(int id);

        //レリックの一覧を取得
        UnaryResult<Relic[]> GetAllRelic();

        //レリックをレア度で取得
        UnaryResult<Relic[]> GetRarityRelic(int rarity);

        //レリックを名前で取得
        UnaryResult<Relic> GetNameRelic(string name);

    }
}
