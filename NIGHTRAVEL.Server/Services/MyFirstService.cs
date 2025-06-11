////////////////////////////////////////////////////////////////
///
/// ���A���^�C���ʐM�e�X�g�p�X�N���v�g
/// 
/// Aughter:���{�S��
///
////////////////////////////////////////////////////////////////

using MagicOnion;
using MagicOnion.Server;
using NIGHTRAVEL.Shared;
using NIGHTRAVEL.Shared.Services;
using UnityEngine;

namespace NIGHTRAVEL.Server.Services;

// Implements RPC service in the server project.
// The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
{
    // `UnaryResult<T>` allows the method to be treated as `async` method.
    public async UnaryResult<int> SumAsync(int x, int y)
    {
        Console.WriteLine($"Received: {x}, {y}");

        return x+y;
    }
}