using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Game.Battle;

namespace Game.Tests.EditMode
{
    [TestFixture]
    public class BattleDifficultyServiceTests
    {
        private static BotDifficultyConfigSO CreateConfig(
            BotDifficultyConfigSO.DifficultyTier[] tiers,
            int poorFinishesPerTierDown = 3,
            int poorFinishPlaceThreshold = 4)
        {
            var config = ScriptableObject.CreateInstance<BotDifficultyConfigSO>();
            var t = typeof(BotDifficultyConfigSO);

            t.GetField("_tiers", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(config, tiers);
            t.GetField("_poorFinishesPerTierDown", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(config, poorFinishesPerTierDown);
            t.GetField("_poorFinishPlaceThreshold", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(config, poorFinishPlaceThreshold);

            return config;
        }

        private static BotDifficultyConfigSO.DifficultyTier Tier(string name, int minBattles) =>
            new() {Name = name, MinBattles = minBattles};

        // Keeps tier setup short, so tests stay easy to read.
        private static BotDifficultyConfigSO ThreeTierConfig(int poorFinishesPerTierDown = 3)
        {
            return CreateConfig(
                new[]
                {
                    Tier("Easy", minBattles: 0),
                    Tier("Medium", minBattles: 5),
                    Tier("Hard", minBattles: 10),
                },
                poorFinishesPerTierDown);
        }

        private static BattleDifficultyService Service(BotDifficultyConfigSO config)
            => new(config);

        [Test]
        public void GetTier_ExactlyAtSecondTierBoundary_ReturnsSecondTier()
        {
            var svc = Service(ThreeTierConfig());
            var tier = svc.GetTier(battlesOnCurrentEquip: 5, poorFinishStreak: 0);
            Assert.AreEqual("Medium", tier.Name);
        }

        [Test]
        public void GetTier_BelowSecondTierBoundary_StaysAtFirstTier()
        {
            var svc = Service(ThreeTierConfig());
            var tier = svc.GetTier(battlesOnCurrentEquip: 4, poorFinishStreak: 0);
            Assert.AreEqual("Easy", tier.Name, "4 battles are still below Medium because it starts at 5.");
        }

        [Test]
        public void GetTier_StreakBelowThreshold_NoPenaltyApplied()
        {
            // Shows that integer division keeps the penalty at zero below the limit.
            var svc = Service(ThreeTierConfig(poorFinishesPerTierDown: 3));
            var tier = svc.GetTier(battlesOnCurrentEquip: 10, poorFinishStreak: 2);
            Assert.AreEqual("Hard", tier.Name, "A streak of 2 with threshold 3 should not add a penalty.");
        }

        [Test]
        public void GetTier_StreakAtThreshold_AppliesOneTierPenalty()
        {
            // Shows the first full penalty step at the exact threshold.
            var svc = Service(ThreeTierConfig(poorFinishesPerTierDown: 3));
            var tier = svc.GetTier(battlesOnCurrentEquip: 10, poorFinishStreak: 3);
            Assert.AreEqual("Medium", tier.Name, "A streak of 3 with threshold 3 should lower the tier by one.");
        }

        [Test]
        public void GetTier_StreakDoubleThreshold_AppliesTwoTierPenalty()
        {
            // Shows that two full penalty steps can drop the tier to Easy.
            var svc = Service(ThreeTierConfig(poorFinishesPerTierDown: 3));
            var tier = svc.GetTier(battlesOnCurrentEquip: 10, poorFinishStreak: 6);
            Assert.AreEqual("Easy", tier.Name, "A streak of 6 with threshold 3 should lower the tier by two.");
        }

        [Test]
        public void GetTier_PenaltyExceedsTiers_ClampsToLowestTier()
        {
            // Shows that the result stays inside the valid tier range.
            var svc = Service(ThreeTierConfig(poorFinishesPerTierDown: 1));
            var tier = svc.GetTier(battlesOnCurrentEquip: 10, poorFinishStreak: 999);
            Assert.AreEqual("Easy", tier.Name,
                "Penalty should never go below index 0, so Easy stays the minimum tier.");
        }

        [Test]
        public void GetTier_SingleTierOnly_AlwaysReturnsThatTier()
        {
            var config = CreateConfig(new[] {Tier("OnlyTier", minBattles: 0)});
            var svc = new BattleDifficultyService(config);

            // Shows that one-tier configs stay stable in every case.
            var tierNoStreak = svc.GetTier(0, 0);
            var tierBigStreak = svc.GetTier(100, 999);

            Assert.AreEqual("OnlyTier", tierNoStreak.Name);
            Assert.AreEqual("OnlyTier", tierBigStreak.Name);
        }
    }
}