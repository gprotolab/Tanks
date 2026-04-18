using Cysharp.Threading.Tasks;
using System.Threading;
using ANut.Core.LoadingScreen;
using ANut.Core.SceneManagement;

namespace Game.App
{
    public class HomeState : IAppState
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly ScenesConfigSO _scenesConfig;
        private readonly ILoadingScreen _loadingScreen;
        private readonly ITransitionScreen _transitionScreen;

        public HomeState(
            ISceneLoader sceneLoader,
            ScenesConfigSO scenesConfig,
            ILoadingScreen loadingScreen,
            ITransitionScreen transitionScreen)
        {
            _sceneLoader = sceneLoader;
            _scenesConfig = scenesConfig;
            _loadingScreen = loadingScreen;
            _transitionScreen = transitionScreen;
        }

        public async UniTask Enter(CancellationToken ct)
        {
            await _sceneLoader.LoadSceneAdditiveAsync(_scenesConfig.HomeScene, ct);

            if (_loadingScreen.IsVisible)
                await _loadingScreen.HideAsync(ct);
            else if (_transitionScreen.IsVisible)
                await _transitionScreen.HideAsync(ct);
        }

        public async UniTask Exit(CancellationToken ct)
        {
            await _transitionScreen.ShowAsync(ct);
            await _sceneLoader.UnloadSceneAsync(_scenesConfig.HomeScene, ct);
        }
    }
}