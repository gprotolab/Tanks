using System;
using Game.Battle;
using ANut.Core.Ads;
using ANut.Core.Analytics;
using ANut.Core.AssetLoading;
using ANut.Core.Audio;
using ANut.Core.Currency;
using ANut.Core.LoadingScreen;
using ANut.Core.Save;
using ANut.Core.SceneManagement;
using Game.Economy;
using Game.Equipment;
using Game.Idle;
using Game.Merge;
using Game.Offline;
using Game.Tutorial;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ANut.Core;

namespace Game.App
{
    public class ProjectLTS : LifetimeScope
    {
        [Header("Configs")] [SerializeField] private EconomyConfigSO _balanceConfig;
        [SerializeField] private ScenesConfigSO _scenesConfig;
        [SerializeField] private TankPartSkinCatalogSO _tankPartCatalog;
        [SerializeField] private AdsConfigSO _adsConfig;
        [SerializeField] private MockAdsConfigSO _mockAdsConfig;
        [SerializeField] private AudioConfigSO _audioConfig;
        [SerializeField] private AnalyticsConfigSO _analyticsConfig;
        [SerializeField] private AdMobConfigSO _adMobConfig;

        [Header("UI (Boot Scene)")] [SerializeField]
        private LoadingScreenView _loadingScreenView;

        [SerializeField] private TransitionScreenView _transitionScreenView;
        [SerializeField] private AdsDebugHelper _adsDebugHelper;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterConfigs(builder);
            RegisterViews(builder);
            RegisterCoreServices(builder);
            RegisterAudio(builder);
            RegisterAnalytics(builder);
            RegisterAds(builder);
            RegisterInfrastructure(builder);
            RegisterApp(builder);

            builder.RegisterEntryPoint<Bootstrap>();
        }

        private void RegisterConfigs(IContainerBuilder builder)
        {
            // Create a runtime copy of the balance config to keep the original asset unchanged.
            builder.RegisterInstance(_balanceConfig.CreateRuntimeCopy());
            builder.RegisterInstance(_scenesConfig);
            builder.RegisterInstance(_tankPartCatalog);
            builder.RegisterInstance(_adsConfig);
            builder.RegisterInstance(_audioConfig);
            builder.RegisterInstance(_analyticsConfig);
        }

        private void RegisterViews(IContainerBuilder builder)
        {
            builder.RegisterComponentInNewPrefab(_loadingScreenView, Lifetime.Singleton)
                .UnderTransform(transform)
                .As<ILoadingScreen>();

            builder.RegisterComponentInNewPrefab(_transitionScreenView, Lifetime.Singleton)
                .UnderTransform(transform)
                .As<ITransitionScreen>();

            builder.RegisterComponentInNewPrefab(_adsDebugHelper, Lifetime.Singleton)
                .UnderTransform(transform)
                .AsSelf();
        }

        private void RegisterCoreServices(IContainerBuilder builder)
        {
            builder.Register<AppMetadataService>(Lifetime.Singleton)
                .AsSelf();

            builder.Register<CurrencyService>(Lifetime.Singleton)
                .As<ICurrencyService>()
                .As<ISaveModule>()
                .As<IDisposable>();

            builder.Register<EquipmentDataService>(Lifetime.Singleton)
                .AsSelf()
                .As<ISaveModule>()
                .As<IDisposable>();

            builder.Register<BattleStatsDataService>(Lifetime.Singleton)
                .AsSelf()
                .As<ISaveModule>()
                .As<IDisposable>();

            builder.Register<IdleDataService>(Lifetime.Singleton)
                .AsSelf()
                .As<ISaveModule>();

            builder.Register<MergeDataService>(Lifetime.Singleton)
                .AsSelf()
                .As<ISaveModule>()
                .As<IDisposable>();

            builder.Register<OfflineIncomeDataService>(Lifetime.Singleton)
                .AsSelf()
                .As<ISaveModule>();

            builder.Register<TutorialDataService>(Lifetime.Singleton)
                .AsSelf()
                .As<ISaveModule>();

            builder.Register<EconomyService>(Lifetime.Singleton);
            builder.Register<SaveService>(Lifetime.Singleton).As<ISaveService>();
            builder.Register<TutorialProgressInitializer>(Lifetime.Singleton);
            builder.RegisterEntryPoint<OfflineIncomeService>().AsSelf();
            builder.RegisterEntryPoint<AutoSaveService>();
        }

        private void RegisterAudio(IContainerBuilder builder)
        {
            builder.Register<AudioService>(Lifetime.Singleton)
                .As<IAudioService>()
                .As<ISaveModule>()
                .As<IInitializable>()
                .As<IDisposable>();
        }

        private void RegisterAnalytics(IContainerBuilder builder)
        {
            builder.Register<AppMetricaAnalyticsService>(Lifetime.Singleton)
                .As<IAnalyticsService>()
                .As<IAnalyticsInitializer>();
        }

        private void RegisterAds(IContainerBuilder builder)
        {
            RegisterAdsProvider(builder);
            builder.Register<AdsService>(Lifetime.Singleton).As<IAdsService>();
        }

        private void RegisterInfrastructure(IContainerBuilder builder)
        {
            builder.Register<AddressableAssetLoader>(Lifetime.Singleton).As<IAssetLoader>();
            builder.Register<SceneLoader>(Lifetime.Singleton).As<ISceneLoader>();
        }

        private void RegisterApp(IContainerBuilder builder)
        {
            builder.Register<HomeState>(Lifetime.Singleton).As<IAppState>();
            builder.Register<BattleState>(Lifetime.Singleton).As<IAppState>();
            builder.Register<TutorialBattleState>(Lifetime.Singleton).As<IAppState>();
            builder.Register<BattleParams>(Lifetime.Singleton);
            builder.Register<AppStateMachine>(Lifetime.Singleton).As<IAppStateMachine>();
        }

        private void RegisterAdsProvider(IContainerBuilder builder)
        {
            switch (_adsConfig.ProviderMode)
            {
                case AdsProviderMode.Mock:
                    if (_mockAdsConfig == null)
                        Log.Error("[ProjectLifetimeScope] MockAdsConfig is not assigned!");

                    builder.RegisterInstance(_mockAdsConfig);
                    builder.Register<MockRawAdsProvider>(Lifetime.Singleton)
                        .As<IRawAdsProvider>()
                        .As<IDisposable>();
                    break;

                case AdsProviderMode.AdMob:
                    if (_adMobConfig == null)
                        Log.Error("[ProjectLifetimeScope] AdMobConfig is not assigned!");

                    builder.RegisterInstance(_adMobConfig);
                    builder.Register<AdMobRawAdsProvider>(Lifetime.Singleton)
                        .As<IRawAdsProvider>()
                        .As<IDisposable>();
                    break;

                default:
                    Log.Error("[ProjectLifetimeScope] Unknown AdsProviderMode: {0}. Falling back to Mock.",
                        _adsConfig.ProviderMode);
                    builder.RegisterInstance(_mockAdsConfig);
                    builder.Register<MockRawAdsProvider>(Lifetime.Singleton)
                        .As<IRawAdsProvider>()
                        .As<IDisposable>();
                    break;
            }
        }
    }
}