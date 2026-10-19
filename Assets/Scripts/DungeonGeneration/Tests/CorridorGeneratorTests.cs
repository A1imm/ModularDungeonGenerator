
using System;
using DungeonGeneration.Core;
using DungeonGeneration.Generation;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class CorridorGeneratorTests
    {
        private DungeonRoom[] CreateRooms()
        {
            return new[]
            {
                new DungeonRoom(0, new GridPosition(1, 5), 3, 3),
                new DungeonRoom(1, new GridPosition(8, 4), 3, 5),
                new DungeonRoom(2, new GridPosition(15, 5), 3, 3)
            };
        }

        private RoomGraph CreateGraph(DungeonRoom[] rooms)
        {
            var graph = new RoomGraph(rooms);
            graph.AddConnection(new RoomConnection(0, 2, 1));
            return graph;
        }

        [Test]
        public void Generate_CreatesCorridorCells()
        {
            var rooms = CreateRooms();
            var graph = CreateGraph(rooms);

            var grid = new CorridorGenerator().Generate(
                20, 15, rooms, graph);

            int corridorCount = 0;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    if (grid.GetCell(new GridPosition(x, y))
                        == CellType.Corridor)
                    {
                        corridorCount++;
                    }
                }
            }

            Assert.Greater(corridorCount, 0);
        }

        [Test]
        public void Generate_DoesNotOverwriteOtherRooms()
        {
            var rooms = CreateRooms();
            var graph = CreateGraph(rooms);

            var grid = new CorridorGenerator().Generate(
                20, 15, rooms, graph);

            foreach (DungeonRoom room in rooms)
            {
                for (int x = room.Position.X;
                     x < room.Position.X + room.Width; x++)
                {
                    for (int y = room.Position.Y;
                         y < room.Position.Y + room.Height; y++)
                    {
                        Assert.AreEqual(
                            CellType.Room,
                            grid.GetCell(new GridPosition(x, y))
                        );
                    }
                }
            }
        }

        [Test]
        public void Generate_SameInput_ReturnsIdenticalGrid()
        {
            var rooms = CreateRooms();
            var graph = CreateGraph(rooms);
            var generator = new CorridorGenerator();

            var first = generator.Generate(20, 15, rooms, graph);
            var second = generator.Generate(20, 15, rooms, graph);

            for (int x = 0; x < first.Width; x++)
            {
                for (int y = 0; y < first.Height; y++)
                {
                    var position = new GridPosition(x, y);

                    Assert.AreEqual(
                        first.GetCell(position),
                        second.GetCell(position)
                    );
                }
            }
        }

        [Test]
        public void Generate_BlockedPath_ThrowsException()
        {
            var rooms = new[]
            {
                new DungeonRoom(0, new GridPosition(1, 2), 2, 2),
                new DungeonRoom(1, new GridPosition(4, 0), 2, 8),
                new DungeonRoom(2, new GridPosition(7, 2), 2, 2)
            };

            var graph = new RoomGraph(rooms);
            graph.AddConnection(new RoomConnection(0, 2, 1));

            Assert.Throws<InvalidOperationException>(() =>
                new CorridorGenerator().Generate(
                    10, 8, rooms, graph)
            );
        }
    }
}
