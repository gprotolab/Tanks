using MessagePipe;
using UnityEngine;

namespace Game.Battle
{
    public class DamageService
    {
        private readonly IPublisher<TankDamagedSignal> _damagedPublisher;
        private readonly IPublisher<TankDiedSignal> _diedPublisher;

        public DamageService(
            IPublisher<TankDamagedSignal> damagedPublisher,
            IPublisher<TankDiedSignal> diedPublisher)
        {
            _damagedPublisher = damagedPublisher;
            _diedPublisher = diedPublisher;
        }

        public void ProcessHit(int attackerId, Tank target, float damage, Vector3 hitPoint)
        {
            if (target == null || target.IsDead) return;

            target.TakeDamage(damage);
            _damagedPublisher.Publish(new TankDamagedSignal(attackerId, target.Id, damage, hitPoint, target.IsPlayer));

            if (target.IsDead)
            {
                target.OnDeath();
                _diedPublisher.Publish(new TankDiedSignal(target.Id, attackerId, target.transform.position));
            }
        }
    }
}