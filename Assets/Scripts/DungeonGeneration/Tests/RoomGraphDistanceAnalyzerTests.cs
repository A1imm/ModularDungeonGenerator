
using System;
using DungeonGeneration.Core;
using DungeonGeneration.Roles;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class RoomGraphDistanceAnalyzerTests
    {
        private RoomGraphDistanceAnalyzer analyzer;

        [SetUp]
        public void SetUp()
        {
            analyzer = new RoomGraphDistanceAnalyzer();
        }

        [Test]
        public void CalculateDistances_StartRoomHasDistanceZero()
        {
            RoomGraph graph = CreateGraph();

            var distances = analyzer.CalculateDistances(graph, 0);

            Assert.AreEqual(0, distances[0]);
        }

        [Test]
        public void CalculateDistances_ReturnsShortestPaths()
        {
            RoomGraph graph = CreateGraph();

            var distances = analyzer.CalculateDistances(graph, 0);

            Assert.AreEqual(0, distances[0]);
            Assert.AreEqual(1, distances[1]);
            Assert.AreEqual(1, distances[2]);
            Assert.AreEqual(2, distances[3]);
            Assert.AreEqual(3, distances[4]);
        }

        [Test]
        public void GetDistance_ReturnsCorrectDistance()
        {
            RoomGraph graph = CreateGraph();

            int distance = analyzer.GetDistance(
                graph,
                0,
                4);

            Assert.AreEqual(3, distance);
        }

        [Test]
        public void GetFarthestRoomId_ReturnsMostDistantRoom()
        {
            RoomGraph graph = CreateGraph();

            int farthest = analyzer.GetFarthestRoomId(
                graph,
                0);

            Assert.AreEqual(4, farthest);
        }

        [Test]
        public void GetFarthestRoomId_TieUsesSmallestId()
        {
            var rooms = new[]
            {
        new DungeonRoom(0, new GridPosition(0, 0), 3, 3),
        new DungeonRoom(1, new GridPosition(5, 0), 3, 3),
        new DungeonRoom(2, new GridPosition(0, 5), 3, 3)
            };

            var graph = new RoomGraph(rooms);

            graph.AddConnection(new RoomConnection(0, 1, 1));
            graph.AddConnection(new RoomConnection(0, 2, 1));

            int farthest = analyzer.GetFarthestRoomId(graph, 0);

            Assert.AreEqual(1, farthest);
        }

        [Test]
        public void CalculateDistances_NullGraph_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                analyzer.CalculateDistances(null, 0));
        }

        [Test]
        public void CalculateDistances_UnknownStart_Throws()
        {
            RoomGraph graph = CreateGraph();

            Assert.Throws<ArgumentException>(() =>
                analyzer.CalculateDistances(graph, 999));
        }

        private static RoomGraph CreateGraph(
            bool includeLastConnection = true)
        {
            var rooms = new[]
            {
                new DungeonRoom(0, new GridPosition(0, 0), 3, 3),
                new DungeonRoom(1, new GridPosition(5, 0), 3, 3),
                new DungeonRoom(2, new GridPosition(0, 5), 3, 3),
                new DungeonRoom(3, new GridPosition(5, 5), 3, 3),
                new DungeonRoom(4, new GridPosition(10, 5), 3, 3)
            };

            var graph = new RoomGraph(rooms);

            graph.AddConnection(
                new RoomConnection(0, 1, 1));

            graph.AddConnection(
                new RoomConnection(0, 2, 1));

            graph.AddConnection(
                new RoomConnection(1, 3, 1));

            if (includeLastConnection)
            {
                graph.AddConnection(
                    new RoomConnection(3, 4, 1));
            }

            return graph;
        }
    }
}
