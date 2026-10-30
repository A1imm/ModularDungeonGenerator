
using System;
using DungeonGeneration.Core;
using DungeonGeneration.Roles;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class RoomStartSelectorTests
    {
        private RoomStartSelector selector;

        [SetUp]
        public void SetUp()
        {
            selector = new RoomStartSelector();
        }

        [Test]
        public void LinearGraph_SelectsEndpointWithSmallestId()
        {
            DungeonRoom[] rooms = CreateRooms();
            RoomGraph graph = CreateLinearGraph(rooms);

            Assert.AreEqual(
                0,
                selector.SelectStartRoomId(graph, rooms));
        }

        [Test]
        public void StarGraph_SelectsPeripheralRoom()
        {
            DungeonRoom[] rooms = CreateRooms();
            var graph = new RoomGraph(rooms);

            graph.AddConnection(new RoomConnection(0, 1, 1));
            graph.AddConnection(new RoomConnection(0, 2, 1));
            graph.AddConnection(new RoomConnection(0, 3, 1));
            graph.AddConnection(new RoomConnection(0, 4, 1));

            Assert.AreEqual(
                1,
                selector.SelectStartRoomId(graph, rooms));
        }

        [Test]
        public void SingleRoom_SelectsOnlyRoom()
        {
            var rooms = new[]
            {
                new DungeonRoom(
                    7, new GridPosition(0, 0), 3, 3)
            };

            var graph = new RoomGraph(rooms);

            Assert.AreEqual(
                7,
                selector.SelectStartRoomId(graph, rooms));
        }

        [Test]
        public void RoomOrder_DoesNotChangeSelection()
        {
            DungeonRoom[] rooms = CreateRooms();

            var reorderedRooms = new[]
            {
                rooms[4],
                rooms[2],
                rooms[0],
                rooms[3],
                rooms[1]
            };

            RoomGraph firstGraph = CreateLinearGraph(rooms);
            RoomGraph secondGraph =
                CreateLinearGraph(reorderedRooms);

            Assert.AreEqual(
                selector.SelectStartRoomId(firstGraph, rooms),
                selector.SelectStartRoomId(
                    secondGraph, reorderedRooms));

            Assert.AreEqual(
                0,
                selector.SelectStartRoomId(
                    secondGraph, reorderedRooms));
        }

        [Test]
        public void DisconnectedGraph_ThrowsException()
        {
            DungeonRoom[] rooms = CreateRooms();
            var graph = new RoomGraph(rooms);

            graph.AddConnection(new RoomConnection(0, 1, 1));
            graph.AddConnection(new RoomConnection(2, 3, 1));

            Assert.Throws<InvalidOperationException>(() =>
                selector.SelectStartRoomId(graph, rooms));
        }

        [Test]
        public void NullGraph_ThrowsException()
        {
            DungeonRoom[] rooms = CreateRooms();

            Assert.Throws<ArgumentNullException>(() =>
                selector.SelectStartRoomId(null, rooms));
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
