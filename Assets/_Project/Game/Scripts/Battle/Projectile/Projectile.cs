using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Common;
using UnityEngine;

namespace Game.Battle
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        private LayerMask _tankMask;
        private LayerMask _wallMask;

        private int _ownerId;
        private float _damage;
        private float _lifetime;
        private float _timer;

        // Previous frame position used for Linecast to find the exact contact point (tunneling fix).
        private Vector3 _prevPosition;

        // Guard against double ReturnToPool calls from repeated trigger events.
        private bool _returned;

        private Rigidbody _rb;
        private TrailRenderer _trail;

        private DamageService _damageService;
        private ProjectilePool _pool;
        private ProjectileConfigSO _config;

        // Dedicated CTS for the hit-effect delayed return; cancelled on ReturnToPool or re-launch.
        private CancellationTokenSource _effectCts;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _trail = GetComponentInChildren<TrailRenderer>(true);
        }

        public void SetPhysicsLayers()
        {
            _tankMask = Layers.TankMask;
            _wallMask = Layers.WallMask;
        }

        public void Launch(
            int ownerId,
            float damage,
            Vector3 direction,
            float speed,
            float lifetime,
            DamageService damageService,
            ProjectilePool pool,
            ProjectileConfigSO config)
        {
            _ownerId = ownerId;
            _damage = damage;
            _lifetime = lifetime;
            _damageService = damageService;
            _pool = pool;
            _config = config;

            _timer = 0f;
            _returned = false;
            _prevPosition = transform.position;

            // Cancel any lingering effect task from a previous use of this pooled instance.
            _effectCts?.Cancel();

            _rb.isKinematic = false;
            _rb.angularVelocity = Vector3.zero;
            _rb.linearVelocity = direction.normalized * speed;

            gameObject.SetActive(true);
        }

        private void FixedUpdate()
        {
            // Store position before physics moves the rigidbody so we can Linecast
            // from the previous frame position to find the exact contact point.
            _prevPosition = transform.position;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _lifetime)
                ReturnToPool();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_returned) return;

            int layer = other.gameObject.layer;
            int layerBit = 1 << layer;

            if ((layerBit & _tankMask) != 0)
            {
                var target = other.GetComponentInParent<Tank>();

                // Skip invalid target, self-hit, and dead targets.
                if (target == null || target.Id == _ownerId || target.IsDead)
                    return;

                // hitPoint — exact contact point via Linecast (tunneling fix).
                // ProcessHit receives transform.position for the damage popup — intentional.
                Vector3 hitPoint = GetContactPoint(other);

                _damageService.ProcessHit(_ownerId, target, _damage, transform.position);
                SpawnHitEffect(hitPoint);
                ReturnToPool();
            }
            else if ((layerBit & _wallMask) != 0)
            {
                ReturnToPool();
            }
        }

        private Vector3 GetContactPoint(Collider other)
        {
            if (Physics.Linecast(_prevPosition, transform.position, out RaycastHit hit, _tankMask | _wallMask))
                return hit.point;

            // Fallback: use previous position for ClosestPoint — more accurate than current position
            // because the projectile has already penetrated the collider surface.
            return other.ClosestPoint(_prevPosition);
        }

        private void ReturnToPool()
        {
            if (_returned) return;
            _returned = true;

            // Stop any pending effect-return task before deactivating the object.
            _effectCts?.Cancel();

            _rb.isKinematic = true;

            if (_trail != null)
                _trail.Clear();

            gameObject.SetActive(false);
            _pool?.Return(this);
        }

        private void SpawnHitEffect(Vector3 hitPoint)
        {
            if (_config == null || _config.HitEffectPrefab == null) return;

            var effect = _pool.GetHitEffect(_config);
            if (effect == null) return;

            // Place effect on collider surface instead of tank center.
            effect.transform.position = hitPoint;
            effect.SetActive(true);

            ReturnEffectDelayed(effect).Forget();
        }

        private async UniTaskVoid ReturnEffectDelayed(GameObject effect)
        {
            // Each call gets its own fresh token so previous calls are cancelled cleanly.
            _effectCts?.Cancel();
            _effectCts = new CancellationTokenSource();

            try
            {
                await UniTask.Delay(1500, cancellationToken: _effectCts.Token);

                if (effect != null && effect.activeSelf)
                    _pool.ReturnHitEffect(effect);
            }
            catch (OperationCanceledException)
            {
                // Effect return was cancelled (projectile returned to pool or re-launched) — no-op.
            }
        }
    }
}