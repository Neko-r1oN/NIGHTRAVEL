////////////////////////////////////////////////////////////////
///
/// 敵関連の通信を管理するスクリプト
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
    /// 敵のインターフェースの追加(Shared)
    /// </summary>
    public interface IEnemyService : IService<IEnemyService>
    {
        //敵をIDで取得
        UnaryResult<Enemy> GetEnemy(int id);

        //敵の一覧を取得
        UnaryResult<Enemy[]> GetAllEnemy();

        //敵をステージIDで取得
        UnaryResult<Enemy[]> GetStageEnemy(string stage_id);

        //敵を名前で取得
        UnaryResult<Enemy> GetNameEnemy(string name);
    }
}
