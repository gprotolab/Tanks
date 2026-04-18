using Cysharp.Threading.Tasks;
using System.Threading;

namespace Game.Battle
{
    // Common interface for different battle modes.
    public interface IBattleFlow
    {
        UniTask RunAsync(CancellationToken ct);
    }
}