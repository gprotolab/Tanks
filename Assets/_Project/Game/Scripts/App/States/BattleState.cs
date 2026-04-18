using Cysharp.Threading.Tasks;
using System.Threading;
using ANut.Core.LoadingScreen;
using ANut.Core.SceneManagement;

namespace Game.App
{
    public class BattleState : IAppState
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly ScenesConfigSO _scenesConfig;
        private readonly ITransitionScreen _transitionScreen;

        public BattleState(
            ISceneLoader sceneLoader,
            ScenesConfigSO scenesConfig,
            ITransitionScreen transitionScreen)
        {
            _sceneLoader = sceneLoader;
            _scenesConfig = scenesConfig;
            _transitionScreen = transitionScreen;
        }

        public async UniTask Enter(CancellationToken ct)
        {
            await _sceneLoader.LoadSceneAdditiveAsync(_scenesConfig.BattleScene, ct);
            await _transitionScreen.HideAsync(ct);
        }

        public async UniTask Exit(CancellationToken ct)
        {
            await _transitionScreen.ShowAsync(ct);
            await _sceneLoader.UnloadSceneAsync(_scenesConfig.BattleScene, ct);
        }
    }
}