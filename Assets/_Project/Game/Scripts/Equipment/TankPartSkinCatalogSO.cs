using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ANut.Core;

namespace Game.Equipment
{
    [CreateAssetMenu(fileName = "TankPartCatalog", menuName = "Configs/TankPartCatalog")]
    public class TankPartSkinCatalogSO : ScriptableObject
    {
        [Header("Turret Parts")] [SerializeField]
        private List<PartEntry> _turretEntries = new();

        [Header("Chassis Parts")] [SerializeField]
        private List<PartEntry> _chassisEntries = new();

        public GameObject GetPrefab(TankPartType type, int level)
        {
            var entries = type == TankPartType.Turret ? _turretEntries : _chassisEntries;
            var valid = entries.Where(e => e.Prefab != null).ToList();

            if (valid.Count == 0)
            {
                Log.Error("[TankPartCatalog] No entries for {0}", type);
                return null;
            }

            var exact = valid.FirstOrDefault(e => e.Level == level);
            if (exact != null)
                return exact.Prefab;

            var fallback = valid.OrderByDescending(e => e.Level).First();
            Log.Warning("[TankPartCatalog] No prefab for {0} lvl {1}, fallback to lvl {2}",
                type, level, fallback.Level);
            return fallback.Prefab;
        }

        [Serializable]
        public class PartEntry
        {
            [SerializeField] private int _level;
            [SerializeField] private GameObject _prefab;

            public int Level => _level;
            public GameObject Prefab => _prefab;
        }
    }
}