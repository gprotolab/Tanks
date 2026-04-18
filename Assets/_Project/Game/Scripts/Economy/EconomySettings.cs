using System;

namespace Game.Economy
{
    [Serializable]
    public class EconomySettings
    {
        public IdleBalanceSettings Idle = new();
        public MergeBalanceSettings Merge = new();
        public BattleBalanceSettings Battle = new();
        public OfflineBalanceSettings Offline = new();

        // Idle

        [Serializable]
        public class IdleBalanceSettings
        {
            public float BaseIncomePerSec = 8f;
            public float IncomeMultiplierPerLevel = 1.12f;
            public long BaseUpgradeCost = 100;
            public float UpgradeCostMultiplier = 1.35f;
        }

        // Merge 

        [Serializable]
        public class MergeBalanceSettings
        {
            // Part purchase / sell
            public double BasePurchaseCost = 150;
            public double CostMultiplier = 1.08;
            public double SellCostRatio = 0.4;

            // Cell purchase — tied to the part-purchase curve
            // cellCost = BasePurchaseCost × CostMultiplier^(purchasedCells × CellPartsRatio) × CellCostRatio
            // Effective per-cell multiplier = CostMultiplier^CellPartsRatio
            // Default: 1.08^6 ≈ 1.59 per cell
            public int CellPartsRatio = 6; // how many part purchases equal 1 cell tier
            public double CellCostRatio = 10.0; // how many times a cell costs more than a part at the same tier

            public PartPriceOverride[] PriceOverrides = Array.Empty<PartPriceOverride>();
        }

        [Serializable]
        public class PartPriceOverride
        {
            public int PurchaseIndex;

            public long Price;
        }

        // Offline

        [Serializable]
        public class OfflineBalanceSettings
        {
            public long MaxOfflineSeconds = 14400;

            public long MinOfflineSeconds = 300;

            public float IncomeEfficiency = 0.5f;

            public int RewardedAdMultiplier = 2;
        }

        // Battle 

        [Serializable]
        public class BattleBalanceSettings
        {
            public long[] RewardsByPlace = {500, 350, 250, 175, 125, 90, 60, 30};
            public float RewardMultiplier = 60f;
            public int RewardedAdMultiplier = 2;

            public float[] PlaceMultipliers = {1.0f, 0.7f, 0.5f, 0.35f, 0.25f, 0.18f, 0.12f, 0.05f};
        }
    }
}