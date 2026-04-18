using System.Collections.Generic;

namespace ANut.Core.Analytics
{
    public class MockAnalyticsService : IAnalyticsService
    {
        public void LogEvent(string eventName)
        {
            Log.Info("[Analytics] {0}", eventName);
        }

        public void LogEvent(string eventName, string key, object value)
        {
            Log.Info("[Analytics] {0} | {1}={2}", eventName, key, value);
        }

        public void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
            Log.Info("[Analytics] {0} | {1}", eventName, AnalyticsUtils.FormatParameters(parameters));
        }

        public void SetUserProperty(string name, string value)
        {
            Log.Info("[Analytics] UserProperty: {0}={1}", name, value);
        }

        public void SetUserId(string userId)
        {
            Log.Info("[Analytics] SetUserId: {0}", userId);
        }

        public void FlushEvents()
        {
            Log.Info("[Analytics] FlushEvents");
        }
    }
}