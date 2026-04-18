using Game.Core.FSM;
using UnityEngine;

namespace Game.Battle
{
    public class AttackState : IState
    {
        private readonly BotBrain _bot;

        private Tank _target;

        public bool IsTargetLost { get; private set; }
        public bool IsAutoAimLost { get; private set; }
        public bool ShouldRetreat { get; private set; }

        public AttackState(BotBrain bot) => _bot = bot;

        public void OnEnter()
        {
            _target = _bot.AutoAim.CurrentTarget;
            _bot.ResetMovement();

            IsTargetLost = _target == null;
            IsAutoAimLost = false;
            ShouldRetreat = false;
        }

        public void OnTick(float dt)
        {
            if (_target == null || _target.IsDead)
            {
                IsTargetLost = true;
                return;
            }

            if (!_bot.AutoAim.HasTarget)
            {
                IsAutoAimLost = true;
                return;
            }

            if (_bot.ControlledTank.GetHpRatio() < _bot.Profile.RetreatHealthThreshold)
            {
                ShouldRetreat = true;
                return;
            }

            _bot.DesiredMoveDirection = Vector3.zero;
        }

        public void OnExit() => _bot.ResetMovement();
    }
}