
using System;
using System.Collections.Generic;
using System.Linq;
using DungeonGeneration.Core;
using DungeonGeneration.Roles;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class RoomDifficultyAnalyzerTests
    {
        private RoomDifficultyAnalyzer analyzer;

        [SetUp]
        public void SetUp()
        {
            analyzer = new RoomDifficultyAnalyzer();
        }

        [Test]
        public void LinearGraph_AssignsIncreasingDifficulty()
        {
            DungeonRoom[] rooms = CreateRooms(5);
            RoomGraph graph = CreateLinearGraph(rooms);

            var result = analyzer.Analyze(graph, rooms, 0);

            Assert.AreEqual(5, result.Count);

            for (int i = 0; i < 5; i++)
            {
                RoomDifficultyAssignment assignment =
                    GetAssignment(result, i);

                Assert.AreEqual(i, assignment.DistanceFromStart);
                Assert.AreEqual(i + 1, assignment.DifficultyLevel);
            }
        }

        [Test]
        public void OppositeStart_ReversesDifficulty()
        {
            DungeonRoom[] rooms = CreateRooms(5);
            RoomGraph graph = CreateLinearGraph(rooms);

            var result = analyzer.Analyze(graph, rooms, 4);

            Assert.AreEqual(1, GetAssignment(result, 4).DifficultyLevel);
            Assert.AreEqual(3, GetAssignment(result, 2).DifficultyLevel);
            Assert.AreEqual(5, GetAssignment(result, 0).DifficultyLevel);
        }

        [Test]
        public void SameDistance_ProducesSameDifficulty()
        {
            DungeonRoom[] rooms = CreateRooms(5);
            var graph = new RoomGraph(rooms);

            graph.AddConnection(new RoomConnection(0, 1, 1));
            graph.AddConnection(new RoomConnection(0, 2, 1));
            graph.AddConnection(new RoomConnection(1, 3, 1));
            graph.AddConnection(new RoomConnection(2, 4, 1));

            var result = analyzer.Analyze(graph, rooms, 0);

            Assert.AreEqual(1, GetAssignment(result, 0).DifficultyLevel);

            Assert.AreEqual(
                GetAssignment(result, 1).DifficultyLevel,
                GetAssignment(result, 2).DifficultyLevel);

            Assert.AreEqual(
                GetAssignment(result, 3).DifficultyLevel,
                GetAssignment(result, 4).DifficultyLevel);

            Assert.AreEqual(5, GetAssignment(result, 3).DifficultyLevel);
        }

        [Test]
        public void ShortGraph_NormalizesDifficultyRange()
        {
            DungeonRoom[] rooms = CreateRooms(3);
            RoomGraph graph = CreateLinearGraph(rooms);

            var result = analyzer.Analyze(graph, rooms, 0);

            Assert.AreEqual(1, GetAssignment(result, 0).DifficultyLevel);
            Assert.AreEqual(3, GetAssignment(result, 1).DifficultyLevel);
            Assert.AreEqual(5, GetAssignment(result, 2).DifficultyLevel);
        }

        [Test]
        public void SingleRoom_HasMinimumDifficulty()
        {
            DungeonRoom[] rooms = CreateRooms(1);
            var graph = new RoomGraph(rooms);

            var result = analyzer.Analyze(graph, rooms, 0);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0, result[0].DistanceFromStart);
            Assert.AreEqual(1, result[0].DifficultyLevel);
        }

        [Test]
        public void ReversedRoomOrder_DoesNotChangeResults()
        {
            DungeonRoom[] rooms = CreateRooms(5);
            RoomGraph graph = CreateLinearGraph(rooms);

            var first = analyzer.Analyze(graph, rooms, 0);
            var second = analyzer.Analyze(
                graph,
                rooms.Reverse().ToArray(),
                0);

            CollectionAssert.AreEqual(first, second);
        }

        [Test]
        public void DisconnectedGraph_ThrowsException()
        {
            DungeonRoom[] rooms = CreateRooms(4);
            var graph = new RoomGraph(rooms);

            graph.AddConnection(new RoomConnection(0, 1, 1));
            graph.AddConnection(new RoomConnection(2, 3, 1));

            Assert.Throws<InvalidOperationException>(() =>
                analyzer.Analyze(graph, rooms, 0));
        }

        [Test]
        public void UnknownStart_ThrowsException()
        {
            DungeonRoom[] rooms = CreateRooms(3);
            RoomGraph graph = CreateLinearGraph(rooms);

            Assert.Throws<ArgumentException>(() =>
                analyzer.Analyze(graph, rooms, 999));
        }

        [Test]
        public void InvalidInputs_ThrowExceptions()
        {
            DungeonRoom[] rooms = CreateRooms(3);
            RoomGraph graph = CreateLinearGraph(rooms);

            Assert.Throws<ArgumentNullException>(() =>
                analyzer.Analyze(null, rooms, 0));

            Assert.Throws<ArgumentNullException>(() =>
                analyzer.Analyze(graph, null, 0));

            Assert.Throws<ArgumentException>(() =>
                analyzer.Analyze(
                    graph,
                    new[] { rooms[0], rooms[0], rooms[2] },
                    0));
        }

        private static RoomDifficultyAssignment GetAssignment(
            IReadOnlyList<RoomDifficultyAssignment> assignments,
            int roomId)
        {
            return assignments.Single(a => a.RoomId == roomId);
        }

        private static DungeonRoom[] CreateRooms(int count)
        {
            var rooms = new DungeonRoom[count];

            for (int i = 0; i < count; i++)
            {
                rooms[i] = new DungeonRoom(
                    i,
                    new GridPosition(i * 5, 0),
                    3,
                    3);
            }

            return rooms;
        }

        private static RoomGraph CreateLinearGraph(
            DungeonRoom[] rooms)
        {
            var graph = new RoomGraph(rooms);

            for (int i = 0; i < rooms.Length - 1; i++)
            {
                graph.AddConnection(
                    new RoomConnection(
                        rooms[i].Id,
                        rooms[i + 1].Id,
                        1));
            }

            return graph;
        }
    }
}
