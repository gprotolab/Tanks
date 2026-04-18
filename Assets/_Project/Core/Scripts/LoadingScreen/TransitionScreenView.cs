using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;

namespace ANut.Core.LoadingScreen
{
    public class TransitionScreenView : MonoBehaviour, ITransitionScreen
    {
        [Header("References")] [SerializeField]
        private RectTransform _panel;

        [Header("Timing")] [SerializeField, Range(0.1f, 1f)]
        private float _duration = 0.35f;

        [SerializeField] private Ease _ease = Ease.InOutQuad;

        // anchorMin.y == 0  → panel fully covers the screen
        // anchorMin.y == 1  → bottom edge is at the top → panel fully hidden above screen
        private const float VisibleAnchor = 0f;
        private const float HiddenAnchor = 1f;

        // ITransitionScreen

        public bool IsVisible => gameObject.activeSelf &&
                                 Mathf.Approximately(_panel.anchorMin.y, VisibleAnchor);

        public async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);

            // Wait one frame so the layout system has resolved sizes before we animate.
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, ct);

            SetAnchorMinY(HiddenAnchor);
            await TweenAnchorMinYAsync(VisibleAnchor, ct);
        }

        public async UniTask HideAsync(CancellationToken ct)
        {
            await TweenAnchorMinYAsync(HiddenAnchor, ct);
            gameObject.SetActive(false);
        }

        // Unity lifecycle 

        private void Awake()
        {
            if (_panel == null)
                _panel = GetComponent<RectTransform>();

            gameObject.SetActive(false);
        }

        // Private 

        private void SetAnchorMinY(float y)
        {
            _panel.anchorMin = new Vector2(_panel.anchorMin.x, y);
            _panel.offsetMin = new Vector2(_panel.offsetMin.x, 0f);
        }

        /// Tweens <c>anchorMin.y</c> to <paramref name="targetY"/> and awaits completion.
        private UniTask TweenAnchorMinYAsync(float targetY, CancellationToken ct)
        {
            var tcs = new UniTaskCompletionSource();

            var tween = DOTween
                .To(() => _panel.anchorMin.y, SetAnchorMinY, targetY, _duration)
                .SetEase(_ease)
                .SetUpdate(true)
                .OnComplete(() => tcs.TrySetResult())
                .OnKill(() => tcs.TrySetResult());

            // Unregister when the tween finishes to avoid a dangling ct registration.
            var ctReg = ct.Register(() =>
            {
                tween.Kill();
                tcs.TrySetCanceled();
            });

            return tcs.Task.ContinueWith(ctReg.Dispose);
        }
    }
}