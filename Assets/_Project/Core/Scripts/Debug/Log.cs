using System;
using System.Diagnostics;

namespace ANut.Core
{
    public static class Log
    {
        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void Info(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(format, args);
        }

        public static void Warning(string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(format, args);
        }

        public static void Error(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            UnityEngine.Debug.LogError(msg);
        }

        public static void Exception(Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }
    }
}