using System.Collections.Generic;
using UnityEngine;
using ANut.Core;

namespace Game.Battle
{
    public class TankRegistry
    {
        private readonly List<Tank> _allTanks = new();
        private readonly Dictionary<int, Tank> _byId = new();

        public void Register(Tank tank)
        {
            if (_byId.ContainsKey(tank.Id))
            {
                Log.Warning("[TankRegistry] tank Id={0} already registred", tank.Id);
                return;
            }

            _allTanks.Add(tank);
            _byId[tank.Id] = tank;
        }

        public Tank GetById(int id) =>
            _byId.TryGetValue(id, out var tank) ? tank : null;

        public Tank GetPlayerTank() =>
            _allTanks.Find(t => t.IsPlayer);

        public IReadOnlyList<Tank> GetAll() => _allTanks;

        public Tank FindClosestEnemy(Vector3 position, float radius, Tank requester)
        {
            float sqrRadius = radius * radius;

            Tank closest = null;
            float closestSqrDist = float.MaxValue;

            foreach (var tank in _allTanks)
            {
                if (!IsAliveEnemy(requester, tank)) continue;

                float sqrDist = (tank.transform.position - position).sqrMagnitude;
                if (sqrDist <= sqrRadius && sqrDist < closestSqrDist)
                {
                    closestSqrDist = sqrDist;
                    closest = tank;
                }
            }

            return closest;
        }

        private static bool IsAliveEnemy(Tank requester, Tank tank)
        {
            if (tank == requester) return false;
            if (tank.IsDead) return false;

            // In team mode, tanks from the same team are not enemies.
            if (requester.TeamId != TeamSide.None && tank.TeamId == requester.TeamId) return false;

            return true;
        }

        public void FreezeAll()
        {
            foreach (var tank in _allTanks)
                tank.SetFrozen(true);
        }

        public void UnfreezeAll()
        {
            foreach (var tank in _allTanks)
                tank.SetFrozen(false);
        }
    }
}