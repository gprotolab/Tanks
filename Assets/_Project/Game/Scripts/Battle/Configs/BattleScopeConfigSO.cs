using UnityEngine;

namespace Game.Battle
{
    [CreateAssetMenu(fileName = "BattleScopeConfig", menuName = "Configs/Battle/BattleScopeConfig")]
    public class BattleScopeConfigSO : ScriptableObject
    {
        [SerializeField] private BattleConfigSO _battleConfig;
        [SerializeField] private ArenaCatalogSO _arenaCatalog;
        [SerializeField] private TankPartStatsCatalogSO _battlePartsCatalog;
        [SerializeField] private BotDifficultyConfigSO _botDifficultyConfig;
        [SerializeField] private BotBehaviorConfigSO _botBehaviorConfig;
        [SerializeField] private BotNamesConfigSO _botNamesList;

        public BattleConfigSO BattleConfig => _battleConfig;
        public ArenaCatalogSO ArenaCatalog => _arenaCatalog;
        public TankPartStatsCatalogSO BattlePartsCatalog => _battlePartsCatalog;
        public BotDifficultyConfigSO BotDifficultyConfig => _botDifficultyConfig;
        public BotBehaviorConfigSO BotBehaviorConfig => _botBehaviorConfig;
        public BotNamesConfigSO BotNamesList => _botNamesList;
    }
}