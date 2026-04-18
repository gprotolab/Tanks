using System;
using System.Collections.Generic;
using Game.Equipment;
using R3;

namespace Game.Merge
{
    public sealed class MergeSessionController : IDisposable
    {
        private readonly MergeModel _model;
        private readonly MergeSettings _settings;
        private readonly MergeDataService _mergeData;
        private readonly EquipmentDataService _equipmentData;
        private readonly PartGeneratorService _generator;
        private readonly MergeGridView _gridView;
        private readonly MergeTankView _tankView;
        private readonly MergeDragInput _dragInput;
        private readonly TankPartSkinCatalogSO _catalog;

        private bool _dirty;
        private readonly List<CellSaveData> _saveBuffer = new();
        private readonly CompositeDisposable _disposables = new();

        public MergeSessionController(
            MergeModel model,
            MergeSettings settings,
            MergeDataService mergeData,
            EquipmentDataService equipmentData,
            PartGeneratorService generator,
            MergeGridView gridView,
            MergeTankView tankView,
            MergeDragInput dragInput,
            TankPartSkinCatalogSO catalog)
        {
            _model = model;
            _settings = settings;
            _mergeData = mergeData;
            _equipmentData = equipmentData;
            _generator = generator;
            _gridView = gridView;
            _tankView = tankView;
            _dragInput = dragInput;
            _catalog = catalog;
        }

        public void Initialize()
        {
            RestoreGrid();
            RestoreEquipment();
            _generator.SyncMaxLevelHistory();

            // Views инициализируются после восстановления модели
            _gridView.Initialize(_model, _catalog);
            _tankView.Initialize(_model, _catalog);
            _dragInput.Initialize(_gridView);

            _model.OnCellChanged
                .Subscribe(_ => _dirty = true)
                .AddTo(_disposables);
        }

        public void Save()
        {
            if (!_dirty) return;
            _dirty = false;

            _saveBuffer.Clear();
            foreach (var v in _model.GetUnlockOrder())
            {
                var cell = _model.GetCell(v.x, v.y);
                if (!cell.IsEmpty)
                    _saveBuffer.Add(CellSaveData.From(v.x, v.y, cell.Part));
            }

            _equipmentData.UpdateEquippedParts(
                _model.EquippedTurret?.Level ?? 0,
                _model.EquippedChassis?.Level ?? 1);

            _mergeData.UpdateGridState(_saveBuffer.ToArray(), _model.UnlockedCellCount);
        }

        private void RestoreGrid()
        {
            int unlockedCells = Math.Max(_mergeData.UnlockedCells, _settings.UnlockedCells);
            _model.Initialize(unlockedCells, _settings.CellUnlockOrder);

            var savedCells = _mergeData.SavedGridCells;
            if (savedCells == null || savedCells.Length == 0) return;

            foreach (var cell in savedCells)
                if (cell != null && _model.IsValidPosition(cell.Col, cell.Row))
                    _model.SetCell(cell.Col, cell.Row, cell.ToPartData());
        }

        private void RestoreEquipment()
        {
            if (_equipmentData.TurretLevel > 0)
                _model.SetEquipped(TankPartType.Turret,
                    new TankPartData(TankPartType.Turret, _equipmentData.TurretLevel));

            int chassisLevel = _equipmentData.ChassisLevel > 0 ? _equipmentData.ChassisLevel : 1;
            _model.SetEquipped(TankPartType.Chassis,
                new TankPartData(TankPartType.Chassis, chassisLevel));
        }

        public void Dispose() => _disposables.Dispose();
    }
}