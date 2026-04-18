using System;
using System.Collections.Generic;
using Game.Equipment;
using UnityEngine;

namespace Game.Merge
{
    [Serializable]
    public class PartPreset
    {
        public int PurchaseIndex;
        public TankPartType Type;
        public int Level = 1;
    }

    [Serializable]
    public class MergeSettings
    {
        [Header("Session")]
        [Tooltip("Number of cells unlocked at session start (before any purchases)")]
        [SerializeField]
        private int _unlockedCells = 6;

        [Tooltip("Whether the player can purchase additional grid cells")] [SerializeField]
        private bool _canExpandGrid = true;

        [Tooltip("Maximum number of part purchases allowed in this session")] [SerializeField]
        private int _maxPurchases = int.MaxValue;

        [Tooltip("Maximum part level allowed in merge. This keeps balance stable")] [SerializeField]
        private int _maxPartLevel = 100;

        [Tooltip("New part level offset below the current grid maximum")] [SerializeField, Range(0, 20)]
        private int _maxLevelDropOffset = 5;

        [Tooltip("Minimum part level that can be generated on purchase")] [SerializeField]
        private int _minPartLevel = 1;

        [Tooltip("Override part type and level per purchase index. " +
                 "Index 0 = first purchase, index 1 = second, etc. " +
                 "If empty or index is out of range, the default algorithm is used.")]
        [SerializeField]
        private PartPreset[] _partPresets = null;

        [Header("Grid Unlock Order")]
        [Tooltip("Defines both the grid shape and the cell unlock sequence.\n" +
                 "Each element is (x = col, y = row).\n" +
                 "Only cells listed here exist in the grid — no rectangular fallback.\n" +
                 "Must not be empty.")]
        [SerializeField]
        private Vector2Int[] _cellUnlockOrder = Array.Empty<Vector2Int>();

        public int UnlockedCells => _unlockedCells;

        public bool CanExpandGrid => _canExpandGrid;

        public int MaxPurchases => _maxPurchases;

        public int MaxPartLevel => _maxPartLevel;

        public int MaxLevelDropOffset => _maxLevelDropOffset;

        public int MinPartLevel => _minPartLevel;

        public IReadOnlyList<PartPreset> PartPresets => _partPresets;
        public IReadOnlyList<Vector2Int> CellUnlockOrder => _cellUnlockOrder;

#if UNITY_EDITOR
        [UnityEngine.ContextMenu("Migrate PartPresets: fill PurchaseIndex by array position")]
        private void MigratePartPresetsIndices()
        {
            var presets = _partPresets ?? Array.Empty<PartPreset>();
            for (int i = 0; i < presets.Length; i++)
                presets[i].PurchaseIndex = i;
        }
#endif
    }
}