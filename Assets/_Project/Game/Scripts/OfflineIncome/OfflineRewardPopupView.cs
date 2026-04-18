using System;
using DG.Tweening;
using Game.Common;
using ANut.Core.Utils;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace Game.Offline
{
    public class OfflineRewardPopupView : MonoBehaviour
    {
        public Observable<Unit> CollectClicked => _collectClicked;
        public Observable<Unit> MultiplyClicked => _multiplyClicked;

        private readonly Subject<Unit> _collectClicked = new();
        private readonly Subject<Unit> _multiplyClicked = new();

        private const float AmountAnimationDuration = 3f;

        [SerializeField] private TMP_Text _amountLabel;
        [SerializeField] private Button _collectButton;
        [SerializeField] private Button _multiplyButton;
        [SerializeField] private GameObject _adLoadingIndicator;
        [SerializeField] private LocalizeStringEvent _multiplyButtonLocalize;

        private Tween _amountTween;

        private void Awake()
        {
            _collectButton.OnClickAsObservable()
                .Subscribe(_ => _collectClicked.OnNext(Unit.Default))
                .AddTo(this);

            _multiplyButton.OnClickAsObservable()
                .Subscribe(_ => _multiplyClicked.OnNext(Unit.Default))
                .AddTo(this);
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide()
        {
            _amountTween?.Kill();
            gameObject.SetActive(false);
        }

        public void SetAmount(long amount)
        {
            _amountTween?.Kill();

            long current = 0;
            SetAmountLabel(current);

            _amountTween = DOTween
                .To(() => current,
                    value =>
                    {
                        current = value;
                        SetAmountLabel(value);
                    },
                    amount,
                    AmountAnimationDuration)
                .SetEase(Ease.OutCubic)
                .SetTarget(_amountLabel)
                .OnComplete(() => SetAmountLabel(amount));
        }

        public void SetMultiplyButtonActive(bool active)
        {
            _multiplyButton.interactable = active;
            _adLoadingIndicator.SetActive(!active);
        }

        private void SetAmountLabel(long amount)
            => _amountLabel.text = $"+{CostFormatter.Detailed(amount)} {FontSprites.Currency.Coin}";

        public void SetRvMultiplicator(int mult) =>
            _multiplyButtonLocalize.StringReference.Arguments = new object[] {mult};

        private void OnDestroy()
        {
            _amountTween?.Kill();
            _collectClicked.Dispose();
            _multiplyClicked.Dispose();
        }
    }
}