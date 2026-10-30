
using System;
using System.Collections.Generic;
using DungeonGeneration.Core;

namespace DungeonGeneration.Roles
{
    public sealed class RoomBossSelector
    {
        private readonly RoomGraphDistanceAnalyzer distanceAnalyzer =
            new RoomGraphDistanceAnalyzer();

        public int SelectBossRoomId(
            RoomGraph graph,
            IReadOnlyList<DungeonRoom> rooms,
            int startRoomId)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            if (rooms == null)
                throw new ArgumentNullException(nameof(rooms));

            if (rooms.Count < 2)
            {
                throw new ArgumentException(
                    "Boss selection requires at least two rooms.",
                    nameof(rooms));
            }

            var roomIds = new HashSet<int>();

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
                        $"Room {room.Id} does not exist in the graph.",
                        nameof(rooms));
                }
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
                    "Cannot select a boss room from a " +
                    "disconnected or inconsistent room graph.");
            }

            int selectedBossId = -1;
            int maximumDistance = -1;

            foreach (int roomId in roomIds)
            {
                if (roomId == startRoomId)
                    continue;

                int distance = distances[roomId];

                if (distance > maximumDistance ||
                    (distance == maximumDistance &&
                     roomId < selectedBossId))
                {
                    maximumDistance = distance;
                    selectedBossId = roomId;
                }
            }

            return selectedBossId;
        }
    }
}
