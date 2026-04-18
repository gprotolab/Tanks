using System;
using NUnit.Framework;
using Game.Economy;
using ANut.Core.Utils;

namespace Game.Tests.EditMode
{
    [TestFixture]
    public class EconomyServiceTests
    {
        // Keeps idle income stable, so reward checks stay predictable.
        private static EconomySettings.IdleBalanceSettings FlatIdleSettings(float incomePerSec = 10f) =>
            new()
            {
                BaseIncomePerSec = incomePerSec,
                IncomeMultiplierPerLevel = 1f,
                BaseUpgradeCost = 100,
                UpgradeCostMultiplier = 1f
            };

        private static EconomySettings.BattleBalanceSettings SimpleBattleSettings(
            long firstPlaceReward = 1000,
            float rewardMultiplier = 60f) =>
            new()
            {
                RewardsByPlace = new[] {firstPlaceReward, 500L, 200L},
                PlaceMultipliers = new[] {1f, 0.5f, 0.2f},
                RewardMultiplier = rewardMultiplier
            };

        [Test]
        public void CalcBattleReward_WhenScaledLessThanBase_ReturnsBaseReward()
        {
            // Shows that the method protects the minimum reward from low scaling.
            var reward = EconomyService.CalcBattleReward(
                SimpleBattleSettings(firstPlaceReward: 1000, rewardMultiplier: 60f),
                FlatIdleSettings(10f),
                idleIncomeLevel: 0,
                place: 1);

            Assert.AreEqual(1000L, reward, "Base reward should win when scaled reward is too small.");
        }

        [Test]
        public void CalcBattleReward_WhenScaledGreaterThanBase_ReturnsScaledReward()
        {
            // Shows that high idle income can raise the final reward.
            var reward = EconomyService.CalcBattleReward(
                SimpleBattleSettings(firstPlaceReward: 1000, rewardMultiplier: 60f),
                FlatIdleSettings(100f),
                idleIncomeLevel: 0,
                place: 1);

            Assert.Greater(reward, 1000L, "High income should give a reward above the base value.");
        }

        [Test]
        public void CalcBattleReward_PlaceZero_ClampsToFirstIndex()
        {
            // Shows that invalid low place values still use the first reward slot.
            var battle = SimpleBattleSettings(firstPlaceReward: 1000);
            var idle = FlatIdleSettings(10f);
            long place0 = EconomyService.CalcBattleReward(battle, idle, 0, place: 0);
            long place1 = EconomyService.CalcBattleReward(battle, idle, 0, place: 1);

            Assert.AreEqual(place1, place0, "Place 0 should use the same reward setup as place 1.");
        }

        [Test]
        public void CalcOfflineIncome_ZeroElapsed_ReturnsZero()
        {
            var offline = new EconomySettings.OfflineBalanceSettings
                {MaxOfflineSeconds = 3600, IncomeEfficiency = 0.5f};

            long reward = EconomyService.CalcOfflineIncome(offline, FlatIdleSettings(10f), 0, 0);

            Assert.AreEqual(0L, reward, "Zero offline time should give no reward.");
        }

        [Test]
        public void CalcOfflineIncome_ElapsedWithinCap_CalculatesCorrectly()
        {
            var offline = new EconomySettings.OfflineBalanceSettings
                {MaxOfflineSeconds = 3600, IncomeEfficiency = 0.5f};

            long reward = EconomyService.CalcOfflineIncome(offline, FlatIdleSettings(10f), 1800, 0);

            Assert.AreEqual(9000L, reward);
        }

        [Test]
        public void CalcOfflineIncome_ElapsedExceedsCap_IsCapped()
        {
            var offline = new EconomySettings.OfflineBalanceSettings
                {MaxOfflineSeconds = 3600, IncomeEfficiency = 1f};

            long rewardAtCap = EconomyService.CalcOfflineIncome(offline, FlatIdleSettings(10f), 3600, 0);
            long rewardOverCap = EconomyService.CalcOfflineIncome(offline, FlatIdleSettings(10f), 36000, 0);

            Assert.AreEqual(rewardAtCap, rewardOverCap,
                "Reward for time above the cap should match the reward at the cap.");
        }

        [Test]
        public void CalcMergePartCost_WhenOverrideMatchesPurchaseIndex_ReturnsOverridePrice()
        {
            var merge = new EconomySettings.MergeBalanceSettings
            {
                BasePurchaseCost = 100,
                CostMultiplier = 1.0,
                PriceOverrides = new[]
                {
                    new EconomySettings.PartPriceOverride {PurchaseIndex = 0, Price = 150}
                }
            };

            long cost = EconomyService.CalcMergePartCost(merge, totalPurchases: 0);

            Assert.AreEqual(150L, cost, "The method should return the override price instead of the formula result.");
        }

        [Test]
        public void CalcMergePartCost_WhenNoOverrideMatches_ReturnsCalculatedPrice()
        {
            var merge = new EconomySettings.MergeBalanceSettings
            {
                BasePurchaseCost = 100,
                CostMultiplier = 1.0,
                PriceOverrides = new[]
                {
                    new EconomySettings.PartPriceOverride {PurchaseIndex = 5, Price = 999}
                }
            };

            long cost = EconomyService.CalcMergePartCost(merge, totalPurchases: 0);

            Assert.AreEqual(100L, cost);
        }

        [Test]
        public void CalcMergePartCost_OverrideMatchesExactIndex_OtherIndexNotAffected()
        {
            var merge = new EconomySettings.MergeBalanceSettings
            {
                BasePurchaseCost = 100,
                CostMultiplier = 1.0,
                PriceOverrides = new[]
                {
                    new EconomySettings.PartPriceOverride {PurchaseIndex = 3, Price = 777}
                }
            };

            long cost3 = EconomyService.CalcMergePartCost(merge, totalPurchases: 3);
            long cost4 = EconomyService.CalcMergePartCost(merge, totalPurchases: 4);

            Assert.AreEqual(777L, cost3, "Index 3 should use the override price.");
            Assert.AreNotEqual(777L, cost4, "Index 4 should not use the override for index 3.");
        }
    }
}