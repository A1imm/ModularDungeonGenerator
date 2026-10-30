
using System;
using DungeonGeneration.Core;
using DungeonGeneration.Roles;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class RoomExitSelectorTests
    {
        private RoomExitSelector selector;

        [SetUp]
        public void SetUp()
        {
            selector = new RoomExitSelector();
        }

        [Test]
        public void LinearGraph_SelectsRoomNextToBoss()
        {
            DungeonRoom[] rooms = CreateRooms();
            RoomGraph graph = CreateLinearGraph(rooms);

            int exitId = selector.SelectExitRoomId(
                graph, rooms, startRoomId: 0, bossRoomId: 4);

            Assert.AreEqual(3, exitId);
        }

        [Test]
        public void Exit_NeverEqualsStartOrBoss()
        {
            DungeonRoom[] rooms = CreateRooms();
            RoomGraph graph = CreateLinearGraph(rooms);

            int startId = 0;
            int bossId = 4;

            int exitId = selector.SelectExitRoomId(
                graph, rooms, startId, bossId);

            Assert.AreNotEqual(startId, exitId);
            Assert.AreNotEqual(bossId, exitId);
        }

        [Test]
        public void EqualDistances_SelectsSmallestRoomId()
        {
            DungeonRoom[] rooms = CreateRooms();
            var graph = new RoomGraph(rooms);

            graph.AddConnection(new RoomConnection(0, 1, 1));
            graph.AddConnection(new RoomConnection(1, 2, 1));
            graph.AddConnection(new RoomConnection(2, 3, 1));
            graph.AddConnection(new RoomConnection(2, 4, 1));

            // Boss = 2.
            // Rooms 1, 3 and 4 are one edge away.
            // Start = 0, so all three remain eligible.
            // Room 1 has the smallest ID.
            int exitId = selector.SelectExitRoomId(
                graph, rooms, 0, 2);

            Assert.AreEqual(1, exitId);
        }

        [Test]
        public void StartRoom_IsExcludedEvenWhenClosest()
        {
            var rooms = new[]
            {
                CreateRoom(0),
                CreateRoom(1),
                CreateRoom(2)
            };

            var graph = new RoomGraph(rooms);

            graph.AddConnection(new RoomConnection(0, 1, 1));
            graph.AddConnection(new RoomConnection(0, 2, 1));

            // Boss = 1, Start = 0.
            // Start is closest, but must be excluded.
            int exitId = selector.SelectExitRoomId(
                graph, rooms, 0, 1);

            Assert.AreEqual(2, exitId);
        }

        [Test]
        public void RoomOrder_DoesNotChangeExitSelection()
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
            RoomGraph secondGraph = CreateLinearGraph(reversedRooms);

            int firstExit = selector.SelectExitRoomId(
                firstGraph, rooms, 0, 4);

            int secondExit = selector.SelectExitRoomId(
                secondGraph, reversedRooms, 0, 4);

            Assert.AreEqual(firstExit, secondExit);
            Assert.AreEqual(3, secondExit);
        }

        [Test]
        public void NonConsecutiveIds_AreSupported()
        {
            var rooms = new[]
            {
                CreateRoom(10),
                CreateRoom(20),
                CreateRoom(30)
            };

            var graph = new RoomGraph(rooms);

            graph.AddConnection(new RoomConnection(10, 20, 1));
            graph.AddConnection(new RoomConnection(20, 30, 1));

            int exitId = selector.SelectExitRoomId(
                graph, rooms, 10, 30);

            Assert.AreEqual(20, exitId);
        }

        [Test]
        public void TwoRooms_ThrowsException()
        {
            var rooms = new[]
            {
                CreateRoom(0),
                CreateRoom(1)
            };

            var graph = new RoomGraph(rooms);

            graph.AddConnection(new RoomConnection(0, 1, 1));

            Assert.Throws<ArgumentException>(() =>
                selector.SelectExitRoomId(graph, rooms, 0, 1));
        }

        [Test]
        public void SameStartAndBoss_ThrowsException()
        {
            DungeonRoom[] rooms = CreateRooms();
            RoomGraph graph = CreateLinearGraph(rooms);

            Assert.Throws<ArgumentException>(() =>
                selector.SelectExitRoomId(graph, rooms, 0, 0));
        }

        [Test]
        public void UnknownStart_ThrowsException()
        {
            DungeonRoom[] rooms = CreateRooms();
            RoomGraph graph = CreateLinearGraph(rooms);

            Assert.Throws<ArgumentException>(() =>
                selector.SelectExitRoomId(graph, rooms, 999, 4));
        }

        [Test]
        public void UnknownBoss_ThrowsException()
        {
            DungeonRoom[] rooms = CreateRooms();
            RoomGraph graph = CreateLinearGraph(rooms);

            Assert.Throws<ArgumentException>(() =>
                selector.SelectExitRoomId(graph, rooms, 0, 999));
        }

        [Test]
        public void DisconnectedGraph_ThrowsException()
        {
            DungeonRoom[] rooms = CreateRooms();
            var graph = new RoomGraph(rooms);

            graph.AddConnection(new RoomConnection(0, 1, 1));
            graph.AddConnection(new RoomConnection(2, 3, 1));

            Assert.Throws<InvalidOperationException>(() =>
                selector.SelectExitRoomId(graph, rooms, 0, 1));
        }

        [Test]
        public void NullGraph_ThrowsException()
        {
            DungeonRoom[] rooms = CreateRooms();

            Assert.Throws<ArgumentNullException>(() =>
                selector.SelectExitRoomId(null, rooms, 0, 4));
        }

        [Test]
        public void NullRooms_ThrowsException()
        {
            DungeonRoom[] rooms = CreateRooms();
            RoomGraph graph = CreateLinearGraph(rooms);

            Assert.Throws<ArgumentNullException>(() =>
                selector.SelectExitRoomId(graph, null, 0, 4));
        }

        [Test]
        public void DuplicateRoomIds_ThrowsException()
        {
            DungeonRoom[] rooms = CreateRooms();

            var duplicateRooms = new[]
            {
                rooms[0],
                rooms[1],
                rooms[1],
                rooms[3],
                rooms[4]
            };

            RoomGraph graph = CreateLinearGraph(rooms);

            Assert.Throws<ArgumentException>(() =>
                selector.SelectExitRoomId(
                    graph, duplicateRooms, 0, 4));
        }

        private static RoomGraph CreateLinearGraph(
            DungeonRoom[] rooms)
        {
            var graph = new RoomGraph(rooms);

            for (int id = 0; id < 4; id++)
            {
                graph.AddConnection(
                    new RoomConnection(id, id + 1, 1));
            }

            return graph;
        }

        private static DungeonRoom[] CreateRooms()
        {
            return new[]
            {
                CreateRoom(0),
                CreateRoom(1),
                CreateRoom(2),
                CreateRoom(3),
                CreateRoom(4)
            };
        }

        private static DungeonRoom CreateRoom(int id)
        {
            return new DungeonRoom(
                id,
                new GridPosition(id * 5, 0),
                3,
                3);
        }
    }
}
