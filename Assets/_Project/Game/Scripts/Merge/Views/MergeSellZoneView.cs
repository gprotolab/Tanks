using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Game.Merge
{
    public class MergeSellZoneView : MonoBehaviour
    {
        [SerializeField] private RectTransform _sellZoneRect;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _sellCostText;
        [SerializeField] private Transform _scaleRoot;

        [Header("Animation")] [SerializeField] private float _showDuration = 0.25f;
        [SerializeField] private float _highlightScale = 1.15f;

        private Tweener _showTween;
        private Tweener _highlightTween;

        private void Awake()
        {
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        public void Show(string sellCost)
        {
            gameObject.SetActive(true);
            _sellCostText.text = sellCost;

            _showTween?.Kill();
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _showTween = DOTween
                    .To(() => _canvasGroup.alpha, value => _canvasGroup.alpha = value, 1f, _showDuration)
                    .SetEase(Ease.OutCubic);
            }
        }

        public void Hide()
        {
            _showTween?.Kill();
            _highlightTween?.Kill();

            if (_canvasGroup != null)
            {
                DOTween
                    .To(() => _canvasGroup.alpha, value => _canvasGroup.alpha = value, 0f, _showDuration * 0.5f)
                    .SetEase(Ease.InCubic)
                    .OnComplete(() => gameObject.SetActive(false));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void SetHighlighted(bool highlighted)
        {
            _highlightTween?.Kill();

            if (_scaleRoot != null)
            {
                float targetScale = highlighted ? _highlightScale : 1f;
                _highlightTween = _scaleRoot.DOScale(targetScale, 0.15f).SetEase(Ease.OutCubic);
            }
        }

        private void OnDestroy()
        {
            _showTween?.Kill();
            _highlightTween?.Kill();
        }
    }
}