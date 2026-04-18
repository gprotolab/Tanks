using System;
using UnityEngine;

namespace Game.Battle
{
    [CreateAssetMenu(fileName = "BotBehaviorConfig", menuName = "Configs/Battle/BotBehaviorConfig")]
    public class BotBehaviorConfigSO : ScriptableObject
    {
        [SerializeField] private BotBehaviorProfile _normalProfile;
        [SerializeField] private BotBehaviorProfile _expertProfile;

        public BotBehaviorProfile NormalProfile => _normalProfile;
        public BotBehaviorProfile ExpertProfile => _expertProfile;

        [Serializable]
        public class BotBehaviorProfile
        {
            [Header("Reaction")] [SerializeField] private float _minReactionDelay = 0.3f;
            [SerializeField] private float _maxReactionDelay = 0.5f;

            [Header("Navigation")] [SerializeField]
            private float _patrolRadius = 12f;

            [SerializeField] private float _destinationReachedThreshold = 1.5f;
            [SerializeField] private float _navMeshSampleRadius = 5f;
            [SerializeField] private float _pathUpdateInterval = 0.3f;

            [Header("Retreat")] [SerializeField] private float _retreatHealthThreshold = 0.2f;
            [SerializeField] private float _safeDistance = 10f;
            [SerializeField] private float _retreatDistance = 10f;

            [Header("Targeting")] [SerializeField] private bool _seeksWallCover;

            [Header("Patrol")] [SerializeField] private float _minIdlePause = 0.5f;
            [SerializeField] private float _maxIdlePause = 2f;

            public float MinReactionDelay => _minReactionDelay;
            public float MaxReactionDelay => _maxReactionDelay;
            public float PatrolRadius => _patrolRadius;
            public float DestinationReachedThreshold => _destinationReachedThreshold;
            public float NavMeshSampleRadius => _navMeshSampleRadius;
            public float PathUpdateInterval => _pathUpdateInterval;
            public float RetreatHealthThreshold => _retreatHealthThreshold;
            public float SafeDistance => _safeDistance;
            public float RetreatDistance => _retreatDistance;
            public bool SeeksWallCover => _seeksWallCover;
            public float MinIdlePause => _minIdlePause;
            public float MaxIdlePause => _maxIdlePause;
        }
    }
}