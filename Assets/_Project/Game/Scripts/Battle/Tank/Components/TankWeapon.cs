using DG.Tweening;
using R3;
using UnityEngine;
using System;
using ANut.Core;

namespace Game.Battle
{
    public class TankWeapon : MonoBehaviour
    {
        private const float RecoilAngle = 6f; // backward kick in degrees
        private const float RecoilDuration = 0.2f; // punch animation length in seconds

        private float _damage;
        private float _fireRate; // shots per second
        private int _ammoCapacity; // clip size
        private float _reloadRatePerSecond; // ammo units restored per second = ammoCapacity / magazineReloadTime

        private float _currentAmmo; // float for smooth reload and normalized event
        private float _fireCooldown; // minimum interval between shots

        private Transform _firePoint;

        public Transform FirePoint
        {
            get => _firePoint;
            set => _firePoint = value;
        }

        // Recoil is applied to a child transform to avoid fighting turret aim rotation.
        private Transform _recoilTarget;

        private ProjectileConfigSO _projectileConfig;
        private ProjectilePool _projectilePool;
        private DamageService _damageService;

        public bool CanFire => _currentAmmo >= 1f && _fireCooldown <= 0f;

        public ReadOnlyReactiveProperty<float> NormalizedAmmo => _normalizedAmmo;

        public ReadOnlyReactiveProperty<int> CurrentAmmoInt => _currentAmmoInt;

        public int AmmoCapacity => _ammoCapacity;

        private readonly ReactiveProperty<float> _normalizedAmmo = new(1f);
        private readonly ReactiveProperty<int> _currentAmmoInt = new(0);
        private readonly Subject<Unit> _fired = new();

        public Observable<Unit> Fired => _fired;

        public void Init(
            TankPartStatsCatalogSO.TurretBattleStats stats,
            ProjectilePool projectilePool,
            DamageService damageService)
        {
            _damage = stats.Damage;
            _fireRate = Mathf.Max(stats.FireRate, 0.01f);
            _ammoCapacity = stats.AmmoCapacity;
            // ammo units restored per second = capacity / total reload time
            _reloadRatePerSecond = stats.AmmoCapacity / Mathf.Max(stats.MagazineReloadTime, 0.01f);
            _projectileConfig = stats.ProjectileConfig;
            _projectilePool = projectilePool;
            _damageService = damageService;

            _currentAmmo = stats.AmmoCapacity;
            _fireCooldown = 0f;

            _normalizedAmmo.Value = 1f;
            _currentAmmoInt.Value = stats.AmmoCapacity;

            _fired.AddTo(this);
            _normalizedAmmo.AddTo(this);
            _currentAmmoInt.AddTo(this);
        }

        public void SetRecoilTarget(Transform recoilTarget) => _recoilTarget = recoilTarget;

        private void Update()
        {
            float dt = Time.deltaTime;

            // Reload continuously over time instead of per-shot steps.
            if (_currentAmmo < _ammoCapacity)
            {
                _currentAmmo = Mathf.Min(_ammoCapacity, _currentAmmo + dt * _reloadRatePerSecond);
                _normalizedAmmo.Value = _currentAmmo / _ammoCapacity;
                _currentAmmoInt.Value = Mathf.FloorToInt(_currentAmmo);
            }

            if (_fireCooldown > 0f)
                _fireCooldown -= dt;
        }

        public void Fire(int ownerId)
        {
            if (!CanFire) return;
            if (_firePoint == null)
            {
                Log.Warning("FirePoint not assigned — shot skipped.");
                return;
            }

            if (_projectilePool == null || _projectileConfig == null)
            {
                Log.Warning("ProjectilePool or ProjectileConfig not initialized.");
                return;
            }

            _currentAmmo -= 1f;
            _fireCooldown = 1f / _fireRate;
            _normalizedAmmo.Value = _currentAmmo / _ammoCapacity;
            _currentAmmoInt.Value = Mathf.FloorToInt(_currentAmmo);

            PlayRecoil();

            var projectile = _projectilePool.Get(_projectileConfig);
            if (projectile == null) return;

            projectile.transform.position = _firePoint.position;
            projectile.transform.rotation = _firePoint.rotation;

            projectile.Launch(
                ownerId,
                _damage,
                _firePoint.forward,
                _projectileConfig.Speed,
                _projectileConfig.Lifetime,
                _damageService,
                _projectilePool,
                _projectileConfig);

            _fired.OnNext(Unit.Default);
        }

        private void PlayRecoil()
        {
            if (_recoilTarget == null) return;

            _recoilTarget.DOKill();
            _recoilTarget.localRotation = Quaternion.identity; // Reset base pose before punch tween.
            _recoilTarget.DOPunchRotation(
                punch: new Vector3(-RecoilAngle, 0f, 0f),
                duration: RecoilDuration,
                vibrato: 1,
                elasticity: 0.3f);
        }

        public void ReloadFull()
        {
            _currentAmmo = _ammoCapacity;
            _fireCooldown = 0f;
            _normalizedAmmo.Value = 1f;
            _currentAmmoInt.Value = _ammoCapacity;
        }
    }
}