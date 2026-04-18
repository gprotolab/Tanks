using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ANut.Core.AssetLoading
{
    public interface IAssetLoader
    {
        UniTask<T> LoadAsync<T>(string address, CancellationToken ct) where T : UnityEngine.Object;

        UniTask<T> LoadAsync<T>(AssetReference reference, CancellationToken ct) where T : UnityEngine.Object;

        UniTask PreloadLabelAsync(string label, CancellationToken ct, Action<float> onProgress = null);

        void Release(string address);

        void ReleaseAll();
    }
}