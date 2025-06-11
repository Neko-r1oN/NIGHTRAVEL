////////////////////////////////////////////////////////////////
///
/// インターフェース管理テスト用スクリプト
/// 
/// Aughter:高宮祐翔
///
////////////////////////////////////////////////////////////////
using MagicOnion;
using UnityEngine;

namespace NIGHTRAVEL.Shared.Services
{
    /// <summary>
    /// インターフェースを設定
    /// </summary>
    public interface IMyFirstService : IService<IMyFirstService>
    {
        //足し算
        UnaryResult<int> SumAsync(int x, int y);
    }
}