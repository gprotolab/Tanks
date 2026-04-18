using System;
using System.Collections.Generic;
using MessagePipe;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Battle
{
    public class RespawnService : IDisposable
    {
        private const int SpawnCandidateCount = 15;
        private const float SpawnSearchRadius = 25f;
        private const float NavMeshSampleRadius = 5f;

        private readonly TankRegistry _tankRegistry;
        private readonly BattleConfigSO _battleConfig;
        private readonly IDisposable _subscription;

        private readonly List<RespawnEntry> _queue = new();

        // Reused buffer to avoid per-frame allocations while filtering ready entries.
        private readonly List<RespawnEntry> _readyBuffer = new();

        public RespawnService(
            TankRegistry tankRegistry,
            BattleConfigSO battleConfig,
            ISubscriber<TankDiedSignal> diedSubscriber)
        {
            _tankRegistry = tankRegistry;
            _battleConfig = battleConfig;

            _subscription = diedSubscriber.Subscribe(OnTankDied);
        }

        public void Tick(float dt)
        {
            if (_queue.Count == 0) return;

            _readyBuffer.Clear();

            for (int i = 0; i < _queue.Count; i++)
            {
                var entry = _queue[i];
                entry.RemainingDelay -= dt;
                _queue[i] = entry;

                if (entry.RemainingDelay <= 0f)
                    _readyBuffer.Add(entry);
            }

            foreach (var entry in _readyBuffer)
            {
                _queue.Remove(entry);
                PerformRespawn(entry.Tank);
            }
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _queue.Clear();
        }

        private void OnTankDied(TankDiedSignal signal)
        {
            var tank = _tankRegistry.GetById(signal.VictimId);
            if (tank == null) return;

            _queue.Add(new RespawnEntry
            {
                Tank = tank,
                RemainingDelay = _battleConfig.RespawnDelay
            });
        }

        private void PerformRespawn(Tank tank)
        {
            var position = FindBestSpawnPoint();
            tank.OnRespawn(position);
        }

        private Vector3 FindBestSpawnPoint()
        {
            var allTanks = _tankRegistry.GetAll();

            // Use average alive position as search center to keep spawns near active combat.
            Vector3 center = Vector3.zero;
            int aliveCount = 0;
            foreach (var t in allTanks)
            {
                if (!t.IsDead)
                {
                    center += t.transform.position;
                    aliveCount++;
                }
            }

            if (aliveCount > 0) center /= aliveCount;

            Vector3 bestPoint = center;
            float bestMinDist = -1f;

            for (int i = 0; i < SpawnCandidateCount; i++)
            {
                Vector2 circle = UnityEngine.Random.insideUnitCircle * SpawnSearchRadius;
                Vector3 candidate = center + new Vector3(circle.x, 0f, circle.y);

                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, NavMeshSampleRadius, NavMesh.AllAreas))
                    continue;

                // Score candidate by distance to the nearest alive tank.
                float minDist = float.MaxValue;
                foreach (var t in allTanks)
                {
                    if (t.IsDead) continue;
                    float dist = Vector3.Distance(hit.position, t.transform.position);
                    if (dist < minDist) minDist = dist;
                }

                if (minDist > bestMinDist)
                {
                    bestMinDist = minDist;
                    bestPoint = hit.position;
                }
            }

            return bestPoint;
        }

        private struct RespawnEntry
        {
            public Tank Tank;
            public float RemainingDelay;
        }
    }
}