using Game.Battle;
using ANut.Core.Save;

namespace Game.Tutorial
{
    public class TutorialProgressInitializer
    {
        private readonly TutorialDataService _tutorialData;
        private readonly BattleStatsDataService _battleStats;
        private readonly ISaveService _saveService;

        public TutorialProgressInitializer(
            TutorialDataService tutorialData,
            BattleStatsDataService battleStats,
            ISaveService saveService)
        {
            _tutorialData = tutorialData;
            _battleStats = battleStats;
            _saveService = saveService;
        }

        public void Initialize()
        {
            if (_battleStats.TotalBattles <= 0)
                return;

            if (_tutorialData.IsTutorialDone)
                return;

            _tutorialData.CompleteHome();
            _tutorialData.CompleteBattle();
            _saveService.Save();
        }
    }
}