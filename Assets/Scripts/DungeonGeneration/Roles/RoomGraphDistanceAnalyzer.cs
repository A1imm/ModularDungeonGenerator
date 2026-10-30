
using System;
using System.Collections.Generic;
using DungeonGeneration.Core;

namespace DungeonGeneration.Roles
{
    public sealed class RoomGraphDistanceAnalyzer
    {
        public Dictionary<int, int> CalculateDistances(
            RoomGraph graph,
            int startRoomId)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            // Ensure that the requested starting room exists.
            if (graph.GetRoom(startRoomId) == null)
            {
                throw new ArgumentException(
                    $"Room with ID {startRoomId} does not exist.",
                    nameof(startRoomId));
            }

            var distances = new Dictionary<int, int>();
            var queue = new Queue<int>();

            distances.Add(startRoomId, 0);
            queue.Enqueue(startRoomId);

            while (queue.Count > 0)
            {
                int currentRoomId = queue.Dequeue();
                int currentDistance = distances[currentRoomId];

                foreach (int neighborId in
                         graph.GetNeighbors(currentRoomId))
                {
                    if (distances.ContainsKey(neighborId))
                        continue;

                    distances.Add(
                        neighborId,
                        currentDistance + 1);

                    queue.Enqueue(neighborId);
                }
            }

            return distances;
        }

        public int GetDistance(
            RoomGraph graph,
            int startRoomId,
            int targetRoomId)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            if (graph.GetRoom(targetRoomId) == null)
            {
                throw new ArgumentException(
                    $"Room with ID {targetRoomId} does not exist.",
                    nameof(targetRoomId));
            }

            Dictionary<int, int> distances =
                CalculateDistances(graph, startRoomId);

            if (!distances.TryGetValue(
                targetRoomId,
                out int distance))
            {
                return -1;
            }

            return distance;
        }

        public int GetFarthestRoomId(
            RoomGraph graph,
            int startRoomId)
        {
            Dictionary<int, int> distances =
                CalculateDistances(graph, startRoomId);

            int farthestRoomId = startRoomId;
            int maximumDistance = 0;

            foreach (KeyValuePair<int, int> entry in distances)
            {
                int roomId = entry.Key;
                int distance = entry.Value;

                if (distance > maximumDistance)
                {
                    maximumDistance = distance;
                    farthestRoomId = roomId;
                }
                else if (distance == maximumDistance &&
                         roomId < farthestRoomId)
                {
                    // Deterministic tie-breaking:
                    // choose the smallest room ID.
                    farthestRoomId = roomId;
                }
            }

            return farthestRoomId;
        }
    }
}
