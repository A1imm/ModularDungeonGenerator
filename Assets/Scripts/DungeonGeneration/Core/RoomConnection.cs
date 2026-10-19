
using System;

namespace DungeonGeneration.Core
{
    public readonly struct RoomConnection : IEquatable<RoomConnection>
    {
        public int RoomAId { get; }
        public int RoomBId { get; }
        public long Weight { get; }

        public RoomConnection(int roomAId, int roomBId, long weight)
        {
            if (roomAId < 0)
                throw new ArgumentOutOfRangeException(nameof(roomAId));

            if (roomBId < 0)
                throw new ArgumentOutOfRangeException(nameof(roomBId));

            if (roomAId == roomBId)
                throw new ArgumentException("A room cannot connect to itself.");

            if (weight < 0)
                throw new ArgumentOutOfRangeException(nameof(weight));

            // Normalize identifiers for an undirected edge.
            RoomAId = Math.Min(roomAId, roomBId);
            RoomBId = Math.Max(roomAId, roomBId);
            Weight = weight;
        }

        public bool Equals(RoomConnection other)
        {
            return RoomAId == other.RoomAId &&
                   RoomBId == other.RoomBId;
        }

        public override bool Equals(object obj)
        {
            return obj is RoomConnection other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(RoomAId, RoomBId);
        }

        public static bool operator ==(
            RoomConnection left, RoomConnection right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            RoomConnection left, RoomConnection right)
        {
            return !left.Equals(right);
        }
    }
}
