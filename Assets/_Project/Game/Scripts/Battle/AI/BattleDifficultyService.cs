using UnityEngine;
using ANut.Core;

namespace Game.Battle
{
    public class BattleDifficultyService
    {
        private readonly BotDifficultyConfigSO _difficultyConfig;

        public BattleDifficultyService(BotDifficultyConfigSO difficultyConfig)
        {
            _difficultyConfig = difficultyConfig;
        }

        public BotDifficultyConfigSO.DifficultyTier GetTier(int battlesOnCurrentEquip, int poorFinishStreak)
        {
            var tiers = _difficultyConfig.Tiers;
            if (tiers == null || tiers.Length == 0)
            {
                Log.Error("[BattleDifficultyService] Tiers array is empty!");
                return new BotDifficultyConfigSO.DifficultyTier();
            }

            int baseIndex = 0;
            for (int i = 0; i < tiers.Length; i++)
            {
                if (battlesOnCurrentEquip >= tiers[i].MinBattles)
                {
                    baseIndex = i;
                }
            }

            int penalty = poorFinishStreak / _difficultyConfig.PoorFinishesPerTierDown;
            int finalIndex = Mathf.Max(0, baseIndex - penalty);
            return tiers[finalIndex];
        }
    }
}