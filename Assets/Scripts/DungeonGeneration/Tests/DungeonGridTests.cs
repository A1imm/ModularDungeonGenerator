
using System;
using DungeonGeneration.Core;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class DungeonGridTests
    {
        [Test]
        public void Constructor_AssignsCorrectDimensions()
        {
            var grid = new DungeonGrid(10, 15);

            Assert.AreEqual(10, grid.Width);
            Assert.AreEqual(15, grid.Height);
        }

        [Test]
        public void Constructor_InitializesCellsAsEmpty()
        {
            var grid = new DungeonGrid(10, 10);

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var position = new GridPosition(x, y);

                    Assert.AreEqual(
                        CellType.Empty,
                        grid.GetCell(position)
                    );
                }
            }
        }

        [Test]
        public void SetCell_StoresCorrectCellType()
        {
            var grid = new DungeonGrid(10, 10);
            var position = new GridPosition(3, 4);

            grid.SetCell(position, CellType.Room);

            Assert.AreEqual(
                CellType.Room,
                grid.GetCell(position)
            );
        }

        [Test]
        public void SetCell_OverwritesPreviousCellType()
        {
            var grid = new DungeonGrid(10, 10);
            var position = new GridPosition(3, 4);

            grid.SetCell(position, CellType.Room);
            grid.SetCell(position, CellType.Corridor);

            Assert.AreEqual(
                CellType.Corridor,
                grid.GetCell(position)
            );
        }

        [Test]
        public void IsInside_ValidCoordinates_ReturnsTrue()
        {
            var grid = new DungeonGrid(10, 10);

            Assert.IsTrue(
                grid.IsInside(new GridPosition(0, 0))
            );

            Assert.IsTrue(
                grid.IsInside(new GridPosition(9, 9))
            );
        }

        [Test]
        public void IsInside_InvalidCoordinates_ReturnsFalse()
        {
            var grid = new DungeonGrid(10, 10);

            Assert.IsFalse(
                grid.IsInside(new GridPosition(-1, 0))
            );

            Assert.IsFalse(
                grid.IsInside(new GridPosition(10, 5))
            );

            Assert.IsFalse(
                grid.IsInside(new GridPosition(5, 10))
            );
        }

        [Test]
        public void Constructor_InvalidDimensions_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new DungeonGrid(0, 10)
            );

            Assert.Throws<ArgumentOutOfRangeException>(
                () => new DungeonGrid(10, -1)
            );
        }

        [Test]
        public void GetCell_OutOfBounds_ThrowsException()
        {
            var grid = new DungeonGrid(10, 10);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => grid.GetCell(new GridPosition(10, 5))
            );
        }

        [Test]
        public void SetCell_OutOfBounds_ThrowsException()
        {
            var grid = new DungeonGrid(10, 10);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => grid.SetCell(
                    new GridPosition(-1, 5),
                    CellType.Room
                )
            );
        }
    }
}
