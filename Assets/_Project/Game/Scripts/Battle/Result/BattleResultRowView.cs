using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Game.Battle
{
    public class BattleResultRowView : MonoBehaviour
    {
        [Header("Text fields")] [SerializeField]
        private TMP_Text _placeText;

        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _killsText;
        [SerializeField] private TMP_Text _deathsText;
        [SerializeField] private TMP_Text _damageText;

        [Header("Place badges (1st / 2nd / 3rd)")] [SerializeField]
        private GameObject _place1Badge;

        [SerializeField] private GameObject _place2Badge;
        [SerializeField] private GameObject _place3Badge;

        [Header("Row backgrounds")] [SerializeField]
        private GameObject _playerBackground;

        [SerializeField] private GameObject _opponentBackground;

        [Header("Animation")] [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField, Range(0.05f, 1f)] private float _fadeDuration = 0.25f;
        [SerializeField, Range(0f, 0.5f)] private float _staggerDelay = 0.08f;

        private void Awake()
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = 0f;
        }

        public void Fill(ScoreEntry entry)
        {
            _placeText.text = entry.Place.ToString();
            _nameText.text = entry.Name.ToUpper();
            _killsText.text = entry.Kills.ToString();
            _deathsText.text = entry.Deaths.ToString();
            _damageText.text = Mathf.RoundToInt(entry.TotalDamage).ToString();

            UpdatePlaceBadges(entry.Place);
            UpdateBackground(entry.IsPlayer);
        }

        // Use row index as delay to create a simple cascade effect.
        public void PlayShowAnim(int index)
        {
            if (_canvasGroup == null) return;

            _canvasGroup
                .DOFade(1f, _fadeDuration)
                .SetDelay(index * _staggerDelay)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);
        }

        private void UpdatePlaceBadges(int place)
        {
            if (_place1Badge != null) _place1Badge.SetActive(place == 1);
            if (_place2Badge != null) _place2Badge.SetActive(place == 2);
            if (_place3Badge != null) _place3Badge.SetActive(place == 3);
        }

        private void UpdateBackground(bool isPlayer)
        {
            if (_playerBackground != null) _playerBackground.SetActive(isPlayer);
            if (_opponentBackground != null) _opponentBackground.SetActive(!isPlayer);
        }
    }
}