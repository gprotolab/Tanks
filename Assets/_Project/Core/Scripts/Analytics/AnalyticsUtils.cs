using System.Collections.Generic;
using System.Text;

namespace ANut.Core.Analytics
{
    public static class AnalyticsUtils
    {
        public static string FormatParameters(IReadOnlyDictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return "(none)";
            }

            var stringBuilder = new StringBuilder();
            bool isFirstParameter = true;

            foreach (var pair in parameters)
            {
                if (!isFirstParameter)
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append(pair.Key);
                stringBuilder.Append('=');
                stringBuilder.Append(pair.Value);
                isFirstParameter = false;
            }

            return stringBuilder.ToString();
        }
    }
}