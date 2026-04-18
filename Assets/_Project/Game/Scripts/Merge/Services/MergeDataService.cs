using System;
using ANut.Core.Save;
using Game.Equipment;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using R3;
using ANut.Core;

namespace Game.Merge
{
    public class MergeDataService : SaveModuleBase<MergeDataService.Save>, IDisposable
    {
        public sealed class Save
        {
            [JsonProperty("total_purchases")] public int TotalPurchases { get; set; }
            [JsonProperty("unlocked_cells")] public int UnlockedCells { get; set; }
            [JsonProperty("grid_cells")] public CellSaveData[] GridCells { get; set; }
            [JsonProperty("max_turret_level")] public int MaxTurretLevel { get; set; }
            [JsonProperty("max_chassis_level")] public int MaxChassisLevel { get; set; }
        }

        // ISaveModule
        public override string Key => "merge";
        protected override int CurrentVersion => 4;

        // Runtime 
        private readonly ReactiveProperty<int> _totalPurchases = new(0);

        public ReadOnlyReactiveProperty<int> TotalPurchasesProperty => _totalPurchases;

        public int TotalPurchases => _totalPurchases.Value;

        public int UnlockedCells => Data.UnlockedCells;

        public CellSaveData[] SavedGridCells => Data.GridCells;

        // Sync after load 
        protected override void OnAfterDeserialize()
        {
            Data.GridCells ??= Array.Empty<CellSaveData>();
            _totalPurchases.Value = Data.TotalPurchases;
        }

        public void IncrementTotalPurchases()
        {
            _totalPurchases.Value++;
            Data.TotalPurchases = _totalPurchases.Value;
            MarkDirty();
        }

        public void UpdateGridState(CellSaveData[] cells, int unlockedCells)
        {
            Data.GridCells = cells ?? Array.Empty<CellSaveData>();
            Data.UnlockedCells = unlockedCells;
            MarkDirty();
        }

        public int GetMaxUnlockedLevel(TankPartType type) => type switch
        {
            TankPartType.Turret => Data.MaxTurretLevel,
            TankPartType.Chassis => Data.MaxChassisLevel,
            _ => 0
        };

        public void TrackMaxLevel(TankPartType type, int level)
        {
            switch (type)
            {
                case TankPartType.Turret:
                    if (level > Data.MaxTurretLevel)
                    {
                        Data.MaxTurretLevel = level;
                        MarkDirty();
                    }

                    break;
                case TankPartType.Chassis:
                    if (level > Data.MaxChassisLevel)
                    {
                        Data.MaxChassisLevel = level;
                        MarkDirty();
                    }

                    break;
            }
        }

        protected override string Migrate(int fromVersion, string payload)
        {
            switch (fromVersion)
            {
                case 1:
                    payload = MigrateV1ToV2(payload);
                    goto case 2;
                case 2:
                    payload = MigrateV2ToV3(payload);
                    goto case 3;
                case 3:
                    payload = MigrateV3ToV4(payload);
                    goto case 4;
                case 4:
                    break;
            }

            return payload;
        }

        private static string MigrateV1ToV2(string payload)
        {
            var obj = JObject.Parse(payload);
            obj["grid_cells"] ??= new JArray();
            obj["unlocked_cells"] ??= 0;
            Log.Info("[MergeDataService] Migrated v1 → v2");
            return obj.ToString(Formatting.None);
        }

        private static string MigrateV2ToV3(string payload)
        {
            var obj = JObject.Parse(payload);

            if (obj["tutorial_done"] == null)
                obj["tutorial_done"] = true;
            Log.Info("[MergeDataService] Migrated v2 → v3");
            return obj.ToString(Formatting.None);
        }

        private static string MigrateV3ToV4(string payload)
        {
            var obj = JObject.Parse(payload);
            obj["max_turret_level"] ??= 0;
            obj["max_chassis_level"] ??= 0;
            Log.Info("[MergeDataService] Migrated v3 → v4");
            return obj.ToString(Formatting.None);
        }

        public void Dispose() => _totalPurchases.Dispose();
    }
}