
using System;
using System.Collections.Generic;
using DungeonGeneration.Core;

namespace DungeonGeneration.Generation
{
    public sealed class MinimumSpanningTreeBuilder
    {
        public RoomGraph Build(
            RoomGraph candidateGraph,
            IReadOnlyList<DungeonRoom> rooms)
        {
            if (candidateGraph == null)
                throw new ArgumentNullException(nameof(candidateGraph));

            if (rooms == null)
                throw new ArgumentNullException(nameof(rooms));

            // RoomGraph validates unique identifiers.
            var mst = new RoomGraph(rooms);

            if (mst.RoomCount != candidateGraph.RoomCount)
                throw new ArgumentException(
                    "Room collections do not match.");

            var roomIds = new List<int>();

            foreach (DungeonRoom room in rooms)
            {
                candidateGraph.GetRoom(room.Id);
                roomIds.Add(room.Id);
            }

            var disjointSet = new DisjointSet(roomIds);

            var sortedEdges = new List<RoomConnection>(
                candidateGraph.Connections
            );

            // Stable tie-breaking for deterministic results.
            sortedEdges.Sort((a, b) =>
            {
                int result = a.Weight.CompareTo(b.Weight);

                if (result != 0)
                    return result;

                result = a.RoomAId.CompareTo(b.RoomAId);

                if (result != 0)
                    return result;

                return a.RoomBId.CompareTo(b.RoomBId);
            });

            foreach (RoomConnection edge in sortedEdges)
            {
                if (!disjointSet.Union(edge.RoomAId, edge.RoomBId))
                    continue;

                mst.AddConnection(edge);

                if (mst.ConnectionCount == mst.RoomCount - 1)
                    break;
            }

            if (mst.RoomCount > 0 &&
                mst.ConnectionCount != mst.RoomCount - 1)
            {
                throw new InvalidOperationException(
                    "Candidate graph is disconnected. " +
                    "Cannot build a spanning tree.");
            }

            return mst;
        }
    }
}
