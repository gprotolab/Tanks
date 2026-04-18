using Game.Core.FSM;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Battle
{
    public class PatrolState : IState
    {
        private readonly BotBrain _bot;

        private float _idleTimer;
        private float _reactionTimer;
        private bool _isIdle;
        private bool _hasDestination;

        public bool HasSpottedTarget { get; private set; }

        public PatrolState(BotBrain bot) => _bot = bot;

        public void OnEnter()
        {
            HasSpottedTarget = false;
            _idleTimer = 0f;
            _reactionTimer = -1f;
            _isIdle = false;
            _hasDestination = false;

            PickNewDestination();
        }

        public void OnTick(float dt)
        {
            if (_bot.AutoAim.HasTarget)
            {
                if (_reactionTimer < 0f)
                    _reactionTimer = Random.Range(_bot.Profile.MinReactionDelay, _bot.Profile.MaxReactionDelay);

                _reactionTimer -= dt;

                if (_reactionTimer <= 0f)
                {
                    HasSpottedTarget = true;
                    return;
                }
            }
            else
            {
                _reactionTimer = -1f;
            }

            if (_isIdle)
            {
                _bot.DesiredMoveDirection = Vector3.zero;
                _idleTimer -= dt;

                if (_idleTimer <= 0f)
                {
                    _isIdle = false;
                    PickNewDestination();
                }

                return;
            }

            if (!_hasDestination || !_bot.Agent.isOnNavMesh)
            {
                PickNewDestination();
                return;
            }

            if (HasReachedDestination())
            {
                _isIdle = true;
                _idleTimer = Random.Range(_bot.Profile.MinIdlePause, _bot.Profile.MaxIdlePause);
                _bot.DesiredMoveDirection = Vector3.zero;
                return;
            }

            var direction = _bot.Agent.steeringTarget - _bot.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
                _bot.DesiredMoveDirection = direction.normalized;
        }

        public void OnExit() => _bot.ResetMovement();

        private void PickNewDestination()
        {
            _hasDestination = false;

            if (!_bot.Agent.isOnNavMesh) return;

            for (int i = 0; i < 5; i++)
            {
                var offset = Random.insideUnitSphere * _bot.Profile.PatrolRadius;
                offset.y = 0f;
                var candidate = _bot.transform.position + offset;

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, _bot.Profile.NavMeshSampleRadius,
                        NavMesh.AllAreas))
                {
                    _bot.Agent.SetDestination(hit.position);
                    _hasDestination = true;
                    return;
                }
            }
        }

        private bool HasReachedDestination()
        {
            if (!_bot.Agent.hasPath) return true;
            return _bot.Agent.remainingDistance <= _bot.Profile.DestinationReachedThreshold;
        }
    }
}