using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;

namespace ANut.Core.Ads
{
    public class MockRawAdsProvider : IRawAdsProvider, IDisposable
    {
        private readonly MockAdsConfigSO _config;
        private readonly CancellationTokenSource _lifetimeCts = new();

        private readonly ReactiveProperty<bool> _isRewardedReady = new(false);
        private readonly ReactiveProperty<bool> _isInterstitialReady = new(false);

        // Version counters cancel stale load tasks when a new load starts
        private int _rewardedLoadVersion;
        private int _interstitialLoadVersion;

        public ReadOnlyReactiveProperty<bool> IsRewardedReady => _isRewardedReady;
        public ReadOnlyReactiveProperty<bool> IsInterstitialReady => _isInterstitialReady;

        public MockRawAdsProvider(MockAdsConfigSO config)
        {
            _config = config;
        }

        public UniTask InitializeAsync(CancellationToken ct)
        {
            if (_config.OverlayPrefab == null)
                Log.Error("[MockRawAdsProvider] OverlayPrefab is not assigned in MockAdsConfig!");

            Log.Info("[MockRawAdsProvider] Initialized. Ads ready in {0}s.", _config.FakeLoadDelay);

            ScheduleRewardedLoad();
            ScheduleInterstitialLoad();
            return UniTask.CompletedTask;
        }

        public async UniTask<bool> ShowRewardedAsync(string placement, CancellationToken ct)
        {
            Log.Info("[MockRawAdsProvider] ShowRewarded: {0}", placement);

            var overlay = InstantiateOverlay();
            if (overlay == null) return false;

            bool result = await overlay.ShowAsync("TEST RV\n" + placement, rewarded: true, ct);

            SetRewardedReady(false);
            ScheduleRewardedLoad();
            Log.Info("[MockRawAdsProvider] Rewarded shown. Next ready in {0}s.", _config.FakeLoadDelay);

            return result;
        }

        public async UniTask ShowInterstitialAsync(string placement, CancellationToken ct)
        {
            Log.Info("[MockRawAdsProvider] ShowInterstitial: {0}", placement);

            var overlay = InstantiateOverlay();
            if (overlay == null) return;

            await overlay.ShowAsync("TEST INTER\n" + placement, rewarded: false, ct);

            _isInterstitialReady.Value = false;
            ScheduleInterstitialLoad();
            Log.Info("[MockRawAdsProvider] Interstitial shown. Next ready in {0}s.", _config.FakeLoadDelay);
        }

        public void Dispose()
        {
            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();
            _isRewardedReady.Dispose();
            _isInterstitialReady.Dispose();
        }

        // Private

        private void SetRewardedReady(bool ready)
        {
            _isRewardedReady.Value = ready;
        }

        private void ScheduleRewardedLoad()
        {
            int version = ++_rewardedLoadVersion;
            LoadRewardedAsync(version, _lifetimeCts.Token).Forget();
        }

        private void ScheduleInterstitialLoad()
        {
            int version = ++_interstitialLoadVersion;
            LoadInterstitialAsync(version, _lifetimeCts.Token).Forget();
        }

        private async UniTaskVoid LoadRewardedAsync(int version, CancellationToken ct)
        {
            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_config.FakeLoadDelay),
                    DelayType.UnscaledDeltaTime,
                    cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (version != _rewardedLoadVersion) return;

            SetRewardedReady(true);
        }

        private async UniTaskVoid LoadInterstitialAsync(int version, CancellationToken ct)
        {
            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_config.FakeLoadDelay),
                    DelayType.UnscaledDeltaTime,
                    cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (version != _interstitialLoadVersion) return;

            _isInterstitialReady.Value = true;
        }

        private MockAdOverlay InstantiateOverlay()
        {
            if (_config.OverlayPrefab == null)
            {
                Log.Error("[MockRawAdsProvider] OverlayPrefab is not assigned in MockAdsConfig!");
                return null;
            }

            return UnityEngine.Object.Instantiate(_config.OverlayPrefab);
        }
    }
}