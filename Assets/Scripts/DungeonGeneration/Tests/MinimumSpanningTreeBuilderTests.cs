
using System;
using DungeonGeneration.Core;
using DungeonGeneration.Generation;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class MinimumSpanningTreeBuilderTests
    {
        private DungeonRoom[] CreateRooms()
        {
            return new[]
            {
                new DungeonRoom(0, new GridPosition(0, 0), 3, 3),
                new DungeonRoom(1, new GridPosition(5, 0), 3, 3),
                new DungeonRoom(2, new GridPosition(10, 0), 3, 3),
                new DungeonRoom(3, new GridPosition(15, 0), 3, 3)
            };
        }

        [Test]
        public void Build_UsesExactlyNMinusOneEdges()
        {
            var rooms = CreateRooms();
            var graph = new RoomGraphBuilder()
                .BuildCompleteGraph(rooms);

            var mst = new MinimumSpanningTreeBuilder()
                .Build(graph, rooms);

            Assert.AreEqual(4, mst.RoomCount);
            Assert.AreEqual(3, mst.ConnectionCount);
        }

        [Test]
        public void Build_SelectsMinimumWeightConnections()
        {
            var rooms = CreateRooms();
            var graph = new RoomGraphBuilder()
                .BuildCompleteGraph(rooms);

            var mst = new MinimumSpanningTreeBuilder()
                .Build(graph, rooms);

            long totalWeight = 0;

            foreach (var edge in mst.Connections)
                totalWeight += edge.Weight;

            Assert.AreEqual(75L, totalWeight);
        }

        [Test]
        public void Build_ConnectsAllRooms()
        {
            var rooms = CreateRooms();
            var graph = new RoomGraphBuilder()
                .BuildCompleteGraph(rooms);

            var mst = new MinimumSpanningTreeBuilder()
                .Build(graph, rooms);

            var visited = new System.Collections.Generic.HashSet<int>();
            var queue = new System.Collections.Generic.Queue<int>();

            visited.Add(0);
            queue.Enqueue(0);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();

                foreach (int neighbor in mst.GetNeighbors(current))
                {
                    if (visited.Add(neighbor))
                        queue.Enqueue(neighbor);
                }
            }

            Assert.AreEqual(rooms.Length, visited.Count);
        }

        [Test]
        public void Build_SameInput_ReturnsSameConnections()
        {
            var rooms = CreateRooms();
            var graph = new RoomGraphBuilder()
                .BuildCompleteGraph(rooms);

            var builder = new MinimumSpanningTreeBuilder();

            var first = builder.Build(graph, rooms);
            var second = builder.Build(graph, rooms);

            CollectionAssert.AreEqual(
                first.Connections,
                second.Connections
            );
        }

        [Test]
        public void Build_DisconnectedGraph_ThrowsException()
        {
            var rooms = CreateRooms();
            var graph = new RoomGraph(rooms);

            graph.AddConnection(new RoomConnection(0, 1, 25));
            graph.AddConnection(new RoomConnection(2, 3, 25));

            Assert.Throws<InvalidOperationException>(() =>
                new MinimumSpanningTreeBuilder()
                    .Build(graph, rooms)
            );
        }
    }
}
