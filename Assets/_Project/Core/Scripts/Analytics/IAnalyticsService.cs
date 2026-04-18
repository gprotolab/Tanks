using System.Collections.Generic;

namespace ANut.Core.Analytics
{
    public interface IAnalyticsService
    {
        void LogEvent(string eventName);
        void LogEvent(string eventName, string key, object value);
        void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters);
        void SetUserProperty(string name, string value);
        void SetUserId(string userId);
        void FlushEvents();
    }
}