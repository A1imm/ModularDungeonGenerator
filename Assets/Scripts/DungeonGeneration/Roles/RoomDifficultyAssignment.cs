
using System;

namespace DungeonGeneration.Roles
{
    public sealed class RoomDifficultyAssignment :
        IEquatable<RoomDifficultyAssignment>
    {
        public int RoomId { get; }
        public int DistanceFromStart { get; }
        public int DifficultyLevel { get; }

        public RoomDifficultyAssignment(
            int roomId,
            int distanceFromStart,
            int difficultyLevel)
        {
            if (roomId < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(roomId));
            }

            if (distanceFromStart < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(distanceFromStart));
            }

            if (difficultyLevel < 1 || difficultyLevel > 5)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(difficultyLevel),
                    "Difficulty must be between 1 and 5.");
            }

            RoomId = roomId;
            DistanceFromStart = distanceFromStart;
            DifficultyLevel = difficultyLevel;
        }

        public bool Equals(RoomDifficultyAssignment other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return RoomId == other.RoomId &&
                   DistanceFromStart == other.DistanceFromStart &&
                   DifficultyLevel == other.DifficultyLevel;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RoomDifficultyAssignment);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = RoomId;
                hash = hash * 397 ^ DistanceFromStart;
                hash = hash * 397 ^ DifficultyLevel;
                return hash;
            }
        }

        public override string ToString()
        {
            return
                $"Room {RoomId}: " +
                $"Distance {DistanceFromStart}, " +
                $"Difficulty {DifficultyLevel}";
        }
    }
}
