
using System.Collections.Generic;
using DungeonGeneration.Core;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class GridPositionTests
    {
        [Test]
        public void Constructor_AssignsCorrectCoordinates()
        {
            var position = new GridPosition(2, 3);

            Assert.AreEqual(2, position.X);
            Assert.AreEqual(3, position.Y);
        }

        [Test]
        public void Equals_SameCoordinates_ReturnsTrue()
        {
            var a = new GridPosition(2, 3);
            var b = new GridPosition(2, 3);

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a == b);
        }

        [Test]
        public void Equals_DifferentCoordinates_ReturnsFalse()
        {
            var a = new GridPosition(2, 3);
            var b = new GridPosition(2, 4);

            Assert.IsFalse(a.Equals(b));
            Assert.IsTrue(a != b);
        }

        [Test]
        public void HashSet_DuplicatePositions_StoresOnlyUniqueValues()
        {
            var visited = new HashSet<GridPosition>();

            visited.Add(new GridPosition(2, 3));
            visited.Add(new GridPosition(2, 3));
            visited.Add(new GridPosition(4, 5));

            Assert.AreEqual(2, visited.Count);
            Assert.IsTrue(
                visited.Contains(new GridPosition(2, 3))
            );
        }
    }
}
