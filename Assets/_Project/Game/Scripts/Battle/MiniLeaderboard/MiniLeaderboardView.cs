using UnityEngine;

namespace Game.Battle
{
    public class MiniLeaderboardView : MonoBehaviour
    {
        [Header("Top-3 rows")] [SerializeField]
        private MiniLeaderboardRowView _row1;

        [SerializeField] private MiniLeaderboardRowView _row2;
        [SerializeField] private MiniLeaderboardRowView _row3;

        [Header("Player row (shown only when player is outside top-3)")] [SerializeField]
        private MiniLeaderboardRowView _playerRow;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Refresh(ScoreEntry[] ranking)
        {
            var topRows = new[] {_row1, _row2, _row3};
            bool playerInTop = false;

            for (int i = 0; i < topRows.Length; i++)
            {
                if (topRows[i] == null) continue;

                if (i < ranking.Length)
                {
                    var entry = ranking[i];
                    topRows[i].gameObject.SetActive(true);
                    topRows[i].Fill(entry.Place, entry.Name, entry.Kills, entry.IsPlayer);

                    if (entry.IsPlayer)
                        playerInTop = true;
                }
                else
                {
                    topRows[i].gameObject.SetActive(false);
                }
            }

            if (_playerRow == null) return;

            if (playerInTop)
            {
                _playerRow.gameObject.SetActive(false);
                return;
            }

            bool found = false;
            foreach (var entry in ranking)
            {
                if (!entry.IsPlayer) continue;
                _playerRow.gameObject.SetActive(true);
                _playerRow.Fill(entry.Place, entry.Name, entry.Kills, isPlayer: true);
                found = true;
                break;
            }

            if (!found)
                _playerRow.gameObject.SetActive(false);
        }
    }
}