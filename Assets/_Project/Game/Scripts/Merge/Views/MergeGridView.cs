using System.Collections.Generic;
using Game.Equipment;
using R3;
using UnityEngine;
using ANut.Core;

namespace Game.Merge
{
    public class MergeGridView : MonoBehaviour
    {
        [Header("Grid Setup")] [SerializeField]
        private MergeCellView _cellPrefab;

        [SerializeField] private MergePartView _partViewPrefab;
        [SerializeField] private Transform _gridRoot;
        [SerializeField] private Transform _partsRoot;
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private float _cellSize = 1.5f;
        [SerializeField] private float _cellSpacing = 0.1f;

        // Sparse cell view storage — only cells in the unlock order exist as keys.
        private Dictionary<(int col, int row), MergeCellView> _cellViews;
        private readonly Dictionary<(int col, int row), MergePartView> _partViews = new();

        private MergeModel _gridModel;
        private TankPartSkinCatalogSO _catalog;

        // Cached grid layout values computed once in SpawnCells.
        // Used by TryWorldToGridPosition and GetActiveZoneWorldCenter.
        private float _totalStep;
        private float _offsetX;
        private float _offsetZ;

        private MergePartView _draggedPart;

        public void Initialize(MergeModel gridModel, TankPartSkinCatalogSO catalog)
        {
            _gridModel = gridModel;
            _catalog = catalog;

            SpawnCells();
            PopulatePartsFromModel();

            _gridModel.OnCellChanged
                .Subscribe(t => HandleCellChanged(t.col, t.row, t.cell))
                .AddTo(this);

            _gridModel.OnCellUnlocked
                .Subscribe(t => HandleCellUnlocked(t.col, t.row))
                .AddTo(this);
        }

        private void SpawnCells()
        {
            _totalStep = _cellSize + _cellSpacing;
            var order = _gridModel.GetUnlockOrder();

            // Compute bounding box to center the grid visually.
            int maxCol = 0, maxRow = 0;
            foreach (var v in order)
            {
                if (v.x > maxCol) maxCol = v.x;
                if (v.y > maxRow) maxRow = v.y;
            }

            _offsetX = maxCol * _totalStep * 0.5f;
            _offsetZ = maxRow * _totalStep * 0.5f;

            _cellViews = new Dictionary<(int, int), MergeCellView>(order.Length);
            foreach (var v in order)
            {
                var localPos = new Vector3(v.x * _totalStep - _offsetX, 0f, v.y * _totalStep - _offsetZ);
                var cell = Instantiate(_cellPrefab, _gridRoot);
                cell.transform.localPosition = localPos;
                cell.Initialize(v.x, v.y);
                _cellViews[(v.x, v.y)] = cell;
            }
        }

        private void PopulatePartsFromModel()
        {
            var order = _gridModel.GetUnlockOrder();
            foreach (var v in order)
            {
                var gridCell = _gridModel.GetCell(v.x, v.y);
                if (!gridCell.IsEmpty)
                    CreatePartView(v.x, v.y, gridCell.Part, animate: false);
            }
        }

        private MergePartView CreatePartView(int col, int row, TankPartData part, bool animate)
        {
            var cellView = _cellViews[(col, row)];
            var partView = Instantiate(_partViewPrefab, _partsRoot);
            partView.transform.position = cellView.SlotWorldPosition;
            partView.Setup(part, _catalog, animate);

            _partViews[(col, row)] = partView;
            return partView;
        }

        private void DestroyPartView(int col, int row)
        {
            if (!_partViews.TryGetValue((col, row), out var partView)) return;

            _partViews.Remove((col, row));

            if (partView == _draggedPart) return;

            partView.KillAnimations();
            Destroy(partView.gameObject);
        }

        public bool HasPart(int col, int row) => _partViews.ContainsKey((col, row));


        public MergePartView DetachPartForDrag(int col, int row)
        {
            if (!_partViews.TryGetValue((col, row), out var partView)) return null;

            _partViews.Remove((col, row));
            _draggedPart = partView;
            return partView;
        }

        public void ReattachDraggedPart(int col, int row, MergePartView partView)
        {
            _partViews[(col, row)] = partView;
            _draggedPart = null;
        }

        public void DestroyDraggedPart()
        {
            if (_draggedPart != null)
            {
                _draggedPart.KillAnimations();
                Destroy(_draggedPart.gameObject);
                _draggedPart = null;
            }
        }

        private void HandleCellChanged(int col, int row, GridCell cell)
        {
            if (cell.IsEmpty)
            {
                DestroyPartView(col, row);
                return;
            }

            // If MergePartView is already in place (reattach after drag) — check match
            if (_partViews.TryGetValue((col, row), out var existing))
            {
                if (existing.Part.Type == cell.Part.Type && existing.Part.Level == cell.Part.Level)
                    return;

                // Different part — replace
                DestroyPartView(col, row);
            }

            CreatePartView(col, row, cell.Part, animate: true);
        }

        private void HandleCellUnlocked(int col, int row)
        {
            var cellView = GetCellView(col, row);
            if (cellView != null)
                cellView.SetCellMode(CellMode.Active);
        }

        public void UpdateCellStates(MergeModel model, bool hasNextCell, bool canExpand, string nextCellCost)
        {
            foreach (var kv in _cellViews)
            {
                int col = kv.Key.col;
                int row = kv.Key.row;
                int index = model.CellToLinearIndex(col, row);

                if (index < model.UnlockedCellCount)
                    kv.Value.SetCellMode(CellMode.Active);
                else if (hasNextCell && index == model.UnlockedCellCount)
                    kv.Value.SetCellMode(CellMode.Purchasable, nextCellCost, canExpand);
                else
                    kv.Value.SetCellMode(CellMode.Hidden);
            }
        }

        public MergeCellView GetCellView(int col, int row)
            => _cellViews.TryGetValue((col, row), out var view) ? view : null;

        public void SetCellHighlight(int col, int row, HighlightType type)
        {
            var cellView = GetCellView(col, row);
            if (cellView != null) cellView.SetSelectedHighlight(type);
        }

        public void ClearAllHighlights()
        {
            foreach (var kv in _cellViews)
            {
                if (kv.Value.Mode == CellMode.Active)
                    kv.Value.SetSelectedHighlight(HighlightType.None);
            }
        }

        public void ShowMergeHints(TankPartData draggedPart, MergeRuleService rules, int sourceCol, int sourceRow)
        {
            foreach (var kv in _cellViews)
            {
                int col = kv.Key.col;
                int row = kv.Key.row;

                if (!_gridModel.IsCellUnlocked(col, row))
                {
                    kv.Value.SetMergeHint(false);
                    continue;
                }

                if (col == sourceCol && row == sourceRow)
                {
                    kv.Value.SetMergeHint(false);
                    continue;
                }

                var cell = _gridModel.GetCell(col, row);
                bool canMerge = !cell.IsEmpty && rules.CanMerge(draggedPart, cell.Part);
                kv.Value.SetMergeHint(canMerge);
            }
        }

        public void HideAllMergeHints()
        {
            foreach (var kv in _cellViews)
                kv.Value.SetMergeHint(false);
        }

        public void SetCameraTargetPosition(Vector3 worldPosition)
        {
            if (_cameraTarget == null)
            {
                Log.Error("MergeGridView camera target is not assigned.");
                return;
            }

            _cameraTarget.position = worldPosition;
        }

        public Vector3 GetActiveZoneWorldCenter(MergeModel model, bool includePurchasable = false)
        {
            if (model.UnlockedCellCount == 0 && !(includePurchasable && model.CanUnlockMore))
                return _gridRoot.TransformPoint(Vector3.zero);

            float minX = float.MaxValue, maxX = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;

            var order = model.GetUnlockOrder();

            // Include all unlocked cells.
            int countToInclude = model.UnlockedCellCount;

            // Optionally include the next purchasable cell.
            if (includePurchasable && model.CanUnlockMore)
                countToInclude++;

            for (int i = 0; i < countToInclude; i++)
            {
                float lx = order[i].x * _totalStep - _offsetX;
                float lz = order[i].y * _totalStep - _offsetZ;

                if (lx < minX) minX = lx;
                if (lx > maxX) maxX = lx;
                if (lz < minZ) minZ = lz;
                if (lz > maxZ) maxZ = lz;
            }

            return _gridRoot.TransformPoint(
                new Vector3((minX + maxX) * 0.5f, 0f, (minZ + maxZ) * 0.5f));
        }

        public bool TryWorldToGridPosition(Vector3 worldPos, out int col, out int row)
        {
            var localPos = _gridRoot.InverseTransformPoint(worldPos);

            col = Mathf.RoundToInt((localPos.x + _offsetX) / _totalStep);
            row = Mathf.RoundToInt((localPos.z + _offsetZ) / _totalStep);

            return _gridModel.IsValidPosition(col, row);
        }
    }
}