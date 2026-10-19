using System;

namespace DungeonGeneration.Core
{
    public sealed class DungeonRoom
    {
        public int Id { get; }
        public GridPosition Position { get; }

        public int Width { get; }
        public int Height { get; }

        public GridPosition Center => new GridPosition(
            Position.X + Width / 2,
            Position.Y + Height / 2
        );

        public DungeonRoom(
            int id,
            GridPosition position,
            int width,
            int height)
        {
            if (id < 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            Id = id;
            Position = position;
            Width = width;
            Height = height;
        }

        public bool Contains(GridPosition position)
        {
            return position.X >= Position.X &&
                   position.X < Position.X + Width &&
                   position.Y >= Position.Y &&
                   position.Y < Position.Y + Height;
        }

        public bool Intersects(DungeonRoom other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return Position.X < other.Position.X + other.Width &&
                   Position.X + Width > other.Position.X &&
                   Position.Y < other.Position.Y + other.Height &&
                   Position.Y + Height > other.Position.Y;
        }
    }
}
