
using System;
using DungeonGeneration.Core;
using DungeonGeneration.Geometry;
using UnityEngine;

namespace DungeonGeneration.Generation
{
    [RequireComponent(typeof(DungeonGeometryGenerator))]
    public sealed class DungeonGenerationController : MonoBehaviour
    {
        [Header("Dungeon Layout")]
        [SerializeField] private int seed = 12345;

        [SerializeField, Min(1)]
        private int gridWidth = 60;

        [SerializeField, Min(1)]
        private int gridHeight = 60;

        [SerializeField, Min(1)]
        private int roomCount = 10;

        [SerializeField, Min(1)]
        private int minRoomSize = 3;

        [SerializeField, Min(1)]
        private int maxRoomSize = 6;

        [SerializeField, Min(0)]
        private int roomSpacing = 1;

        [SerializeField, Min(1)]
        private int maxAttempts = 10000;

        [SerializeField, Range(0f, 1f)]
        private float loopProbability = 0.15f;

        [Header("Debug Visualization")]
        [SerializeField]
        private bool showRoomCenters = true;

        [SerializeField]
        private bool showGraphConnections = true;

        private DungeonGeometryGenerator geometryGenerator;
        private DungeonLayout currentLayout;

        public int Seed => seed;
        public DungeonLayout CurrentLayout => currentLayout;

        public int GeneratedFloorCount =>
            GeometryGenerator.GeneratedFloorCount;

        public int GeneratedWallCount =>
            GeometryGenerator.GeneratedWallCount;

        private DungeonGeometryGenerator GeometryGenerator
        {
            get
            {
                if (geometryGenerator == null)
                {
                    geometryGenerator =
                        GetComponent<DungeonGeometryGenerator>();
                }

                return geometryGenerator;
            }
        }

        public void Generate()
        {
            ValidateConfiguration();

            DungeonModuleSet moduleSet = GeometryGenerator.ModuleSet;


            if (moduleSet == null)
            {
                throw new InvalidOperationException(
                    "DungeonModuleSet is not assigned.");
            }

            if (!moduleSet.IsValid(out string errorMessage))
            {
                throw new InvalidOperationException(errorMessage);
            }


            var layoutGenerator = new DungeonLayoutGenerator();

            DungeonLayout newLayout = layoutGenerator.Generate(
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

            GeometryGenerator.GenerateGeometry(newLayout.Grid, seed);

            currentLayout = newLayout;

            Debug.Log(
                $"Dungeon generated (seed {seed}): " +
                $"{currentLayout.Rooms.Count} rooms, " +
                $"{GeneratedFloorCount} floors, " +
                $"{GeneratedWallCount} walls.",
                this);
        }

        public void Clear()
        {
            GeometryGenerator.ClearGeometry();
            currentLayout = null;
        }

        private void ValidateConfiguration()
        {
            if (gridWidth <= 0 || gridHeight <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(gridWidth),
                    "Grid dimensions must be positive.");

            if (roomCount <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(roomCount),
                    "Room count must be positive.");

            if (minRoomSize <= 0 ||
                maxRoomSize < minRoomSize)
            {
                throw new ArgumentException(
                    "Room size range is invalid.");
            }

            if (roomSpacing < 0 || maxAttempts <= 0)
                throw new ArgumentException(
                    "Room spacing or maximum attempts is invalid.");

            if (loopProbability < 0f || loopProbability > 1f)
                throw new ArgumentOutOfRangeException(
                    nameof(loopProbability));
        }

        private void OnDrawGizmosSelected()
        {
            if (currentLayout == null)
                return;

            DungeonModuleSet moduleSet = GeometryGenerator.ModuleSet;

            if (moduleSet == null)
                return;

            float cellSize = moduleSet.CellSize;

            if (showGraphConnections)
            {
                Gizmos.color = Color.green;

                foreach (RoomConnection connection
                         in currentLayout.FinalGraph.Connections)
                {
                    DungeonRoom roomA =
                        currentLayout.FinalGraph.GetRoom(
                            connection.RoomAId);

                    DungeonRoom roomB =
                        currentLayout.FinalGraph.GetRoom(
                            connection.RoomBId);

                    Gizmos.DrawLine(
                        ToWorld(roomA.Center, cellSize),
                        ToWorld(roomB.Center, cellSize));
                }
            }

            if (showRoomCenters)
            {
                Gizmos.color = Color.yellow;

                foreach (DungeonRoom room in currentLayout.Rooms)
                {
                    Gizmos.DrawSphere(
                        ToWorld(room.Center, cellSize),
                        0.4f);
                }
            }
        }

        private static Vector3 ToWorld(
            GridPosition position,
            float cellSize)
        {
            return new Vector3(
                (position.X + 0.5f) * cellSize,
                1f,
                (position.Y + 0.5f) * cellSize
            );
        }

        private void OnValidate()
        {
            gridWidth = Mathf.Max(1, gridWidth);
            gridHeight = Mathf.Max(1, gridHeight);
            roomCount = Mathf.Max(1, roomCount);
            minRoomSize = Mathf.Max(1, minRoomSize);
            maxRoomSize = Mathf.Max(
                minRoomSize, maxRoomSize);

            roomSpacing = Mathf.Max(0, roomSpacing);
            maxAttempts = Mathf.Max(1, maxAttempts);

            loopProbability =
                Mathf.Clamp01(loopProbability);
        }
    }
}
