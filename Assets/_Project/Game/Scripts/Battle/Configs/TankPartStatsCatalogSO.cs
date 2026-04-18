using System;
using System.Collections.Generic;
using UnityEngine;
using ANut.Core;


namespace Game.Battle
{
    [CreateAssetMenu(fileName = "BattlePartsCatalog", menuName = "Configs/Battle/BattlePartsCatalog")]
    public class TankPartStatsCatalogSO : ScriptableObject
    {
        [SerializeField] private TurretBattleStats[] _turretStatsByLevel;
        [SerializeField] private ChassisBattleStats[] _chassisStatsByLevel;

        public TurretBattleStats GetTurretStats(int level)
        {
            if (_turretStatsByLevel == null || _turretStatsByLevel.Length == 0)
            {
                Log.Error("[BattlePartsCatalog] TurretStatsByLevel is empty!");
                return new TurretBattleStats();
            }

            int idx = Mathf.Clamp(level - 1, 0, _turretStatsByLevel.Length - 1);
            return _turretStatsByLevel[idx];
        }

        public ChassisBattleStats GetChassisStats(int level)
        {
            if (_chassisStatsByLevel == null || _chassisStatsByLevel.Length == 0)
            {
                Log.Error("[BattlePartsCatalog] ChassisStatsByLevel is empty!");
                return new ChassisBattleStats();
            }

            int idx = Mathf.Clamp(level - 1, 0, _chassisStatsByLevel.Length - 1);
            return _chassisStatsByLevel[idx];
        }

        public IEnumerable<ProjectileConfigSO> GetAllProjectileConfigs()
        {
            if (_turretStatsByLevel == null) yield break;
            foreach (var stats in _turretStatsByLevel)
                if (stats.ProjectileConfig != null)
                    yield return stats.ProjectileConfig;
        }

        [Serializable]
        public class TurretBattleStats
        {
            public float Damage = 10f;
            public float FireRate = 1f;

            public int AmmoCapacity = 4;

            // Full reload time for an empty magazine.
            public float MagazineReloadTime = 2f;
            public ProjectileConfigSO ProjectileConfig;
        }

        [Serializable]
        public class ChassisBattleStats
        {
            public float MaxHp = 100f;
            public float MoveSpeed = 4f;
            public float RotationSpeed = 180f;
        }
    }
}