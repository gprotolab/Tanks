using System;
using Cysharp.Threading.Tasks;
using ANut.Core.Ads;
using ANut.Core.Currency;
using Game.Economy;
using R3;
using VContainer.Unity;

namespace Game.Offline
{
    public class OfflineRewardPopupPresenter : IStartable, IDisposable
    {
        private readonly OfflineIncomeDataService _data;
        private readonly OfflineIncomeService _service;
        private readonly OfflineRewardPopupView _view;
        private readonly IAdsService _adsService;
        private readonly ICurrencyService _currencyService;
        private readonly EconomyService _economyService;
        private readonly CompositeDisposable _disposables = new();

        public OfflineRewardPopupPresenter(
            OfflineIncomeDataService data,
            OfflineIncomeService service,
            OfflineRewardPopupView view,
            IAdsService adsService,
            ICurrencyService currencyService,
            EconomyService economyService)
        {
            _data = data;
            _service = service;
            _view = view;
            _adsService = adsService;
            _currencyService = currencyService;
            _economyService = economyService;
        }

        public void Start()
        {
            _service.RewardReady
                .Subscribe(_ => OnRewardReady())
                .AddTo(_disposables);

            _adsService.IsRewardedReady
                .Subscribe(isReady => OnRewardedReadyChanged(isReady))
                .AddTo(_disposables);

            _view.CollectClicked
                .Subscribe(_ => OnCollect())
                .AddTo(_disposables);

            _view.MultiplyClicked
                .Subscribe(_ => OnMultiplyClicked())
                .AddTo(_disposables);

            if (!_data.HasPendingReward)
            {
                _view.Hide();
                return;
            }

            ShowPopup(_data.PendingReward);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void OnRewardReady()
        {
            if (!_data.HasPendingReward)
                return;

            ShowPopup(_data.PendingReward);
        }

        private void ShowPopup(long reward)
        {
            int multiplier = _economyService.GetOfflineRewardedAdMultiplier();

            _view.SetAmount(reward);
            _view.SetRvMultiplicator(multiplier);
            _view.SetMultiplyButtonActive(_adsService.IsRewardedReady.CurrentValue);
            _view.Show();
        }

        private void OnCollect()
        {
            _service.AcknowledgeReward();
            _view.Hide();
        }

        private async UniTaskVoid OnMultiplyCollect()
        {
            int multiplier = _economyService.GetOfflineRewardedAdMultiplier();
            bool watched =
                await _adsService.ShowRewardedAsync("offline_income_multiply", _view.destroyCancellationToken);

            if (watched)
            {
                long extraReward = _data.PendingReward * (multiplier - 1);

                if (extraReward > 0)
                    _currencyService.Add(CurrencyType.Coins, extraReward, OfflineReasons.OfflineIncomeRv);
            }

            _service.AcknowledgeReward();
            _view.Hide();
        }

        private void OnMultiplyClicked()
        {
            OnMultiplyCollect().Forget();
        }

        private void OnRewardedReadyChanged(bool isReady)
        {
            _view.SetMultiplyButtonActive(isReady);
        }
    }
}