using MagicOnion;
using UnityEngine;

namespace NIGHTRAVEL.Shared.Unity
{
    public interface IMyFirstService : IService<IMyFirstService>
    {
        UnaryResult<int> SumAsync(int x, int y);
    }
}