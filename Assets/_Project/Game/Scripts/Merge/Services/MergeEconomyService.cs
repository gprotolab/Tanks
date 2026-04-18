using System;
using ANut.Core.Currency;
using Game.Economy;
using Game.Equipment;

namespace Game.Merge
{
    public sealed class MergeEconomyService : IMergeEconomyService
    {
        private readonly MergeModel _model;
        private readonly PartGeneratorService _generator;
        private readonly GridMutationService _gridMutation;
        private readonly MergeSettings _settings;
        private readonly MergeDataService _data;
        private readonly EconomyService _economy;
        private readonly ICurrencyService _currency;

        public MergeEconomyService(
            MergeModel model,
            PartGeneratorService generator,
            GridMutationService gridMutation,
            MergeSettings settings,
            MergeDataService data,
            EconomyService economy,
            ICurrencyService currency)
        {
            _model = model;
            _generator = generator;
            _gridMutation = gridMutation;
            _settings = settings;
            _data = data;
            _economy = economy;
            _currency = currency;
        }

        public bool CanBuyPart()
        {
            return _data.TotalPurchases < _settings.MaxPurchases
                   && _model.HasEmptyCell()
                   && _currency.CanAfford(CurrencyType.Coins, GetBuyPrice());
        }

        public long GetBuyPrice()
        {
            return _economy.GetMergePartCost(_data.TotalPurchases);
        }

        public TankPartData TryBuyPart()
        {
            if (!CanBuyPart()) return null;

            long cost = GetBuyPrice();
            int purchaseIndex = _data.TotalPurchases;

            _currency.TrySpend(CurrencyType.Coins, cost, "merge_purchase");
            _data.IncrementTotalPurchases();

            var part = _generator.Generate(purchaseIndex);

            if (_model.TryFindRandomEmptyCell(out int col, out int row))
                _gridMutation.PlacePart(col, row, part);

            return part;
        }

        public long GetSellPrice(TankPartData part)
        {
            if (part == null)
                return 0;

            return _economy.GetMergeSellCost(_data.TotalPurchases);
        }

        public long GetSellPriceAtCell(int col, int row)
        {
            var cell = _model.GetCell(col, row);
            return cell.IsEmpty ? 0 : GetSellPrice(cell.Part);
        }

        public void ExecuteSell(int col, int row)
        {
            var cell = _model.GetCell(col, row);
            if (cell.IsEmpty)
                return;

            long price = GetSellPrice(cell.Part);
            _currency.Add(CurrencyType.Coins, price, "merge_sell");
            _gridMutation.RemovePart(col, row);
        }

        public bool CanExpandGrid()
        {
            return _settings.CanExpandGrid
                   && _model.CanUnlockMore
                   && _currency.CanAfford(CurrencyType.Coins, GetExpandPrice());
        }

        public long GetExpandPrice()
        {
            int alreadyPurchased = Math.Max(0, _model.UnlockedCellCount - _settings.UnlockedCells);
            return _economy.GetMergeCellCost(alreadyPurchased);
        }

        public bool TryExpandGrid()
        {
            if (!CanExpandGrid())
                return false;

            _currency.TrySpend(CurrencyType.Coins, GetExpandPrice(), "cell_purchase");
            _gridMutation.UnlockNextCell();
            return true;
        }
    }
}