
using System;
using System.Diagnostics;
using DungeonGeneration.Core;
using DungeonGeneration.Generation;
using UnityEngine;

namespace DungeonGeneration.Debugging
{
    public sealed class DungeonDebugVisualizer : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField, Min(1)] private int gridWidth = 60;
        [SerializeField, Min(1)] private int gridHeight = 60;
        [SerializeField, Min(0.1f)] private float cellSize = 4f;

        [Header("Rooms")]
        [SerializeField, Min(1)] private int roomCount = 10;
        [SerializeField, Min(1)] private int minRoomSize = 3;
        [SerializeField, Min(1)] private int maxRoomSize = 6;
        [SerializeField, Min(0)] private int roomSpacing = 1;
        [SerializeField, Min(1)] private int maxAttempts = 10000;

        [Header("Randomness")]
        [SerializeField] private int seed = 12345;
        [SerializeField, Range(0f, 1f)]
        private float loopProbability = 0.15f;

        [Header("Visualization")]
        [SerializeField] private bool showGridBounds = true;
        [SerializeField] private bool showRoomCenters = true;
        [SerializeField] private bool showGraphConnections = true;

        private DungeonLayout layout;
        private double generationTimeMs;

        public int Seed
        {
            get => seed;
            set => seed = value;
        }

        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public float CellSize => cellSize;

        public int GeneratedRoomCount => layout?.Rooms.Count ?? 0;
        public double GenerationTimeMs => generationTimeMs;

        public void Generate()
        {
            var generator = new DungeonLayoutGenerator();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                DungeonLayout generated = generator.Generate(
                    seed,
                    gridWidth,
                    gridHeight,
                    roomCount,
                    minRoomSize,
                    maxRoomSize,
                    roomSpacing,
                    maxAttempts,
                    loopProbability
                );

                stopwatch.Stop();

                layout = generated;
                generationTimeMs = stopwatch.Elapsed.TotalMilliseconds;

                UnityEngine.Debug.Log(
                    $"Dungeon generated successfully. " +
                    $"Seed: {seed}, " +
                    $"Rooms: {layout.Rooms.Count}, " +
                    $"Connections: {layout.FinalGraph.ConnectionCount}, " +
                    $"Time: {generationTimeMs:F3} ms",
                    this
                );
            }
            catch (Exception exception)
            {
                stopwatch.Stop();
                layout = null;
                generationTimeMs = 0;

                UnityEngine.Debug.LogException(exception, this);
            }
        }

        private void OnDrawGizmos()
        {
            if (cellSize <= 0f)
                return;

            Gizmos.matrix = Matrix4x4.identity;

            if (showGridBounds)
                DrawGridBounds();

            if (layout == null)
                return;

            DrawCells();

            if (showGraphConnections)
                DrawGraphConnections();

            if (showRoomCenters)
                DrawRoomCenters();
        }

        private void DrawGridBounds()
        {
            if (gridWidth <= 0 || gridHeight <= 0)
                return;

            Gizmos.color = Color.gray;

            Vector3 center = transform.position + new Vector3(
                gridWidth * cellSize / 2f,
                0f,
                gridHeight * cellSize / 2f
            );

            Gizmos.DrawWireCube(
                center,
                new Vector3(
                    gridWidth * cellSize,
                    0.1f,
                    gridHeight * cellSize
                )
            );
        }

        private void DrawCells()
        {
            DungeonGrid grid = layout.Grid;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var position = new GridPosition(x, y);
                    CellType type = grid.GetCell(position);

                    if (type == CellType.Empty)
                        continue;

                    Gizmos.color = type == CellType.Room
                        ? new Color(0.2f, 0.6f, 1f, 0.8f)
                        : new Color(1f, 0.7f, 0.2f, 0.85f);

                    Vector3 center = transform.position + new Vector3(
                        (x + 0.5f) * cellSize,
                        0.1f,
                        (y + 0.5f) * cellSize
                    );

                    Gizmos.DrawCube(
                        center,
                        new Vector3(
                            cellSize * 0.97f,
                            0.2f,
                            cellSize * 0.97f
                        )
                    );
                }
            }
        }

        private void DrawGraphConnections()
        {
            Gizmos.color = Color.green;

            foreach (RoomConnection edge in layout.FinalGraph.Connections)
            {
                DungeonRoom roomA =
                    layout.FinalGraph.GetRoom(edge.RoomAId);

                DungeonRoom roomB =
                    layout.FinalGraph.GetRoom(edge.RoomBId);

                Vector3 from = GetRoomCenter(roomA) +
                    Vector3.up * 1f;

                Vector3 to = GetRoomCenter(roomB) +
                    Vector3.up * 1f;

                Gizmos.DrawLine(from, to);
            }
        }

        private void DrawRoomCenters()
        {
            Gizmos.color = Color.yellow;

            foreach (DungeonRoom room in layout.Rooms)
            {
                Gizmos.DrawSphere(
                    GetRoomCenter(room) + Vector3.up * 1.5f,
                    cellSize * 0.15f
                );
            }
        }

        private Vector3 GetRoomCenter(DungeonRoom room)
        {
            return transform.position + new Vector3(
                (room.Position.X + room.Width / 2f) * cellSize,
                0f,
                (room.Position.Y + room.Height / 2f) * cellSize
            );
        }
    }
}
