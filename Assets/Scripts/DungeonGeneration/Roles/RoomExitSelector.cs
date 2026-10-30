
using System;
using System.Collections.Generic;
using DungeonGeneration.Core;

namespace DungeonGeneration.Roles
{
    public sealed class RoomExitSelector
    {
        private readonly RoomGraphDistanceAnalyzer distanceAnalyzer =
            new RoomGraphDistanceAnalyzer();

        public int SelectExitRoomId(
            RoomGraph graph,
            IReadOnlyList<DungeonRoom> rooms,
            int startRoomId,
            int bossRoomId)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            if (rooms == null)
                throw new ArgumentNullException(nameof(rooms));

            if (rooms.Count < 3)
            {
                throw new ArgumentException(
                    "Exit selection requires at least three rooms.",
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

            if (!roomIds.Contains(bossRoomId))
            {
                throw new ArgumentException(
                    $"Boss room {bossRoomId} does not exist.",
                    nameof(bossRoomId));
            }

            if (startRoomId == bossRoomId)
            {
                throw new ArgumentException(
                    "Start and Boss must be different rooms.");
            }

            Dictionary<int, int> distances =
                distanceAnalyzer.CalculateDistances(
                    graph,
                    bossRoomId);

            if (distances.Count != roomIds.Count)
            {
                throw new InvalidOperationException(
                    "Cannot select Exit from a disconnected " +
                    "or inconsistent room graph.");
            }

            int selectedExitId = -1;
            int minimumDistance = int.MaxValue;

            foreach (int roomId in roomIds)
            {
                if (roomId == startRoomId ||
                    roomId == bossRoomId)
                {
                    continue;
                }

                int distance = distances[roomId];

                if (distance < minimumDistance ||
                    (distance == minimumDistance &&
                     (selectedExitId == -1 ||
                      roomId < selectedExitId)))
                {
                    minimumDistance = distance;
                    selectedExitId = roomId;
                }
            }

            if (selectedExitId == -1)
            {
                throw new InvalidOperationException(
                    "No valid Exit room could be selected.");
            }

            return selectedExitId;
        }
    }
}
