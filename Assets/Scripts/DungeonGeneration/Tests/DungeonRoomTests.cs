using System;
using DungeonGeneration.Core;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class DungeonRoomTests
    {
        [Test]
        public void Constructor_AssignsCorrectValues()
        {
            var room = new DungeonRoom(
                1, new GridPosition(2, 3), 4, 6);

            Assert.AreEqual(1, room.Id);
            Assert.AreEqual(new GridPosition(2, 3), room.Position);
            Assert.AreEqual(4, room.Width);
            Assert.AreEqual(6, room.Height);
        }

        [Test]
        public void Center_ReturnsCorrectPosition()
        {
            var room = new DungeonRoom(
                0, new GridPosition(2, 1), 4, 3);

            Assert.AreEqual(
                new GridPosition(4, 2),
                room.Center
            );
        }

        [Test]
        public void Contains_InsidePosition_ReturnsTrue()
        {
            var room = new DungeonRoom(
                0, new GridPosition(2, 2), 4, 4);

            Assert.IsTrue(
                room.Contains(new GridPosition(3, 3))
            );
        }

        [Test]
        public void Contains_OutsidePosition_ReturnsFalse()
        {
            var room = new DungeonRoom(
                0, new GridPosition(2, 2), 4, 4);

            Assert.IsFalse(
                room.Contains(new GridPosition(6, 3))
            );
        }

        [Test]
        public void Intersects_OverlappingRooms_ReturnsTrue()
        {
            var a = new DungeonRoom(
                0, new GridPosition(2, 2), 4, 4);

            var b = new DungeonRoom(
                1, new GridPosition(4, 4), 4, 4);

            Assert.IsTrue(a.Intersects(b));
        }

        [Test]
        public void Intersects_SeparateRooms_ReturnsFalse()
        {
            var a = new DungeonRoom(
                0, new GridPosition(2, 2), 4, 4);

            var b = new DungeonRoom(
                1, new GridPosition(8, 8), 4, 4);

            Assert.IsFalse(a.Intersects(b));
        }

        [Test]
        public void Intersects_TouchingEdges_ReturnsFalse()
        {
            var a = new DungeonRoom(
                0, new GridPosition(2, 2), 4, 4);

            var b = new DungeonRoom(
                1, new GridPosition(6, 2), 4, 4);

            Assert.IsFalse(a.Intersects(b));
        }

        [Test]
        public void Constructor_InvalidValues_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new DungeonRoom(
                    -1, new GridPosition(0, 0), 4, 4)
            );

            Assert.Throws<ArgumentOutOfRangeException>(
                () => new DungeonRoom(
                    0, new GridPosition(0, 0), 0, 4)
            );
        }
    }
}
