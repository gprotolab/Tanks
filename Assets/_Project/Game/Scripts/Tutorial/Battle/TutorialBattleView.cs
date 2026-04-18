using Game.Tutorial;
using UnityEngine;

namespace Game.Battle
{
    public class TutorialBattleView : MonoBehaviour
    {
        [SerializeField] private Transform _spawnPointPlayer;
        [SerializeField] private Transform _spawnPointBot1;
        [SerializeField] private Transform _spawnPointBot2;
        [SerializeField] private GameObject _stopToShootHint;
        [SerializeField] private TutorialFinishTrigger _finishTrigger;
        [SerializeField] private BattleJoystickView _joystickView;

        public Transform SpawnPointPlayer => _spawnPointPlayer;
        public Transform SpawnPointBot1 => _spawnPointBot1;
        public Transform SpawnPointBot2 => _spawnPointBot2;
        public GameObject StopToShootHint => _stopToShootHint;
        public TutorialFinishTrigger FinishTrigger => _finishTrigger;
        public BattleJoystickView JoystickView => _joystickView;
    }
}