
using System;
using DungeonGeneration.Core;
using DungeonGeneration.Generation;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class RoomGraphBuilderTests
    {
        private RoomGraphBuilder builder;

        [SetUp]
        public void SetUp()
        {
            builder = new RoomGraphBuilder();
        }

        private DungeonRoom[] CreateRooms()
        {
            return new[]
            {
                new DungeonRoom(
                    0, new GridPosition(0, 0), 3, 3),

                new DungeonRoom(
                    1, new GridPosition(5, 0), 3, 3),

                new DungeonRoom(
                    2, new GridPosition(10, 0), 3, 3)
            };
        }

        [Test]
        public void BuildCompleteGraph_CreatesAllConnections()
        {
            var graph = builder.BuildCompleteGraph(CreateRooms());

            Assert.AreEqual(3, graph.RoomCount);
            Assert.AreEqual(3, graph.ConnectionCount);

            Assert.IsTrue(graph.AreConnected(0, 1));
            Assert.IsTrue(graph.AreConnected(0, 2));
            Assert.IsTrue(graph.AreConnected(1, 2));
        }

        [Test]
        public void BuildCompleteGraph_CalculatesCorrectWeights()
        {
            var graph = builder.BuildCompleteGraph(CreateRooms());

            Assert.AreEqual(25L, graph.Connections[0].Weight);
            Assert.AreEqual(100L, graph.Connections[1].Weight);
            Assert.AreEqual(25L, graph.Connections[2].Weight);
        }

        [Test]
        public void BuildCompleteGraph_SingleRoom_HasNoConnections()
        {
            var rooms = new[]
            {
                new DungeonRoom(
                    0, new GridPosition(0, 0), 3, 3)
            };

            var graph = builder.BuildCompleteGraph(rooms);

            Assert.AreEqual(1, graph.RoomCount);
            Assert.AreEqual(0, graph.ConnectionCount);
        }

        [Test]
        public void BuildCompleteGraph_NullInput_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                builder.BuildCompleteGraph(null));
        }
    }
}
