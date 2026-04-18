using System;
using Game.Battle;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Home
{
    public class HomeView : MonoBehaviour
    {
        private readonly Subject<Unit> _battleStartClicked = new();
        private readonly Subject<Unit> _settingsClicked = new();

        public Observable<Unit> BattleStartClicked => _battleStartClicked;
        public Observable<Unit> SettingsClicked => _settingsClicked;

        [SerializeField] private Button _battleStartButton;
        [SerializeField] private Button _settingsButton;

        [SerializeField] private CanvasGroup _bottomPanelGroup;
        [SerializeField] private TankStatsView _tankStatsView;

        private void Awake()
        {
            _battleStartButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(0.5f))
                .Subscribe(_ => _battleStartClicked.OnNext(Unit.Default))
                .AddTo(this);

            _settingsButton?.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(0.5f))
                .Subscribe(_ => _settingsClicked.OnNext(Unit.Default))
                .AddTo(this);
        }

        public void SetHudVisible(bool visible)
        {
            if (_bottomPanelGroup == null) return;

            _bottomPanelGroup.alpha = visible ? 1f : 0f;
            _bottomPanelGroup.interactable = visible;
            _bottomPanelGroup.blocksRaycasts = visible;
        }

        public void SetBattleButtonInteractable(bool interactable) =>
            _battleStartButton.interactable = interactable;

        public void SetSettingsButtonInteractable(bool interactable) =>
            _settingsButton.interactable = interactable;

        public void ShowTankStats(
            int turretLevel, TankPartStatsCatalogSO.TurretBattleStats turret,
            int chassisLevel, TankPartStatsCatalogSO.ChassisBattleStats chassis)
        {
            if (_tankStatsView == null) return;
            _tankStatsView.Show(turretLevel, turret, chassisLevel, chassis);
        }

        private void OnDestroy()
        {
            _battleStartClicked.Dispose();
            _settingsClicked.Dispose();
        }
    }
}