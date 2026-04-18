using System;
using System.Collections.Generic;
using ANut.Core.Analytics;
using ANut.Core.Currency;
using ANut.Core.Utils;
using Game.Common;
using R3;

namespace Game.Merge
{
    public sealed class MergePurchaseController : IDisposable
    {
        private readonly IMergeEconomyService _economy;
        private readonly MergeDataService _mergeData;
        private readonly ICurrencyService _currency;
        private readonly IAnalyticsService _analytics;
        private readonly MergePurchaseView _view;
        private readonly MergeSessionController _session;
        private readonly CompositeDisposable _disposables = new();

        public MergePurchaseController(
            IMergeEconomyService economy,
            MergeDataService mergeData,
            ICurrencyService currency,
            IAnalyticsService analytics,
            MergePurchaseView view,
            MergeSessionController session)
        {
            _economy = economy;
            _mergeData = mergeData;
            _currency = currency;
            _analytics = analytics;
            _view = view;
            _session = session;
        }

        public void Initialize()
        {
            _view.OnBuyClicked
                .Subscribe(_ => OnBuyClicked())
                .AddTo(_disposables);

            _currency.GetBalance(CurrencyType.Coins)
                .Subscribe(_ => RefreshButton())
                .AddTo(_disposables);

            _mergeData.TotalPurchasesProperty
                .Subscribe(_ => RefreshButton())
                .AddTo(_disposables);

            RefreshButton();
        }

        private void OnBuyClicked()
        {
            long cost = _economy.GetBuyPrice();
            var part = _economy.TryBuyPart();

            if (part == null)
            {
                _view.PlayShakeAnimation();
                return;
            }

            _analytics.LogEvent(AnalyticsEvents.PartPurchased, new Dictionary<string, object>
            {
                ["purchase_number"] = _mergeData.TotalPurchases,
                ["cost"] = cost,
                ["part_type"] = part.Type.ToString(),
                ["part_level"] = part.Level,
            });

            _session.Save();
        }

        private void RefreshButton()
        {
            long cost = _economy.GetBuyPrice();
            _view.SetCost($"{CostFormatter.Compact(cost)} {FontSprites.Currency.Coin}");
            _view.SetInteractable(_economy.CanBuyPart());
        }

        public void Dispose() => _disposables.Dispose();
    }
}