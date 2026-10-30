
using System;
using System.Collections.Generic;
using DungeonGeneration.Core;

namespace DungeonGeneration.Roles
{
    public sealed class RoomDifficultyAnalyzer
    {
        public const int MinimumDifficulty = 1;
        public const int MaximumDifficulty = 5;

        private readonly RoomGraphDistanceAnalyzer distanceAnalyzer =
            new RoomGraphDistanceAnalyzer();

        public IReadOnlyList<RoomDifficultyAssignment> Analyze(
            RoomGraph graph,
            IReadOnlyList<DungeonRoom> rooms,
            int startRoomId)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            if (rooms == null)
                throw new ArgumentNullException(nameof(rooms));

            var roomIds = new SortedSet<int>();

            foreach (DungeonRoom room in rooms)
            {
                if (room == null)
                {
                    throw new ArgumentException(
                        "Room collection contains null.",
                        nameof(rooms));
                }

                if (!roomIds.Add(room.Id))
                {
                    throw new ArgumentException(
                        $"Duplicate room ID: {room.Id}.",
                        nameof(rooms));
                }

                if (graph.GetRoom(room.Id) == null)
                {
                    throw new ArgumentException(
                        $"Room {room.Id} does not exist in graph.",
                        nameof(rooms));
                }
            }

            if (roomIds.Count == 0)
            {
                throw new ArgumentException(
                    "At least one room is required.",
                    nameof(rooms));
            }

            if (!roomIds.Contains(startRoomId))
            {
                throw new ArgumentException(
                    $"Start room {startRoomId} does not exist.",
                    nameof(startRoomId));
            }

            Dictionary<int, int> distances =
                distanceAnalyzer.CalculateDistances(
                    graph,
                    startRoomId);

            if (distances.Count != roomIds.Count)
            {
                throw new InvalidOperationException(
                    "The room graph is disconnected or " +
                    "the room collection is inconsistent.");
            }

            foreach (int roomId in distances.Keys)
            {
                if (!roomIds.Contains(roomId))
                {
                    throw new InvalidOperationException(
                        "The graph contains a reachable room " +
                        "missing from the room collection.");
                }
            }

            int maximumDistance = 0;

            foreach (int distance in distances.Values)
            {
                if (distance > maximumDistance)
                    maximumDistance = distance;
            }

            var assignments =
                new List<RoomDifficultyAssignment>(
                    roomIds.Count);

            foreach (int roomId in roomIds)
            {
                int distance = distances[roomId];

                int difficulty = CalculateDifficulty(
                    distance,
                    maximumDistance);

                assignments.Add(
                    new RoomDifficultyAssignment(
                        roomId,
                        distance,
                        difficulty));
            }

            return assignments.AsReadOnly();
        }

        private static int CalculateDifficulty(
            int distance,
            int maximumDistance)
        {
            // A single-room graph has maximumDistance == 0.
            if (maximumDistance == 0)
                return MinimumDifficulty;

            long scaledDistance =
                (long)distance *
                (MaximumDifficulty - MinimumDifficulty);

            return MinimumDifficulty +
                   (int)(scaledDistance / maximumDistance);
        }
    }
}
