using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Game.Merge;
using Game.Equipment;

namespace Game.Tests.EditMode
{
    [TestFixture]
    public class MergeServiceTests
    {
        private MergeModel _model;
        private MergeSettings _settings;
        private MergeDataService _data;
        private MergeRuleService _rules;
        private GridMutationService _mutation;
        private EquipService _equip;

        private static readonly Vector2Int[] CellOrder =
        {
            new(0, 0), new(1, 0), new(2, 0), new(3, 0)
        };

        [SetUp]
        public void SetUp()
        {
            _model = new MergeModel();
            _model.Initialize(4, CellOrder);

            _settings = new MergeSettings();
            _data = new MergeDataService();
            _rules = new MergeRuleService();
            _mutation = new GridMutationService(_model, _rules, _settings, _data);
            _equip = new EquipService(_model, _rules, _settings);
        }

        [Test]
        public void CanMerge_BothNull_ReturnsFalse()
            => Assert.IsFalse(_rules.CanMerge(null, null));

        [Test]
        public void CanMerge_FirstPartNull_ReturnsFalse()
            => Assert.IsFalse(_rules.CanMerge(null, new TankPartData(TankPartType.Turret, 1)));

        [Test]
        public void CanMerge_SecondPartNull_ReturnsFalse()
            => Assert.IsFalse(_rules.CanMerge(new TankPartData(TankPartType.Turret, 1), null));

        [Test]
        public void CanMerge_SameTypeAndSameLevel_ReturnsTrue()
        {
            var a = new TankPartData(TankPartType.Turret, 3);
            var b = new TankPartData(TankPartType.Turret, 3);
            Assert.IsTrue(_rules.CanMerge(a, b));
        }

        [Test]
        public void CanMerge_SameType_DifferentLevel_ReturnsFalse()
        {
            var a = new TankPartData(TankPartType.Turret, 3);
            var b = new TankPartData(TankPartType.Turret, 4);
            Assert.IsFalse(_rules.CanMerge(a, b));
        }

        [Test]
        public void CanMerge_DifferentType_SameLevel_ReturnsFalse()
        {
            var a = new TankPartData(TankPartType.Turret, 3);
            var b = new TankPartData(TankPartType.Chassis, 3);
            Assert.IsFalse(_rules.CanMerge(a, b));
        }

        // ── Merge: level cap ─────────────────────────────────────────────────

        [Test]
        public void Merge_BelowMaxLevel_ReturnsLevelPlusOne()
        {
            SetMaxPartLevel(100);
            var a = new TankPartData(TankPartType.Turret, 5);
            var b = new TankPartData(TankPartType.Turret, 5);

            var result = _rules.TryMerge(a, b, _settings.MaxPartLevel);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(6, result.Part.Level, "A normal merge should increase the level by one.");
        }

        [Test]
        public void Merge_AtMaxLevel_ReturnsFailWithOriginalPart()
        {
            // Shows that the level cap blocks the upgrade result.
            SetMaxPartLevel(5);
            var a = new TankPartData(TankPartType.Turret, 5);
            var b = new TankPartData(TankPartType.Turret, 5);

            var result = _rules.TryMerge(a, b, _settings.MaxPartLevel);

            Assert.IsFalse(result.Success, "Merge at cap should fail.");
            Assert.AreEqual(5, result.Part.Level);
            Assert.AreEqual(a.Type, result.Part.Type);
        }

        [Test]
        public void Merge_PreservesPartType()
        {
            var a = new TankPartData(TankPartType.Chassis, 3);
            var b = new TankPartData(TankPartType.Chassis, 3);

            var result = _rules.TryMerge(a, b, _settings.MaxPartLevel);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(TankPartType.Chassis, result.Part.Type,
                "Part type should stay the same after merge.");
        }

        [Test]
        public void TryMerge_EmptySourceCell_ReturnsFalse()
        {
            _model.SetCell(1, 0, new TankPartData(TankPartType.Turret, 2));

            Assert.IsFalse(_mutation.TryMerge(0, 0, 1, 0));
        }

        [Test]
        public void TryMerge_EmptyTargetCell_ReturnsFalse()
        {
            _model.SetCell(0, 0, new TankPartData(TankPartType.Turret, 2));

            Assert.IsFalse(_mutation.TryMerge(0, 0, 1, 0));
        }

        [Test]
        public void TryMerge_IncompatibleParts_ReturnsFalse()
        {
            _model.SetCell(0, 0, new TankPartData(TankPartType.Turret, 2));
            _model.SetCell(1, 0, new TankPartData(TankPartType.Chassis, 2));

            Assert.IsFalse(_mutation.TryMerge(0, 0, 1, 0));
        }

        [Test]
        public void TryMerge_CompatibleParts_ReturnsTrue()
        {
            _model.SetCell(0, 0, new TankPartData(TankPartType.Turret, 2));
            _model.SetCell(1, 0, new TankPartData(TankPartType.Turret, 2));

            Assert.IsTrue(_mutation.TryMerge(0, 0, 1, 0));
        }

        [Test]
        public void TryMerge_CompatibleParts_SourceClearedTargetUpgraded()
        {
            _model.SetCell(0, 0, new TankPartData(TankPartType.Turret, 2));
            _model.SetCell(1, 0, new TankPartData(TankPartType.Turret, 2));

            _mutation.TryMerge(0, 0, 1, 0);

            Assert.IsTrue(_model.GetCell(0, 0).IsEmpty, "Source cell should be cleared.");
            Assert.AreEqual(3, _model.GetCell(1, 0).Part.Level, "Target cell should get level +1.");
        }

        [Test]
        public void TryMerge_AtLevelCap_ReturnsFalse()
        {
            SetMaxPartLevel(5);
            _model.SetCell(0, 0, new TankPartData(TankPartType.Turret, 5));
            _model.SetCell(1, 0, new TankPartData(TankPartType.Turret, 5));

            Assert.IsFalse(_mutation.TryMerge(0, 0, 1, 0),
                "A merge blocked by the cap should not count as successful.");
        }

        [Test]
        public void TryEquip_EmptySourceCell_ReturnsInvalidType()
        {
            Assert.AreEqual(EquipResult.InvalidType, _equip.TryEquip(0, 0));
        }

        [Test]
        public void TryEquip_PartWithZeroLevel_ReturnsInvalidLevel()
        {
            _model.SetCell(0, 0, new TankPartData(TankPartType.Turret, 0));
            Assert.AreEqual(EquipResult.InvalidLevel, _equip.TryEquip(0, 0));
        }

        [Test]
        public void TryEquip_SlotEmpty_Equips_ReturnsEquipped()
        {
            _model.SetCell(0, 0, new TankPartData(TankPartType.Turret, 3));

            var result = _equip.TryEquip(0, 0);

            Assert.AreEqual(EquipResult.Equipped, result);
            Assert.AreEqual(3, _model.EquippedTurret.Level, "A level 3 turret should be equipped.");
            Assert.IsTrue(_model.GetCell(0, 0).IsEmpty, "The cell should be cleared after equip.");
        }

        [Test]
        public void TryEquip_SlotOccupiedSameLevel_MergesAndUpgrades_ReturnsMerged()
        {
            _model.SetEquipped(TankPartType.Turret, new TankPartData(TankPartType.Turret, 2));
            _model.SetCell(0, 0, new TankPartData(TankPartType.Turret, 2));

            var result = _equip.TryEquip(0, 0);

            Assert.AreEqual(EquipResult.Merged, result);
            Assert.AreEqual(3, _model.EquippedTurret.Level, "Merge should produce level 3.");
            Assert.IsTrue(_model.GetCell(0, 0).IsEmpty, "Source cell should be cleared.");
        }

        [Test]
        public void TryEquip_SlotOccupiedDifferentLevel_Swaps_ReturnsSwapped()
        {
            _model.SetEquipped(TankPartType.Turret, new TankPartData(TankPartType.Turret, 5));
            _model.SetCell(0, 0, new TankPartData(TankPartType.Turret, 2));

            var result = _equip.TryEquip(0, 0);

            Assert.AreEqual(EquipResult.Swapped, result);
            Assert.AreEqual(2, _model.EquippedTurret.Level, "The new level 2 part should be equipped.");
            Assert.AreEqual(5, _model.GetCell(0, 0).Part.Level, "The old level 5 part should move into the cell.");
        }

        [Test]
        public void TryEquip_MergeBlockedByCap_ReturnsSwapped()
        {
            // Note: in the new implementation, if it can't merge at cap, it Swaps (replaces equipped with the new one)
            // unless we specifically handle InvalidLevel in EquipService.
            // Let's check EquipService.cs:38-42. It tries rules.TryMerge.
            // If it fails (cap reached), it falls through to Swapped.
            SetMaxPartLevel(5);
            _model.SetEquipped(TankPartType.Turret, new TankPartData(TankPartType.Turret, 5));
            _model.SetCell(0, 0, new TankPartData(TankPartType.Turret, 5));

            var result = _equip.TryEquip(0, 0);

            Assert.AreEqual(EquipResult.Swapped, result,
                "A merge at the cap should fall back to Swap in the current EquipService implementation.");
        }

        private void SetMaxPartLevel(int value)
        {
            typeof(MergeSettings)
                .GetField("_maxPartLevel", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_settings, value);
        }
    }
}