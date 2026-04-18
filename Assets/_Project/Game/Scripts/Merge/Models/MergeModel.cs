using System;
using System.Collections.Generic;
using Game.Equipment;
using R3;
using UnityEngine;

namespace Game.Merge
{
    public class MergeModel : IDisposable
    {
        // Sparse cell storage — only cells present in _unlockOrder are valid keys.
        private Dictionary<(int col, int row), GridCell> _cells;

        // Ordered list of all cells in the sequence they become unlocked.
        private Vector2Int[] _unlockOrder;

        // Reverse lookup: (col, row) → index in _unlockOrder. O(1) access.
        private Dictionary<(int col, int row), int> _unlockOrderIndex;

        public const int CellNotInOrder = int.MaxValue;

        public int UnlockedCellCount { get; private set; }
        public int TotalCellCount => _unlockOrder.Length;
        public bool CanUnlockMore => UnlockedCellCount < TotalCellCount;

        public TankPartData EquippedTurret { get; private set; }
        public TankPartData EquippedChassis { get; private set; }

        public Observable<(int col, int row, GridCell cell)> OnCellChanged => _onCellChanged;

        public Observable<(TankPartType type, TankPartData part)> OnTankSlotChanged => _onTankSlotChanged;

        public Observable<(int col, int row)> OnCellUnlocked => _onCellUnlocked;

        private readonly Subject<(int col, int row, GridCell cell)> _onCellChanged = new();
        private readonly Subject<(TankPartType type, TankPartData part)> _onTankSlotChanged = new();
        private readonly Subject<(int col, int row)> _onCellUnlocked = new();

        public void Initialize(int unlockedCells, IReadOnlyList<Vector2Int> unlockOrder)
        {
            if (unlockOrder == null || unlockOrder.Count == 0)
                throw new ArgumentException(
                    "[MergeModel] unlockOrder must not be null or empty. " +
                    "Configure CellUnlockOrder in MergeSettings.", nameof(unlockOrder));

            _unlockOrder = BuildUnlockOrder(unlockOrder);
            _unlockOrderIndex = BuildUnlockIndex(_unlockOrder);

            UnlockedCellCount = Mathf.Clamp(unlockedCells, 0, _unlockOrder.Length);

            _cells = new Dictionary<(int, int), GridCell>(_unlockOrder.Length);
            foreach (var v in _unlockOrder)
                _cells[(v.x, v.y)] = GridCell.Empty();
        }

        // Cell Unlock

        public int CellToLinearIndex(int col, int row)
            => _unlockOrderIndex.TryGetValue((col, row), out int idx) ? idx : CellNotInOrder;

        public (int col, int row) LinearIndexToCell(int index)
        {
            var v = _unlockOrder[index];
            return (v.x, v.y);
        }

        public bool IsCellUnlocked(int col, int row) => CellToLinearIndex(col, row) < UnlockedCellCount;

        public bool IsValidPosition(int col, int row) => _cells.ContainsKey((col, row));

        public ReadOnlySpan<Vector2Int> GetUnlockOrder()
            => new ReadOnlySpan<Vector2Int>(_unlockOrder, 0, _unlockOrder.Length);

        public void UnlockNextCell()
        {
            if (!CanUnlockMore) return;
            var (col, row) = LinearIndexToCell(UnlockedCellCount);
            UnlockedCellCount++;
            _onCellUnlocked.OnNext((col, row));
        }

        // Grid Operations

        public GridCell GetCell(int col, int row)
        {
            if (!_cells.TryGetValue((col, row), out var cell)) return GridCell.Empty();
            return cell;
        }

        public void SetCell(int col, int row, TankPartData part)
        {
            if (!_cells.ContainsKey((col, row))) return;
            _cells[(col, row)] = GridCell.WithPart(part);
            _onCellChanged.OnNext((col, row, _cells[(col, row)]));
        }

        public void ClearCell(int col, int row)
        {
            if (!_cells.ContainsKey((col, row))) return;
            _cells[(col, row)] = GridCell.Empty();
            _onCellChanged.OnNext((col, row, _cells[(col, row)]));
        }

        public bool HasEmptyCell()
        {
            for (int i = 0; i < UnlockedCellCount; i++)
            {
                var v = _unlockOrder[i];
                if (_cells[(v.x, v.y)].IsEmpty) return true;
            }

            return false;
        }

        public bool TryFindRandomEmptyCell(out int col, out int row)
        {
            int emptyCellCount = 0;
            for (int i = 0; i < UnlockedCellCount; i++)
            {
                var v = _unlockOrder[i];
                if (_cells[(v.x, v.y)].IsEmpty) emptyCellCount++;
            }

            if (emptyCellCount == 0)
            {
                col = -1;
                row = -1;
                return false;
            }

            int target = UnityEngine.Random.Range(0, emptyCellCount);
            int current = 0;

            for (int i = 0; i < UnlockedCellCount; i++)
            {
                var v = _unlockOrder[i];
                if (_cells[(v.x, v.y)].IsEmpty)
                {
                    if (current == target)
                    {
                        col = v.x;
                        row = v.y;
                        return true;
                    }

                    current++;
                }
            }

            col = -1;
            row = -1;
            return false;
        }

        public int GetMaxPartLevel()
        {
            int max = 0;
            foreach (var kv in _cells)
            {
                if (!kv.Value.IsEmpty && kv.Value.Part.Level > max)
                    max = kv.Value.Part.Level;
            }

            return max;
        }

        public int GetMaxPartLevelForType(TankPartType type)
        {
            int max = 0;
            foreach (var kv in _cells)
                if (!kv.Value.IsEmpty && kv.Value.Part.Type == type && kv.Value.Part.Level > max)
                    max = kv.Value.Part.Level;

            var equipped = GetEquipped(type);
            if (equipped != null && equipped.Level > max)
                max = equipped.Level;

            return max;
        }

        // === Equipment ===

        public TankPartData GetEquipped(TankPartType type) => type switch
        {
            TankPartType.Turret => EquippedTurret,
            TankPartType.Chassis => EquippedChassis,
            _ => null
        };

        public void SetEquipped(TankPartType type, TankPartData part)
        {
            switch (type)
            {
                case TankPartType.Turret:
                    EquippedTurret = part;
                    break;
                case TankPartType.Chassis:
                    EquippedChassis = part;
                    break;
            }

            _onTankSlotChanged.OnNext((type, part));
        }

        // === Builders ===

        private static Vector2Int[] BuildUnlockOrder(IReadOnlyList<Vector2Int> custom)
        {
            var order = new Vector2Int[custom.Count];
            for (int i = 0; i < custom.Count; i++)
                order[i] = custom[i];
            return order;
        }

        private static Dictionary<(int col, int row), int> BuildUnlockIndex(Vector2Int[] order)
        {
            var index = new Dictionary<(int, int), int>(order.Length);
            for (int i = 0; i < order.Length; i++)
                index[(order[i].x, order[i].y)] = i;
            return index;
        }

        // === Lifecycle ===

        public void Dispose()
        {
            _onCellChanged.Dispose();
            _onTankSlotChanged.Dispose();
            _onCellUnlocked.Dispose();
        }
    }
}