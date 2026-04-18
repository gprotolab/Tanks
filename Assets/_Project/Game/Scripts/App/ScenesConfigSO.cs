using UnityEngine;

namespace Game.App
{
    [CreateAssetMenu(fileName = "ScenesConfig", menuName = "Configs/Scenes")]
    public class ScenesConfigSO : ScriptableObject
    {
        [Header("Core Scenes")] [SerializeField]
        private string _bootScene = "BootScene";

        [SerializeField] private string _homeScene = "HomeScene";
        [SerializeField] private string _battleScene = "BattleScene";
        [SerializeField] private string _tutorialBattleScene = "TutorialBattleScene";

        public string BootScene => _bootScene;
        public string HomeScene => _homeScene;
        public string BattleScene => _battleScene;
        public string TutorialBattleScene => _tutorialBattleScene;
    }
}