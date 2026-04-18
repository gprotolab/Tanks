using System.Collections.Generic;
using UnityEngine;

#if APPMETRICA_ENABLED
using Io.AppMetrica;
using Io.AppMetrica.Profile;
using Newtonsoft.Json;
#endif

namespace ANut.Core.Analytics
{
    public class AppMetricaAnalyticsService : IAnalyticsService, IAnalyticsInitializer
    {
        private readonly AnalyticsConfigSO _config;

        public AppMetricaAnalyticsService(AnalyticsConfigSO config)
        {
            _config = config;
        }

        public void Initialize(bool dataSendingEnabled, bool isFirstLaunch)
        {
            Log.Info("[AppMetricaAnalyticsService] Initialize");
#if APPMETRICA_ENABLED
            AppMetrica.Activate(new AppMetricaConfig(_config.AppMetricaApiKey)
            {
                FirstActivationAsUpdate = !isFirstLaunch,
                DataSendingEnabled = dataSendingEnabled,
                Logs = Debug.isDebugBuild,
            });

            Log.Info("[AppMetricaAnalyticsService] Activate");
#endif
        }

        public void SetDataSendingEnabled(bool enabled)
        {
            // Consent flow is intentionally not wired yet.
#if APPMETRICA_ENABLED
            AppMetrica.SetDataSendingEnabled(enabled);

            Log.Info("[AppMetricaAnalyticsService] SetDataSendingEnabled {0}", enabled);
#endif
        }

        public void LogEvent(string eventName)
        {
            Log.Info("[AppMetricaAnalyticsService] LogEvent: {0}", eventName);
#if APPMETRICA_ENABLED
            AppMetrica.ReportEvent(eventName);
#endif
        }

        public void LogEvent(string eventName, string key, object value)
        {
            Log.Info("[AppMetricaAnalyticsService] LogEvent: {0} | {1}={2}", eventName, key, value);
#if APPMETRICA_ENABLED
            LogEvent(eventName, new Dictionary<string, object> {[key] = value});
#endif
        }

        public void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
            Log.Info("[AppMetricaAnalyticsService] LogEvent: {0} | payload={1}", eventName,
                AnalyticsUtils.FormatParameters(parameters));
#if APPMETRICA_ENABLED
            AppMetrica.ReportEvent(eventName, JsonConvert.SerializeObject(parameters));
#endif
        }

        public void SetUserId(string userId)
        {
            Log.Info("[AppMetricaAnalyticsService] SetUserId: {0}", userId);
#if APPMETRICA_ENABLED
            AppMetrica.SetUserProfileID(userId);
#endif
        }

        public void SetUserProperty(string name, string value)
        {
            Log.Info("[AppMetricaAnalyticsService] SetUserProperty: {0}={1}", name, value);
#if APPMETRICA_ENABLED
            UserProfile profile;

            if (double.TryParse(value, out double number))
                profile = new UserProfile().Apply(Attribute.CustomNumber(name).WithValue(number));
            else
                profile = new UserProfile().Apply(Attribute.CustomString(name).WithValue(value));

            AppMetrica.ReportUserProfile(profile);
#endif
        }

        public void FlushEvents()
        {
            Log.Info("[AppMetricaAnalyticsService] FlushEvents");
#if APPMETRICA_ENABLED
            AppMetrica.SendEventsBuffer();
#endif
        }
    }
}