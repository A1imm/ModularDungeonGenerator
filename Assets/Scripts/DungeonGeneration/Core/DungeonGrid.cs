
using System;

namespace DungeonGeneration.Core
{
    public class DungeonGrid
    {
        private readonly CellType[,] cells;

        public int Width { get; }
        public int Height { get; }

        public DungeonGrid(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;

            cells = new CellType[width, height];
        }

        public bool IsInside(GridPosition position)
        {
            return position.X >= 0 &&
                   position.X < Width &&
                   position.Y >= 0 &&
                   position.Y < Height;
        }

        public CellType GetCell(GridPosition position)
        {
            ValidatePosition(position);

            return cells[position.X, position.Y];
        }

        public void SetCell(GridPosition position, CellType type)
        {
            ValidatePosition(position);

            cells[position.X, position.Y] = type;
        }

        private void ValidatePosition(GridPosition position)
        {
            if (!IsInside(position))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(position),
                    "Position is outside dungeon grid bounds."
                );
            }
        }
    }
}
