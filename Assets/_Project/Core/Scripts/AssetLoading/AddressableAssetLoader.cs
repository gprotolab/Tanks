using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ANut.Core.AssetLoading
{
    public class AddressableAssetLoader : IAssetLoader
    {
        private struct CacheEntry
        {
            public AsyncOperationHandle Handle;
            public int RefCount;

            public CacheEntry(AsyncOperationHandle handle, int refCount)
            {
                Handle = handle;
                RefCount = refCount;
            }
        }

        private readonly Dictionary<string, CacheEntry> _cache = new();

        public async UniTask<T> LoadAsync<T>(string address, CancellationToken ct)
            where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("[AssetLoader] Address must not be null or empty.", nameof(address));

            if (_cache.TryGetValue(address, out var cached))
            {
                cached.RefCount++;
                _cache[address] = cached;
                return (T) cached.Handle.Result;
            }

            var handle = Addressables.LoadAssetAsync<T>(address);

            try
            {
                // ToUniTask throws on Failed status — no need to re-check handle.Status afterwards.
                await handle.ToUniTask(cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                // Release the handle if loading was cancelled before completion.
                if (handle.IsValid())
                    Addressables.Release(handle);
                throw;
            }
            catch
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
                throw;
            }

            _cache[address] = new CacheEntry(handle, 1);

            return handle.Result;
        }

        public UniTask<T> LoadAsync<T>(AssetReference reference, CancellationToken ct)
            where T : UnityEngine.Object
        {
            if (reference == null)
                throw new ArgumentNullException(nameof(reference), "[AssetLoader] AssetReference must not be null.");

            // AssetGUID is the stable identifier; matches what Addressables uses internally.
            return LoadAsync<T>(reference.AssetGUID, ct);
        }

        public async UniTask PreloadLabelAsync(string label, CancellationToken ct, Action<float> onProgress = null)
        {
            if (string.IsNullOrEmpty(label))
                throw new ArgumentException("[AssetLoader] Label must not be null or empty.", nameof(label));

            // Step 1: resolve all resource locations for the label.
            AsyncOperationHandle<IList<IResourceLocation>> locationsHandle =
                Addressables.LoadResourceLocationsAsync(label, typeof(UnityEngine.Object));
            IList<IResourceLocation> locations;

            try
            {
                await locationsHandle.ToUniTask(cancellationToken: ct);
                locations = locationsHandle.Result;
            }
            finally
            {
                // Always release the locations handle — it is a temporary query handle.
                if (locationsHandle.IsValid())
                    Addressables.Release(locationsHandle);
            }

            if (locations == null || locations.Count == 0)
            {
                onProgress?.Invoke(1f);
                return;
            }

            // Step 2: if a progress callback is provided, load sequentially for granular reporting.
            //        Otherwise load all assets in parallel for maximum throughput.
            if (onProgress != null)
            {
                int total = locations.Count;
                int loaded = 0;

                foreach (var location in locations)
                {
                    ct.ThrowIfCancellationRequested();

                    string address = location.PrimaryKey;

                    if (_cache.TryGetValue(address, out var entry))
                    {
                        entry.RefCount++;
                        _cache[address] = entry;
                    }
                    else
                    {
                        await LoadAsync<UnityEngine.Object>(address, ct);
                    }

                    onProgress.Invoke((float) ++loaded / total);
                }
            }
            else
            {
                // Parallel load — significantly faster when progress reporting is not needed.
                var tasks = locations
                    .Where(loc => !_cache.ContainsKey(loc.PrimaryKey))
                    .Select(loc => LoadAsync<UnityEngine.Object>(loc.PrimaryKey, ct));

                await UniTask.WhenAll(tasks);
            }

            onProgress?.Invoke(1f);
        }

        public void Release(string address)
        {
            if (string.IsNullOrEmpty(address))
                return;

            if (!_cache.TryGetValue(address, out var entry))
                return;

            if (entry.RefCount > 1)
            {
                entry.RefCount--;
                _cache[address] = entry;
                return;
            }

            // Reference count reached zero — unload from memory.
            if (entry.Handle.IsValid())
                Addressables.Release(entry.Handle);

            _cache.Remove(address);
        }

        public void ReleaseAll()
        {
            foreach (var entry in _cache.Values)
            {
                if (entry.Handle.IsValid())
                    Addressables.Release(entry.Handle);
            }

            _cache.Clear();
        }
    }
}