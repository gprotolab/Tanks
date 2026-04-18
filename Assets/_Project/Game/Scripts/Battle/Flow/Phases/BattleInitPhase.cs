using Cysharp.Threading.Tasks;
using Game.Equipment;
using System.Linq;
using System.Threading;
using Unity.Cinemachine;
using ANut.Core;

namespace Game.Battle
{
    public class BattleInitPhase
    {
        private readonly BattleSession _session;
        private readonly ArenaLoader _arenaLoader;
        private readonly TankFactory _tankFactory;
        private readonly TankRegistry _tankRegistry;
        private readonly EquipmentDataService _equipmentData;
        private readonly BattleStatsDataService _battleStatsData;
        private readonly BattleJoystickView _joystickView;
        private readonly ProjectilePool _projectilePool;
        private readonly TankPartStatsCatalogSO _battlePartsCatalog;
        private readonly CinemachineCamera _virtualCamera;
        private readonly BotConfigGenerator _botConfigGenerator;
        private readonly BattleHUDView _battleHudView;

        public BattleInitPhase(
            BattleSession session,
            ArenaLoader arenaLoader,
            TankFactory tankFactory,
            TankRegistry tankRegistry,
            EquipmentDataService equipmentData,
            BattleStatsDataService battleStatsData,
            BattleJoystickView joystickView,
            ProjectilePool projectilePool,
            TankPartStatsCatalogSO battlePartsCatalog,
            CinemachineCamera virtualCamera,
            BotConfigGenerator botConfigGenerator,
            BattleHUDView battleHUDView
        )
        {
            _session = session;
            _arenaLoader = arenaLoader;
            _tankFactory = tankFactory;
            _tankRegistry = tankRegistry;
            _equipmentData = equipmentData;
            _battleStatsData = battleStatsData;
            _joystickView = joystickView;
            _projectilePool = projectilePool;
            _battlePartsCatalog = battlePartsCatalog;
            _virtualCamera = virtualCamera;
            _botConfigGenerator = botConfigGenerator;
            _battleHudView = battleHUDView;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            Log.Info("[BattleInitPhase] Initialising battle...");

            var arena = await _arenaLoader.LoadAsync(ct);
            if (arena == null)
            {
                Log.Error("[BattleInitPhase] Failed to load arena — battle aborted.");
                return;
            }

            _session.Arena = arena;

            _battleHudView.Hide();

            // Warm pools before combat to avoid runtime spikes on first shots.
            WarmUpProjectilePool();

            SpawnPlayer();

            BindCameraToPlayer();

            SpawnBots();

            // Keep tanks frozen until countdown phase sends GO.
            _tankRegistry.FreezeAll();

            Log.Info("[BattleInitPhase] Init complete.");
        }

        private void SpawnBots()
        {
            int turretLevel = _equipmentData.TurretLevel;
            int chassisLevel = _equipmentData.ChassisLevel;
            int battlesOnCurrentEquip = _battleStatsData.BattlesOnCurrentEquip;
            int poorFinishStreak = _battleStatsData.PoorFinishStreak;

            var botConfigs =
                _botConfigGenerator.Generate(turretLevel, chassisLevel, battlesOnCurrentEquip, poorFinishStreak);
            var spawnPoints = _session.Arena.FfaSpawnPoints;

            for (int i = 0; i < botConfigs.Length; i++)
            {
                // Spawn index starts at 1 because index 0 is reserved for the player.
                int spawnIndex = (i + 1) % spawnPoints.Length;
                var spawnPoint = spawnPoints.Length > 0
                    ? spawnPoints[spawnIndex]
                    : _session.Arena.Instance.transform;

                _tankFactory.CreateBotTank(botConfigs[i], spawnPoint);
            }

            Log.Info("[BattleInitPhase] Spawned {0} bots.", botConfigs.Length);
        }

        private void WarmUpProjectilePool()
        {
            var configs = _battlePartsCatalog.GetAllProjectileConfigs().Distinct().ToArray();

            if (configs.Length == 0)
            {
                Log.Warning("[BattleInitPhase] WarmUpProjectilePool: no ProjectileConfigs found.");
                return;
            }

            _projectilePool.WarmUp(configs, countPerType: 10);
            Log.Info("[BattleInitPhase] ProjectilePool warmed up: {0} types, 10 each.", configs.Length);
        }

        private void SpawnPlayer()
        {
            int turretLevel = _equipmentData.TurretLevel;
            int chassisLevel = _equipmentData.ChassisLevel;

            var spawnPoints = _session.Arena.FfaSpawnPoints;
            var spawnPoint = spawnPoints.Length > 0 ? spawnPoints[0] : _session.Arena.Instance.transform;

            var playerInput = new PlayerTankInput(_joystickView);
            var playerTank = _tankFactory.CreatePlayerTank(turretLevel, chassisLevel, spawnPoint, playerInput);
            _session.PlayerTank = playerTank;
        }

        private void BindCameraToPlayer()
        {
            if (_session.PlayerTank == null)
            {
                Log.Error("[BattleInitPhase] BindCameraToPlayer: PlayerTank not created, camera not bound.");
                return;
            }

            if (_virtualCamera == null)
            {
                Log.Error("[BattleInitPhase] CinemachineCamera not assigned in BattleLifetimeScope.");
                return;
            }

            _virtualCamera.Follow = _session.PlayerTank.transform;
            _virtualCamera.PreviousStateIsValid = false;
            Log.Info("[BattleInitPhase] Camera bound to player tank.");
        }
    }
}