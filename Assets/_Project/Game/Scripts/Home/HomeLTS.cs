using System;
using Game.Battle;
using Game.Equipment;
using ANut.Core.Currency;
using Game.Idle;
using Game.Merge;
using Game.Offline;
using Game.Tutorial;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Home
{
    public class HomeLTS : LifetimeScope
    {
        [Header("Home")] [SerializeField] private HomeView _homeView;
        [SerializeField] private CurrencyDisplay _coinsDisplay;
        [SerializeField] private TankPartStatsCatalogSO _battlePartsCatalog;

        [Header("Merge")] [SerializeField] private MergeConfigSO _mergeConfig;
        [SerializeField] private TankPartSkinCatalogSO _catalog;

        [Header("Merge Views")] [SerializeField]
        private MergeGridView _mergeGridView;

        [SerializeField] private MergeTankView _mergeTankView;
        [SerializeField] private MergePurchaseView _mergePurchaseView;
        [SerializeField] private MergeSellZoneView _mergeSellZoneView;
        [SerializeField] private MergeDragInput _mergeDragInput;
        [SerializeField] private MergeDragVisual _mergeDragVisual;

        [Header("Idle Views")] [SerializeField]
        private IdleView _idleView;

        [Header("Tutorial")] [SerializeField] private HomeTutorialView _homeTutorialView;

        [Header("Offline Reward")] [SerializeField]
        private OfflineRewardPopupView _offlineRewardPopupView;

        protected override void Configure(IContainerBuilder builder)
        {
            ConfigureTutorial(builder);
            ConfigureHome(builder);
            ConfigureIdle(builder);
            ConfigureMerge(builder);
        }

        private void ConfigureMerge(IContainerBuilder builder)
        {
            builder.Register<MergeSettings>(resolver =>
            {
                var tutorialData = resolver.Resolve<TutorialDataService>();
                return tutorialData.IsHomeDone
                    ? _mergeConfig.NormalSession
                    : _mergeConfig.TutorialSession;
            }, Lifetime.Scoped);


            builder.RegisterInstance(_catalog);
            builder.RegisterInstance(_mergeGridView);
            builder.RegisterInstance(_mergeTankView);
            builder.RegisterInstance(_mergePurchaseView);
            builder.RegisterInstance(_mergeSellZoneView);
            builder.RegisterInstance(_mergeDragInput);
            builder.RegisterInstance(_mergeDragVisual);

            builder.Register<MergeModel>(Lifetime.Scoped);

            builder.Register<MergeRuleService>(Lifetime.Scoped);
            builder.Register<GridMutationService>(Lifetime.Scoped);
            builder.Register<EquipService>(Lifetime.Scoped);
            builder.Register<PartGeneratorService>(Lifetime.Scoped);
            builder.Register<IMergeEconomyService, MergeEconomyService>(Lifetime.Scoped);

            builder.Register<MergeSessionController>(Lifetime.Scoped);
            builder.Register<MergePurchaseController>(Lifetime.Scoped);
            builder.Register<MergeExpansionController>(Lifetime.Scoped);
            builder.Register<MergeDragController>(Lifetime.Scoped)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.RegisterEntryPoint<MergeInitializer>();
        }

        private void ConfigureTutorial(IContainerBuilder builder)
        {
            builder.RegisterComponent(_homeTutorialView);
            builder.RegisterEntryPoint<HomeTutorialController>();
        }

        private void ConfigureHome(IContainerBuilder builder)
        {
            builder.RegisterInstance(_battlePartsCatalog);
            builder.RegisterComponent(_coinsDisplay);
            builder.RegisterComponent(_homeView);
            builder.RegisterComponentInNewPrefab(_offlineRewardPopupView, Lifetime.Scoped);
            builder.RegisterEntryPoint<HomePresenter>();
            builder.RegisterEntryPoint<OfflineRewardPopupPresenter>();
        }

        private void ConfigureIdle(IContainerBuilder builder)
        {
            builder.RegisterComponent(_idleView);
            builder.RegisterEntryPoint<IdleService>().AsSelf();
            builder.RegisterEntryPoint<IdlePresenter>();
        }
    }
}