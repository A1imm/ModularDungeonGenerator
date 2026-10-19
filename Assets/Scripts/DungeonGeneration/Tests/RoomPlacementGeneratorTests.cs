
using System;
using System.Collections.Generic;
using DungeonGeneration.Core;
using DungeonGeneration.Generation;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class RoomPlacementGeneratorTests
    {
        private RoomPlacementGenerator generator;

        [SetUp]
        public void SetUp()
        {
            generator = new RoomPlacementGenerator();
        }

        private List<DungeonRoom> Generate(int seed)
        {
            return generator.Generate(
                seed: seed,
                gridWidth: 60,
                gridHeight: 60,
                roomCount: 10,
                minRoomSize: 3,
                maxRoomSize: 6,
                roomSpacing: 1,
                maxAttempts: 10000
            );
        }

        [Test]
        public void Generate_ReturnsRequestedRoomCount()
        {
            var rooms = Generate(12345);

            Assert.AreEqual(10, rooms.Count);
        }

        [Test]
        public void Generate_SameSeed_ReturnsSameRooms()
        {
            var first = Generate(12345);
            var second = Generate(12345);

            Assert.AreEqual(first.Count, second.Count);

            for (int i = 0; i < first.Count; i++)
            {
                Assert.AreEqual(first[i].Id, second[i].Id);
                Assert.AreEqual(first[i].Position, second[i].Position);
                Assert.AreEqual(first[i].Width, second[i].Width);
                Assert.AreEqual(first[i].Height, second[i].Height);
            }
        }

        [Test]
        public void Generate_RoomsDoNotOverlapOrTouch()
        {
            var rooms = Generate(12345);

            foreach (var room in rooms)
            {
                Assert.GreaterOrEqual(room.Position.X, 1);
                Assert.GreaterOrEqual(room.Position.Y, 1);

                Assert.LessOrEqual(
                    room.Position.X + room.Width, 59);

                Assert.LessOrEqual(
                    room.Position.Y + room.Height, 59);
            }

            for (int i = 0; i < rooms.Count; i++)
            {
                for (int j = i + 1; j < rooms.Count; j++)
                {
                    var a = rooms[i];
                    var b = rooms[j];

                    bool separated =
                        a.Position.X >= b.Position.X + b.Width + 1 ||
                        b.Position.X >= a.Position.X + a.Width + 1 ||
                        a.Position.Y >= b.Position.Y + b.Height + 1 ||
                        b.Position.Y >= a.Position.Y + a.Height + 1;

                    Assert.IsTrue(separated,
                        $"Rooms {a.Id} and {b.Id} are too close.");
                }
            }
        }

        [Test]
        public void Generate_ImpossiblePlacement_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                generator.Generate(
                    seed: 123,
                    gridWidth: 12,
                    gridHeight: 12,
                    roomCount: 20,
                    minRoomSize: 4,
                    maxRoomSize: 4,
                    roomSpacing: 1,
                    maxAttempts: 100
                )
            );
        }
    }
}
