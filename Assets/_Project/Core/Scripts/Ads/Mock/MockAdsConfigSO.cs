using UnityEngine;

namespace ANut.Core.Ads
{
    [CreateAssetMenu(fileName = "MockAdsConfig", menuName = "Configs/Ads/Mock")]
    public class MockAdsConfigSO : ScriptableObject
    {
        [SerializeField] private MockAdOverlay _overlayPrefab;

        [SerializeField, Range(0f, 300f),
         Tooltip("Seconds after startup (and after each show) before the ad is ready again.")]
        private float _fakeLoadDelay = 2f;

        public MockAdOverlay OverlayPrefab => _overlayPrefab;
        public float FakeLoadDelay => _fakeLoadDelay;
    }
}