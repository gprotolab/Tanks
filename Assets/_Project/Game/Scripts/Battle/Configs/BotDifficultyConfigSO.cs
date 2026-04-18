using System;
using UnityEngine;


namespace Game.Battle
{
    [CreateAssetMenu(fileName = "BotDifficultyConfig", menuName = "Configs/Battle/BotDifficultyConfig")]
    public class BotDifficultyConfigSO : ScriptableObject
    {
        [SerializeField] private DifficultyTier[] _tiers;

        [Header("DDA Settings")] [SerializeField, Range(1, 8)]
        private int _poorFinishPlaceThreshold = 4;

        [SerializeField, Range(1, 10)] private int _poorFinishesPerTierDown = 3;

        public DifficultyTier[] Tiers => _tiers;

        // Poor results can lower difficulty over time.
        public int PoorFinishPlaceThreshold => _poorFinishPlaceThreshold;

        public int PoorFinishesPerTierDown => _poorFinishesPerTierDown;

        [Serializable]
        public class DifficultyTier
        {
            public string Name = "Easy";
            public int MinBattles = 0;

            [Header("Behavior composition")] public int ExpertCount = 0;

            [Header("Bot distribution (total 7)")] public int WeakCount = 4;
            public int MediumCount = 2;
            public int StrongCount = 1;

            [Header("Level offset from player (negative = weaker)")]
            public int WeakTurretOffset = -2;

            public int WeakChassisOffset = -2;
            public int MediumTurretOffset = -1;
            public int MediumChassisOffset = -1;
            public int StrongTurretOffset = 0;
            public int StrongChassisOffset = 0;
        }
    }
}