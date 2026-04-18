using Cysharp.Threading.Tasks;
using ANut.Core.Analytics;
using R3;
using System.Threading;
using UnityEngine;

namespace Game.Battle
{
    // Distinguishes timer expiry from manual exit.
    public enum BattleActiveResult
    {
        Completed,

        HomeExit
    }

    public class BattleActivePhase
    {
        private readonly BattleSession _session;
        private readonly TankRegistry _tankRegistry;
        private readonly RespawnService _respawnService;
        private readonly BattleHUDView _hudView;
        private readonly BattleJoystickView _joystickView;
        private readonly MiniLeaderboardView _miniLeaderboard;
        private readonly IAnalyticsService _analytics;
        private readonly float _battleDuration;

        public BattleActivePhase(
            BattleSession session,
            TankRegistry tankRegistry,
            RespawnService respawnService,
            BattleHUDView hudView,
            BattleJoystickView joystickView,
            MiniLeaderboardView miniLeaderboard,
            BattleConfigSO config,
            IAnalyticsService analytics)
        {
            _session = session;
            _tankRegistry = tankRegistry;
            _respawnService = respawnService;
            _hudView = hudView;
            _joystickView = joystickView;
            _miniLeaderboard = miniLeaderboard;
            _analytics = analytics;
            _battleDuration = config.BattleDuration;
        }

        public async UniTask<BattleActiveResult> ExecuteAsync(CancellationToken ct)
        {
            _analytics.LogEvent(AnalyticsEvents.BattleStart);

            var disposables = new CompositeDisposable();
            _session.RemainingTime = _battleDuration;
            _tankRegistry.UnfreezeAll();
            _joystickView.Enable();
            _hudView.Show();
            _miniLeaderboard.Show();

            using var homeCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var homeExitRequested = false;

            _hudView.HomeClicked
                .Subscribe(_ =>
                {
                    homeExitRequested = true;
                    _miniLeaderboard.Hide();
                    homeCts.Cancel();
                })
                .AddTo(disposables);

            try
            {
                while (_session.RemainingTime > 0f)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, homeCts.Token);
                    float dt = Time.deltaTime;
                    _session.RemainingTime -= dt;
                    _hudView.UpdateTimer(_session.RemainingTime);
                    _respawnService.Tick(dt);
                }
            }
            catch (System.OperationCanceledException) when (homeExitRequested)
            {
                // Home button pressed — not an error, just an intentional early exit.
                Cleanup();
                return BattleActiveResult.HomeExit;
            }
            finally
            {
                disposables.Dispose();
            }

            // Any other OperationCanceledException (scope disposed via outer ct) propagates naturally.

            // Natural timer expiry.
            Cleanup();
            return BattleActiveResult.Completed;
        }

        // Private 

        private void Cleanup()
        {
            _tankRegistry.FreezeAll();
            _joystickView.Disable();
            _miniLeaderboard.Hide();
        }
    }
}