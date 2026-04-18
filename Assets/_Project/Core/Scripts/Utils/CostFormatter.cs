using System.Globalization;

namespace ANut.Core.Utils
{
    public static class CostFormatter
    {
        private static readonly (long Threshold, string Suffix)[] Tiers =
        {
            (1_000_000_000_000_000L, "Q"),
            (1_000_000_000_000L, "T"),
            (1_000_000_000L, "B"),
            (1_000_000L, "M"),
            (1_000L, "K"),
        };

        // Lower "1,234,567". Higher "12.345M"
        private const long FullNumberThreshold = 1_000_000L;

        public static string Compact(long value)
        {
            foreach (var (threshold, suffix) in Tiers)
                if (value >= threshold)
                    return FormatValue((double) value / threshold, suffix, precision: 1);

            return value.ToString();
        }

        public static string Detailed(long value)
        {
            foreach (var (threshold, suffix) in Tiers)
            {
                if (value >= threshold)
                {
                    if (value < FullNumberThreshold)
                        return value.ToString("N0", CultureInfo.InvariantCulture);

                    return FormatValue((double) value / threshold, suffix, precision: 3);
                }
            }

            return value.ToString("N0", CultureInfo.InvariantCulture);
        }

        private static string FormatValue(double v, string suffix, int precision)
        {
            string number = v.ToString($"F{precision}", CultureInfo.InvariantCulture)
                .TrimEnd('0')
                .TrimEnd('.');
            return $"{number}{suffix}";
        }
    }
}