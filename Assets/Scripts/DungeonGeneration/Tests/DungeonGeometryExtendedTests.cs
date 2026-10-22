
using System;
using System.Reflection;
using DungeonGeneration.Core;
using DungeonGeneration.Geometry;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DungeonGeneration.Tests
{
    public class DungeonGeometryExtendedTests
    {
        private GameObject root;
        private GameObject baseFloor;
        private GameObject variantFloor;
        private GameObject baseWall;
        private GameObject variantWall;

        private DungeonModuleSet moduleSet;
        private DungeonGeometryGenerator generator;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("TestDungeon");

            generator =
                root.AddComponent<DungeonGeometryGenerator>();

            baseFloor = CreateTestPrefab("BaseFloor");
            variantFloor = CreateTestPrefab("VariantFloor");
            baseWall = CreateTestPrefab("BaseWall");
            variantWall = CreateTestPrefab("VariantWall");

            moduleSet =
                ScriptableObject.CreateInstance<DungeonModuleSet>();

            SetField(moduleSet, "floorPrefab", baseFloor);
            SetField(moduleSet, "wallPrefab", baseWall);

            SetField(
                moduleSet,
                "floorVariants",
                new[] { variantFloor });

            SetField(
                moduleSet,
                "wallVariants",
                new[] { variantWall });

            SetField(generator, "moduleSet", moduleSet);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(baseFloor);
            Object.DestroyImmediate(variantFloor);
            Object.DestroyImmediate(baseWall);
            Object.DestroyImmediate(variantWall);
            Object.DestroyImmediate(moduleSet);
        }

        [Test]
        public void EmptyGrid_GeneratesNoModules()
        {
            var grid = new DungeonGrid(5, 5);

            generator.GenerateGeometry(grid, 12345);

            AssertGeometry(grid, 12345, true);
        }

        [TestCase(1, 1)]
        [TestCase(2, 3)]
        [TestCase(4, 3)]
        [TestCase(8, 6)]
        public void FilledGrid_UsesOnlyOuterWalls(
            int width,
            int height)
        {
            DungeonGrid grid = CreateFilledGrid(
                width, height);

            generator.GenerateGeometry(grid, 12345);

            Assert.AreEqual(
                width * height,
                generator.GeneratedFloorCount);

            Assert.AreEqual(
                2 * width + 2 * height,
                generator.GeneratedWallCount);

            AssertGeometry(grid, 12345, true);
        }

        [Test]
        public void IsolatedCells_HaveFourWallsEach()
        {
            var grid = new DungeonGrid(5, 5);

            grid.SetCell(
                new GridPosition(0, 0),
                CellType.Room);

            grid.SetCell(
                new GridPosition(2, 2),
                CellType.Corridor);

            grid.SetCell(
                new GridPosition(4, 4),
                CellType.Room);

            generator.GenerateGeometry(grid, 12345);

            Assert.AreEqual(3, generator.GeneratedFloorCount);
            Assert.AreEqual(12, generator.GeneratedWallCount);

            AssertGeometry(grid, 12345, true);
        }

        [Test]
        public void LShapedCorridor_HasCorrectBoundary()
        {
            var grid = new DungeonGrid(4, 4);

            grid.SetCell(
                new GridPosition(0, 1),
                CellType.Room);

            grid.SetCell(
                new GridPosition(1, 1),
                CellType.Corridor);

            grid.SetCell(
                new GridPosition(1, 2),
                CellType.Corridor);

            grid.SetCell(
                new GridPosition(1, 3),
                CellType.Room);

            generator.GenerateGeometry(grid, 12345);

            Assert.AreEqual(4, generator.GeneratedFloorCount);
            Assert.AreEqual(10, generator.GeneratedWallCount);

            AssertGeometry(grid, 12345, true);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(12345)]
        [TestCase(-54321)]
        public void DifferentSeeds_SelectExpectedPrefabs(int seed)
        {
            DungeonGrid grid = CreateFilledGrid(5, 4);

            generator.GenerateGeometry(grid, seed);

            AssertGeometry(grid, seed, true);
        }

        [Test]
        public void Regeneration_LeavesOneCompleteHierarchy()
        {
            DungeonGrid grid = CreateFilledGrid(3, 3);

            generator.GenerateGeometry(grid, 12345);
            generator.GenerateGeometry(grid, 54321);

            AssertGeometry(grid, 54321, true);

            Assert.IsNull(
                root.transform.Find(
                    DungeonGeometryHierarchy.PendingRootName));

            Assert.AreEqual(1, root.transform.childCount);
        }

        [Test]
        public void FloorsOnly_DoesNotGenerateWalls()
        {
            DungeonGrid grid = CreateFilledGrid(3, 2);

            generator.GenerateFloors(grid, 12345);

            AssertGeometry(grid, 12345, false);
        }

        [Test]
        public void InvalidGrid_PreservesPreviousGeometry()
        {
            DungeonGrid grid = CreateFilledGrid(2, 2);

            generator.GenerateGeometry(grid, 12345);

            Transform previousRoot = root.transform.Find(
                DungeonGeometryHierarchy.RootName);

            int previousFloors = generator.GeneratedFloorCount;
            int previousWalls = generator.GeneratedWallCount;

            Assert.Throws<ArgumentNullException>(() =>
                generator.GenerateGeometry(null, 54321));

            Assert.AreSame(
                previousRoot,
                root.transform.Find(
                    DungeonGeometryHierarchy.RootName));

            Assert.AreEqual(
                previousFloors,
                generator.GeneratedFloorCount);

            Assert.AreEqual(
                previousWalls,
                generator.GeneratedWallCount);
        }

        private void AssertGeometry(
            DungeonGrid grid,
            int seed,
            bool includeWalls)
        {
            Transform generatedRoot =
                root.transform.Find(
                    DungeonGeometryHierarchy.RootName);

            Assert.IsNotNull(generatedRoot);

            Transform floors =
                generatedRoot.Find(
                    DungeonGeometryHierarchy.FloorsName);

            Transform walls =
                generatedRoot.Find(
                    DungeonGeometryHierarchy.WallsName);

            Transform decorations =
                generatedRoot.Find(
                    DungeonGeometryHierarchy.DecorationsName);

            Assert.IsNotNull(floors);
            Assert.IsNotNull(walls);
            Assert.IsNotNull(decorations);

            Assert.AreEqual(3, generatedRoot.childCount);
            Assert.AreEqual(0, decorations.childCount);

            int expectedFloors = 0;
            int expectedWalls = 0;

            float cellSize = moduleSet.CellSize;

            CellDirection[] directions =
            {
                CellDirection.North,
                CellDirection.East,
                CellDirection.South,
                CellDirection.West
            };

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var position = new GridPosition(x, y);

                    bool walkable = IsWalkable(grid, x, y);

                    Transform floor =
                        floors.Find($"Floor_{x}_{y}");

                    Assert.AreEqual(
                        walkable,
                        floor != null,
                        $"Floor mismatch at ({x}, {y}).");

                    if (!walkable)
                        continue;

                    expectedFloors++;

                    Vector3 floorPosition = new Vector3(
                        (x + 0.5f) * cellSize,
                        0f,
                        (y + 0.5f) * cellSize);

                    AssertPosition(floor, floorPosition);

                    AssertPrefabVariant(
                        floor,
                        moduleSet.GetFloorPrefab(seed, x, y));

                    foreach (CellDirection direction in directions)
                    {
                        GetOffset(direction, out int dx, out int dy);

                        bool needsWall = includeWalls &&
                            !IsWalkable(grid, x + dx, y + dy);

                        Transform wall = walls.Find(
                            $"Wall_{x}_{y}_{direction}");

                        Assert.AreEqual(
                            needsWall,
                            wall != null,
                            $"Wall mismatch at ({x}, {y}), {direction}.");

                        if (!needsWall)
                            continue;

                        expectedWalls++;

                        Vector3 wallPosition = floorPosition +
                            new Vector3(
                                dx * cellSize * 0.5f,
                                0f,
                                dy * cellSize * 0.5f);

                        AssertPosition(wall, wallPosition);

                        float rotation =
                            direction == CellDirection.East ||
                            direction == CellDirection.West
                                ? 90f
                                : 0f;

                        Assert.Less(
                            Quaternion.Angle(
                                wall.rotation,
                                Quaternion.Euler(0f, rotation, 0f)),
                            0.001f);

                        AssertPrefabVariant(
                            wall,
                            moduleSet.GetWallPrefab(
                                seed, x, y, direction));
                    }
                }
            }

            Assert.AreEqual(
                expectedFloors,
                generator.GeneratedFloorCount);

            Assert.AreEqual(
                expectedWalls,
                generator.GeneratedWallCount);

            Assert.AreEqual(expectedFloors, floors.childCount);
            Assert.AreEqual(expectedWalls, walls.childCount);

            Assert.IsNull(
                root.transform.Find(
                    DungeonGeometryHierarchy.PendingRootName));
        }

        private static void AssertPosition(
            Transform actual,
            Vector3 expected)
        {
            Assert.IsNotNull(actual);

            Assert.Less(
                Vector3.Distance(actual.position, expected),
                0.001f);
        }

        private static void AssertPrefabVariant(
            Transform instance,
            GameObject expectedPrefab)
        {
            string expectedMarker =
                $"Marker_{expectedPrefab.name}";

            Assert.IsNotNull(
                instance.Find(expectedMarker),
                $"Incorrect prefab used for {instance.name}.");
        }

        private static bool IsWalkable(
            DungeonGrid grid,
            int x,
            int y)
        {
            var position = new GridPosition(x, y);

            if (!grid.IsInside(position))
                return false;

            CellType type = grid.GetCell(position);

            return type == CellType.Room ||
                   type == CellType.Corridor;
        }

        private static void GetOffset(
            CellDirection direction,
            out int dx,
            out int dy)
        {
            dx = 0;
            dy = 0;

            switch (direction)
            {
                case CellDirection.North:
                    dy = 1;
                    break;

                case CellDirection.East:
                    dx = 1;
                    break;

                case CellDirection.South:
                    dy = -1;
                    break;

                case CellDirection.West:
                    dx = -1;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(direction));
            }
        }

        private static DungeonGrid CreateFilledGrid(
            int width,
            int height)
        {
            var grid = new DungeonGrid(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid.SetCell(
                        new GridPosition(x, y),
                        CellType.Room);
                }
            }

            return grid;
        }

        private static GameObject CreateTestPrefab(string name)
        {
            var prefab = new GameObject(name);

            var marker = new GameObject($"Marker_{name}");

            marker.transform.SetParent(
                prefab.transform,
                false);

            return prefab;
        }

        private static void SetField(
            object target,
            string fieldName,
            object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance |
                BindingFlags.NonPublic);

            Assert.IsNotNull(
                field,
                $"Missing field: {fieldName}");

            field.SetValue(target, value);
        }
    }
}
