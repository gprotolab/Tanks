using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ANut.Core.SceneManagement
{
    public class SceneLoader : ISceneLoader
    {
        private bool _isLoading;

        public UniTask LoadSceneAsync(string sceneName, CancellationToken ct, Action<float> onProgress = null)
            => LoadInternalAsync(sceneName, LoadSceneMode.Single, ct, onProgress);

        public UniTask LoadSceneAdditiveAsync(string sceneName, CancellationToken ct, Action<float> onProgress = null)
            => LoadInternalAsync(sceneName, LoadSceneMode.Additive, ct, onProgress);

        public async UniTask UnloadSceneAsync(string sceneName, CancellationToken ct)
        {
            var operation = SceneManager.UnloadSceneAsync(sceneName);

            if (operation == null)
                throw new InvalidOperationException($"[SceneLoader] Failed to unload scene: {sceneName}");

            await operation.ToUniTask(cancellationToken: ct);
        }

        // AsyncOperation.progress goes from 0 to 0.9; the last 10% is scene activation.
        private async UniTask LoadInternalAsync(string sceneName, LoadSceneMode mode, CancellationToken ct,
            Action<float> onProgress)
        {
            if (_isLoading)
                throw new InvalidOperationException($"[SceneLoader] Already loading a scene. Requested: {sceneName}");

            _isLoading = true;
            try
            {
                var operation = SceneManager.LoadSceneAsync(sceneName, mode);

                if (operation == null)
                    throw new InvalidOperationException($"[SceneLoader] Failed to start loading scene: {sceneName}");
                while (!operation.isDone)
                {
                    ct.ThrowIfCancellationRequested();
                    onProgress?.Invoke(Mathf.Clamp01(operation.progress / 0.9f));
                    await UniTask.Yield(ct);
                }

                onProgress?.Invoke(1f);
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}