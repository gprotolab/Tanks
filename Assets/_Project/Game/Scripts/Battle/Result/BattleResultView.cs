using System;
using DG.Tweening;
using ANut.Core.Utils;
using TMPro;
using R3;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace Game.Battle
{
    public class BattleResultView : MonoBehaviour
    {
        private readonly Subject<Unit> _claimClicked = new();
        private readonly Subject<Unit> _multiplyClicked = new();

        public Observable<Unit> ClaimClicked => _claimClicked;
        public Observable<Unit> MultiplyClicked => _multiplyClicked;

        private const float RewardCountDuration = 2.0f;
        private const float ClaimButtonDelay = 2.5f;

        [Header("Ranking table")] [SerializeField]
        private BattleResultRowView _rowPrefab;

        [SerializeField] private Transform _rowsContainer;

        [Header("Reward")] [SerializeField] private TMP_Text _rewardText;

        [Header("Buttons")] [SerializeField] private Button _claimButton;
        [SerializeField] private Button _multiplyButton;
        [SerializeField] private LocalizeStringEvent _multiplyButtonLocalize;
        [SerializeField] private GameObject _rvLoadingIndicator;

        private Tween _rewardTween;

        private void Awake()
        {
            _claimButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(0.5f))
                .Subscribe(_ => _claimClicked.OnNext(Unit.Default))
                .AddTo(this);

            _multiplyButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(0.5f))
                .Subscribe(_ => _multiplyClicked.OnNext(Unit.Default))
                .AddTo(this);
        }

        public void Show(ScoreEntry[] ranking, int playerPlace, long reward)
        {
            for (int i = 0; i < ranking.Length; i++)
            {
                var row = Instantiate(_rowPrefab, _rowsContainer);
                row.Fill(ranking[i]);
                row.PlayShowAnim(i);
            }

            _claimButton.gameObject.SetActive(false);
            SetMultiplyButtonActive(true);
            gameObject.SetActive(true);

            AnimateReward(reward);
        }

        public void SetMultiplier(int multiplier)
        {
            _multiplyButtonLocalize.StringReference.Arguments = new object[] {multiplier};
            _multiplyButtonLocalize.RefreshString();
        }

        public void UpdateReward(long reward)
        {
            _rewardText.text = CostFormatter.Compact(reward);
        }

        public void DisableMultiplyButton()
        {
            SetMultiplyButtonActive(false);
        }

        public void SetMultiplyButtonActive(bool isActive)
        {
            _multiplyButton.interactable = isActive;

            if (_rvLoadingIndicator == null)
                return;

            _rvLoadingIndicator.SetActive(!isActive);
        }

        // Delay claim button to let reward animation finish first.
        private void AnimateReward(long target)
        {
            _rewardTween?.Kill();

            long current = 0L;
            _rewardText.text = CostFormatter.Compact(0);

            _rewardTween = DOTween
                .To(() => current,
                    value =>
                    {
                        current = value;
                        _rewardText.text = CostFormatter.Compact(value);
                    },
                    target,
                    RewardCountDuration)
                .SetEase(Ease.OutCubic)
                .SetTarget(_rewardText)
                .OnComplete(() => _rewardText.text = CostFormatter.Compact(target));

            DOVirtual.DelayedCall(ClaimButtonDelay, () =>
            {
                if (_claimButton != null)
                    _claimButton.gameObject.SetActive(true);
            }).SetTarget(_claimButton);
        }

        private void OnDestroy()
        {
            _rewardTween?.Kill();
            _claimClicked.Dispose();
            _multiplyClicked.Dispose();
        }
    }
}