
using System;
using System.Collections.Generic;
using DungeonGeneration.Core;

namespace DungeonGeneration.Generation
{
    public sealed class RoomGraphBuilder
    {
        public RoomGraph BuildCompleteGraph(
            IReadOnlyList<DungeonRoom> rooms)
        {
            if (rooms == null)
                throw new ArgumentNullException(nameof(rooms));

            // Validate rooms and register all vertices.
            var graph = new RoomGraph(rooms);

            for (int i = 0; i < rooms.Count; i++)
            {
                for (int j = i + 1; j < rooms.Count; j++)
                {
                    DungeonRoom roomA = rooms[i];
                    DungeonRoom roomB = rooms[j];

                    long weight = CalculateSquaredDistance(
                        roomA.Center,
                        roomB.Center
                    );

                    var connection = new RoomConnection(
                        roomA.Id,
                        roomB.Id,
                        weight
                    );

                    graph.AddConnection(connection);
                }
            }

            return graph;
        }

        private static long CalculateSquaredDistance(
            GridPosition a,
            GridPosition b)
        {
            long dx = (long)a.X - b.X;
            long dy = (long)a.Y - b.Y;

            return checked(dx * dx + dy * dy);
        }
    }
}
