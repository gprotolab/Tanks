using System;
using TMPro;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Battle
{
    public class BattleHUDView : MonoBehaviour
    {
        private readonly Subject<Unit> _homeClicked = new();

        public Observable<Unit> HomeClicked => _homeClicked;

        [Header("References")] [SerializeField]
        private TMP_Text _timerText;

        [SerializeField] private Button _homeButton;

        private void Awake()
        {
            if (_homeButton == null)
                return;

            _homeButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(0.5f))
                .Subscribe(_ => _homeClicked.OnNext(Unit.Default))
                .AddTo(this);

            _homeClicked.AddTo(this);
        }

        public void Show()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }

        public void UpdateTimer(float remainingSeconds)
        {
            if (_timerText == null)
                return;

            float clamped = Mathf.Max(0f, remainingSeconds);
            int minutes = Mathf.FloorToInt(clamped / 60f);
            int seconds = Mathf.FloorToInt(clamped % 60f);
            _timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}