using Game.Core.FSM;
using UnityEngine;

namespace Game.Battle
{
    public class ChaseState : IState
    {
        private readonly BotBrain _bot;

        private Tank _target;
        private float _pathUpdateTimer;

        public bool IsTargetLost { get; private set; }
        public bool IsTargetInRange { get; private set; }

        public ChaseState(BotBrain bot) => _bot = bot;

        public void OnEnter()
        {
            _target = _bot.AutoAim.CurrentTarget;
            _pathUpdateTimer = 0f;

            IsTargetLost = _target == null;
            IsTargetInRange = false;

            if (_target != null)
                UpdatePath();
        }

        public void OnTick(float dt)
        {
            if (_target == null || _target.IsDead)
            {
                IsTargetLost = true;
                return;
            }

            if (_bot.AutoAim.HasTarget && _bot.AutoAim.CurrentTarget == _target)
            {
                IsTargetInRange = true;
                return;
            }

            _pathUpdateTimer -= dt;
            if (_pathUpdateTimer <= 0f)
            {
                _pathUpdateTimer = _bot.Profile.PathUpdateInterval;
                UpdatePath();
            }

            if (_bot.Agent.isOnNavMesh && _bot.Agent.hasPath)
            {
                var direction = _bot.Agent.steeringTarget - _bot.transform.position;
                direction.y = 0f;

                if (direction.sqrMagnitude > 0.01f)
                    _bot.DesiredMoveDirection = direction.normalized;
            }
            else
            {
                var direct = _target.transform.position - _bot.transform.position;
                direct.y = 0f;

                if (direct.sqrMagnitude > 0.01f)
                    _bot.DesiredMoveDirection = direct.normalized;
            }
        }

        public void OnExit() => _bot.ResetMovement();

        private void UpdatePath()
        {
            if (_target != null && _bot.Agent.isOnNavMesh)
                _bot.Agent.SetDestination(_target.transform.position);
        }
    }
}