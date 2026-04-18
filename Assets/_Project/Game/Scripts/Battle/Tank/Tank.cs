using System;
using ANut.Core.Audio;
using TMPro;
using R3;
using Unity.Cinemachine;
using UnityEngine;
using ANut.Core;

namespace Game.Battle
{
    public class Tank : MonoBehaviour
    {
        public int Id { get; private set; }
        public string DisplayName { get; private set; }
        public TeamSide TeamId { get; private set; }
        public bool IsPlayer { get; private set; }

        [SerializeField] private TankHealth _health;
        [SerializeField] private TankWeapon _weapon;
        [SerializeField] private TankMovement _movement;
        [SerializeField] private TankAutoAim _autoAim;
        [SerializeField] private TankDamageFlash _damageFlash;

        [SerializeField] private Transform _chassisSlot;
        [SerializeField] private Transform _turretSlot;

        [SerializeField] private TankExplosionEffect _explosionEffect;
        [SerializeField] private CinemachineImpulseSource _impulseSource;
        [SerializeField] private TMP_Text _displayNameText;

        private float _shakeIntensity = 1f;

        // World-space 3D indicator (assigned in the inspector on the prefab) 

        [SerializeField] private GameObject _targetIndicator;

        [SerializeField] private HealthBarView _healthBarView;
        [SerializeField] private GameObject _hudCanvas;

        private Collider[] _colliders;
        private Rigidbody _rb;
        private IAudioService _audioService;
        private readonly Subject<Unit> _respawned = new();
        private readonly Subject<bool> _frozenChanged = new();

        public Observable<Unit> OnDied => _health.OnDied;

        public Observable<Unit> Respawned => _respawned;

        public Observable<bool> FrozenChanged => _frozenChanged;

        public bool IsDead => _health != null && _health.IsDead;

        public float GetHpRatio() => _health != null ? _health.CurrentHpValue / _health.MaxHp : 0f;

        public void TakeDamage(float damage) => _health?.TakeDamage(damage);

        public void ShowTargetIndicator(bool show)
        {
            if (_targetIndicator != null)
                _targetIndicator.SetActive(show);
        }

        public void Init(
            TankInitData initData,
            ProjectilePool projectilePool,
            DamageService damageService,
            TankRegistry tankRegistry,
            IAudioService audioService)
        {
            Id = initData.Id;
            DisplayName = initData.DisplayName;
            TeamId = TeamSide.None;
            IsPlayer = initData.IsPlayer;
            _shakeIntensity = initData.ShakeIntensity;
            _audioService = audioService;

            _displayNameText.text = initData.DisplayName;

            MountVisuals(initData);

            _health.Init(initData.Chassis.MaxHp);
            _movement.Init(initData.Chassis.MoveSpeed, initData.Chassis.RotationSpeed);
            _weapon.Init(initData.Turret, projectilePool, damageService);
            _autoAim.Init(initData.AimRadius, tankRegistry);
            _damageFlash.Init(transform);
            _healthBarView.Init(initData.IsPlayer);
            _explosionEffect.Init();
            _weapon.Fired.Subscribe(_ => OnWeaponFired()).AddTo(this);

            // Cache physics components after visuals are mounted
            _colliders = GetComponentsInChildren<Collider>();
            TryGetComponent(out _rb);
        }

        public void BindInput(ITankInput input) => _movement.SetInput(input);

        private void MountVisuals(TankInitData initData)
        {
            if (_turretSlot != null && initData.TurretPrefab != null)
            {
                var turretInstance = Instantiate(initData.TurretPrefab, _turretSlot);
                turretInstance.transform.localPosition = Vector3.zero;
                turretInstance.transform.localRotation = Quaternion.identity;

                var visualRoot = turretInstance.GetComponent<TurretVisualRoot>();
                if (visualRoot != null)
                {
                    _weapon.FirePoint = visualRoot.FirePoint;
                    _weapon.SetRecoilTarget(visualRoot.RecoilTarget);
                }
                else
                {
                    Log.Warning(
                        "[Tank] TurretVisualRoot not found on turret prefab '{0}'. Add TurretVisualRoot and assign _firePoint / _recoilTarget in the Inspector.",
                        initData.TurretPrefab.name);
                }
            }
            else
            {
                Log.Warning("[Tank] TurretSlot or TurretPrefab is null for '{0}'.", initData.DisplayName);
            }

            if (_chassisSlot != null && initData.ChassisPrefab != null)
            {
                var chassisInstance = Instantiate(initData.ChassisPrefab, _chassisSlot);
                chassisInstance.transform.localPosition = Vector3.zero;
                chassisInstance.transform.localRotation = Quaternion.identity;
            }
            else
            {
                Log.Warning("[Tank] ChassisSlot or ChassisPrefab is null for '{0}'.", initData.DisplayName);
            }
        }

        public void SetFrozen(bool frozen)
        {
            if (_movement != null) _movement.enabled = !frozen;
            if (_weapon != null) _weapon.enabled = !frozen;
            if (_autoAim != null) _autoAim.enabled = !frozen;
            _frozenChanged.OnNext(frozen);
        }

        public void OnDeath()
        {
            SetFrozen(true);

            TogglePhysics(false);

            if (_chassisSlot != null) _chassisSlot.gameObject.SetActive(false);
            if (_turretSlot != null) _turretSlot.gameObject.SetActive(false);
            if (_audioService != null) _audioService.PlaySfx(SoundId.Battle_Explosion);
            if (_explosionEffect != null) _explosionEffect.Play();
            if (_impulseSource != null) _impulseSource.GenerateImpulse(_shakeIntensity);
            if (_hudCanvas != null) _hudCanvas.SetActive(false);
        }

        private void OnWeaponFired()
        {
            if (!IsPlayer || _audioService == null)
                return;

            _audioService.PlaySfx(SoundId.Battle_Shoot);
        }

        public void OnRespawn(Vector3 position)
        {
            transform.position = position;

            TogglePhysics(true);

            _health?.RestoreFullHealth();
            _weapon?.ReloadFull();

            if (_chassisSlot != null) _chassisSlot.gameObject.SetActive(true);
            if (_turretSlot != null) _turretSlot.gameObject.SetActive(true);
            if (_explosionEffect != null) _explosionEffect.Stop();
            if (_hudCanvas != null) _hudCanvas.SetActive(true);

            _respawned.OnNext(Unit.Default);

            SetFrozen(false);
        }

        private void TogglePhysics(bool active)
        {
            if (_colliders != null)
            {
                foreach (var c in _colliders)
                    c.enabled = active;
            }

            if (_rb != null)
            {
                _rb.isKinematic = !active;
                _rb.detectCollisions = active;
            }
        }

        private void OnDestroy()
        {
            _respawned.Dispose();
            _frozenChanged.Dispose();
        }
    }
}