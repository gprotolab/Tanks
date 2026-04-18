using Cysharp.Threading.Tasks;
using ANut.Core.Audio;
using System.Threading;
using UnityEngine;

namespace Game.Battle
{
    // Shows the 3-2-1 countdown before battle starts.
    public class BattleCountdownPhase
    {
        private const int CountdownSteps = 3;

        private readonly CountdownOverlayView _overlay;
        private readonly MiniLeaderboardView _miniLeaderboard;
        private readonly float _countdownDuration;
        private readonly IAudioService _audioService;

        public BattleCountdownPhase(
            CountdownOverlayView overlay,
            MiniLeaderboardView miniLeaderboard,
            BattleConfigSO config,
            IAudioService audioService)
        {
            _overlay = overlay;
            _miniLeaderboard = miniLeaderboard;
            _countdownDuration = config.CountdownDuration;
            _audioService = audioService;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            _miniLeaderboard.Hide();
            _overlay.Show();

            float timer = _countdownDuration;
            int lastShownStep = -1;

            // Show the first digit immediately without waiting for the first frame
            ShowStep(timer, ref lastShownStep);

            while (timer > 0f)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
                timer -= Time.deltaTime;
                ShowStep(timer, ref lastShownStep);
            }

            _overlay.Hide();
        }

        // Update text only when the visible number changes.
        private void ShowStep(float timer, ref int lastShownStep)
        {
            float stepDuration = _countdownDuration / CountdownSteps;
            int step = Mathf.CeilToInt(timer / stepDuration);

            step = Mathf.Clamp(step, 0, CountdownSteps);
            if (step <= 0 || step == lastShownStep) return;

            lastShownStep = step;
            _audioService.PlaySfx(SoundId.Battle_CountdownTick);
            _overlay.UpdateText(step.ToString());
        }
    }
}