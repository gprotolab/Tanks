using Cysharp.Threading.Tasks;
using ANut.Core.Analytics;
using R3;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace ANut.Core.Ads
{
    public class AdsService : IAdsService
    {
        private readonly IRawAdsProvider _provider;
        private readonly AdsConfigSO _config;
        private readonly IAnalyticsService _analytics;

        private readonly float _sessionStartTime;
        private float _lastInterstitialTime = float.NegativeInfinity;

        public AdsService(IRawAdsProvider provider, AdsConfigSO config, IAnalyticsService analytics)
        {
            _provider = provider;
            _config = config;
            _analytics = analytics;
            _sessionStartTime = Time.realtimeSinceStartup;
        }

        public ReadOnlyReactiveProperty<bool> IsRewardedReady => _provider.IsRewardedReady;
        public ReadOnlyReactiveProperty<bool> IsInterstitialReady => _provider.IsInterstitialReady;

        public async UniTask<bool> ShowRewardedAsync(string placement, CancellationToken ct)
        {
            if (!_provider.IsRewardedReady.CurrentValue) return false;

            bool watched = await _provider.ShowRewardedAsync(placement, ct);

            _analytics.LogEvent(AnalyticsEvents.AdRewarded, new Dictionary<string, object>
            {
                ["placement"] = placement,
                ["watched"] = watched,
            });

            return watched;
        }

        public async UniTask ShowInterstitialAsync(string placement, CancellationToken ct)
        {
            if (!CanShowInterstitial()) return;
            if (!_provider.IsInterstitialReady.CurrentValue) return;

            _lastInterstitialTime = Time.realtimeSinceStartup;
            await _provider.ShowInterstitialAsync(placement, ct);

            _analytics.LogEvent(AnalyticsEvents.AdInterstitial, "placement", placement);
        }

        // Private 

        private bool CanShowInterstitial()
        {
            float now = Time.realtimeSinceStartup;
            return (now - _sessionStartTime) >= _config.SessionStartDelaySeconds
                   && (now - _lastInterstitialTime) >= _config.InterstitialCooldownSeconds;
        }
    }
}