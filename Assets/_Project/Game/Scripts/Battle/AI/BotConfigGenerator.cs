using UnityEngine;
using ANut.Core;

namespace Game.Battle
{
    public class BotConfigGenerator
    {
        private readonly BattleConfigSO _battleConfig;
        private readonly BattleDifficultyService _battleDifficultyService;
        private readonly BotBehaviorConfigSO _behaviorConfig;
        private readonly BotNamesConfigSO _namesList;

        public BotConfigGenerator(
            BattleConfigSO battleConfig,
            BattleDifficultyService battleDifficultyService,
            BotBehaviorConfigSO behaviorConfig,
            BotNamesConfigSO namesList)
        {
            _battleConfig = battleConfig;
            _battleDifficultyService = battleDifficultyService;
            _behaviorConfig = behaviorConfig;
            _namesList = namesList;
        }

        public BotInitData[] Generate(
            int playerTurretLevel,
            int playerChassisLevel,
            int battlesOnCurrentEquip,
            int poorFinishStreak)
        {
            int botCount = _battleConfig.BotCount;
            var tier = _battleDifficultyService.GetTier(battlesOnCurrentEquip, poorFinishStreak);
            var names = PickUniqueNames(botCount);
            int expertCount = GetExpertCount(tier);

            var botInitDataArray = new BotInitData[botCount];
            int index = 0;

            var slots = new[]
            {
                (tier.WeakCount, tier.WeakTurretOffset, tier.WeakChassisOffset),
                (tier.MediumCount, tier.MediumTurretOffset, tier.MediumChassisOffset),
                (tier.StrongCount, tier.StrongTurretOffset, tier.StrongChassisOffset),
            };

            foreach (var (count, turretOffset, chassisOffset) in slots)
            {
                for (int i = 0; i < count && index < botCount; i++, index++)
                {
                    botInitDataArray[index] = CreateConfig(
                        names[index],
                        playerTurretLevel + turretOffset,
                        playerChassisLevel + chassisOffset,
                        index < expertCount);
                }
            }

            // Fallback when tier slot totals are smaller than botCount.
            while (index < botCount)
            {
                botInitDataArray[index] = CreateConfig(names[index], playerTurretLevel, playerChassisLevel,
                    index < expertCount);
                index++;
            }

            Log.Info(
                "[BotConfigGenerator] Generated {0} bots, tier={1}, experts={2}, battlesOnEquip={3}, poorStreak={4}",
                botCount, tier.Name, expertCount, battlesOnCurrentEquip, poorFinishStreak);

            return botInitDataArray;
        }

        private BotInitData CreateConfig(string name, int turretLevel, int chassisLevel, bool isExpert)
        {
            return new BotInitData
            {
                Name = name,
                TurretLevel = Mathf.Max(1, turretLevel),
                ChassisLevel = Mathf.Max(1, chassisLevel),
                Profile = isExpert ? _behaviorConfig.ExpertProfile : _behaviorConfig.NormalProfile,
            };
        }

        private int GetExpertCount(BotDifficultyConfigSO.DifficultyTier tier)
        {
            return Mathf.Max(0, tier.ExpertCount);
        }

        private string[] PickUniqueNames(int count)
        {
            var allNames = (string[]) _namesList.GetNames().Clone();
            count = Mathf.Min(count, allNames.Length);

            // Shuffle once, then take first N names.
            for (int i = allNames.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (allNames[i], allNames[j]) = (allNames[j], allNames[i]);
            }

            var result = new string[count];
            for (int i = 0; i < count; i++)
                result[i] = allNames[i].Trim().ToUpper();

            return result;
        }
    }
}