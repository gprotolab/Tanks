using System;
using ANut.Core.Utils;
using Game.Idle;

namespace Game.Economy
{
    public class EconomyService
    {
        private const int CostPrecision = 3;

        private readonly EconomySettings _settings;
        private readonly IdleDataService _idleData;

        public EconomyService(EconomySettings settings, IdleDataService idleData)
        {
            _settings = settings;
            _idleData = idleData;
        }

        public float GetIdleIncomePerSec(int incomeLevel)
            => CalcIdleIncome(_settings.Idle, incomeLevel);

        public long GetIdleUpgradeCost(int incomeLevel)
            => CalcIdleUpgradeCost(_settings.Idle, incomeLevel);

        public long CalcClickReward(int incomeLevel)
        {
            float income = GetIdleIncomePerSec(incomeLevel);
            return Math.Max(1, BalanceMath.ToLong(income));
        }

        public long GetMergePartCost(int totalPurchases)
            => CalcMergePartCost(_settings.Merge, totalPurchases);

        public long GetMergeSellCost(int totalPurchases)
            => CalcMergeSellCost(_settings.Merge, totalPurchases);

        public long GetMergeCellCost(int purchasedCells)
            => CalcMergeCellCost(_settings.Merge, purchasedCells);

        public long GetBattleReward(int place)
            => CalcBattleReward(_settings.Battle, _settings.Idle, _idleData.IncomeLevel, place);

        public int GetRewardedAdMultiplier() => _settings.Battle.RewardedAdMultiplier;

        public int GetOfflineRewardedAdMultiplier() => _settings.Offline.RewardedAdMultiplier;

        public long GetBattleReward(int place, int idleIncomeLevel)
            => CalcBattleReward(_settings.Battle, _settings.Idle, idleIncomeLevel, place);

        public long CalcOfflineIncome(long elapsedSeconds)
            => CalcOfflineIncome(_settings.Offline, _settings.Idle, elapsedSeconds, _idleData.IncomeLevel);

        public static long CalcOfflineIncome(
            EconomySettings.OfflineBalanceSettings offline,
            EconomySettings.IdleBalanceSettings idle,
            long elapsedSeconds,
            int incomeLevel)
        {
            long capped = Math.Clamp(elapsedSeconds, 0, offline.MaxOfflineSeconds);
            float income = CalcIdleIncome(idle, incomeLevel);
            return BalanceMath.ToLong(income * capped * offline.IncomeEfficiency);
        }

        public static float CalcIdleIncome(EconomySettings.IdleBalanceSettings s, int level)
            => BalanceMath.RoundCost(
                BalanceMath.Exponential(s.BaseIncomePerSec, s.IncomeMultiplierPerLevel, level),
                CostPrecision + 2);

        public static long CalcIdleUpgradeCost(EconomySettings.IdleBalanceSettings s, int level)
            => BalanceMath.RoundCost(
                BalanceMath.ToLong(BalanceMath.Exponential(s.BaseUpgradeCost, s.UpgradeCostMultiplier, level)),
                CostPrecision);

        public static long CalcMergePartCost(EconomySettings.MergeBalanceSettings s, int totalPurchases)
        {
            if (s.PriceOverrides != null)
            {
                foreach (var o in s.PriceOverrides)
                {
                    if (o.PurchaseIndex == totalPurchases)
                        return o.Price;
                }
            }

            return BalanceMath.RoundCost(
                BalanceMath.ToLong(BalanceMath.Exponential(s.BasePurchaseCost, s.CostMultiplier, totalPurchases)),
                CostPrecision);
        }

        public static long CalcMergeSellCost(EconomySettings.MergeBalanceSettings s, int totalPurchases)
            => BalanceMath.RoundCost(
                BalanceMath.ToLong(CalcMergePartCost(s, totalPurchases) * s.SellCostRatio),
                CostPrecision);

        public static long CalcMergeCellCost(EconomySettings.MergeBalanceSettings s, int purchasedCells)
        {
            int partIndex = purchasedCells * s.CellPartsRatio;
            double partCostAtTier = BalanceMath.Exponential(s.BasePurchaseCost, s.CostMultiplier, partIndex);
            return BalanceMath.RoundCost(BalanceMath.ToLong(partCostAtTier * s.CellCostRatio), CostPrecision);
        }

        public static long CalcBattleReward(
            EconomySettings.BattleBalanceSettings battle,
            EconomySettings.IdleBalanceSettings idle,
            int idleIncomeLevel,
            int place)
        {
            int index = Math.Clamp(place - 1, 0, battle.RewardsByPlace.Length - 1);
            long baseReward = battle.RewardsByPlace[index];

            float incomePerSec = CalcIdleIncome(idle, idleIncomeLevel);

            float placeMult = (battle.PlaceMultipliers != null && index < battle.PlaceMultipliers.Length)
                ? battle.PlaceMultipliers[index]
                : 1f;

            long scaledReward = BalanceMath.ToLong(incomePerSec * battle.RewardMultiplier * placeMult);

            return Math.Max(baseReward, scaledReward);
        }
    }
}