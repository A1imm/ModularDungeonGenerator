
using System;
using System.Collections.Generic;
using DungeonGeneration.Core;

namespace DungeonGeneration.Generation
{
    public sealed class DungeonConnectivityValidator
    {
        private static readonly GridPosition[] Directions =
        {
            new GridPosition(0, 1),
            new GridPosition(1, 0),
            new GridPosition(0, -1),
            new GridPosition(-1, 0)
        };

        public bool AreAllRoomsReachable(
            DungeonGrid grid,
            IReadOnlyList<DungeonRoom> rooms)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (rooms == null)
                throw new ArgumentNullException(nameof(rooms));

            // An empty dungeon does not satisfy our requirements.
            if (rooms.Count == 0)
                return false;

            var start = rooms[0]?.Center
                ?? throw new ArgumentException(
                    "Room collection contains null.");

            if (!IsWalkable(grid, start))
                return false;

            var visited = new HashSet<GridPosition>();
            var queue = new Queue<GridPosition>();

            visited.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                GridPosition current = queue.Dequeue();

                foreach (GridPosition direction in Directions)
                {
                    var next = new GridPosition(
                        current.X + direction.X,
                        current.Y + direction.Y
                    );

                    if (!IsWalkable(grid, next))
                        continue;

                    if (visited.Add(next))
                    {
                        queue.Enqueue(next);
                    }
                }
            }

            foreach (DungeonRoom room in rooms)
            {
                if (room == null || !visited.Contains(room.Center))
                    return false;
            }

            return true;
        }

        private static bool IsWalkable(
            DungeonGrid grid,
            GridPosition position)
        {
            if (!grid.IsInside(position))
                return false;

            CellType type = grid.GetCell(position);

            return type == CellType.Room ||
                   type == CellType.Corridor;
        }
    }
}
