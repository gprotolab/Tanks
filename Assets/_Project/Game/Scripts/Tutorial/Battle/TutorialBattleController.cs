using Cysharp.Threading.Tasks;
using Game.App;
using ANut.Core.Analytics;
using ANut.Core.Save;
using Game.Equipment;
using Game.Tutorial;
using MessagePipe;
using System;
using System.Threading;
using R3;
using Unity.Cinemachine;
using VContainer.Unity;
using UnityEngine;
using ANut.Core;

namespace Game.Battle
{
    public class TutorialBattleController : IAsyncStartable, IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly TankFactory _tankFactory;
        private readonly EquipmentDataService _equipmentData;
        private readonly ISubscriber<TankDiedSignal> _tankDied;
        private readonly TutorialBattleView _view;
        private readonly TutorialDataService _tutorialData;
        private readonly ISaveService _saveService;
        private readonly IAppStateMachine _appState;
        private readonly IAnalyticsService _analytics;
        private readonly CinemachineCamera _virtualCamera;
        private readonly BotBehaviorConfigSO _behaviorConfig;

        public TutorialBattleController(
            TankFactory tankFactory,
            EquipmentDataService equipmentData,
            ISubscriber<TankDiedSignal> tankDied,
            TutorialBattleView view,
            TutorialDataService tutorialData,
            ISaveService saveService,
            IAppStateMachine appState,
            IAnalyticsService analytics,
            CinemachineCamera virtualCamera,
            BotBehaviorConfigSO behaviorConfig)
        {
            _tankFactory = tankFactory;
            _equipmentData = equipmentData;
            _tankDied = tankDied;
            _view = view;
            _tutorialData = tutorialData;
            _saveService = saveService;
            _appState = appState;
            _analytics = analytics;
            _virtualCamera = virtualCamera;
            _behaviorConfig = behaviorConfig;
        }

        public async UniTask StartAsync(CancellationToken ct)
        {
            Log.Info("[TutorialBattleController] Starting tutorial battle.");

            try
            {
                await RunAsync(ct);
            }
            catch (OperationCanceledException)
            {
                Log.Info("[TutorialBattleController] Cancelled (scope disposed).");
            }
        }

        public void Dispose() => _disposables.Dispose();

        private async UniTask RunAsync(CancellationToken ct)
        {
            _analytics.LogEvent(AnalyticsEvents.BattleStart);

            _view.FinishTrigger.gameObject.SetActive(false);

            // Spawn player
            var playerInput = new PlayerTankInput(_view.JoystickView);
            var playerTank = _tankFactory.CreatePlayerTank(
                _equipmentData.TurretLevel,
                _equipmentData.ChassisLevel,
                _view.SpawnPointPlayer,
                playerInput);

            _virtualCamera.Target.TrackingTarget = playerTank.transform;

            _view.JoystickView.Enable();

            // Keep the tutorial running even if the player dies.
            playerTank.OnDied
                .Subscribe(_ => RespawnPlayerAsync(playerTank, _view.SpawnPointPlayer, ct).Forget())
                .AddTo(_disposables);

            var botInitData = new BotInitData
            {
                ChassisLevel = 1,
                TurretLevel = 1,
                Profile = null,
            };

            // First bot stays still so the player can learn basic shooting.
            var bot1 = _tankFactory.CreateBotTank(botInitData, _view.SpawnPointBot1);
            bot1.SetFrozen(true);

            await WaitUntilBotKilledAsync(bot1, ct);
            _analytics.LogEvent(AnalyticsEvents.TutorialStep, "step", "bot1_killed");

            botInitData.Profile = _behaviorConfig.NormalProfile;

            // Second bot fights back to teach a real duel.
            var bot2 = _tankFactory.CreateBotTank(botInitData, _view.SpawnPointBot2);

            await WaitUntilBotKilledAsync(bot2, ct);
            _analytics.LogEvent(AnalyticsEvents.TutorialStep, "step", "bot2_killed");

            // Open the exit only after both tutorial fights are done.
            _view.StopToShootHint.SetActive(false);
            _view.FinishTrigger.gameObject.SetActive(true);

            await WaitForFinishTriggerAsync(_view.FinishTrigger, ct);
            _analytics.LogEvent(AnalyticsEvents.TutorialBattleComplete);

            // Save progress before leaving the tutorial scene.
            _tutorialData.CompleteBattle();
            _saveService.Save();

            _appState.EnterBattle(BattleMode.FFA);
        }

        private async UniTaskVoid RespawnPlayerAsync(Tank player, Transform spawnPoint, CancellationToken ct)
        {
            await UniTask.Delay(2000, cancellationToken: ct);
            player.OnRespawn(spawnPoint.position);
            Log.Info("[TutorialBattleController] Player respawned at spawn point.");
        }

        private async UniTask WaitUntilBotKilledAsync(Tank bot, CancellationToken ct)
        {
            await _tankDied
                .AsObservable()
                .ToObservable()
                .Where(signal => signal.VictimId == bot.Id)
                .FirstAsync(cancellationToken: ct);
        }

        private async UniTask WaitForFinishTriggerAsync(TutorialFinishTrigger finishTrigger, CancellationToken ct)
        {
            await finishTrigger.OnPlayerEntered
                .FirstAsync(cancellationToken: ct);
        }
    }
}