
using System;
using System.Collections.Generic;
using DungeonGeneration.Core;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class RoomGraphTests
    {
        private RoomGraph CreateGraph()
        {
            return new RoomGraph(new[]
            {
                new DungeonRoom(0, new GridPosition(0, 0), 3, 3),
                new DungeonRoom(1, new GridPosition(5, 0), 3, 3),
                new DungeonRoom(2, new GridPosition(10, 0), 3, 3)
            });
        }

        [Test]
        public void Constructor_StoresAllRooms()
        {
            var graph = CreateGraph();

            Assert.AreEqual(3, graph.RoomCount);
            Assert.AreEqual(0, graph.ConnectionCount);
            Assert.AreEqual(1, graph.GetRoom(1).Id);
        }

        [Test]
        public void AddConnection_ConnectsBothDirections()
        {
            var graph = CreateGraph();

            bool added = graph.AddConnection(
                new RoomConnection(0, 1, 25));

            Assert.IsTrue(added);
            Assert.IsTrue(graph.AreConnected(0, 1));
            Assert.IsTrue(graph.AreConnected(1, 0));
            Assert.AreEqual(1, graph.ConnectionCount);
        }

        [Test]
        public void AddConnection_DuplicateIsRejected()
        {
            var graph = CreateGraph();

            graph.AddConnection(new RoomConnection(0, 1, 25));

            bool added = graph.AddConnection(
                new RoomConnection(1, 0, 50));

            Assert.IsFalse(added);
            Assert.AreEqual(1, graph.ConnectionCount);
            Assert.AreEqual(25L, graph.Connections[0].Weight);
        }

        [Test]
        public void GetNeighbors_ReturnsDirectNeighbors()
        {
            var graph = CreateGraph();

            graph.AddConnection(new RoomConnection(0, 1, 25));
            graph.AddConnection(new RoomConnection(1, 2, 25));

            CollectionAssert.AreEquivalent(
                new[] { 0, 2 },
                graph.GetNeighbors(1)
            );

            Assert.IsFalse(graph.AreConnected(0, 2));
        }

        [Test]
        public void AddConnection_UnknownRoom_ThrowsException()
        {
            var graph = CreateGraph();

            Assert.Throws<ArgumentException>(() =>
                graph.AddConnection(
                    new RoomConnection(0, 99, 100)));
        }

        [Test]
        public void Constructor_DuplicateRoomIds_ThrowsException()
        {
            var rooms = new List<DungeonRoom>
            {
                new DungeonRoom(0, new GridPosition(0, 0), 3, 3),
                new DungeonRoom(0, new GridPosition(5, 0), 3, 3)
            };

            Assert.Throws<ArgumentException>(
                () => new RoomGraph(rooms));
        }

        [Test]
        public void RoomConnection_ReversedEndpoints_AreEqual()
        {
            var a = new RoomConnection(0, 2, 100);
            var b = new RoomConnection(2, 0, 100);

            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }
    }
}
