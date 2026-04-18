using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Battle
{
    public class MiniLeaderboardRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _placeText;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _killsText;
        [SerializeField] private Image _backgroundImage;

        [SerializeField] private Color _playerHighlightColor = new Color(1f, 0.85f, 0.1f, 0.5f);
        [SerializeField] private Color _defaultColor = new Color(1f, 1f, 1f, 0f);

        public void Fill(int place, string displayName, int kills, bool isPlayer)
        {
            _placeText.text = $"{place}";
            _nameText.text = displayName.ToUpper();
            _killsText.text = kills.ToString();

            _nameText.color = isPlayer ? _playerHighlightColor : _defaultColor;
            _killsText.color = isPlayer ? _playerHighlightColor : _defaultColor;
        }
    }
}