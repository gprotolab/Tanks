using System;
using System.Collections.Generic;
using UnityEngine;

namespace ANut.Core.Ads
{
    [CreateAssetMenu(fileName = "AdMobConfig", menuName = "Configs/Ads/AdMob")]
    public class AdMobConfigSO : ScriptableObject
    {
        private const string TestInterstitialAdUnitIdAndroid = "ca-app-pub-3940256099942544/1033173712";
        private const string TestInterstitialAdUnitIdIos = "ca-app-pub-3940256099942544/4411468910";
        private const string TestRewardedAdUnitIdAndroid = "ca-app-pub-3940256099942544/5224354917";
        private const string TestRewardedAdUnitIdIos = "ca-app-pub-3940256099942544/1712485313";

        [Header("Test Ads")] [SerializeField] private bool _useTestAdUnitIds;

        [Header("Interstitial Ad Unit IDs")] [SerializeField]
        private string _interstitialAdUnitIdAndroid;

        [SerializeField] private string _interstitialAdUnitIdIos;

        [Header("Rewarded Ad Unit IDs")] [SerializeField]
        private string _rewardedAdUnitIdAndroid;

        [SerializeField] private string _rewardedAdUnitIdIos;

        [Header("Test Devices")] [SerializeField]
        private List<string> _testDeviceIds = new();

        [Header("Load Retry (exponential back-off)")] [SerializeField, Range(1, 8)]
        private int _maxRetryCount = 4;

        [SerializeField, Range(1f, 60f)] private float _baseRetryDelaySeconds = 5f;

        [Header("Ad Expiry Pre-fetch")] [SerializeField, Range(600f, 3500f)]
        private float _adExpiryReloadSeconds = 3000f;

        public string InterstitialAdUnitId
        {
            get
            {
#if UNITY_ANDROID
                if (_useTestAdUnitIds)
                {
                    return TestInterstitialAdUnitIdAndroid;
                }

                return _interstitialAdUnitIdAndroid;
#elif UNITY_IOS
                if (_useTestAdUnitIds)
                {
                    return TestInterstitialAdUnitIdIos;
                }

                return _interstitialAdUnitIdIos;
#else
                return "unused";
#endif
            }
        }

        public string RewardedAdUnitId
        {
            get
            {
#if UNITY_ANDROID
                if (_useTestAdUnitIds)
                {
                    return TestRewardedAdUnitIdAndroid;
                }

                return _rewardedAdUnitIdAndroid;
#elif UNITY_IOS
                if (_useTestAdUnitIds)
                {
                    return TestRewardedAdUnitIdIos;
                }

                return _rewardedAdUnitIdIos;
#else
                return "unused";
#endif
            }
        }

        public IReadOnlyList<string> TestDeviceIds => _testDeviceIds;
        public int MaxRetryCount => _maxRetryCount;
        public float BaseRetryDelaySeconds => _baseRetryDelaySeconds;
        public float AdExpiryReloadSeconds => _adExpiryReloadSeconds;
    }
}