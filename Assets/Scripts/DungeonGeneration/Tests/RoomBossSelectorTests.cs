
using System;
using DungeonGeneration.Core;
using DungeonGeneration.Roles;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class RoomBossSelectorTests
    {
        private RoomBossSelector selector;

        [SetUp]
        public void SetUp()
        {
            selector = new RoomBossSelector();
        }

        [Test]
        public void LinearGraph_SelectsFarthestRoom()
        {
            DungeonRoom[] rooms = CreateRooms();
            RoomGraph graph = CreateLinearGraph(rooms);

            int bossId = selector.SelectBossRoomId(
                graph, rooms, 0);

            Assert.AreEqual(4, bossId);
        }

        [Test]
        public void StartAtOppositeEnd_SelectsOtherEndpoint()
        {
            DungeonRoom[] rooms = CreateRooms();
            RoomGraph graph = CreateLinearGraph(rooms);

            int bossId = selector.SelectBossRoomId(
                graph, rooms, 4);

            Assert.AreEqual(0, bossId);
        }

        [Test]
        public void EqualDistances_SelectsSmallestRoomId()
        {
            DungeonRoom[] rooms = CreateRooms();

            var graph = new RoomGraph(rooms);

            graph.AddConnection(new RoomConnection(0, 1, 1));
            graph.AddConnection(new RoomConnection(0, 2, 1));
            graph.AddConnection(new RoomConnection(0, 3, 1));
            graph.AddConnection(new RoomConnection(0, 4, 1));

            // Every leaf is one edge away from Start.
            // The smallest eligible ID is 1.
            int bossId = selector.SelectBossRoomId(
                graph, rooms, 0);

            Assert.AreEqual(1, bossId);
        }

        [Test]
        public void RoomOrder_DoesNotAffectSelection()
        {
            DungeonRoom[] rooms = CreateRooms();

            DungeonRoom[] reversedRooms =
            {
                rooms[4],
                rooms[3],
                rooms[2],
                rooms[1],
                rooms[0]
            };

            RoomGraph firstGraph = CreateLinearGraph(rooms);
            RoomGraph secondGraph =
                CreateLinearGraph(reversedRooms);

            int firstResult = selector.SelectBossRoomId(
                firstGraph, rooms, 0);

            int secondResult = selector.SelectBossRoomId(
                secondGraph, reversedRooms, 0);

            Assert.AreEqual(4, firstResult);
            Assert.AreEqual(firstResult, secondResult);
        }

        [Test]
        public void TwoRooms_BossIsNotStart()
        {
            var rooms = new[]
            {
                new DungeonRoom(
                    10, new GridPosition(0, 0), 3, 3),

                new DungeonRoom(
                    20, new GridPosition(5, 0), 3, 3)
            };

            var graph = new RoomGraph(rooms);

            graph.AddConnection(
                new RoomConnection(10, 20, 1));

            int bossId = selector.SelectBossRoomId(
                graph, rooms, 10);

            Assert.AreEqual(20, bossId);
        }

        [Test]
        public void DisconnectedGraph_ThrowsException()
        {
            DungeonRoom[] rooms = CreateRooms();
            var graph = new RoomGraph(rooms);

            graph.AddConnection(new RoomConnection(0, 1, 1));
            graph.AddConnection(new RoomConnection(2, 3, 1));

            Assert.Throws<InvalidOperationException>(() =>
                selector.SelectBossRoomId(graph, rooms, 0));
        }

        [Test]
        public void SingleRoom_ThrowsException()
        {
            var rooms = new[]
            {
                new DungeonRoom(
                    7, new GridPosition(0, 0), 3, 3)
            };

            var graph = new RoomGraph(rooms);

            Assert.Throws<ArgumentException>(() =>
                selector.SelectBossRoomId(graph, rooms, 7));
        }

        [Test]
        public void UnknownStartRoom_ThrowsException()
        {
            DungeonRoom[] rooms = CreateRooms();
            RoomGraph graph = CreateLinearGraph(rooms);

            Assert.Throws<ArgumentException>(() =>
                selector.SelectBossRoomId(graph, rooms, 999));
        }

        [Test]
        public void NullGraph_ThrowsException()
        {
            DungeonRoom[] rooms = CreateRooms();

            Assert.Throws<ArgumentNullException>(() =>
                selector.SelectBossRoomId(null, rooms, 0));
        }

        [Test]
        public void NullRooms_ThrowsException()
        {
            DungeonRoom[] rooms = CreateRooms();
            RoomGraph graph = CreateLinearGraph(rooms);

            Assert.Throws<ArgumentNullException>(() =>
                selector.SelectBossRoomId(graph, null, 0));
        }

        private static RoomGraph CreateLinearGraph(
            DungeonRoom[] rooms)
        {
            var graph = new RoomGraph(rooms);

            for (int i = 0; i < 4; i++)
            {
                graph.AddConnection(
                    new RoomConnection(i, i + 1, 1));
            }

            return graph;
        }

        private static DungeonRoom[] CreateRooms()
        {
            return new[]
            {
                new DungeonRoom(
                    0, new GridPosition(0, 0), 3, 3),

                new DungeonRoom(
                    1, new GridPosition(5, 0), 3, 3),

                new DungeonRoom(
                    2, new GridPosition(10, 0), 3, 3),

                new DungeonRoom(
                    3, new GridPosition(15, 0), 3, 3),

                new DungeonRoom(
                    4, new GridPosition(20, 0), 3, 3)
            };
        }
    }
}
