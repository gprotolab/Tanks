using UnityEngine;
using TMPro;
using DG.Tweening;
using Game.Common;
using ANut.Core.Utils;

namespace Game.Idle
{
    public class CoinAnimationItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        private RectTransform _rectTransform;
        private Sequence _sequence;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Play(Vector2 position, long amount, System.Action onComplete)
        {
            _rectTransform.anchoredPosition = position;
            _text.text = $"+{CostFormatter.Compact(amount)}{FontSprites.Currency.Coin}";
            _rectTransform.localScale = Vector3.zero;
            gameObject.SetActive(true);

            _sequence?.Kill();
            _sequence = DOTween.Sequence();
            _sequence.Append(_rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
            _sequence.Append(
                DOTween
                    .To(() => _rectTransform.anchoredPosition.y,
                        value =>
                        {
                            Vector2 anchoredPosition = _rectTransform.anchoredPosition;
                            anchoredPosition.y = value;
                            _rectTransform.anchoredPosition = anchoredPosition;
                        },
                        position.y + 100f,
                        0.8f)
                    .SetEase(Ease.OutQuad));

            _sequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }

        private void OnDestroy()
        {
            _sequence?.Kill();
        }
    }
}