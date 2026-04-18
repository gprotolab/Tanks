using UnityEngine;


namespace Game.Battle
{
    [CreateAssetMenu(fileName = "BattleConfig", menuName = "Configs/Battle/BattleConfig")]
    public class BattleConfigSO : ScriptableObject
    {
        [Header("General")] [SerializeField] private float _battleDuration = 60f;
        [SerializeField] private float _respawnDelay = 3f;
        [SerializeField, Min(0)] private int _botCount = 7;
        [SerializeField] private float _aimRadius = 8f;
        [SerializeField] private float _countdownDuration = 3f;
        [SerializeField] private float _timeUpDisplayDuration = 1.5f;

        [Header("Tank")] [SerializeField] private Tank _tankPrefab;
        [SerializeField] private Tank _botTankPrefab;

        [Header("Camera")] [SerializeField] private float _cameraShakeIntensity = 0.5f;

        public float BattleDuration => _battleDuration;
        public float RespawnDelay => _respawnDelay;
        public int BotCount => _botCount;
        public float AimRadius => _aimRadius;
        public float CountdownDuration => _countdownDuration;
        public float TimeUpDisplayDuration => _timeUpDisplayDuration;
        public Tank PlayerTankPrefab => _tankPrefab;
        public Tank BotTankPrefab => _botTankPrefab;
        public float CameraShakeIntensity => _cameraShakeIntensity;
    }
}