using R3;
using UnityEngine;

namespace Game.Battle
{
    public class PlayerTargetIndicatorController : MonoBehaviour
    {
        [SerializeField] private Tank _owner;
        [SerializeField] private TankAutoAim _autoAim;

        private Tank _indicatedTarget;
        private readonly SerialDisposable _targetDeathSub = new();

        private void Awake()
        {
            _autoAim.OnTargetChanged
                .Subscribe(OnTargetChanged)
                .AddTo(this);

            _owner.OnDied
                .Subscribe(_ => HideCurrentIndicator())
                .AddTo(this);
        }

        private void OnTargetChanged(Tank newTarget)
        {
            HideCurrentIndicator();

            _indicatedTarget = newTarget;

            if (_indicatedTarget == null) return;

            _indicatedTarget.ShowTargetIndicator(true);

            _targetDeathSub.Disposable = _indicatedTarget.OnDied
                .Take(1)
                .Subscribe(_ => HideCurrentIndicator());
        }

        private void HideCurrentIndicator()
        {
            if (_indicatedTarget == null) return;

            _indicatedTarget.ShowTargetIndicator(false);
            _indicatedTarget = null;
            _targetDeathSub.Disposable = Disposable.Empty;
        }

        private void OnDestroy() => _targetDeathSub.Dispose();
    }
}