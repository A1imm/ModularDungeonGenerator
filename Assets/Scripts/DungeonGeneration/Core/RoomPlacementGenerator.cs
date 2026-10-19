
using System;
using System.Collections.Generic;
using DungeonGeneration.Core;

namespace DungeonGeneration.Generation
{
    public sealed class RoomPlacementGenerator
    {
        public List<DungeonRoom> Generate(
            int seed,
            int gridWidth,
            int gridHeight,
            int roomCount,
            int minRoomSize,
            int maxRoomSize,
            int roomSpacing,
            int maxAttempts)
        {
            if (gridWidth <= 0 || gridHeight <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(gridWidth), "Invalid grid dimensions.");

            if (roomCount < 0)
                throw new ArgumentOutOfRangeException(nameof(roomCount));

            if (minRoomSize <= 0 || maxRoomSize < minRoomSize)
                throw new ArgumentOutOfRangeException(nameof(minRoomSize));

            if (roomSpacing < 0)
                throw new ArgumentOutOfRangeException(nameof(roomSpacing));

            if (maxAttempts <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxAttempts));

            var random = new Random(seed);
            var rooms = new List<DungeonRoom>();

            // Largest room must fit, with a margin from grid borders.
            if ((long)maxRoomSize + 2L * roomSpacing > gridWidth ||
                (long)maxRoomSize + 2L * roomSpacing > gridHeight)
            {
                throw new ArgumentException(
                    "Maximum room size does not fit the grid.");
            }

            for (int attempt = 0;
                 attempt < maxAttempts && rooms.Count < roomCount;
                 attempt++)
            {
                int width = random.Next(
                    minRoomSize, maxRoomSize + 1);

                int height = random.Next(
                    minRoomSize, maxRoomSize + 1);

                int x = random.Next(
                    roomSpacing,
                    gridWidth - width - roomSpacing + 1);

                int y = random.Next(
                    roomSpacing,
                    gridHeight - height - roomSpacing + 1);

                var candidate = new DungeonRoom(
                    rooms.Count,
                    new GridPosition(x, y),
                    width,
                    height
                );

                if (CanPlace(candidate, rooms, roomSpacing))
                {
                    rooms.Add(candidate);
                }
            }

            if (rooms.Count != roomCount)
            {
                throw new InvalidOperationException(
                    $"Could only place {rooms.Count} of " +
                    $"{roomCount} requested rooms. " +
                    "Try a larger grid or a higher attempt limit.");
            }

            return rooms;
        }

        private static bool CanPlace(
            DungeonRoom candidate,
            List<DungeonRoom> rooms,
            int spacing)
        {
            foreach (DungeonRoom room in rooms)
            {
                bool separated =
                    (long)candidate.Position.X >=
                        (long)room.Position.X + room.Width + spacing ||
                    (long)room.Position.X >=
                        (long)candidate.Position.X + candidate.Width + spacing ||
                    (long)candidate.Position.Y >=
                        (long)room.Position.Y + room.Height + spacing ||
                    (long)room.Position.Y >=
                        (long)candidate.Position.Y + candidate.Height + spacing;

                if (!separated)
                    return false;
            }

            return true;
        }
    }
}
