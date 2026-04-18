using Game.App;
using ANut.Core.Currency;
using ANut.Core.Save;
using ANut.Core;

namespace Game.Battle
{
    public class BattleExitService
    {
        private readonly IAppStateMachine _appFsm;
        private readonly ICurrencyService _currencyService;
        private readonly BattleStatsDataService _battleStatsData;
        private readonly ISaveService _saveService;
        private readonly BotDifficultyConfigSO _difficultyConfig;

        public BattleExitService(
            IAppStateMachine appFsm,
            ICurrencyService currencyService,
            BattleStatsDataService battleStatsData,
            ISaveService saveService,
            BotDifficultyConfigSO difficultyConfig)
        {
            _appFsm = appFsm;
            _currencyService = currencyService;
            _battleStatsData = battleStatsData;
            _saveService = saveService;
            _difficultyConfig = difficultyConfig;
        }

        public void ExitToHome(long reward, int playerPlace)
        {
            ApplyReward(reward, playerPlace);

            // playerPlace == 0 means early exit, so do not write battle stats.
            if (playerPlace > 0)
            {
                _battleStatsData.RecordBattleResult(
                    place: playerPlace,
                    isWin: playerPlace == 1,
                    poorFinishThreshold: _difficultyConfig.PoorFinishPlaceThreshold);
            }

            _appFsm.EnterHome();
        }

        private void ApplyReward(long reward, int playerPlace)
        {
            if (reward > 0)
                _currencyService.Add(CurrencyType.Coins, reward, "battle_reward");

            _saveService.Save();

            Log.Info("[BattleExitService] reward={0}, place={1}, totalBattles={2}, totalWins={3}",
                reward, playerPlace, _battleStatsData.TotalBattles, _battleStatsData.TotalWins);
        }
    }
}