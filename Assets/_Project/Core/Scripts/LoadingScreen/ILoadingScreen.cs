using Cysharp.Threading.Tasks;
using System.Threading;

namespace ANut.Core.LoadingScreen
{
    public interface ILoadingScreen
    {
        void ShowImmediate();

        bool IsVisible { get; }

        UniTask ShowAsync(CancellationToken ct);

        UniTask HideAsync(CancellationToken ct);

        void SetProgress(float progress);
    }
}