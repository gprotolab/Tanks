using Cysharp.Threading.Tasks;
using System.Threading;

namespace Game.Battle
{
    public class BattleEndPhase
    {
        private readonly CountdownOverlayView _overlay;
        private readonly float _displayDuration;

        public BattleEndPhase(
            CountdownOverlayView overlay,
            BattleConfigSO config)
        {
            _overlay = overlay;
            _displayDuration = config.TimeUpDisplayDuration;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            _overlay.Show();
            _overlay.ShowTimeUp();

            await UniTask.Delay(
                System.TimeSpan.FromSeconds(_displayDuration),
                cancellationToken: ct);

            _overlay.Hide();
        }
    }
}