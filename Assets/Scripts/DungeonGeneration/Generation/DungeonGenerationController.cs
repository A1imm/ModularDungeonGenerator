
using System;
using System.Collections.Generic;
using DungeonGeneration.Core;
using DungeonGeneration.Geometry;
using DungeonGeneration.Roles;
using UnityEngine;

namespace DungeonGeneration.Generation
{
    [RequireComponent(typeof(DungeonGeometryGenerator))]
    public sealed class DungeonGenerationController : MonoBehaviour
    {
        [Header("Dungeon Layout")]
        [SerializeField]
        private int seed = 12345;

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

        [SerializeField]
        private bool showRoomRoles = true;

        [SerializeField]
        private bool showNormalRoomRoles = false;

        private DungeonGeometryGenerator geometryGenerator;
        private DungeonLayout currentLayout;

        private IReadOnlyList<RoomRoleAssignment> currentRoles =
            Array.Empty<RoomRoleAssignment>();

        private readonly RoomProgressionAssigner roleAssigner =
            new RoomProgressionAssigner();

        private bool usedTreeFallback;

        public bool UsedTreeFallback => usedTreeFallback;

        private readonly RoomDifficultyAnalyzer difficultyAnalyzer =
            new RoomDifficultyAnalyzer();

        private IReadOnlyList<RoomDifficultyAssignment> currentDifficulties =
            Array.Empty<RoomDifficultyAssignment>();


        public int Seed => seed;

        public DungeonLayout CurrentLayout => currentLayout;

        public bool ShowRoomRoles => showRoomRoles;

        public bool ShowNormalRoomRoles => showNormalRoomRoles;

        public float CellSize =>
            GeometryGenerator.ModuleSet != null
                ? GeometryGenerator.ModuleSet.CellSize
                : 4f;

        public IReadOnlyList<RoomRoleAssignment> CurrentRoles =>
            currentRoles;

        public IReadOnlyList<RoomDifficultyAssignment> CurrentDifficulties =>
            currentDifficulties;

        public int GetRoomDifficulty(int roomId)
        {
            foreach (RoomDifficultyAssignment assignment
                     in currentDifficulties)
            {
                if (assignment.RoomId == roomId)
                    return assignment.DifficultyLevel;
            }

            throw new ArgumentException(
                $"No difficulty assignment found for room {roomId}.",
                nameof(roomId));
        }

        public int GetRoomDistanceFromStart(int roomId)
        {
            foreach (RoomDifficultyAssignment assignment
                     in currentDifficulties)
            {
                if (assignment.RoomId == roomId)
                    return assignment.DistanceFromStart;
            }

            throw new ArgumentException(
                $"No difficulty assignment found for room {roomId}.",
                nameof(roomId));
        }

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

            // Generate logical dungeon layout.
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

            IReadOnlyList<RoomRoleAssignment> newRoles;

            bool newUsedTreeFallback = false;

            try
            {
                newRoles = roleAssigner.AssignRoles(newLayout);
            }
            catch (InvalidOperationException)
            {
                // The original graph has no suitable mandatory Boss.
                // Rebuild using only the spanning-tree connections.
                if (loopProbability <= 0f)
                    throw;

                newLayout = layoutGenerator.Generate(
                    seed,
                    gridWidth,
                    gridHeight,
                    roomCount,
                    minRoomSize,
                    maxRoomSize,
                    roomSpacing,
                    maxAttempts,
                    0f
                );

                newRoles = roleAssigner.AssignRoles(newLayout);
                newUsedTreeFallback = true;

                Debug.LogWarning(
                    "Mandatory Boss-to-Exit progression was unavailable " +
                    "with the requested loops. The dungeon was " +
                    "regenerated without optional graph loops.",
                    this);
            }

            // Calculate difficulty from the selected Start room.
            int startRoomId = -1;

            foreach (RoomRoleAssignment assignment in newRoles)
            {
                if (assignment.Role == RoomRole.Start)
                {
                    startRoomId = assignment.RoomId;
                    break;
                }
            }

            if (startRoomId < 0)
            {
                throw new InvalidOperationException(
                    "No Start room was assigned.");
            }

            IReadOnlyList<RoomDifficultyAssignment> newDifficulties =
                difficultyAnalyzer.Analyze(
                    newLayout.FinalGraph,
                    newLayout.Rooms,
                    startRoomId);

            int bossRoomId = -1;

            foreach (RoomRoleAssignment assignment in newRoles)
            {
                if (assignment.Role == RoomRole.Boss)
                {
                    bossRoomId = assignment.RoomId;
                    break;
                }
            }

            if (bossRoomId < 0)
                throw new InvalidOperationException(
                    "No Boss room was assigned.");

            var adjustedDifficulties =
                new List<RoomDifficultyAssignment>(
                    newDifficulties.Count);

            foreach (RoomDifficultyAssignment assignment in newDifficulties)
            {
                int difficulty = assignment.RoomId == bossRoomId
                    ? RoomDifficultyAnalyzer.MaximumDifficulty
                    : assignment.DifficultyLevel;

                adjustedDifficulties.Add(
                    new RoomDifficultyAssignment(
                        assignment.RoomId,
                        assignment.DistanceFromStart,
                        difficulty));
            }

            newDifficulties = adjustedDifficulties.AsReadOnly();

            // Generate geometry only after all logical analysis succeeds.
            GeometryGenerator.GenerateGeometry(
                newLayout.Grid,
                seed);

            // Commit all new data together.
            currentLayout = newLayout;
            currentRoles = newRoles;
            currentDifficulties = newDifficulties;
            usedTreeFallback = newUsedTreeFallback;

            Debug.Log(
                $"Dungeon generated (seed {seed}): " +
                $"{currentLayout.Rooms.Count} rooms, " +
                $"{GeneratedFloorCount} floors, " +
                $"{GeneratedWallCount} walls. " +
                $"Start: {GetRoomId(RoomRole.Start)}, " +
                $"Boss: {GetRoomId(RoomRole.Boss)}, " +
                $"Exit: {GetRoomId(RoomRole.Exit)}.",
                this);
        }

        public void Clear()
        {
            usedTreeFallback = false;
            GeometryGenerator.ClearGeometry();

            currentLayout = null;

            currentRoles =
                Array.Empty<RoomRoleAssignment>();

            currentDifficulties =
                Array.Empty<RoomDifficultyAssignment>();
        }

        public int GetRoomId(RoomRole role)
        {
            foreach (RoomRoleAssignment assignment in currentRoles)
            {
                if (assignment.Role == role)
                    return assignment.RoomId;
            }

            return -1;
        }

        public RoomRole GetRoomRole(int roomId)
        {
            foreach (RoomRoleAssignment assignment in currentRoles)
            {
                if (assignment.RoomId == roomId)
                    return assignment.Role;
            }

            throw new ArgumentException(
                $"No role assignment found for room {roomId}.",
                nameof(roomId));
        }

        private void ValidateConfiguration()
        {
            if (gridWidth <= 0 || gridHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(gridWidth),
                    "Grid dimensions must be positive.");
            }

            if (roomCount < 3)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(roomCount),
                    "At least three rooms are required " +
                    "for Start, Boss and Exit.");
            }

            if (minRoomSize <= 0 ||
                maxRoomSize < minRoomSize)
            {
                throw new ArgumentException(
                    "Room size range is invalid.");
            }

            if (roomSpacing < 0 || maxAttempts <= 0)
            {
                throw new ArgumentException(
                    "Room spacing or maximum attempts is invalid.");
            }

            if (loopProbability < 0f || loopProbability > 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(loopProbability));
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (currentLayout == null)
                return;

            DungeonModuleSet moduleSet =
                GeometryGenerator.ModuleSet;

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

            roomCount = Mathf.Max(3, roomCount);

            minRoomSize = Mathf.Max(1, minRoomSize);
            maxRoomSize = Mathf.Max(
                minRoomSize,
                maxRoomSize);

            roomSpacing = Mathf.Max(0, roomSpacing);
            maxAttempts = Mathf.Max(1, maxAttempts);

            loopProbability =
                Mathf.Clamp01(loopProbability);
        }
    }
}
