using NUnit.Framework;
using ANut.Core.Utils;

namespace Game.Tests.EditMode
{
    [TestFixture]
    public class CostFormatterTests
    {
        [Test]
        public void Compact_ExactlyThousand_ShowsKSuffix()
            => Assert.AreEqual("1K", CostFormatter.Compact(1_000));

        [Test]
        public void Compact_ExactlyMillion_ShowsMSuffix()
            => Assert.AreEqual("1M", CostFormatter.Compact(1_000_000));

        [Test]
        public void Compact_ExactlyBillion_ShowsBSuffix()
            => Assert.AreEqual("1B", CostFormatter.Compact(1_000_000_000));

        [Test]
        public void Compact_ExactlyTrillion_ShowsTSuffix()
            => Assert.AreEqual("1T", CostFormatter.Compact(1_000_000_000_000L));

        [Test]
        public void Detailed_JustBelowMillion_UsesFullFormat()
        {
            // Shows that values below the suffix threshold keep the full number format.
            Assert.AreEqual("999,999", CostFormatter.Detailed(999_999));
        }

        [Test]
        public void Detailed_AtOrAboveMillion_UsesSuffixWithThreeDecimalPrecision()
        {
            // Shows that large values switch to the suffix format with three decimals.
            Assert.AreEqual("1.234M", CostFormatter.Detailed(1_234_000));
        }
    }
}