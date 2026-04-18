using System;
using System.Collections.Generic;
using System.Linq;
using Game.Equipment;
using UnityEngine;

namespace Game.Merge
{
    public sealed class PartGeneratorService
    {
        private readonly MergeModel _model;
        private readonly MergeSettings _settings;
        private readonly MergeDataService _data;
        private readonly IReadOnlyDictionary<int, PartPreset> _presetLookup;

        public PartGeneratorService(
            MergeModel model,
            MergeSettings settings,
            MergeDataService data)
        {
            _model = model;
            _settings = settings;
            _data = data;
            _presetLookup = (settings.PartPresets ?? Array.Empty<PartPreset>())
                .ToDictionary(p => p.PurchaseIndex, p => p);
        }

        public TankPartData Generate(int purchaseIndex)
        {
            if (_presetLookup.TryGetValue(purchaseIndex, out var preset))
                return Track(new TankPartData(preset.Type, preset.Level));

            var type = UnityEngine.Random.Range(0, 2) == 0 ? TankPartType.Turret : TankPartType.Chassis;
            int maxLevel = _data.GetMaxUnlockedLevel(type);
            if (maxLevel == 0) maxLevel = _model.GetMaxPartLevelForType(type);
            int level = Mathf.Max(_settings.MinPartLevel, maxLevel - _settings.MaxLevelDropOffset);

            return Track(new TankPartData(type, level));
        }

        public void SyncMaxLevelHistory()
        {
            foreach (TankPartType type in Enum.GetValues(typeof(TankPartType)))
                _data.TrackMaxLevel(type, _model.GetMaxPartLevelForType(type));
        }

        private TankPartData Track(TankPartData part)
        {
            _data.TrackMaxLevel(part.Type, part.Level);
            return part;
        }
    }
}