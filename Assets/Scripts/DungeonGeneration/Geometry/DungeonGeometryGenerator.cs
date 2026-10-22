
using System;
using DungeonGeneration.Core;
using UnityEngine;

namespace DungeonGeneration.Geometry
{
    public sealed class DungeonGeometryGenerator : MonoBehaviour
    {
        [Header("Module Configuration")]
        [SerializeField]
        private DungeonModuleSet moduleSet;

        private static readonly CellDirection[] Directions =
        {
            CellDirection.North,
            CellDirection.East,
            CellDirection.South,
            CellDirection.West
        };

        private readonly CellNeighborAnalyzer neighborAnalyzer =
            new CellNeighborAnalyzer();

        public DungeonModuleSet ModuleSet => moduleSet;

        public int GeneratedFloorCount { get; private set; }
        public int GeneratedWallCount { get; private set; }

        public void GenerateGeometry(DungeonGrid grid)
        {
            GenerateGeometry(grid, 0);
        }

        public void GenerateGeometry(DungeonGrid grid, int seed)
        {
            GenerateInternal(grid, seed, true);
        }

        public void GenerateFloors(DungeonGrid grid)
        {
            GenerateFloors(grid, 0);
        }

        public void GenerateFloors(DungeonGrid grid, int seed)
        {
            GenerateInternal(grid, seed, false);
        }

        private void GenerateInternal(
            DungeonGrid grid,
            int seed,
            bool includeWalls)
        {
            ValidateInputs(grid);

            DestroyGeneratedObject(
                transform.Find(
                    DungeonGeometryHierarchy.PendingRootName));

            DungeonGeometryHierarchy pending = null;
            int newFloorCount = 0;
            int newWallCount = 0;

            try
            {
                pending = new DungeonGeometryHierarchy(
                    transform,
                    DungeonGeometryHierarchy.PendingRootName);

                float cellSize = moduleSet.CellSize;

                for (int x = 0; x < grid.Width; x++)
                {
                    for (int y = 0; y < grid.Height; y++)
                    {
                        GridPosition position =
                            new GridPosition(x, y);

                        if (!neighborAnalyzer.IsWalkable(
                            grid, position))
                        {
                            continue;
                        }

                        CreateFloor(
                            position,
                            cellSize,
                            seed,
                            pending.Floors);

                        newFloorCount++;

                        if (!includeWalls)
                            continue;

                        foreach (CellDirection direction in Directions)
                        {
                            if (!neighborAnalyzer.NeedsWall(
                                grid, position, direction))
                            {
                                continue;
                            }

                            CreateWall(
                                position,
                                direction,
                                cellSize,
                                seed,
                                pending.Walls);

                            newWallCount++;
                        }
                    }
                }

                // Commit the completed geometry.
                DestroyGeneratedObject(
                    transform.Find(
                        DungeonGeometryHierarchy.RootName));

                DestroyGeneratedObject(
                    transform.Find("Generated_Floors"));

                DestroyGeneratedObject(
                    transform.Find("Generated_Walls"));

                pending.Root.name =
                    DungeonGeometryHierarchy.RootName;

                GeneratedFloorCount = newFloorCount;
                GeneratedWallCount = newWallCount;

                Debug.Log(
                    $"Dungeon geometry generated: " +
                    $"{GeneratedFloorCount} floors, " +
                    $"{GeneratedWallCount} walls.",
                    this);
            }
            catch
            {
                if (pending != null)
                    DestroyGeneratedObject(pending.Root);

                throw;
            }
        }

        private void CreateFloor(
            GridPosition position,
            float cellSize,
            int seed,
            Transform parent)
        {
            Vector3 worldPosition = new Vector3(
                (position.X + 0.5f) * cellSize,
                0f,
                (position.Y + 0.5f) * cellSize
            );

            GameObject selectedPrefab =
                moduleSet.GetFloorPrefab(
                    seed,
                    position.X,
                    position.Y);

            GameObject floor = Instantiate(
                selectedPrefab,
                worldPosition,
                Quaternion.identity,
                parent);

            floor.name =
                $"Floor_{position.X}_{position.Y}";
        }

        private void CreateWall(
            GridPosition position,
            CellDirection direction,
            float cellSize,
            int seed,
            Transform parent)
        {
            float centerX =
                (position.X + 0.5f) * cellSize;

            float centerZ =
                (position.Y + 0.5f) * cellSize;

            float halfCell = cellSize * 0.5f;

            Vector3 wallPosition;
            Quaternion wallRotation;

            switch (direction)
            {
                case CellDirection.North:
                    wallPosition = new Vector3(
                        centerX, 0f, centerZ + halfCell);
                    wallRotation = Quaternion.identity;
                    break;

                case CellDirection.East:
                    wallPosition = new Vector3(
                        centerX + halfCell, 0f, centerZ);
                    wallRotation =
                        Quaternion.Euler(0f, 90f, 0f);
                    break;

                case CellDirection.South:
                    wallPosition = new Vector3(
                        centerX, 0f, centerZ - halfCell);
                    wallRotation = Quaternion.identity;
                    break;

                case CellDirection.West:
                    wallPosition = new Vector3(
                        centerX - halfCell, 0f, centerZ);
                    wallRotation =
                        Quaternion.Euler(0f, 90f, 0f);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(direction));
            }

            GameObject selectedPrefab =
                moduleSet.GetWallPrefab(
                    seed,
                    position.X,
                    position.Y,
                    direction);

            GameObject wall = Instantiate(
                selectedPrefab,
                wallPosition,
                wallRotation,
                parent);

            wall.name =
                $"Wall_{position.X}_{position.Y}_{direction}";
        }

        public void ClearFloors()
        {
            Transform root = transform.Find(
                DungeonGeometryHierarchy.RootName);

            if (root != null)
            {
                DestroyGeneratedObject(
                    root.Find(
                        DungeonGeometryHierarchy.FloorsName));
            }

            DestroyGeneratedObject(
                transform.Find("Generated_Floors"));

            GeneratedFloorCount = 0;
        }

        public void ClearGeometry()
        {
            DestroyGeneratedObject(
                transform.Find(
                    DungeonGeometryHierarchy.RootName));

            DestroyGeneratedObject(
                transform.Find(
                    DungeonGeometryHierarchy.PendingRootName));

            DestroyGeneratedObject(
                transform.Find("Generated_Floors"));

            DestroyGeneratedObject(
                transform.Find("Generated_Walls"));

            GeneratedFloorCount = 0;
            GeneratedWallCount = 0;
        }

        private static void DestroyGeneratedObject(Transform target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
            {
                target.gameObject.SetActive(false);
                target.SetParent(null);
                Destroy(target.gameObject);
            }
            else
            {
                DestroyImmediate(target.gameObject);
            }
        }

        private void ValidateInputs(DungeonGrid grid)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (moduleSet == null)
                throw new InvalidOperationException(
                    "DungeonModuleSet is not assigned.");

            if (!moduleSet.IsValid(out string errorMessage))
                throw new InvalidOperationException(errorMessage);
        }
    }
}
