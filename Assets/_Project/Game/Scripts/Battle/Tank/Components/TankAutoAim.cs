using System;
using R3;
using UnityEngine;
using ANut.Core;
using Game.Common;

namespace Game.Battle
{
    public class TankAutoAim : MonoBehaviour
    {
        [SerializeField] private Tank _owner;

        [SerializeField] private TankWeapon _weapon;

        [SerializeField] private TankMovement _movement;

        // Pivot must be a child transform, not the tank root.
        [SerializeField] private Transform _turretPivot;

        [Header("Turret")] [SerializeField, Range(1f, 10f)]
        private float _aimAngleThreshold = 5f;

        [SerializeField, Range(60f, 720f)] private float _turretRotationSpeed = 540f;

        [Header("Target Search")] [SerializeField, Range(0.05f, 0.5f)]
        private float _targetSearchInterval = 0.15f;

        public Transform TurretPivot => _turretPivot;

        private float _aimRadius;
        private Tank _currentTarget;
        private float _searchTimer;

        private TankRegistry _registry;

        public bool HasTarget => _currentTarget != null;

        public Tank CurrentTarget => _currentTarget;

        public Observable<Tank> OnTargetChanged => _onTargetChanged;

        private readonly Subject<Tank> _onTargetChanged = new();

        public void Init(float aimRadius, TankRegistry registry)
        {
            _aimRadius = aimRadius;
            _registry = registry;

            if (_turretPivot == null)
                _turretPivot = ResolveTurretPivot(_owner.transform, _turretPivot);

            _searchTimer = 0f;
        }

        private Transform ResolveTurretPivot(Transform tankRoot, Transform turretSlot)
        {
            if (turretSlot != null)
            {
                var slotParent = turretSlot.parent;

                if (slotParent != null && slotParent != tankRoot)
                    return slotParent;

                return turretSlot;
            }

            if (transform == tankRoot)
                Log.Error("TurretPivot coincides with tank root! " +
                          "The turret will rotate the entire hull. Assign TurretPivot in the inspector.");

            return transform;
        }

        private void Update()
        {
            if (_registry == null || _owner == null) return;

            UpdateTarget();

            if (_currentTarget == null)
            {
                RotateTurretTowardsMovement();
                return;
            }

            RotateTurretTowardsTarget();
            TryFire();
        }

        private void UpdateTarget()
        {
            _searchTimer -= Time.deltaTime;
            if (_searchTimer > 0f) return;

            _searchTimer = _targetSearchInterval;

            var candidate = _registry.FindClosestEnemy(TurretPivot.position, _aimRadius, _owner);

            // Ignore target when line of sight is blocked.
            if (candidate != null && !HasLineOfSight(candidate))
                candidate = null;

            SetTarget(candidate);
        }

        private bool HasLineOfSight(Tank target)
        {
            var origin = TurretPivot.position;
            var targetPos = target.transform.position;
            targetPos.y = origin.y;
            var dir = targetPos - origin;
            return !Physics.Raycast(origin, dir.normalized, dir.magnitude, Layers.WallMask);
        }

        private void SetTarget(Tank newTarget)
        {
            if (newTarget == _currentTarget) return;

            _currentTarget = newTarget;
            _onTargetChanged.OnNext(_currentTarget);
        }

        private void RotateTurretTowardsMovement()
        {
            var bodyForward = _owner.transform.forward;
            var targetRotation = Quaternion.LookRotation(bodyForward, Vector3.up);
            TurretPivot.rotation = Quaternion.RotateTowards(
                TurretPivot.rotation,
                targetRotation,
                _turretRotationSpeed * Time.deltaTime);
        }

        private void RotateTurretTowardsTarget()
        {
            var dir = _currentTarget.transform.position - TurretPivot.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.001f) return;

            var targetRotation = Quaternion.LookRotation(dir, Vector3.up);
            TurretPivot.rotation = Quaternion.RotateTowards(
                TurretPivot.rotation,
                targetRotation,
                _turretRotationSpeed * Time.deltaTime);
        }

        private void TryFire()
        {
            if (_weapon == null || _movement == null) return;
            if (_movement.IsMoving) return;

            var dir = _currentTarget.transform.position - TurretPivot.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.001f) return;

            float angle = Vector3.Angle(TurretPivot.forward, dir);
            if (angle < _aimAngleThreshold)
                _weapon.Fire(_owner.Id);
        }

        private void OnDestroy()
        {
            _onTargetChanged.Dispose();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, _aimRadius);
        }
#endif
    }
}