using System;
using Game.Common;
using ANut.Core.Currency;
using ANut.Core.Utils;
using R3;
using VContainer.Unity;
using ANut.Core;

namespace Game.Idle
{
    public class IdlePresenter : IStartable, IDisposable
    {
        private readonly IdleService _idleService;
        private readonly IdleView _view;
        private readonly ICurrencyService _currencyService;
        private readonly CompositeDisposable _disposables = new();

        public IdlePresenter(
            IdleService idleService,
            IdleView view,
            ICurrencyService currencyService)
        {
            _idleService = idleService;
            _view = view;
            _currencyService = currencyService;
        }

        public void Start()
        {
            Log.Info("[IdlePresenter] Start");

            _view.OnAreaClicked
                .Subscribe(_ =>
                {
                    Log.Info("[IdlePresenter] Area clicked");
                    _idleService.HandleClick();
                })
                .AddTo(_disposables);

            _view.OnUpgradeClicked
                .Subscribe(_ =>
                {
                    bool isUpgraded = _idleService.TryUpgradeIncome();
                    if (isUpgraded)
                        RefreshUpgradeButton();
                })
                .AddTo(_disposables);

            // Re-evaluate button state whenever coin balance changes.
            _currencyService.GetBalance(CurrencyType.Coins)
                .Subscribe(_ => RefreshUpgradeButton())
                .AddTo(_disposables);

            _idleService.CoinsAdded
                .Subscribe(amount =>
                    _view.SpawnCoinAnimation(amount))
                .AddTo(_disposables);

            RefreshUpgradeButton();
        }

        public void Dispose() => _disposables.Dispose();

        // === Private ===

        private void RefreshUpgradeButton()
        {
            long cost = _idleService.GetUpgradeCost();
            _view.SetUpgradeCost($"{CostFormatter.Compact(cost)} {FontSprites.Currency.Coin}");
            _view.SetUpgradeInteractable(_currencyService.CanAfford(CurrencyType.Coins, cost));
        }
    }
}