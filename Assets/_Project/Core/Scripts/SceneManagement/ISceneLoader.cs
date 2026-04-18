using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace ANut.Core.SceneManagement
{
    public interface ISceneLoader
    {
        UniTask LoadSceneAsync(string sceneName, CancellationToken ct, Action<float> onProgress = null);

        UniTask LoadSceneAdditiveAsync(string sceneName, CancellationToken ct, Action<float> onProgress = null);

        UniTask UnloadSceneAsync(string sceneName, CancellationToken ct);
    }
}