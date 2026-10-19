
using System;
using System.Collections.Generic;
using DungeonGeneration.Core;

namespace DungeonGeneration.Generation
{
    public sealed class CorridorGenerator
    {
        private static readonly GridPosition[] Directions =
        {
            new GridPosition(0, 1),
            new GridPosition(1, 0),
            new GridPosition(0, -1),
            new GridPosition(-1, 0)
        };

        public DungeonGrid Generate(
            int gridWidth,
            int gridHeight,
            IReadOnlyList<DungeonRoom> rooms,
            RoomGraph graph)
        {
            if (rooms == null)
                throw new ArgumentNullException(nameof(rooms));

            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            var grid = new DungeonGrid(gridWidth, gridHeight);

            if (graph.RoomCount != rooms.Count)
                throw new ArgumentException(
                    "Room count does not match graph.");

            // First place all rooms on the grid.
            foreach (DungeonRoom room in rooms)
            {
                if (room == null)
                    throw new ArgumentException("Room cannot be null.");

                graph.GetRoom(room.Id);

                if (room.Position.X < 0 ||
                    room.Position.Y < 0 ||
                    (long)room.Position.X + room.Width > gridWidth ||
                    (long)room.Position.Y + room.Height > gridHeight)
                {
                    throw new ArgumentException(
                        $"Room {room.Id} is outside grid bounds.");
                }

                for (int x = room.Position.X;
                     (long)x < (long)room.Position.X + room.Width;
                     x++)
                {
                    for (int y = room.Position.Y;
                         (long)y < (long)room.Position.Y + room.Height;
                         y++)
                    {
                        var position = new GridPosition(x, y);

                        if (grid.GetCell(position) != CellType.Empty)
                            throw new ArgumentException(
                                "Rooms must not overlap.");

                        grid.SetCell(position, CellType.Room);
                    }
                }
            }

            // Route each connection through the grid.
            foreach (RoomConnection edge in graph.Connections)
            {
                DungeonRoom roomA = graph.GetRoom(edge.RoomAId);
                DungeonRoom roomB = graph.GetRoom(edge.RoomBId);

                List<GridPosition> path =
                    FindPath(grid, roomA, roomB);

                if (path == null)
                    throw new InvalidOperationException(
                        $"No corridor path between rooms " +
                        $"{roomA.Id} and {roomB.Id}.");

                foreach (GridPosition position in path)
                {
                    if (grid.GetCell(position) == CellType.Empty)
                        grid.SetCell(position, CellType.Corridor);
                }
            }

            return grid;
        }

        private static List<GridPosition> FindPath(
            DungeonGrid grid,
            DungeonRoom roomA,
            DungeonRoom roomB)
        {
            GridPosition start = roomA.Center;
            GridPosition goal = roomB.Center;

            var queue = new Queue<GridPosition>();
            var visited = new HashSet<GridPosition>();
            var previous =
                new Dictionary<GridPosition, GridPosition>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                GridPosition current = queue.Dequeue();

                if (current == goal)
                    return ReconstructPath(previous, start, goal);

                foreach (GridPosition direction in Directions)
                {
                    var next = new GridPosition(
                        current.X + direction.X,
                        current.Y + direction.Y
                    );

                    if (!grid.IsInside(next) ||
                        visited.Contains(next))
                        continue;

                    // Other rooms are obstacles.
                    if (grid.GetCell(next) == CellType.Room &&
                        !roomA.Contains(next) &&
                        !roomB.Contains(next))
                    {
                        continue;
                    }

                    visited.Add(next);
                    previous.Add(next, current);
                    queue.Enqueue(next);
                }
            }

            return null;
        }

        private static List<GridPosition> ReconstructPath(
            Dictionary<GridPosition, GridPosition> previous,
            GridPosition start,
            GridPosition goal)
        {
            var path = new List<GridPosition>();
            GridPosition current = goal;

            path.Add(current);

            while (current != start)
            {
                current = previous[current];
                path.Add(current);
            }

            path.Reverse();
            return path;
        }
    }
}
