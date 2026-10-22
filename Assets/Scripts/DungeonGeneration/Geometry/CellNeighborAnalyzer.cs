
using System;
using DungeonGeneration.Core;

namespace DungeonGeneration.Geometry
{
    public sealed class CellNeighborAnalyzer
    {
        public bool IsWalkable(
            DungeonGrid grid,
            GridPosition position)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (!grid.IsInside(position))
                return false;

            CellType type = grid.GetCell(position);

            return type == CellType.Room ||
                   type == CellType.Corridor;
        }

        public GridPosition GetNeighborPosition(
            GridPosition position,
            CellDirection direction)
        {
            switch (direction)
            {
                case CellDirection.North:
                    return new GridPosition(
                        position.X, position.Y + 1);

                case CellDirection.East:
                    return new GridPosition(
                        position.X + 1, position.Y);

                case CellDirection.South:
                    return new GridPosition(
                        position.X, position.Y - 1);

                case CellDirection.West:
                    return new GridPosition(
                        position.X - 1, position.Y);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(direction),
                        direction,
                        "Unsupported cell direction.");
            }
        }

        public bool NeedsWall(
            DungeonGrid grid,
            GridPosition position,
            CellDirection direction)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (!grid.IsInside(position))
                throw new ArgumentOutOfRangeException(
                    nameof(position),
                    "The source cell is outside the dungeon grid.");

            if (!IsWalkable(grid, position))
                return false;

            GridPosition neighbor =
                GetNeighborPosition(position, direction);

            return !IsWalkable(grid, neighbor);
        }
    }
}
