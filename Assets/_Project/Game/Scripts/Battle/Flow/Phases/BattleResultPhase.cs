using Cysharp.Threading.Tasks;
using ANut.Core.Ads;
using ANut.Core.Analytics;
using Game.Economy;
using ANut.Core;
using R3;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Game.Battle
{
    public class BattleResultPhase
    {
        private readonly ScoreService _scoreService;
        private readonly EconomyService _economy;
        private readonly BattleResultView _resultView;
        private readonly IAdsService _adService;
        private readonly IAnalyticsService _analytics;
        private readonly BattleStatsDataService _battleStatsData;

        private const string RewardedPlacement = "battle_reward_multiply";

        // Mutable state for one execution. Reset on each ExecuteAsync call.
        private long _currentReward;
        private bool _multiplied;
        private UniTaskCompletionSource _claimTcs;

        public BattleResultPhase(
            ScoreService scoreService,
            EconomyService economy,
            BattleResultView resultView,
            IAdsService adService,
            IAnalyticsService analytics,
            BattleStatsDataService battleStatsData)
        {
            _scoreService = scoreService;
            _economy = economy;
            _resultView = resultView;
            _adService = adService;
            _analytics = analytics;
            _battleStatsData = battleStatsData;
        }

        public async UniTask<BattleResultData> ExecuteAsync(CancellationToken ct)
        {
            float phaseStartTime = Time.realtimeSinceStartup;
            var disposables = new CompositeDisposable();

            var ranking = _scoreService.GetCurrentRanking();
            int playerPlace = _scoreService.GetPlayerPlace();
            _currentReward = _economy.GetBattleReward(playerPlace);
            _multiplied = false;
            _claimTcs = new UniTaskCompletionSource();

            _resultView.Show(ranking, playerPlace, _currentReward);
            _resultView.SetMultiplier(_economy.GetRewardedAdMultiplier());
            _resultView.SetMultiplyButtonActive(_adService.IsRewardedReady.CurrentValue);

            Log.Info("[BattleResultPhase] Player place: {0}, reward: {1}", playerPlace, _currentReward);

            _adService.IsRewardedReady
                .Subscribe(isReady => OnRewardedReadyChanged(isReady))
                .AddTo(disposables);

            _resultView.ClaimClicked
                .Subscribe(_ => OnClaimClicked())
                .AddTo(disposables);

            _resultView.MultiplyClicked
                .Subscribe(_ => HandleMultiplyAsync(ct).Forget())
                .AddTo(disposables);

            using var reg = ct.Register(() => _claimTcs.TrySetCanceled());

            try
            {
                await _claimTcs.Task;
            }
            finally
            {
                disposables.Dispose();
            }

            _analytics.LogEvent(AnalyticsEvents.BattleEnd, new Dictionary<string, object>
            {
                ["place"] = playerPlace,
                ["reward"] = _currentReward,
                ["duration"] = Time.realtimeSinceStartup - phaseStartTime,
                ["battles_on_equip"] = _battleStatsData.BattlesOnCurrentEquip,
                ["poor_finish_streak"] = _battleStatsData.PoorFinishStreak,
            });

            return new BattleResultData(_currentReward, playerPlace);
        }

        private async UniTaskVoid HandleMultiplyAsync(CancellationToken ct)
        {
            if (_multiplied) return;

            bool watched = await _adService.ShowRewardedAsync(RewardedPlacement, ct);
            if (!watched) return;

            int multiplier = _economy.GetRewardedAdMultiplier();
            _currentReward *= multiplier;
            _multiplied = true;
            _resultView.UpdateReward(_currentReward);
            _resultView.DisableMultiplyButton();

            Log.Info("[BattleResultPhase] Reward multiplied x{0}: {1}", multiplier, _currentReward);

            // Auto-claim after rewarded ad to avoid an extra tap.
            _claimTcs.TrySetResult();
        }

        private void OnClaimClicked()
        {
            _claimTcs.TrySetResult();
        }

        private void OnRewardedReadyChanged(bool isReady)
        {
            _resultView.SetMultiplyButtonActive(isReady);
        }
    }

    // Result payload for battle flow exit handling.
    public readonly struct BattleResultData
    {
        public readonly long Reward;
        public readonly int PlayerPlace;

        public BattleResultData(long reward, int playerPlace)
        {
            Reward = reward;
            PlayerPlace = playerPlace;
        }
    }
}