using UnityEngine;

namespace Game.Battle
{
    public class TankInitData
    {
        public readonly int Id;
        public readonly string DisplayName;
        public readonly bool IsPlayer;

        public readonly TankPartStatsCatalogSO.TurretBattleStats Turret;
        public readonly TankPartStatsCatalogSO.ChassisBattleStats Chassis;

        public readonly float AimRadius;
        public readonly float ShakeIntensity;
        public readonly GameObject TurretPrefab;

        public readonly GameObject ChassisPrefab;

        public TankInitData(
            int id, string displayName, bool isPlayer,
            TankPartStatsCatalogSO.TurretBattleStats turret,
            TankPartStatsCatalogSO.ChassisBattleStats chassis,
            float aimRadius, float shakeIntensity,
            GameObject turretPrefab, GameObject chassisPrefab)
        {
            Id = id;
            DisplayName = displayName;
            IsPlayer = isPlayer;
            Turret = turret;
            Chassis = chassis;
            AimRadius = aimRadius;
            ShakeIntensity = shakeIntensity;
            TurretPrefab = turretPrefab;
            ChassisPrefab = chassisPrefab;
        }
    }
}