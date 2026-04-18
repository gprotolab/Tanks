using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ANut.Core.Audio;
using ANut.Core.Currency;
using ANut.Core.Utils;
using Game.Economy;
using R3;
using VContainer.Unity;

namespace Game.Idle
{
    public class IdleService : IAsyncStartable, IDisposable
    {
        private readonly IdleDataService _idleData;
        private readonly ICurrencyService _currencyService;
        private readonly EconomyService _economy;
        private readonly IAudioService _audioService;
        private readonly Subject<long> _coinsAdded = new();

        public Observable<long> CoinsAdded => _coinsAdded;

        public IdleService(
            IdleDataService idleData,
            ICurrencyService currencyService,
            EconomyService economy,
            IAudioService audioService)
        {
            _idleData = idleData;
            _currencyService = currencyService;
            _economy = economy;
            _audioService = audioService;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    await UniTask.Delay(1000, cancellationToken: ct);
                    TickIncome();
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        public void HandleClick()
        {
            long clickReward = _economy.CalcClickReward(_idleData.IncomeLevel);
            AddCoins(clickReward, IdleReasons.Click);
            _audioService.PlaySfx(SoundId.Idle_CoinCollect);
        }

        public bool TryUpgradeIncome()
        {
            long cost = _economy.GetIdleUpgradeCost(_idleData.IncomeLevel);
            if (!_currencyService.CanAfford(CurrencyType.Coins, cost))
                return false;

            _currencyService.TrySpend(CurrencyType.Coins, cost, IdleReasons.IncomeUpgrade);
            _idleData.IncrementIncomeLevel();
            return true;
        }

        public long GetUpgradeCost() => _economy.GetIdleUpgradeCost(_idleData.IncomeLevel);

        // Private 

        private void TickIncome()
        {
            long gained = BalanceMath.ToLong(_economy.GetIdleIncomePerSec(_idleData.IncomeLevel));
            if (gained > 0)
                AddCoins(gained, IdleReasons.Idle);
        }

        private void AddCoins(long amount, string reason)
        {
            if (amount <= 0)
                return;

            _currencyService.Add(CurrencyType.Coins, amount, reason);
            _coinsAdded.OnNext(amount);
        }

        public void Dispose() => _coinsAdded.Dispose();
    }
}