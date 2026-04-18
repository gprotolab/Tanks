using Cysharp.Threading.Tasks;
using System.Threading;
using ANut.Core.LoadingScreen;
using ANut.Core.SceneManagement;

namespace Game.App
{
    public class TutorialBattleState : IAppState
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly ScenesConfigSO _scenesConfig;
        private readonly ITransitionScreen _transitionScreen;
        private readonly ILoadingScreen _loadingScreen;

        public TutorialBattleState(
            ISceneLoader sceneLoader,
            ScenesConfigSO scenesConfig,
            ITransitionScreen transitionScreen,
            ILoadingScreen loadingScreen)
        {
            _sceneLoader = sceneLoader;
            _scenesConfig = scenesConfig;
            _transitionScreen = transitionScreen;
            _loadingScreen = loadingScreen;
        }

        public async UniTask Enter(CancellationToken ct)
        {
            await _sceneLoader.LoadSceneAdditiveAsync(_scenesConfig.TutorialBattleScene, ct);

            if (_loadingScreen.IsVisible)
                await _loadingScreen.HideAsync(ct);

            await _transitionScreen.HideAsync(ct);
        }

        public async UniTask Exit(CancellationToken ct)
        {
            await _transitionScreen.ShowAsync(ct);
            await _sceneLoader.UnloadSceneAsync(_scenesConfig.TutorialBattleScene, ct);
        }
    }
}