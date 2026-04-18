using Game.Battle;
using TMPro;
using UnityEngine;

namespace Game.Home
{
    public class TankStatsView : MonoBehaviour
    {
        [Header("Turret")] [SerializeField] private TMP_Text _turretLevelLabel;
        [SerializeField] private TMP_Text _turretBurstLabel;
        [SerializeField] private TMP_Text _turretReloadLabel;

        [Header("Chassis")] [SerializeField] private TMP_Text _chassisLevelLabel;
        [SerializeField] private TMP_Text _chassisHpLabel;
        [SerializeField] private TMP_Text _chassisSpeedLabel;

        public void Show(
            int turretLevel, TankPartStatsCatalogSO.TurretBattleStats turret,
            int chassisLevel, TankPartStatsCatalogSO.ChassisBattleStats chassis)
        {
            if (_turretLevelLabel != null) _turretLevelLabel.text = $"{turretLevel}";

            float burst = turret.Damage * turret.AmmoCapacity;
            if (_turretBurstLabel != null) _turretBurstLabel.text = $"{burst:0}";
            if (_turretReloadLabel != null) _turretReloadLabel.text = $"{turret.MagazineReloadTime:0.0}s";

            if (_chassisLevelLabel != null) _chassisLevelLabel.text = $"{chassisLevel}";
            if (_chassisHpLabel != null) _chassisHpLabel.text = $"{chassis.MaxHp:0}";
            if (_chassisSpeedLabel != null) _chassisSpeedLabel.text = $"{chassis.MoveSpeed:0.0}";
        }
    }
}