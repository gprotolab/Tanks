using System;
using MessagePipe;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Battle
{
    public class BattleLTS : LifetimeScope
    {
        [Header("Configs")] [SerializeField] private BattleScopeConfigSO _scopeConfig;

        [Header("Scene")] [SerializeField] private DynamicObjectsRoot _dynamicObjectsRoot;

        [Header("Views")] [SerializeField] private BattleJoystickView _joystickView;
        [SerializeField] private BattleHUDView _hudView;
        [SerializeField] private CountdownOverlayView _countdownOverlay;
        [SerializeField] private MiniLeaderboardView _miniLeaderboardView;
        [SerializeField] private BattleResultView _resultView;

        [Header("Camera")] [SerializeField] private CinemachineCamera _virtualCamera;

        [SerializeField] private DamagePopupSpawner _damagePopupSpawner;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterConfigs(builder);
            RegisterSignals(builder);
            RegisterScene(builder);
            RegisterServices(builder);
            RegisterFlow(builder);
            RegisterViews(builder);
            RegisterEntryPoints(builder);
        }

        private void RegisterConfigs(IContainerBuilder builder)
        {
            builder.RegisterInstance(_scopeConfig.BattleConfig);
            builder.RegisterInstance(_scopeConfig.ArenaCatalog);
            builder.RegisterInstance(_scopeConfig.BattlePartsCatalog);
            builder.RegisterInstance(_scopeConfig.BotDifficultyConfig);
            builder.RegisterInstance(_scopeConfig.BotBehaviorConfig);
            builder.RegisterInstance(_scopeConfig.BotNamesList);
        }

        private static void RegisterSignals(IContainerBuilder builder)
        {
            // Signal bus for the Battle scope.
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<TankSpawnedSignal>(options);
            builder.RegisterMessageBroker<TankDamagedSignal>(options);
            builder.RegisterMessageBroker<TankDiedSignal>(options);
        }

        private void RegisterScene(IContainerBuilder builder)
        {
            builder.RegisterComponent(_dynamicObjectsRoot);
        }

        private static void RegisterServices(IContainerBuilder builder)
        {
            builder.Register<ArenaLoader>(Lifetime.Scoped);
            builder.Register<TankRegistry>(Lifetime.Scoped);
            builder.Register<ProjectilePool>(Lifetime.Scoped);
            builder.Register<ScoreService>(Lifetime.Scoped)
                .AsSelf()
                .As<IDisposable>();

            builder.Register<DamageService>(Lifetime.Scoped);
            builder.Register<TankFactory>(Lifetime.Scoped);

            builder.Register<RespawnService>(Lifetime.Scoped)
                .AsSelf()
                .As<IDisposable>();

            builder.Register<BattleDifficultyService>(Lifetime.Scoped);
            builder.Register<BotConfigGenerator>(Lifetime.Scoped);
            builder.Register<BattleExitService>(Lifetime.Scoped);
            builder.Register<BattleSession>(Lifetime.Scoped);
        }

        private static void RegisterFlow(IContainerBuilder builder)
        {
            builder.Register<BattleInitPhase>(Lifetime.Scoped);
            builder.Register<BattleActivePhase>(Lifetime.Scoped);
            builder.Register<BattleCountdownPhase>(Lifetime.Scoped);
            builder.Register<BattleEndPhase>(Lifetime.Scoped);
            builder.Register<BattleResultPhase>(Lifetime.Scoped);

            builder.Register<FfaBattleFlow>(Lifetime.Scoped)
                .As<IBattleFlow>();
        }

        private void RegisterViews(IContainerBuilder builder)
        {
            builder.RegisterComponentInNewPrefab(_joystickView, Lifetime.Scoped);
            builder.RegisterComponentInNewPrefab(_hudView, Lifetime.Scoped);
            builder.RegisterComponentInNewPrefab(_countdownOverlay, Lifetime.Scoped);
            builder.RegisterComponentInNewPrefab(_miniLeaderboardView, Lifetime.Scoped);
            builder.RegisterComponentInNewPrefab(_resultView, Lifetime.Scoped);
            builder.RegisterComponent(_virtualCamera);
            builder.RegisterComponentInNewPrefab(_damagePopupSpawner, Lifetime.Scoped);
        }

        private static void RegisterEntryPoints(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<BattleController>();
            builder.RegisterEntryPoint<MiniLeaderboardPresenter>();
        }
    }
}