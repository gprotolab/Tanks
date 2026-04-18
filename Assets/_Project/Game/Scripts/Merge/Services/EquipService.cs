using System;
using Game.Equipment;
using R3;

namespace Game.Merge
{
    public sealed class EquipService : IDisposable
    {
        private readonly MergeModel _model;
        private readonly MergeRuleService _rules;
        private readonly MergeSettings _settings;
        private readonly Subject<Unit> _partEquipped = new();

        public Observable<Unit> PartEquipped => _partEquipped;

        public EquipService(
            MergeModel model,
            MergeRuleService rules,
            MergeSettings settings)
        {
            _model = model;
            _rules = rules;
            _settings = settings;
        }

        public EquipResult TryEquip(int col, int row)
        {
            var cell = _model.GetCell(col, row);
            if (cell.IsEmpty) return EquipResult.InvalidType;
            if (cell.Part.Level <= 0) return EquipResult.InvalidLevel;

            var part = cell.Part;
            var equipped = _model.GetEquipped(part.Type);

            if (equipped == null)
                return Commit(EquipResult.Equipped, part.Type, part, col, row, returnPart: null);

            var merge = _rules.TryMerge(part, equipped, _settings.MaxPartLevel);
            if (merge.Success)
                return Commit(EquipResult.Merged, part.Type, merge.Part, col, row, returnPart: null);

            return Commit(EquipResult.Swapped, part.Type, part, col, row, returnPart: equipped);
        }

        private EquipResult Commit(
            EquipResult result,
            TankPartType type,
            TankPartData equip,
            int col, int row,
            TankPartData returnPart)
        {
            _model.SetEquipped(type, equip);
            if (returnPart != null) _model.SetCell(col, row, returnPart);
            else _model.ClearCell(col, row);
            _partEquipped.OnNext(Unit.Default);
            return result;
        }

        public void Dispose() => _partEquipped.Dispose();
    }
}