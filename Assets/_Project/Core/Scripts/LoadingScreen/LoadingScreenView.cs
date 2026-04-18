using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace ANut.Core.LoadingScreen
{
    public class LoadingScreenView : MonoBehaviour, ILoadingScreen
    {
        [Header("References")] [SerializeField]
        private CanvasGroup _canvasGroup;

        [SerializeField] private Slider _progressBar;

        [Header("Timing")] [SerializeField, Range(0.1f, 1f)]
        private float _fadeDuration = 0.3f;

        [SerializeField, Range(0f, 3f)] private float _minDisplayTime = 0.8f;

        [Header("Progress")] [SerializeField, Range(0.05f, 1f)]
        private float _progressSpeed = 0.3f; // seconds per tween

        // Tracks when ShowAsync was called so HideAsync can enforce minimum display time
        private float _showTimestamp;
        private Tweener _progressTween;

        // ILoadingScreen
        public bool IsVisible => gameObject.activeSelf && Mathf.Approximately(_canvasGroup.alpha, 1f);

        public void ShowImmediate()
        {
            gameObject.SetActive(true);

            // Kill any running fade/progress tween before forcing values
            DOTween.Kill(_canvasGroup);
            _progressTween?.Kill();

            _canvasGroup.alpha = 1f;
            SetProgressImmediate(0f);

            _showTimestamp = Time.unscaledTime;
        }

        public UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 0f;

            // Reset progress instantly on show (no tween — screen is invisible)
            _progressTween?.Kill();
            SetProgressImmediate(0f);

            _showTimestamp = Time.unscaledTime;

            return FadeAsync(1f, ct);
        }

        public async UniTask HideAsync(CancellationToken ct)
        {
            // Ensure minimum display time before hiding
            float elapsed = Time.unscaledTime - _showTimestamp;
            float remaining = _minDisplayTime - elapsed;

            if (remaining > 0f)
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(remaining),
                    ignoreTimeScale: true,
                    cancellationToken: ct);

            // Smoothly fill to 100% before fading out
            await AnimateProgressAsync(1f, ct);

            await FadeAsync(0f, ct);
            gameObject.SetActive(false);
        }

        public void SetProgress(float progress)
        {
            float target = Mathf.Clamp01(progress);

            _progressTween?.Kill();
            _progressTween = DOTween
                .To(GetDisplayedProgress, SetProgressImmediate, target, _progressSpeed)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
        }

        // Unity lifecycle 

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            // Start hidden — Boot scene shows it only when needed
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _progressTween?.Kill();
        }

        // Private 

        private float GetDisplayedProgress()
            => _progressBar != null ? _progressBar.value : 0f;

        private void SetProgressImmediate(float value)
        {
            if (_progressBar != null)
                _progressBar.value = value;
        }

        private UniTask AnimateProgressAsync(float target, CancellationToken ct)
        {
            float current = GetDisplayedProgress();
            if (Mathf.Approximately(current, target))
                return UniTask.CompletedTask;

            var tcs = new UniTaskCompletionSource();

            _progressTween?.Kill();
            _progressTween = DOTween
                .To(GetDisplayedProgress, SetProgressImmediate, target, _progressSpeed)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .OnComplete(() => tcs.TrySetResult())
                .OnKill(() => tcs.TrySetResult());

            ct.Register(() =>
            {
                _progressTween?.Kill();
                tcs.TrySetCanceled();
            });

            return tcs.Task;
        }

        private UniTask FadeAsync(float targetAlpha, CancellationToken ct)
        {
            var tcs = new UniTaskCompletionSource();

            DOTween
                .To(() => _canvasGroup.alpha, v => _canvasGroup.alpha = v, targetAlpha, _fadeDuration)
                .SetUpdate(true)
                .OnComplete(() => tcs.TrySetResult())
                .OnKill(() => tcs.TrySetResult());

            ct.Register(() =>
            {
                DOTween.Kill(_canvasGroup);
                tcs.TrySetCanceled();
            });

            return tcs.Task;
        }
    }
}