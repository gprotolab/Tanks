using UnityEngine;

namespace Game.Battle
{
    public class ArenaSpawnPoint : MonoBehaviour
    {
        [SerializeField] private SpawnMode _mode;
        [SerializeField] private TeamSide _side;

        public SpawnMode Mode => _mode;
        public TeamSide Side => _side;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = GetGizmoColor();
            Gizmos.DrawSphere(transform.position, 0.4f);

            // Show spawn facing direction in the scene view.
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.8f);
            Gizmos.DrawSphere(transform.position + transform.forward * 0.8f, 0.1f);
        }

        private void OnDrawGizmosSelected()
        {
            // Make the selected spawn point easier to spot.
            Gizmos.color = GetGizmoColor();
            Gizmos.DrawWireSphere(transform.position, 1f);

            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.8f,
                $"{_mode} {(_mode == SpawnMode.Team ? _side.ToString() : "")}");
        }

        private Color GetGizmoColor() => _mode switch
        {
            SpawnMode.FFA => Color.yellow,
            SpawnMode.Team when _side == TeamSide.A => Color.blue,
            SpawnMode.Team when _side == TeamSide.B => Color.red,
            _ => Color.white
        };
#endif
    }
}