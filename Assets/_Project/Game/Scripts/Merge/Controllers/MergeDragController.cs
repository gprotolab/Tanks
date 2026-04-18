using Game.Common;
using ANut.Core.Audio;
using ANut.Core.Utils;
using R3;
using UnityEngine;

namespace Game.Merge
{
    public sealed class MergeDragController
    {
        private readonly MergeModel _model;
        private readonly MergeRuleService _rules;
        private readonly GridMutationService _gridMutation;
        private readonly EquipService _equipService;
        private readonly IMergeEconomyService _economy;
        private readonly IAudioService _audio;
        private readonly MergeGridView _gridView;
        private readonly MergeDragInput _dragInput;
        private readonly MergeDragVisual _dragVisual;
        private readonly MergeSellZoneView _sellZoneView;
        private readonly MergeSessionController _session;

        private readonly Subject<Unit> _dropCompleted = new();
        private readonly CompositeDisposable _disposables = new();
        private DragContext _currentDrag;

        public Observable<Unit> DropCompleted => _dropCompleted;

        public MergeDragController(
            MergeModel model,
            MergeRuleService rules,
            GridMutationService gridMutation,
            EquipService equipService,
            IMergeEconomyService economy,
            IAudioService audio,
            MergeGridView gridView,
            MergeDragInput dragInput,
            MergeDragVisual dragVisual,
            MergeSellZoneView sellZoneView,
            MergeSessionController session)
        {
            _model = model;
            _rules = rules;
            _gridMutation = gridMutation;
            _equipService = equipService;
            _economy = economy;
            _audio = audio;
            _gridView = gridView;
            _dragInput = dragInput;
            _dragVisual = dragVisual;
            _sellZoneView = sellZoneView;
            _session = session;
        }

        public void Initialize()
        {
            _dragInput.DragStarted
                .Subscribe(t => HandleDragStarted(t.col, t.row))
                .AddTo(_disposables);
            _dragInput.DragMoved
                .Subscribe(t => HandleDragMoved(t.pos, t.target))
                .AddTo(_disposables);
            _dragInput.DragDropped
                .Subscribe(t => HandleDragDropped(t))
                .AddTo(_disposables);
            _dragInput.DragCancelled
                .Subscribe(_ => HandleDragCancelled())
                .AddTo(_disposables);
        }

        private void HandleDragStarted(int col, int row)
        {
            var cell = _model.GetCell(col, row);
            if (cell.IsEmpty) return;
            var partView = _gridView.DetachPartForDrag(col, row);
            if (partView == null) return;
            _currentDrag = new DragContext(col, row, cell.Part, partView);
            var cellView = _gridView.GetCellView(col, row);
            _dragVisual.StartDrag(partView, cellView.SlotWorldPosition);
            long sellCost = _economy.GetSellPriceAtCell(col, row);
            _sellZoneView.Show($"{CostFormatter.Compact(sellCost)} {FontSprites.Currency.Coin}");
            _gridView.ShowMergeHints(cell.Part, _rules, col, row);
        }

        private void HandleDragMoved(Vector2 screenPos, DropTarget target)
        {
            if (_currentDrag == null) return;
            _dragVisual.UpdatePosition(screenPos);
            UpdateHighlights(target);
        }

        private void HandleDragDropped(DropTarget target)
        {
            if (_currentDrag == null) return;
            var drag = _currentDrag;
            _currentDrag = null;
            CleanupDragVisuals();
            switch (target.Zone)
            {
                case DropZoneType.GridCell: HandleDropOnGridCell(drag, target); break;
                case DropZoneType.TankSlot: HandleDropOnTank(drag); break;
                case DropZoneType.SellZone: HandleDropOnSellZone(drag); break;
                default: HandleDropOnNothing(drag); break;
            }
        }

        private void HandleDragCancelled()
        {
            if (_currentDrag == null) return;
            var drag = _currentDrag;
            _currentDrag = null;
            _gridView.HideAllMergeHints();
            _gridView.ClearAllHighlights();
            _sellZoneView.SetHighlighted(false);
            var cellView = _gridView.GetCellView(drag.SourceCol, drag.SourceRow);
            if (cellView != null)
            {
                drag.PartView.transform.position = cellView.SlotWorldPosition;
                drag.PartView.transform.localScale = Vector3.one;
            }

            _gridView.ReattachDraggedPart(drag.SourceCol, drag.SourceRow, drag.PartView);
            _dragVisual.EndDrag();
            _sellZoneView.Hide();
        }

        private void HandleDropOnGridCell(DragContext drag, DropTarget target)
        {
            if (target.Col == drag.SourceCol && target.Row == drag.SourceRow)
            {
                HandleDropOnNothing(drag);
                return;
            }

            if (!_model.IsCellUnlocked(target.Col, target.Row))
            {
                HandleDropOnNothing(drag);
                return;
            }

            var targetCell = _model.GetCell(target.Col, target.Row);
            if (targetCell.IsEmpty)
            {
                var targetPos = _gridView.GetCellView(target.Col, target.Row)?.SlotWorldPosition ?? Vector3.zero;
                _dragVisual.FlyToTarget(targetPos, shrink: false, () =>
                {
                    _gridView.ReattachDraggedPart(target.Col, target.Row, drag.PartView);
                    _gridMutation.MovePart(drag.SourceCol, drag.SourceRow, target.Col, target.Row);
                    AfterDrop();
                });
            }
            else if (_rules.CanMerge(drag.Part, targetCell.Part))
            {
                var targetPos = _gridView.GetCellView(target.Col, target.Row)?.SlotWorldPosition ?? Vector3.zero;
                _dragVisual.FlyToTarget(targetPos, shrink: true, () =>
                {
                    _gridView.DestroyDraggedPart();
                    _gridMutation.TryMerge(drag.SourceCol, drag.SourceRow, target.Col, target.Row);
                    _audio.PlaySfx(SoundId.Merge_Success);
                    AfterDrop();
                });
            }
            else
            {
                HandleDropOnNothing(drag);
            }
        }

        private void HandleDropOnTank(DragContext drag)
        {
            var result = _equipService.TryEquip(drag.SourceCol, drag.SourceRow);
            if (result is EquipResult.InvalidType or EquipResult.InvalidLevel)
            {
                _dragVisual.ReturnToOrigin(() =>
                    _gridView.ReattachDraggedPart(drag.SourceCol, drag.SourceRow, drag.PartView));
                return;
            }

            _audio.PlaySfx(SoundId.Merge_TankEquip);
            _dragVisual.EndDrag();
            _gridView.DestroyDraggedPart();
            AfterDrop();
        }

        private void HandleDropOnSellZone(DragContext drag)
        {
            _dragVisual.EndDrag();
            _gridView.DestroyDraggedPart();
            _economy.ExecuteSell(drag.SourceCol, drag.SourceRow);
            _audio.PlaySfx(SoundId.Merge_Sell);
            AfterDrop();
        }

        private void HandleDropOnNothing(DragContext drag)
        {
            _dragVisual.ReturnToOrigin(() =>
                _gridView.ReattachDraggedPart(drag.SourceCol, drag.SourceRow, drag.PartView));
        }

        private void AfterDrop()
        {
            _session.Save();
            _dropCompleted.OnNext(Unit.Default);
        }

        private void UpdateHighlights(DropTarget target)
        {
            _gridView.ClearAllHighlights();
            _sellZoneView.SetHighlighted(false);
            if (_currentDrag == null) return;
            switch (target.Zone)
            {
                case DropZoneType.GridCell:
                    if (!_model.IsCellUnlocked(target.Col, target.Row)) break;
                    if (target.Col == _currentDrag.SourceCol && target.Row == _currentDrag.SourceRow)
                    {
                        _gridView.SetCellHighlight(target.Col, target.Row, HighlightType.CanPlace);
                        break;
                    }

                    var cell = _model.GetCell(target.Col, target.Row);
                    _gridView.SetCellHighlight(target.Col, target.Row,
                        cell.IsEmpty ? HighlightType.CanPlace :
                        _rules.CanMerge(_currentDrag.Part, cell.Part) ? HighlightType.CanMerge :
                        HighlightType.Invalid);
                    break;
                case DropZoneType.SellZone:
                    _sellZoneView.SetHighlighted(true);
                    break;
            }
        }

        private void CleanupDragVisuals()
        {
            _gridView.HideAllMergeHints();
            _gridView.ClearAllHighlights();
            _sellZoneView.SetHighlighted(false);
            _sellZoneView.Hide();
        }

        public void Dispose()
        {
            _dropCompleted.Dispose();
            _disposables.Dispose();
        }
    }
}