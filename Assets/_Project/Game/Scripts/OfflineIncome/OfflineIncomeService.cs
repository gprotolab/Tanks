using System;
using ANut.Core.Currency;
using Game.Economy;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace Game.Offline
{
    public class OfflineIncomeService : IStartable, IDisposable
    {
        private readonly OfflineIncomeDataService _data;
        private readonly EconomySettings _settings;
        private readonly EconomyService _economy;
        private readonly ICurrencyService _currency;
        private readonly Subject<Unit> _rewardReady = new();
        private bool _isInitialized;

        public Observable<Unit> RewardReady => _rewardReady;

        public OfflineIncomeService(
            OfflineIncomeDataService data,
            EconomySettings settings,
            EconomyService economy,
            ICurrencyService currency)
        {
            _data = data;
            _settings = settings;
            _economy = economy;
            _currency = currency;
        }

        void IStartable.Start()
        {
            // Must subscribe before AutoSaveService to persist latest timestamp first.
            Application.focusChanged += OnFocusChanged;
            Application.quitting += OnApplicationQuit;
        }

        public void ProcessOfflineIncome()
        {
            long nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long lastEnd = _data.LastSessionEndUnix;

            // First launch: no offline gap yet, only store baseline timestamp.
            if (lastEnd == 0)
            {
                _data.RecordSessionEnd();
                _isInitialized = true;
                return;
            }

            long elapsedSeconds = nowUnix - lastEnd;
            long reward = _economy.CalcOfflineIncome(elapsedSeconds);

            long minSeconds = _settings.Offline.MinOfflineSeconds;

            if (reward > 0 && elapsedSeconds >= minSeconds)
            {
                _currency.Add(CurrencyType.Coins, reward, OfflineReasons.OfflineIncome);

                // Store reward for Home popup flow.
                _data.SetPendingReward(reward);
                _rewardReady.OnNext(Unit.Default);
            }
            else
            {
                _data.ClearPendingReward();
            }

            // Always refresh timestamp, even when reward is zero.
            _data.RecordSessionEnd();
            _isInitialized = true;
        }

        public void AcknowledgeReward() => _data.ClearPendingReward();

        public void Dispose()
        {
            Application.focusChanged -= OnFocusChanged;
            Application.quitting -= OnApplicationQuit;
            _rewardReady.Dispose();
        }

        private void OnFocusChanged(bool hasFocus)
        {
            // Going to background: update time before autosave runs.
            if (!hasFocus)
            {
                _data.RecordSessionEnd();
                return;
            }

            if (!_isInitialized)
                return;

            ProcessOfflineIncome();
        }

        private void OnApplicationQuit() => _data.RecordSessionEnd();
    }
}