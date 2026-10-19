
using System;
using DungeonGeneration.Core;
using DungeonGeneration.Generation;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class RoomLoopGeneratorTests
    {
        private DungeonRoom[] CreateRooms()
        {
            return new[]
            {
                new DungeonRoom(0, new GridPosition(0, 0), 3, 3),
                new DungeonRoom(1, new GridPosition(6, 0), 3, 3),
                new DungeonRoom(2, new GridPosition(0, 6), 3, 3),
                new DungeonRoom(3, new GridPosition(6, 6), 3, 3)
            };
        }

        private void CreateGraphs(
            out DungeonRoom[] rooms,
            out RoomGraph candidates,
            out RoomGraph mst)
        {
            rooms = CreateRooms();

            candidates = new RoomGraphBuilder()
                .BuildCompleteGraph(rooms);

            mst = new MinimumSpanningTreeBuilder()
                .Build(candidates, rooms);
        }

        [Test]
        public void AddLoops_ZeroProbability_PreservesMST()
        {
            CreateGraphs(out var rooms, out var candidates, out var mst);

            var result = new RoomLoopGenerator().AddLoops(
                candidates, mst, rooms, 12345, 0.0);

            Assert.AreEqual(mst.ConnectionCount, result.ConnectionCount);

            CollectionAssert.AreEquivalent(
                mst.Connections, result.Connections);
        }

        [Test]
        public void AddLoops_FullProbability_AddsAdditionalConnections()
        {
            CreateGraphs(out var rooms, out var candidates, out var mst);

            var result = new RoomLoopGenerator().AddLoops(
                candidates, mst, rooms, 12345, 1.0);

            Assert.Greater(
                result.ConnectionCount,
                mst.ConnectionCount);
        }

        [Test]
        public void AddLoops_SameSeed_ReturnsSameConnections()
        {
            CreateGraphs(out var rooms, out var candidates, out var mst);

            var generator = new RoomLoopGenerator();

            var first = generator.AddLoops(
                candidates, mst, rooms, 12345, 0.5);

            var second = generator.AddLoops(
                candidates, mst, rooms, 12345, 0.5);

            CollectionAssert.AreEqual(
                first.Connections, second.Connections);
        }

        [Test]
        public void AddLoops_InvalidProbability_ThrowsException()
        {
            CreateGraphs(out var rooms, out var candidates, out var mst);

            var generator = new RoomLoopGenerator();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                generator.AddLoops(
                    candidates, mst, rooms, 12345, -0.1));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                generator.AddLoops(
                    candidates, mst, rooms, 12345, 1.1));
        }

        [Test]
        public void AddLoops_DoesNotModifyOriginalMST()
        {
            CreateGraphs(out var rooms, out var candidates, out var mst);

            int originalCount = mst.ConnectionCount;

            new RoomLoopGenerator().AddLoops(
                candidates, mst, rooms, 12345, 1.0);

            Assert.AreEqual(originalCount, mst.ConnectionCount);
        }
    }
}
