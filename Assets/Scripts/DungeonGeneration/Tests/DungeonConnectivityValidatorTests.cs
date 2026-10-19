
using System;
using DungeonGeneration.Core;
using DungeonGeneration.Generation;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class DungeonConnectivityValidatorTests
    {
        private readonly DungeonConnectivityValidator validator =
            new DungeonConnectivityValidator();

        private DungeonRoom[] CreateRooms()
        {
            return new[]
            {
                new DungeonRoom(0, new GridPosition(1, 1), 2, 2),
                new DungeonRoom(1, new GridPosition(6, 1), 2, 2)
            };
        }

        private DungeonGrid CreateGrid(
            DungeonRoom[] rooms,
            bool connectRooms)
        {
            var grid = new DungeonGrid(10, 5);

            foreach (var room in rooms)
            {
                for (int x = room.Position.X;
                     x < room.Position.X + room.Width; x++)
                {
                    for (int y = room.Position.Y;
                         y < room.Position.Y + room.Height; y++)
                    {
                        grid.SetCell(
                            new GridPosition(x, y),
                            CellType.Room
                        );
                    }
                }
            }

            if (connectRooms)
            {
                for (int x = 3; x <= 5; x++)
                {
                    grid.SetCell(
                        new GridPosition(x, 1),
                        CellType.Corridor
                    );
                }
            }

            return grid;
        }

        [Test]
        public void AreAllRoomsReachable_ConnectedRooms_ReturnsTrue()
        {
            var rooms = CreateRooms();
            var grid = CreateGrid(rooms, true);

            Assert.IsTrue(
                validator.AreAllRoomsReachable(grid, rooms)
            );
        }

        [Test]
        public void AreAllRoomsReachable_DisconnectedRooms_ReturnsFalse()
        {
            var rooms = CreateRooms();
            var grid = CreateGrid(rooms, false);

            Assert.IsFalse(
                validator.AreAllRoomsReachable(grid, rooms)
            );
        }

        [Test]
        public void AreAllRoomsReachable_SingleRoom_ReturnsTrue()
        {
            var rooms = new[]
            {
                new DungeonRoom(0, new GridPosition(1, 1), 2, 2)
            };

            var grid = CreateGrid(rooms, false);

            Assert.IsTrue(
                validator.AreAllRoomsReachable(grid, rooms)
            );
        }

        [Test]
        public void AreAllRoomsReachable_EmptyDungeon_ReturnsFalse()
        {
            var grid = new DungeonGrid(10, 10);
            var rooms = Array.Empty<DungeonRoom>();

            Assert.IsFalse(
                validator.AreAllRoomsReachable(grid, rooms)
            );
        }

        [Test]
        public void AreAllRoomsReachable_NullGrid_ThrowsException()
        {
            var rooms = CreateRooms();

            Assert.Throws<ArgumentNullException>(() =>
                validator.AreAllRoomsReachable(null, rooms));
        }
    }
}
