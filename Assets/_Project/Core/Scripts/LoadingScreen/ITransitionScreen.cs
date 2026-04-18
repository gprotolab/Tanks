using Cysharp.Threading.Tasks;
using System.Threading;

namespace ANut.Core.LoadingScreen
{
    public interface ITransitionScreen
    {
        bool IsVisible { get; }

        UniTask ShowAsync(CancellationToken ct);

        UniTask HideAsync(CancellationToken ct);
    }
}