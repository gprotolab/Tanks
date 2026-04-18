using System;
using NUnit.Framework;
using UnityEngine;
using Game.Merge;
using Game.Equipment;

namespace Game.Tests.EditMode
{
    [TestFixture]
    public class MergeModelTests
    {
        // Reuses one simple cell order, so setup stays small.
        private static readonly Vector2Int[] DefaultOrder =
        {
            new(0, 0), new(1, 0), new(2, 0), new(3, 0)
        };

        private MergeModel CreateModel(int unlockedCells = 2, Vector2Int[] order = null)
        {
            var model = new MergeModel();
            model.Initialize(unlockedCells, order ?? DefaultOrder);
            return model;
        }

        [Test]
        public void Initialize_NullOrder_ThrowsArgumentException()
        {
            var model = new MergeModel();
            Assert.Throws<ArgumentException>(() => model.Initialize(2, null));
        }

        [Test]
        public void Initialize_EmptyOrder_ThrowsArgumentException()
        {
            var model = new MergeModel();
            Assert.Throws<ArgumentException>(() => model.Initialize(2, Array.Empty<Vector2Int>()));
        }

        [Test]
        public void Initialize_UnlockedCellsExceedTotal_ClampsToTotalCount()
        {
            // Shows that unlock count cannot grow past the available cells.
            var model = CreateModel(unlockedCells: 999, order: new[]
            {
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0)
            });
            Assert.AreEqual(3, model.UnlockedCellCount);
        }

        [Test]
        public void IsValidPosition_And_IsCellUnlocked_CanDiffer()
        {
            // Shows that a valid cell can still stay locked.
            var model = CreateModel(unlockedCells: 1);
            Assert.IsTrue(model.IsValidPosition(3, 0), "The cell should exist in the grid.");
            Assert.IsFalse(model.IsCellUnlocked(3, 0), "The cell should still be locked.");
        }

        [Test]
        public void UnlockNextCell_AtMaxCapacity_DoesNotExceedTotal()
        {
            var model = CreateModel(unlockedCells: 4);
            model.UnlockNextCell();
            Assert.AreEqual(4, model.UnlockedCellCount, "The count should not go above TotalCellCount.");
        }

        [Test]
        public void GetMaxPartLevel_WithParts_ReturnsHighestLevel()
        {
            var model = CreateModel(unlockedCells: 4);
            model.SetCell(0, 0, new TankPartData(TankPartType.Turret, 3));
            model.SetCell(1, 0, new TankPartData(TankPartType.Chassis, 7));
            model.SetCell(2, 0, new TankPartData(TankPartType.Turret, 2));

            Assert.AreEqual(7, model.GetMaxPartLevel());
        }

        [Test]
        public void GetMaxPartLevelForType_IncludesEquippedPart()
        {
            // Shows that equipped parts also affect the max level check.
            var model = CreateModel(unlockedCells: 4);
            model.SetCell(0, 0, new TankPartData(TankPartType.Turret, 3));
            model.SetEquipped(TankPartType.Turret, new TankPartData(TankPartType.Turret, 10));

            int max = model.GetMaxPartLevelForType(TankPartType.Turret);

            Assert.AreEqual(10, max,
                "An equipped level 10 part should be included in the max value.");
        }

        [Test]
        public void GetMaxPartLevelForType_DoesNotMixTypes()
        {
            var model = CreateModel(unlockedCells: 4);
            model.SetCell(0, 0, new TankPartData(TankPartType.Chassis, 15));

            int turretMax = model.GetMaxPartLevelForType(TankPartType.Turret);

            Assert.AreEqual(0, turretMax, "Chassis parts should not affect the Turret max level.");
        }

        [Test]
        public void HasEmptyCell_AllOccupied_ReturnsFalse()
        {
            var model = CreateModel(unlockedCells: 2);
            model.SetCell(0, 0, new TankPartData(TankPartType.Turret, 1));
            model.SetCell(1, 0, new TankPartData(TankPartType.Turret, 1));

            Assert.IsFalse(model.HasEmptyCell());
        }
    }
}