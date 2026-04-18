using System.Collections.Generic;
using UnityEngine;
using ANut.Core;

namespace Game.Battle
{
    public class ProjectilePool
    {
        private readonly Transform _dynamicRoot;

        private readonly Dictionary<ProjectileConfigSO, Queue<Projectile>> _projectilePools = new();
        private readonly Dictionary<ProjectileConfigSO, Queue<GameObject>> _effectPools = new();

        // Reverse map instance -> config to return objects into the correct queue.
        private readonly Dictionary<Projectile, ProjectileConfigSO> _projectileToConfig = new();
        private readonly Dictionary<GameObject, ProjectileConfigSO> _effectToConfig = new();

        public ProjectilePool(DynamicObjectsRoot dynamicObjectsRoot)
        {
            _dynamicRoot = dynamicObjectsRoot.transform;
        }

        public void WarmUp(ProjectileConfigSO[] configs, int countPerType = 10)
        {
            if (configs == null) return;

            foreach (var config in configs)
            {
                if (config == null) continue;

                if (!_projectilePools.ContainsKey(config))
                    WarmUpProjectiles(config, countPerType);

                if (config.HitEffectPrefab != null && !_effectPools.ContainsKey(config))
                    WarmUpEffects(config, countPerType);
            }
        }

        public Projectile Get(ProjectileConfigSO config)
        {
            if (config == null || config.Prefab == null)
            {
                Log.Error("[ProjectilePool] ProjectileConfig or Prefab is null!");
                return null;
            }

            if (_projectilePools.TryGetValue(config, out var queue) && queue.Count > 0)
            {
                var pooled = queue.Dequeue();
                if (pooled != null) return pooled;
            }

            return CreateProjectile(config);
        }

        public void Return(Projectile projectile)
        {
            if (projectile == null) return;

            if (!_projectileToConfig.TryGetValue(projectile, out var config)) return;

            if (!_projectilePools.TryGetValue(config, out var queue))
            {
                queue = new Queue<Projectile>();
                _projectilePools[config] = queue;
            }

            queue.Enqueue(projectile);
        }

        public GameObject GetHitEffect(ProjectileConfigSO config)
        {
            if (config?.HitEffectPrefab == null) return null;

            if (_effectPools.TryGetValue(config, out var queue) && queue.Count > 0)
            {
                var pooled = queue.Dequeue();
                if (pooled != null) return pooled;
            }

            return CreateEffect(config);
        }

        public void ReturnHitEffect(GameObject effect)
        {
            if (effect == null) return;

            effect.SetActive(false);

            if (!_effectToConfig.TryGetValue(effect, out var config)) return;

            if (!_effectPools.TryGetValue(config, out var queue))
            {
                queue = new Queue<GameObject>();
                _effectPools[config] = queue;
            }

            queue.Enqueue(effect);
        }

        private void WarmUpProjectiles(ProjectileConfigSO config, int count)
        {
            var queue = new Queue<Projectile>(count);
            _projectilePools[config] = queue;

            for (int i = 0; i < count; i++)
            {
                var p = CreateProjectile(config);
                if (p != null)
                    queue.Enqueue(p);
            }
        }

        private void WarmUpEffects(ProjectileConfigSO config, int count)
        {
            var queue = new Queue<GameObject>(count);
            _effectPools[config] = queue;

            for (int i = 0; i < count; i++)
            {
                var e = CreateEffect(config);
                if (e != null)
                    queue.Enqueue(e);
            }
        }

        private Projectile CreateProjectile(ProjectileConfigSO config)
        {
            var projectile = Object.Instantiate(config.Prefab, _dynamicRoot);
            projectile.gameObject.SetActive(false);
            projectile.SetPhysicsLayers();
            _projectileToConfig[projectile] = config;
            return projectile;
        }

        private GameObject CreateEffect(ProjectileConfigSO config)
        {
            var go = Object.Instantiate(config.HitEffectPrefab, _dynamicRoot);
            go.SetActive(false);
            _effectToConfig[go] = config;
            return go;
        }
    }
}