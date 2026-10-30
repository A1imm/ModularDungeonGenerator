
using System;
using System.Collections.Generic;
using DungeonGeneration.Core;

namespace DungeonGeneration.Roles
{
    public sealed class RoomStartSelector
    {
        private readonly RoomGraphDistanceAnalyzer distanceAnalyzer =
            new RoomGraphDistanceAnalyzer();

        public int SelectStartRoomId(
            RoomGraph graph,
            IReadOnlyList<DungeonRoom> rooms)
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

                if (graph.GetRoom(room.Id) == null)
                {
                    throw new ArgumentException(
                        $"Room {room.Id} does not exist in the graph.",
                        nameof(rooms));
                }

                if (!roomIds.Add(room.Id))
                {
                    throw new ArgumentException(
                        $"Duplicate room ID: {room.Id}.",
                        nameof(rooms));
                }
            }

            if (roomIds.Count == 0)
            {
                throw new ArgumentException(
                    "Cannot select a start room from an empty collection.",
                    nameof(rooms));
            }

            int selectedRoomId = -1;
            int highestEccentricity = -1;

            foreach (int roomId in roomIds)
            {
                Dictionary<int, int> distances =
                    distanceAnalyzer.CalculateDistances(
                        graph,
                        roomId);

                if (distances.Count != roomIds.Count)
                {
                    throw new InvalidOperationException(
                        "The room graph is disconnected or " +
                        "the room list does not match the graph.");
                }

                int eccentricity = 0;

                foreach (int distance in distances.Values)
                {
                    if (distance > eccentricity)
                        eccentricity = distance;
                }

                if (eccentricity > highestEccentricity)
                {
                    highestEccentricity = eccentricity;
                    selectedRoomId = roomId;
                }
            }

            return selectedRoomId;
        }
    }
}
