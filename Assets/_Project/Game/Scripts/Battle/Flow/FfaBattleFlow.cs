using Cysharp.Threading.Tasks;
using System.Threading;
using ANut.Core;

namespace Game.Battle
{
    public class FfaBattleFlow : IBattleFlow
    {
        private readonly BattleInitPhase _initPhase;
        private readonly BattleCountdownPhase _countdownPhase;
        private readonly BattleActivePhase _activePhase;
        private readonly BattleEndPhase _endPhase;
        private readonly BattleResultPhase _resultPhase;
        private readonly BattleExitService _exitService;

        public FfaBattleFlow(
            BattleInitPhase initPhase,
            BattleCountdownPhase countdownPhase,
            BattleActivePhase activePhase,
            BattleEndPhase endPhase,
            BattleResultPhase resultPhase,
            BattleExitService exitService)
        {
            _initPhase = initPhase;
            _countdownPhase = countdownPhase;
            _activePhase = activePhase;
            _endPhase = endPhase;
            _resultPhase = resultPhase;
            _exitService = exitService;
        }

        public async UniTask RunAsync(CancellationToken ct)
        {
            await _initPhase.ExecuteAsync(ct);
            await _countdownPhase.ExecuteAsync(ct);

            var activeResult = await _activePhase.ExecuteAsync(ct);

            if (activeResult == BattleActiveResult.HomeExit)
            {
                Log.Info("[FfaBattleFlow] Early exit via Home button.");
                _exitService.ExitToHome(reward: 0, playerPlace: 0);
                return;
            }

            // Natural timer expiry — show end/result screens.
            await _endPhase.ExecuteAsync(ct);

            var result = await _resultPhase.ExecuteAsync(ct);
            _exitService.ExitToHome(result.Reward, result.PlayerPlace);
        }
    }
}