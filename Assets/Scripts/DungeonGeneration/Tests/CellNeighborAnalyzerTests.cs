
using System;
using DungeonGeneration.Core;
using DungeonGeneration.Geometry;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class CellNeighborAnalyzerTests
    {
        private CellNeighborAnalyzer analyzer;

        [SetUp]
        public void SetUp()
        {
            analyzer = new CellNeighborAnalyzer();
        }

        [Test]
        public void IsWalkable_Room_ReturnsTrue()
        {
            var grid = new DungeonGrid(3, 3);
            var position = new GridPosition(1, 1);

            grid.SetCell(position, CellType.Room);

            Assert.IsTrue(analyzer.IsWalkable(grid, position));
        }

        [Test]
        public void IsWalkable_Corridor_ReturnsTrue()
        {
            var grid = new DungeonGrid(3, 3);
            var position = new GridPosition(1, 1);

            grid.SetCell(position, CellType.Corridor);

            Assert.IsTrue(analyzer.IsWalkable(grid, position));
        }

        [Test]
        public void IsWalkable_Empty_ReturnsFalse()
        {
            var grid = new DungeonGrid(3, 3);

            Assert.IsFalse(
                analyzer.IsWalkable(grid, new GridPosition(1, 1)));
        }

        [Test]
        public void IsWalkable_OutsideGrid_ReturnsFalse()
        {
            var grid = new DungeonGrid(3, 3);

            Assert.IsFalse(
                analyzer.IsWalkable(grid, new GridPosition(-1, 1)));
        }

        [TestCase(CellDirection.North, 1, 2)]
        [TestCase(CellDirection.East, 2, 1)]
        [TestCase(CellDirection.South, 1, 0)]
        [TestCase(CellDirection.West, 0, 1)]
        public void GetNeighborPosition_ReturnsCorrectPosition(
            CellDirection direction,
            int expectedX,
            int expectedY)
        {
            GridPosition neighbor =
                analyzer.GetNeighborPosition(
                    new GridPosition(1, 1),
                    direction);

            Assert.AreEqual(
                new GridPosition(expectedX, expectedY),
                neighbor);
        }

        [Test]
        public void NeedsWall_EmptyNeighbor_ReturnsTrue()
        {
            var grid = new DungeonGrid(3, 3);
            var position = new GridPosition(1, 1);

            grid.SetCell(position, CellType.Room);

            Assert.IsTrue(analyzer.NeedsWall(
                grid, position, CellDirection.North));
        }

        [Test]
        public void NeedsWall_WalkableNeighbor_ReturnsFalse()
        {
            var grid = new DungeonGrid(3, 3);
            var room = new GridPosition(1, 1);
            var corridor = new GridPosition(2, 1);

            grid.SetCell(room, CellType.Room);
            grid.SetCell(corridor, CellType.Corridor);

            Assert.IsFalse(analyzer.NeedsWall(
                grid, room, CellDirection.East));
        }

        [Test]
        public void NeedsWall_OutsideGrid_ReturnsTrue()
        {
            var grid = new DungeonGrid(3, 3);
            var position = new GridPosition(0, 0);

            grid.SetCell(position, CellType.Corridor);

            Assert.IsTrue(analyzer.NeedsWall(
                grid, position, CellDirection.West));

            Assert.IsTrue(analyzer.NeedsWall(
                grid, position, CellDirection.South));
        }

        [Test]
        public void NeedsWall_EmptySource_ReturnsFalse()
        {
            var grid = new DungeonGrid(3, 3);

            Assert.IsFalse(analyzer.NeedsWall(
                grid,
                new GridPosition(1, 1),
                CellDirection.North));
        }

        [Test]
        public void NeedsWall_SourceOutsideGrid_Throws()
        {
            var grid = new DungeonGrid(3, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                analyzer.NeedsWall(
                    grid,
                    new GridPosition(-1, 0),
                    CellDirection.East));
        }

        [Test]
        public void NeedsWall_NullGrid_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                analyzer.NeedsWall(
                    null,
                    new GridPosition(0, 0),
                    CellDirection.North));
        }
    }
}
