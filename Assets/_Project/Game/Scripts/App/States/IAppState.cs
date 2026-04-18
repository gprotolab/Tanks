using Cysharp.Threading.Tasks;
using System.Threading;

namespace Game.App
{
    public interface IAppState
    {
        UniTask Enter(CancellationToken ct);
        UniTask Exit(CancellationToken ct);
    }
}