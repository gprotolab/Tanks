using UnityEngine;

namespace ANut.Core.Analytics
{
    [CreateAssetMenu(fileName = "AnalyticsConfig", menuName = "Configs/Analytics")]
    public class AnalyticsConfigSO : ScriptableObject
    {
        [SerializeField] private string _appMetricaApiKeyAndroid;
        [SerializeField] private string _appMetricaApiKeyIOS;

        public string AppMetricaApiKey
        {
            get
            {
#if UNITY_ANDROID
                return _appMetricaApiKeyAndroid;
#elif UNITY_IOS
                return _appMetricaApiKeyIOS;
#endif
                return _appMetricaApiKeyAndroid;
            }
        }
    }
}