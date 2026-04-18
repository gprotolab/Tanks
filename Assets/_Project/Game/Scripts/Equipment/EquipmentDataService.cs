using System;
using ANut.Core.Save;
using Newtonsoft.Json;
using R3;

namespace Game.Equipment
{
    public class EquipmentDataService : SaveModuleBase<EquipmentDataService.Save>, IDisposable
    {
        public sealed class Save
        {
            [JsonProperty("turret_level")] public int TurretLevel { get; set; } = 0;
            [JsonProperty("chassis_level")] public int ChassisLevel { get; set; } = 1;
            [JsonProperty("max_turret_level")] public int MaxTurretLevelEquipped { get; set; } = 0;
            [JsonProperty("max_chassis_level")] public int MaxChassisLevelEquipped { get; set; } = 1;
        }

        public override string Key => "equipment";

        private readonly Subject<TankPartData> _equippedPartChanged = new();
        private readonly Subject<TankPartType> _maxLevelChanged = new();

        public Observable<TankPartData> EquippedPartChanged => _equippedPartChanged;

        public Observable<TankPartType> MaxLevelChanged => _maxLevelChanged;

        public int TurretLevel => Data.TurretLevel;
        public int ChassisLevel => Data.ChassisLevel;

        public int GetLevel(TankPartType partType) => partType switch
        {
            TankPartType.Turret => Data.TurretLevel,
            TankPartType.Chassis => Data.ChassisLevel,
            _ => 0
        };

        public int GetMaxLevel(TankPartType partType) => partType switch
        {
            TankPartType.Turret => Data.MaxTurretLevelEquipped,
            TankPartType.Chassis => Data.MaxChassisLevelEquipped,
            _ => 0
        };

        protected override void OnAfterDeserialize()
        {
            // Backward compatibility: seed max levels from current equipped if never saved before.
            if (Data.MaxTurretLevelEquipped < Data.TurretLevel)
                Data.MaxTurretLevelEquipped = Data.TurretLevel;
            if (Data.MaxChassisLevelEquipped < Data.ChassisLevel)
                Data.MaxChassisLevelEquipped = Data.ChassisLevel;
        }

        public void UpdateEquippedParts(int turretLevel, int chassisLevel)
        {
            int previousTurretLevel = Data.TurretLevel;
            int previousChassisLevel = Data.ChassisLevel;

            Data.TurretLevel = turretLevel;
            Data.ChassisLevel = chassisLevel;

            if (previousTurretLevel != turretLevel)
                _equippedPartChanged.OnNext(new TankPartData(TankPartType.Turret, turretLevel));

            if (previousChassisLevel != chassisLevel)
                _equippedPartChanged.OnNext(new TankPartData(TankPartType.Chassis, chassisLevel));

            if (turretLevel > Data.MaxTurretLevelEquipped)
            {
                Data.MaxTurretLevelEquipped = turretLevel;
                _maxLevelChanged.OnNext(TankPartType.Turret);
            }

            if (chassisLevel > Data.MaxChassisLevelEquipped)
            {
                Data.MaxChassisLevelEquipped = chassisLevel;
                _maxLevelChanged.OnNext(TankPartType.Chassis);
            }

            MarkDirty();
        }

        public void Dispose()
        {
            _equippedPartChanged.Dispose();
            _maxLevelChanged.Dispose();
        }
    }
}