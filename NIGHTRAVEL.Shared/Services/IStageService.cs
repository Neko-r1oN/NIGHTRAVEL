////////////////////////////////////////////////////////////////
///
/// ステージ関連の通信を管理するスクリプト
/// 
/// Aughter:木田晃輔
///
////////////////////////////////////////////////////////////////
using MagicOnion;
using NIGHTRAVEL.Shared.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NIGHTRAVEL.Shared.Services
{
    /// <summary>
    /// ステージのインターフェースの追加(Shared)
    /// </summary>
    public interface IStageService : IService<IStageService>
    {
        //ステージをID指定で取得
        UnaryResult<Stage> GetStage(int id);

        //ステージ一覧を取得
        UnaryResult<Stage[]> GetAllStage();
    }
}
