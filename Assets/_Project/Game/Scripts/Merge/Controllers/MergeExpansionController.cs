using System;
using ANut.Core.Currency;
using ANut.Core.Utils;
using Game.Common;
using R3;
using UnityEngine;

namespace Game.Merge
{
    public sealed class MergeExpansionController : IDisposable
    {
        private readonly MergeModel _model;
        private readonly IMergeEconomyService _economy;
        private readonly MergeSettings _settings;
        private readonly MergeGridView _gridView;
        private readonly MergeDragInput _dragInput;
        private readonly ICurrencyService _currency;
        private readonly MergeSessionController _session;
        private readonly CompositeDisposable _disposables = new();

        public MergeExpansionController(
            MergeModel model,
            IMergeEconomyService economy,
            MergeSettings settings,
            MergeGridView gridView,
            MergeDragInput dragInput,
            ICurrencyService currency,
            MergeSessionController session)
        {
            _model = model;
            _economy = economy;
            _settings = settings;
            _gridView = gridView;
            _dragInput = dragInput;
            _currency = currency;
            _session = session;
        }

        public void Initialize()
        {
            _dragInput.CellTapped
                .Subscribe(t => HandleCellTapped(t.col, t.row))
                .AddTo(_disposables);

            _model.OnCellChanged
                .Subscribe(_ => Refresh())
                .AddTo(_disposables);

            _model.OnCellUnlocked
                .Subscribe(_ => Refresh())
                .AddTo(_disposables);

            _currency.GetBalance(CurrencyType.Coins)
                .Subscribe(_ => Refresh())
                .AddTo(_disposables);

            Refresh();
        }

        private void HandleCellTapped(int col, int row)
        {
            int index = _model.CellToLinearIndex(col, row);
            if (index != _model.UnlockedCellCount) return;
            if (!_economy.TryExpandGrid()) return;

            _session.Save();
        }

        public void Refresh()
        {
            RefreshCellStates();
            UpdateGridFocus();
        }

        private void RefreshCellStates()
        {
            bool hasNextCell = _settings.CanExpandGrid && _model.CanUnlockMore;
            bool canExpand = _economy.CanExpandGrid();
            string nextCost = hasNextCell
                ? $"{CostFormatter.Compact(_economy.GetExpandPrice())} {FontSprites.Currency.Coin}"
                : string.Empty;
            _gridView.UpdateCellStates(_model, hasNextCell, canExpand, nextCost);
        }

        private void UpdateGridFocus()
        {
            bool includePurchasable = _settings.CanExpandGrid && _model.CanUnlockMore;
            Vector3 focusPos = _gridView.GetActiveZoneWorldCenter(_model, includePurchasable);
            _gridView.SetCameraTargetPosition(focusPos);
        }

        public void Dispose() => _disposables.Dispose();
    }
}