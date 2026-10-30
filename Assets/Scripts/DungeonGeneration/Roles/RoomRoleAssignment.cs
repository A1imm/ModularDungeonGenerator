
using System;

namespace DungeonGeneration.Roles
{
    public sealed class RoomRoleAssignment :
        IEquatable<RoomRoleAssignment>
    {
        public int RoomId { get; }
        public RoomRole Role { get; }

        public RoomRoleAssignment(int roomId, RoomRole role)
        {
            if (roomId < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(roomId),
                    "Room ID cannot be negative.");
            }

            if (!Enum.IsDefined(typeof(RoomRole), role))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(role),
                    "Unsupported room role.");
            }

            RoomId = roomId;
            Role = role;
        }

        public bool Equals(RoomRoleAssignment other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return RoomId == other.RoomId &&
                   Role == other.Role;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RoomRoleAssignment);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (RoomId * 397) ^ (int)Role;
            }
        }

        public override string ToString()
        {
            return $"Room {RoomId}: {Role}";
        }
    }
}
