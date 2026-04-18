using Game.Equipment;
using ANut.Core.Audio;
using MessagePipe;
using UnityEngine;
using ANut.Core;

namespace Game.Battle
{
    public class TankFactory
    {
        private readonly BattleConfigSO _battleConfig;
        private readonly TankPartStatsCatalogSO _battlePartsCatalog;
        private readonly TankPartSkinCatalogSO _mergePartCatalog;
        private readonly TankRegistry _tankRegistry;
        private readonly ProjectilePool _projectilePool;
        private readonly DamageService _damageService;
        private readonly IAudioService _audioService;
        private readonly IPublisher<TankSpawnedSignal> _tankSpawned;
        private readonly Transform _dynamicRoot;

        private int _nextId = 1;

        public TankFactory(
            BattleConfigSO battleConfig,
            TankPartStatsCatalogSO battlePartsCatalog,
            TankPartSkinCatalogSO mergePartCatalog,
            TankRegistry tankRegistry,
            ProjectilePool projectilePool,
            DamageService damageService,
            IAudioService audioService,
            IPublisher<TankSpawnedSignal> tankSpawned,
            DynamicObjectsRoot dynamicObjectsRoot)
        {
            _battleConfig = battleConfig;
            _battlePartsCatalog = battlePartsCatalog;
            _mergePartCatalog = mergePartCatalog;
            _tankRegistry = tankRegistry;
            _projectilePool = projectilePool;
            _damageService = damageService;
            _audioService = audioService;
            _tankSpawned = tankSpawned;
            _dynamicRoot = dynamicObjectsRoot.transform;
        }

        public Tank CreatePlayerTank(int turretLevel, int chassisLevel, Transform spawnPoint, ITankInput input)
        {
            var tank = SpawnAndInit(_battleConfig.PlayerTankPrefab, "Player", isPlayer: true,
                turretLevel, chassisLevel, spawnPoint);
            if (tank == null) return null;

            tank.BindInput(input);

            // Player HUD exists only on the player prefab.
            tank.GetComponent<PlayerTankHud>()?.Init();

            RegisterTank(tank);

            Log.Info("[TankFactory] Player tank created: turret={0}, chassis={1}, pos={2}",
                turretLevel, chassisLevel, spawnPoint.position);
            return tank;
        }

        public Tank CreateBotTank(BotInitData initData, Transform spawnPoint)
        {
            var tank = SpawnAndInit(_battleConfig.BotTankPrefab, initData.Name, isPlayer: false,
                initData.TurretLevel, initData.ChassisLevel, spawnPoint);
            if (tank == null) return null;

            var botBrain = tank.GetComponent<BotBrain>();
            botBrain.Init(initData.Profile, _tankRegistry);
            tank.BindInput(botBrain);

            RegisterTank(tank);

            Log.Info("[TankFactory] Bot '{0}' created: turret={1}, chassis={2}, pos={3}",
                initData.Name, initData.TurretLevel, initData.ChassisLevel, spawnPoint.position);
            return tank;
        }

        private Tank SpawnAndInit(
            Tank prefab, string displayName, bool isPlayer,
            int turretLevel, int chassisLevel, Transform spawnPoint)
        {
            var tank = Spawn(prefab, spawnPoint);
            if (tank == null) return null;

            var initData = new TankInitData(
                id: _nextId++,
                displayName: displayName,
                isPlayer: isPlayer,
                turret: _battlePartsCatalog.GetTurretStats(turretLevel),
                chassis: _battlePartsCatalog.GetChassisStats(chassisLevel),
                aimRadius: _battleConfig.AimRadius,
                shakeIntensity: _battleConfig.CameraShakeIntensity,
                turretPrefab: _mergePartCatalog.GetPrefab(TankPartType.Turret, turretLevel),
                chassisPrefab: _mergePartCatalog.GetPrefab(TankPartType.Chassis, chassisLevel)
            );

            tank.Init(initData, _projectilePool, _damageService, _tankRegistry, _audioService);
            return tank;
        }

        private void RegisterTank(Tank tank)
        {
            _tankRegistry.Register(tank);
            _tankSpawned.Publish(new TankSpawnedSignal(tank.Id, tank.DisplayName, tank.IsPlayer));
        }

        private Tank Spawn(Tank prefab, Transform spawnPoint)
        {
            if (prefab == null)
            {
                Log.Error("[TankFactory] BattleConfig field is not assigned!");
                return null;
            }

            // Keep only Y rotation to avoid unwanted tilt from helper transforms.
            var rotation = Quaternion.Euler(0f, spawnPoint.eulerAngles.y, 0f);
            return Object.Instantiate(prefab, spawnPoint.position, rotation, _dynamicRoot);
        }
    }
}