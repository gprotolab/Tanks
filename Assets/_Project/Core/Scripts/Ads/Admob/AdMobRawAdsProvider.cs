using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
#if ADMOB_ENABLED
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;
#endif

namespace ANut.Core.Ads
{
    public sealed class AdMobRawAdsProvider : IRawAdsProvider, IDisposable
    {
        private readonly AdMobConfigSO _config;
        private readonly CancellationTokenSource _lifetimeCts = new();

#if ADMOB_ENABLED
        private RewardedAd _rewardedAd;
#endif
        private readonly ReactiveProperty<bool> _isRewardedReady = new(false);
        private bool _rewardedLoadPending;
        private int _rewardedRetryCount;

        private UniTaskCompletionSource<bool> _rewardedShowTcs;

        private bool _rewardedGranted;

#if ADMOB_ENABLED
        private InterstitialAd _interstitialAd;
#endif
        private readonly ReactiveProperty<bool> _isInterstitialReady = new(false);
        private bool _interstitialLoadPending;
        private int _interstitialRetryCount;

        private UniTaskCompletionSource _interstitialShowTcs;

        public ReadOnlyReactiveProperty<bool> IsRewardedReady => _isRewardedReady;
        public ReadOnlyReactiveProperty<bool> IsInterstitialReady => _isInterstitialReady;

        public AdMobRawAdsProvider(AdMobConfigSO config)
        {
            _config = config;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
#if ADMOB_ENABLED
            ConfigureTestDevices();
            await GatherConsentAsync(ct);
            await InitializeSdkAsync(ct);
#else
            Log.Warning("[AdMob] ADMOB_ENABLED is not defined — AdMob provider is inactive.");
            await UniTask.CompletedTask;
#endif
        }

        public async UniTask<bool> ShowRewardedAsync(string placement, CancellationToken ct)
        {
#if ADMOB_ENABLED
            if (_rewardedAd == null || !_rewardedAd.CanShowAd())
            {
                Log.Warning("[AdMob] ShowRewarded called but ad is not ready. Placement: {0}", placement);
                return false;
            }

            _rewardedGranted = false;
            _rewardedShowTcs = new UniTaskCompletionSource<bool>();

            _rewardedAd.Show(reward =>
            {
                _rewardedGranted = true;
                Log.Info("[AdMob] Reward earned: {0} × {1}", reward.Amount, reward.Type);
            });

            bool result;
            try
            {
                result = await _rewardedShowTcs.Task.AttachExternalCancellation(ct);
            }
            catch (OperationCanceledException)
            {
                result = false;
            }

            return result;
#else
            await UniTask.CompletedTask;
            return false;
#endif
        }

        public async UniTask ShowInterstitialAsync(string placement, CancellationToken ct)
        {
#if ADMOB_ENABLED
            if (_interstitialAd == null || !_interstitialAd.CanShowAd())
            {
                Log.Warning("[AdMob] ShowInterstitial called but ad is not ready. Placement: {0}", placement);
                return;
            }

            _interstitialShowTcs = new UniTaskCompletionSource();
            _interstitialAd.Show();

            try
            {
                await _interstitialShowTcs.Task.AttachExternalCancellation(ct);
            }
            catch (OperationCanceledException)
            {
                // Swallow — caller does not need to wait for the ad to close.
            }
#else
            await UniTask.CompletedTask;
#endif
        }

        // Dispose

        public void Dispose()
        {
            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();
            _isRewardedReady.Dispose();
            _isInterstitialReady.Dispose();

#if ADMOB_ENABLED
            DestroyRewardedAd();
            DestroyInterstitialAd();
#endif
        }


        // PRIVATE — REWARDED AD

#if ADMOB_ENABLED
        private async UniTask GatherConsentAsync(CancellationToken ct)
        {
            var updateTcs = new UniTaskCompletionSource();

            ConsentInformation.Update(new ConsentRequestParameters(), error =>
            {
                if (error != null)
                {
                    Log.Warning("[AdMob] Consent update error: {0}", error.Message);
                }

                MobileAdsEventExecutor.ExecuteInUpdate(() => updateTcs.TrySetResult());
            });

            await updateTcs.Task.AttachExternalCancellation(ct);

            if (!ConsentInformation.IsConsentFormAvailable() ||
                ConsentInformation.ConsentStatus != ConsentStatus.Required)
            {
                return;
            }

            var loadTcs = new UniTaskCompletionSource<ConsentForm>();

            ConsentForm.Load((form, error) =>
            {
                if (error != null)
                {
                    Log.Warning("[AdMob] Consent form load error: {0}", error.Message);
                }

                MobileAdsEventExecutor.ExecuteInUpdate(() => loadTcs.TrySetResult(form));
            });

            ConsentForm consentForm = await loadTcs.Task.AttachExternalCancellation(ct);
            if (consentForm == null)
            {
                return;
            }

            var showTcs = new UniTaskCompletionSource();

            consentForm.Show(error =>
            {
                if (error != null)
                {
                    Log.Warning("[AdMob] Consent form show error: {0}", error.Message);
                }

                MobileAdsEventExecutor.ExecuteInUpdate(() => showTcs.TrySetResult());
            });

            await showTcs.Task.AttachExternalCancellation(ct);
        }

        private async UniTask InitializeSdkAsync(CancellationToken ct)
        {
            var tcs = new UniTaskCompletionSource();

            MobileAds.Initialize(status =>
            {
                if (status == null)
                {
                    Log.Error("[AdMob] SDK initialization failed (null status).");
                }
                else
                {
                    Log.Info("[AdMob] SDK initialized.");
                }

                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    tcs.TrySetResult();
                    LoadRewardedAd();
                    LoadInterstitialAd();
                });
            });

            await tcs.Task.AttachExternalCancellation(ct);
        }

        private void LoadRewardedAd()
        {
            if (_rewardedLoadPending) return;
            if (_lifetimeCts.IsCancellationRequested) return;

            _rewardedLoadPending = true;
            DestroyRewardedAd();

            Log.Info("[AdMob] Loading rewarded ad…");

            RewardedAd.Load(_config.RewardedAdUnitId, new AdRequest(), OnRewardedAdLoaded);
        }

        private void OnRewardedAdLoaded(RewardedAd ad, LoadAdError error)
        {
            // Raised off main thread — marshal everything to main thread.
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                _rewardedLoadPending = false;

                if (error != null)
                {
                    Log.Info("[AdMob] Rewarded load failed: {0}", error);
                    ScheduleRewardedRetry();
                    return;
                }

                if (ad == null)
                {
                    Log.Info("[AdMob] Rewarded load returned null ad with null error (unexpected).");
                    ScheduleRewardedRetry();
                    return;
                }

                Log.Info("[AdMob] Rewarded loaded. Response: {0}", ad.GetResponseInfo());

                _rewardedRetryCount = 0;
                _rewardedAd = ad;

                RegisterRewardedEventHandlers(ad);
                SetRewardedReady(true);
                RewardedExpiryReloadAsync(_lifetimeCts.Token).Forget();
            });
        }

        private void RegisterRewardedEventHandlers(RewardedAd ad)
        {
            ad.OnAdPaid += adValue =>
                Log.Info("[AdMob] Rewarded paid: {0} {1}", adValue.Value, adValue.CurrencyCode);

            ad.OnAdImpressionRecorded += () =>
                Log.Info("[AdMob] Rewarded impression recorded.");

            ad.OnAdClicked += () =>
                Log.Info("[AdMob] Rewarded clicked.");

            ad.OnAdFullScreenContentOpened += () =>
                Log.Info("[AdMob] Rewarded opened full screen.");

            ad.OnAdFullScreenContentClosed += () =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    Log.Info("[AdMob] Rewarded closed. Reward granted: {0}", _rewardedGranted);

                    _rewardedShowTcs?.TrySetResult(_rewardedGranted);
                    _rewardedShowTcs = null;

                    SetRewardedReady(false);
                    DestroyRewardedAd();
                    LoadRewardedAd(); // preload next immediately
                });
            };

            ad.OnAdFullScreenContentFailed += adError =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    Log.Error("[AdMob] Rewarded failed to open full screen: {0}", adError);

                    _rewardedShowTcs?.TrySetResult(false);
                    _rewardedShowTcs = null;

                    SetRewardedReady(false);
                    DestroyRewardedAd();
                    LoadRewardedAd();
                });
            };
        }

        private void ScheduleRewardedRetry()
        {
            if (_rewardedRetryCount >= _config.MaxRetryCount)
            {
                Log.Warning("[AdMob] Rewarded: max retries ({0}) reached.", _config.MaxRetryCount);
                return;
            }

            float delay = _config.BaseRetryDelaySeconds * Mathf.Pow(2f, _rewardedRetryCount);
            _rewardedRetryCount++;
            Log.Info("[AdMob] Rewarded retry #{0} in {1:F0}s.", _rewardedRetryCount, delay);

            RetryLoadAsync(delay, LoadRewardedAd, _lifetimeCts.Token).Forget();
        }

        private void SetRewardedReady(bool ready)
        {
            _isRewardedReady.Value = ready;
        }

        private void DestroyRewardedAd()
        {
            if (_rewardedAd == null) return;
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        private void LoadInterstitialAd()
        {
            if (_interstitialLoadPending) return;
            if (_lifetimeCts.IsCancellationRequested) return;

            _interstitialLoadPending = true;
            DestroyInterstitialAd();

            Log.Info("[AdMob] Loading interstitial ad…");

            InterstitialAd.Load(_config.InterstitialAdUnitId, new AdRequest(), OnInterstitialAdLoaded);
        }

        private void OnInterstitialAdLoaded(InterstitialAd ad, LoadAdError error)
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                _interstitialLoadPending = false;

                if (error != null)
                {
                    Log.Info("[AdMob] Interstitial load failed: {0}", error);
                    ScheduleInterstitialRetry();
                    return;
                }

                if (ad == null)
                {
                    Log.Info("[AdMob] Interstitial load returned null ad with null error (unexpected).");
                    ScheduleInterstitialRetry();
                    return;
                }

                Log.Info("[AdMob] Interstitial loaded. Response: {0}", ad.GetResponseInfo());

                _interstitialRetryCount = 0;
                _interstitialAd = ad;

                RegisterInterstitialEventHandlers(ad);
                SetInterstitialReady(true);
                InterstitialExpiryReloadAsync(_lifetimeCts.Token).Forget();
            });
        }

        private void RegisterInterstitialEventHandlers(InterstitialAd ad)
        {
            ad.OnAdPaid += adValue =>
                Log.Info("[AdMob] Interstitial paid: {0} {1}", adValue.Value, adValue.CurrencyCode);

            ad.OnAdImpressionRecorded += () =>
                Log.Info("[AdMob] Interstitial impression recorded.");

            ad.OnAdClicked += () =>
                Log.Info("[AdMob] Interstitial clicked.");

            ad.OnAdFullScreenContentOpened += () =>
                Log.Info("[AdMob] Interstitial opened full screen.");

            ad.OnAdFullScreenContentClosed += () =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    Log.Info("[AdMob] Interstitial closed.");

                    _interstitialShowTcs?.TrySetResult();
                    _interstitialShowTcs = null;

                    SetInterstitialReady(false);
                    DestroyInterstitialAd();
                    LoadInterstitialAd();
                });
            };

            ad.OnAdFullScreenContentFailed += adError =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    Log.Error("[AdMob] Interstitial failed to open full screen: {0}", adError);

                    _interstitialShowTcs?.TrySetResult();
                    _interstitialShowTcs = null;

                    SetInterstitialReady(false);
                    DestroyInterstitialAd();
                    LoadInterstitialAd();
                });
            };
        }

        private void ScheduleInterstitialRetry()
        {
            if (_interstitialRetryCount >= _config.MaxRetryCount)
            {
                Log.Warning("[AdMob] Interstitial: max retries ({0}) reached.", _config.MaxRetryCount);
                return;
            }

            float delay = _config.BaseRetryDelaySeconds * Mathf.Pow(2f, _interstitialRetryCount);
            _interstitialRetryCount++;
            Log.Info("[AdMob] Interstitial retry #{0} in {1:F0}s.", _interstitialRetryCount, delay);

            RetryLoadAsync(delay, LoadInterstitialAd, _lifetimeCts.Token).Forget();
        }

        private void SetInterstitialReady(bool ready)
        {
            _isInterstitialReady.Value = ready;
        }

        private void DestroyInterstitialAd()
        {
            if (_interstitialAd == null) return;
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        // PRIVATE — SHARED HELPERS
        private void ConfigureTestDevices()
        {
            if (_config.TestDeviceIds.Count == 0) return;

            MobileAds.SetRequestConfiguration(new RequestConfiguration
            {
                TestDeviceIds = new System.Collections.Generic.List<string>(_config.TestDeviceIds)
            });

            Log.Info("[AdMob] Test devices configured: {0}", _config.TestDeviceIds.Count);
        }

        /// Respects lifetime cancellation so no load fires after Dispose().
        private static async UniTaskVoid RetryLoadAsync(float delay, Action loadAction, CancellationToken ct)
        {
            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(delay),
                    DelayType.UnscaledDeltaTime,
                    cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            loadAction();
        }

        private async UniTaskVoid RewardedExpiryReloadAsync(CancellationToken ct)
        {
            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_config.AdExpiryReloadSeconds),
                    DelayType.UnscaledDeltaTime,
                    cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (!_isRewardedReady.Value) return;

            Log.Info("[AdMob] Rewarded ad expiry pre-fetch.");
            SetRewardedReady(false);
            DestroyRewardedAd();
            LoadRewardedAd();
        }

        private async UniTaskVoid InterstitialExpiryReloadAsync(CancellationToken ct)
        {
            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_config.AdExpiryReloadSeconds),
                    DelayType.UnscaledDeltaTime,
                    cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (!_isInterstitialReady.CurrentValue) return;

            Log.Info("[AdMob] Interstitial ad expiry pre-fetch.");
            SetInterstitialReady(false);
            DestroyInterstitialAd();
            LoadInterstitialAd();
        }

#endif
    }
}