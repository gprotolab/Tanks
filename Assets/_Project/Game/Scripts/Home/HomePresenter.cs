using System;
using Game.App;
using Game.Battle;
using Game.Equipment;
using Game.Merge;
using R3;
using VContainer.Unity;

namespace Game.Home
{
    public class HomePresenter : IStartable, IDisposable
    {
        private readonly HomeView _view;
        private readonly IAppStateMachine _stateMachine;
        private readonly MergeDragInput _dragInput;
        private readonly EquipmentDataService _equipment;
        private readonly TankPartStatsCatalogSO _partsCatalog;
        private readonly CompositeDisposable _disposables = new();

        public HomePresenter(
            HomeView view,
            IAppStateMachine stateMachine,
            MergeDragInput dragInput,
            EquipmentDataService equipment,
            TankPartStatsCatalogSO partsCatalog)
        {
            _view = view;
            _stateMachine = stateMachine;
            _dragInput = dragInput;
            _equipment = equipment;
            _partsCatalog = partsCatalog;
        }

        public void Start()
        {
            _view.BattleStartClicked
                .Subscribe(_ => HandleBattleStart())
                .AddTo(_disposables);

            _view.SettingsClicked
                .Subscribe(_ => HandleSettings())
                .AddTo(_disposables);

            _dragInput.IsDragging
                .Subscribe(dragging => _view.SetHudVisible(!dragging))
                .AddTo(_disposables);

            RefreshTankStats(
                _equipment.GetLevel(TankPartType.Turret),
                _equipment.GetLevel(TankPartType.Chassis));

            _equipment.EquippedPartChanged
                .Subscribe(_ => RefreshTankStats(
                    _equipment.GetLevel(TankPartType.Turret),
                    _equipment.GetLevel(TankPartType.Chassis)))
                .AddTo(_disposables);
        }

        private void RefreshTankStats(int turretLevel, int chassisLevel)
        {
            var turretStats = _partsCatalog.GetTurretStats(turretLevel);
            var chassisStats = _partsCatalog.GetChassisStats(chassisLevel);
            _view.ShowTankStats(turretLevel, turretStats, chassisLevel, chassisStats);
        }

        private void HandleBattleStart() => _stateMachine.EnterBattle();

        private void HandleSettings() => _view.SetSettingsButtonInteractable(false);

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}