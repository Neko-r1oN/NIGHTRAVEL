////////////////////////////////////////////////////////////////
///
/// 敵関連の通信を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////

using MagicOnion;
using NIGHTRAVEL.Shared.Interfaces.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NIGHTRAVEL.Shared.Interfaces.Services
{
    /// <summary>
    /// 難易度のインターフェースの追加(Shared)
    /// </summary>
    public interface IDifficultyService : IService<IDifficultyService>
    {
        //難易度をidで指定して取得
        UnaryResult<Difficulty> GetDifficulty(int id);

        //難易度一覧を取得
        UnaryResult<Difficulty[]> GetAllDifficulty();

    }
}
