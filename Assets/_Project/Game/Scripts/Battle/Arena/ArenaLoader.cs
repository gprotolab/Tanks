using Cysharp.Threading.Tasks;
using ANut.Core.AssetLoading;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using ANut.Core;

namespace Game.Battle
{
    public class ArenaLoader
    {
        private readonly ArenaCatalogSO _catalog;
        private readonly IAssetLoader _assetLoader;

        public ArenaLoader(
            ArenaCatalogSO catalog,
            IAssetLoader assetLoader)
        {
            _catalog = catalog;
            _assetLoader = assetLoader;
        }

        public async UniTask<ArenaData> LoadAsync(CancellationToken ct)
        {
            var prefabReference = ResolveArenaReference();
            if (prefabReference == null || !prefabReference.RuntimeKeyIsValid())
            {
                Log.Error("[ArenaLoader] Arena reference is not configured for current battle mode.");
                return null;
            }

            var address = prefabReference.AssetGUID;

            // Use IAssetLoader so Addressables handles are cached and reference-counted.
            var prefab = await _assetLoader.LoadAsync<GameObject>(prefabReference, ct);

            var instance = Object.Instantiate(prefab);

            var allPoints = instance.GetComponentsInChildren<ArenaSpawnPoint>();

            var ffaPoints = allPoints
                .Where(sp => sp.Mode == SpawnMode.FFA)
                .Select(sp => sp.transform)
                .ToArray();

            var teamAPoints = allPoints
                .Where(sp => sp.Mode == SpawnMode.Team && sp.Side == TeamSide.A)
                .Select(sp => sp.transform)
                .ToArray();

            var teamBPoints = allPoints
                .Where(sp => sp.Mode == SpawnMode.Team && sp.Side == TeamSide.B)
                .Select(sp => sp.transform)
                .ToArray();

            return new ArenaData(instance, ffaPoints, teamAPoints, teamBPoints, address);
        }

        private AssetReferenceGameObject ResolveArenaReference()
        {
            var arenas = _catalog.Arenas;
            if (arenas == null || arenas.Length == 0)
                return null;

            var activeArenas = new System.Collections.Generic.List<ArenaCatalogSO.ArenaEntry>();
            for (int i = 0; i < arenas.Length; i++)
            {
                var arena = arenas[i];
                if (arena.IsActive && arena.PrefabReference != null && arena.PrefabReference.RuntimeKeyIsValid())
                    activeArenas.Add(arena);
            }

            if (activeArenas.Count == 0)
                return null;

            return activeArenas[Random.Range(0, activeArenas.Count)].PrefabReference;
        }

        public void Unload(ArenaData arena)
        {
            if (arena == null) return;

            if (arena.Instance != null)
                Object.Destroy(arena.Instance);

            if (!string.IsNullOrEmpty(arena.Address))
                _assetLoader.Release(arena.Address);
        }
    }
}