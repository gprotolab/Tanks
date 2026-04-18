using R3;
using UnityEngine;

namespace Game.Battle
{
    public class TankHealth : MonoBehaviour
    {
        private float _maxHp;

        public bool IsDead => _currentHp.Value <= 0f;

        public float MaxHp => _maxHp;

        public ReadOnlyReactiveProperty<float> CurrentHp => _currentHp;

        public float CurrentHpValue => _currentHp.Value;

        public Observable<Unit> OnDied => _onDied;

        private readonly ReactiveProperty<float> _currentHp = new(0f);
        private readonly Subject<Unit> _onDied = new();

        public void Init(float maxHp)
        {
            _maxHp = maxHp;
            _currentHp.Value = _maxHp;

            _currentHp.AddTo(this);
            _onDied.AddTo(this);
        }

        public void TakeDamage(float damage)
        {
            if (IsDead) return;

            _currentHp.Value = Mathf.Max(0f, _currentHp.Value - damage);

            if (IsDead)
                _onDied.OnNext(Unit.Default);
        }

        public void RestoreFullHealth()
        {
            _currentHp.Value = _maxHp;
        }
    }
}