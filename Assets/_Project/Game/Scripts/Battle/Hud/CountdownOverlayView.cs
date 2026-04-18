using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Battle
{
    public class CountdownOverlayView : MonoBehaviour
    {
        private const float PunchDuration = 0.35f;
        private static readonly Vector3 PunchStrength = Vector3.one * 0.4f;

        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _countdownText;
        [SerializeField] private GameObject _timeOut;

        private Tweener _punchTween;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void UpdateText(string text)
        {
            _countdownText.gameObject.SetActive(true);
            _timeOut.SetActive(false);

            _countdownText.text = text;

            _punchTween?.Kill();
            _countdownText.transform.localScale = Vector3.one;
            _punchTween = _countdownText.transform
                .DOPunchScale(PunchStrength, PunchDuration, vibrato: 1, elasticity: 0.5f)
                .SetLink(gameObject);
        }

        public void ShowTimeUp()
        {
            _timeOut.SetActive(true);
            _countdownText.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _punchTween?.Kill();
        }
    }
}