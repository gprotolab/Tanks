using UnityEngine;

namespace ANut.Core.Ads
{
    public enum AdsProviderMode
    {
        Mock = 0,
        AdMob = 1,
    }


    [CreateAssetMenu(fileName = "AdsConfig", menuName = "Configs/Ads")]
    public class AdsConfigSO : ScriptableObject
    {
        // Provider 

        [Header("Provider")] [SerializeField] private AdsProviderMode _providerMode = AdsProviderMode.Mock;

        // Throttle 

        [Header("Throttle")] [SerializeField, Range(0f, 300f)]
        private float _interstitialCooldownSeconds = 30f;

        [SerializeField, Range(0f, 600f)] private float _sessionStartDelaySeconds = 60f;

        // Public API 

        public AdsProviderMode ProviderMode => _providerMode;
        public float InterstitialCooldownSeconds => _interstitialCooldownSeconds;
        public float SessionStartDelaySeconds => _sessionStartDelaySeconds;
    }
}