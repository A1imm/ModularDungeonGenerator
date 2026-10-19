
using System;
using System.Collections.Generic;

namespace DungeonGeneration.Core
{
    public sealed class RoomGraph
    {
        private readonly Dictionary<int, DungeonRoom> rooms;
        private readonly Dictionary<int, HashSet<int>> adjacency;
        private readonly List<RoomConnection> connections;
        private readonly HashSet<RoomConnection> connectionSet;

        public int RoomCount => rooms.Count;
        public int ConnectionCount => connections.Count;

        public IReadOnlyList<RoomConnection> Connections =>
            connections.AsReadOnly();

        public RoomGraph(IEnumerable<DungeonRoom> dungeonRooms)
        {
            if (dungeonRooms == null)
                throw new ArgumentNullException(nameof(dungeonRooms));

            rooms = new Dictionary<int, DungeonRoom>();
            adjacency = new Dictionary<int, HashSet<int>>();
            connections = new List<RoomConnection>();
            connectionSet = new HashSet<RoomConnection>();

            foreach (DungeonRoom room in dungeonRooms)
            {
                if (room == null)
                    throw new ArgumentException(
                        "Room collection contains null.");

                if (rooms.ContainsKey(room.Id))
                    throw new ArgumentException(
                        $"Duplicate room ID: {room.Id}");

                rooms.Add(room.Id, room);
                adjacency.Add(room.Id, new HashSet<int>());
            }
        }

        public DungeonRoom GetRoom(int roomId)
        {
            if (!rooms.TryGetValue(roomId, out DungeonRoom room))
                throw new ArgumentException(
                    $"Room {roomId} does not exist.");

            return room;
        }

        public bool AddConnection(RoomConnection connection)
        {
            if (!rooms.ContainsKey(connection.RoomAId) ||
                !rooms.ContainsKey(connection.RoomBId))
            {
                throw new ArgumentException(
                    "Connection references an unknown room.");
            }

            if (!connectionSet.Add(connection))
                return false;

            connections.Add(connection);

            adjacency[connection.RoomAId].Add(connection.RoomBId);
            adjacency[connection.RoomBId].Add(connection.RoomAId);

            return true;
        }

        public bool AreConnected(int roomAId, int roomBId)
        {
            ValidateRoom(roomAId);
            ValidateRoom(roomBId);

            return adjacency[roomAId].Contains(roomBId);
        }

        public IReadOnlyCollection<int> GetNeighbors(int roomId)
        {
            ValidateRoom(roomId);

            return new List<int>(adjacency[roomId]).AsReadOnly();
        }

        private void ValidateRoom(int roomId)
        {
            if (!rooms.ContainsKey(roomId))
                throw new ArgumentException(
                    $"Room {roomId} does not exist.");
        }
    }
}
