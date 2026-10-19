
using System;
using System.Collections.Generic;
using DungeonGeneration.Core;

namespace DungeonGeneration.Generation
{
    public sealed class RoomLoopGenerator
    {
        public RoomGraph AddLoops(
            RoomGraph candidateGraph,
            RoomGraph mst,
            IReadOnlyList<DungeonRoom> rooms,
            int seed,
            double loopProbability,
            int maxCandidatesPerRoom = 3)
        {
            if (candidateGraph == null)
                throw new ArgumentNullException(nameof(candidateGraph));

            if (mst == null)
                throw new ArgumentNullException(nameof(mst));

            if (rooms == null)
                throw new ArgumentNullException(nameof(rooms));

            if (loopProbability < 0.0 ||
                loopProbability > 1.0 ||
                double.IsNaN(loopProbability))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(loopProbability));
            }

            if (maxCandidatesPerRoom <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(maxCandidatesPerRoom));

            var result = new RoomGraph(rooms);

            if (candidateGraph.RoomCount != result.RoomCount ||
                mst.RoomCount != result.RoomCount)
            {
                throw new ArgumentException(
                    "Room graph vertex counts do not match.");
            }

            // Copy all MST connections.
            foreach (RoomConnection edge in mst.Connections)
            {
                result.AddConnection(edge);
            }

            // Collect possible additional connections.
            var candidates = new List<RoomConnection>();

            foreach (RoomConnection edge in candidateGraph.Connections)
            {
                if (!result.AreConnected(
                    edge.RoomAId,
                    edge.RoomBId))
                {
                    candidates.Add(edge);
                }
            }

            // Stable order for deterministic selection.
            candidates.Sort((a, b) =>
            {
                int comparison = a.Weight.CompareTo(b.Weight);

                if (comparison != 0)
                    return comparison;

                comparison = a.RoomAId.CompareTo(b.RoomAId);

                return comparison != 0
                    ? comparison
                    : a.RoomBId.CompareTo(b.RoomBId);
            });

            var random = new Random(seed);
            var candidateCounts = new Dictionary<int, int>();

            foreach (DungeonRoom room in rooms)
            {
                candidateCounts.Add(room.Id, 0);
            }

            foreach (RoomConnection edge in candidates)
            {
                if (candidateCounts[edge.RoomAId] >= maxCandidatesPerRoom ||
                    candidateCounts[edge.RoomBId] >= maxCandidatesPerRoom)
                {
                    continue;
                }

                candidateCounts[edge.RoomAId]++;
                candidateCounts[edge.RoomBId]++;

                if (random.NextDouble() < loopProbability)
                {
                    result.AddConnection(edge);
                }
            }

            return result;
        }
    }
}
