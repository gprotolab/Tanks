using System;

namespace ANut.Core.Utils
{
    public static class BalanceMath
    {
        public static double Exponential(double baseValue, double multiplier, int level)
            => baseValue * System.Math.Pow(multiplier, level);

        public static double Linear(double baseValue, double step, int level)
            => baseValue + step * level;

        public static long ToLong(double value)
            => (long) System.Math.Round(System.Math.Max(0.0, value));

        public static long RoundCost(double value, int precision = 2)
        {
            if (value <= 0) return 0;

            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)) - precision + 1);
            return ToLong(Math.Round(value / magnitude) * magnitude);
        }
    }
}