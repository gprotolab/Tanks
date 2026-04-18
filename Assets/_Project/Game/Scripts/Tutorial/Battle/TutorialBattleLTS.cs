using MessagePipe;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Battle
{
    public class TutorialBattleLTS : LifetimeScope
    {
        [Header("Configs")] [SerializeField] private BattleScopeConfigSO _scopeConfig;

        [Header("Scene")] [SerializeField] private DynamicObjectsRoot _dynamicObjectsRoot;
        [SerializeField] private TutorialBattleView _tutorialBattleView;
        [SerializeField] private DamagePopupSpawner _damagePopupSpawner;
        [SerializeField] private CinemachineCamera _virtualCamera;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_scopeConfig.BattleConfig);
            builder.RegisterInstance(_scopeConfig.BattlePartsCatalog);
            builder.RegisterInstance(_scopeConfig.BotDifficultyConfig);
            builder.RegisterInstance(_scopeConfig.BotBehaviorConfig);
            builder.RegisterInstance(_scopeConfig.BotNamesList);

            // Use local signals so tutorial systems stay loosely coupled.
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<TankSpawnedSignal>(options);
            builder.RegisterMessageBroker<TankDamagedSignal>(options);
            builder.RegisterMessageBroker<TankDiedSignal>(options);

            builder.RegisterComponent(_dynamicObjectsRoot);
            builder.RegisterComponent(_tutorialBattleView);
            builder.RegisterComponent(_damagePopupSpawner);
            builder.RegisterComponent(_virtualCamera);

            builder.Register<TankRegistry>(Lifetime.Scoped);
            builder.Register<ProjectilePool>(Lifetime.Scoped);
            builder.Register<DamageService>(Lifetime.Scoped);
            builder.Register<TankFactory>(Lifetime.Scoped);

            builder.RegisterEntryPoint<TutorialBattleController>();
        }
    }
}