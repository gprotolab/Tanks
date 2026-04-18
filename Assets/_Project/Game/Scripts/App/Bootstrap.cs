using Cysharp.Threading.Tasks;
using ANut.Core.Ads;
using ANut.Core.Analytics;
using ANut.Core.LoadingScreen;
using ANut.Core.Save;
using Game.Offline;
using Game.Tutorial;
using System.Threading;
using VContainer.Unity;

namespace Game.App
{
    public class Bootstrap : IAsyncStartable
    {
        private readonly IAppStateMachine _appStateMachine;
        private readonly AppMetadataService _appMetadataService;
        private readonly ISaveService _saveService;
        private readonly ILoadingScreen _loadingScreen;
        private readonly IAnalyticsInitializer _analyticsInitializer;
        private readonly TutorialProgressInitializer _tutorialProgressInitializer;
        private readonly OfflineIncomeService _offlineIncomeService;
        private readonly IRawAdsProvider _rawAdsProvider;

        public Bootstrap(
            IAppStateMachine appStateMachine,
            AppMetadataService appMetadataService,
            ISaveService saveService,
            ILoadingScreen loadingScreen,
            IAnalyticsInitializer analyticsInitializer,
            TutorialProgressInitializer tutorialProgressInitializer,
            OfflineIncomeService offlineIncomeService,
            IRawAdsProvider rawAdsProvider)
        {
            _appStateMachine = appStateMachine;
            _appMetadataService = appMetadataService;
            _saveService = saveService;
            _loadingScreen = loadingScreen;
            _analyticsInitializer = analyticsInitializer;
            _tutorialProgressInitializer = tutorialProgressInitializer;
            _offlineIncomeService = offlineIncomeService;
            _rawAdsProvider = rawAdsProvider;
        }

        public async UniTask StartAsync(CancellationToken ct)
        {
            _loadingScreen.ShowImmediate();

            bool isFirstLaunch = _appMetadataService.IsFirstLaunch;
            if (isFirstLaunch)
                _appMetadataService.RecordFirstLaunch();

            _appMetadataService.IncrementSessionCount();

            _loadingScreen.SetProgress(0.2f);

            await _saveService.LoadAsync(ct);

            _loadingScreen.SetProgress(0.4f);

            _analyticsInitializer.Initialize(dataSendingEnabled: true, isFirstLaunch: isFirstLaunch);

            _loadingScreen.SetProgress(0.7f);


            await _rawAdsProvider.InitializeAsync(ct);
            _tutorialProgressInitializer.Initialize();
            _offlineIncomeService.ProcessOfflineIncome();
            
            _loadingScreen.SetProgress(1f);

            _appStateMachine.EnterHome();
        }
    }
}