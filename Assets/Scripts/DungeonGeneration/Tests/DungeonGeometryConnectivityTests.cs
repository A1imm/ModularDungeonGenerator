
using System.Reflection;
using DungeonGeneration.Core;
using DungeonGeneration.Generation;
using DungeonGeneration.Geometry;
using NUnit.Framework;
using UnityEngine;

namespace DungeonGeneration.Tests
{
    public class DungeonGeometryConnectivityTests
    {
        private static readonly CellDirection[] Directions =
        {
            CellDirection.North,
            CellDirection.East,
            CellDirection.South,
            CellDirection.West
        };

        private GameObject root;
        private GameObject floorPrefab;
        private GameObject wallPrefab;

        private DungeonModuleSet moduleSet;
        private DungeonGeometryGenerator generator;
        private CellNeighborAnalyzer analyzer;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("TestDungeon");

            generator =
                root.AddComponent<DungeonGeometryGenerator>();

            floorPrefab = new GameObject("TestFloor");
            wallPrefab = new GameObject("TestWall");

            moduleSet =
                ScriptableObject.CreateInstance<DungeonModuleSet>();

            SetPrivateField(moduleSet, "floorPrefab", floorPrefab);
            SetPrivateField(moduleSet, "wallPrefab", wallPrefab);
            SetPrivateField(generator, "moduleSet", moduleSet);

            analyzer = new CellNeighborAnalyzer();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(floorPrefab);
            Object.DestroyImmediate(wallPrefab);
            Object.DestroyImmediate(moduleSet);
        }

        [Test]
        public void RoomConnectedToCorridor_HasOpenPassage()
        {
            var grid = new DungeonGrid(2, 1);

            grid.SetCell(
                new GridPosition(0, 0),
                CellType.Room);

            grid.SetCell(
                new GridPosition(1, 0),
                CellType.Corridor);

            generator.GenerateGeometry(grid);

            AssertGeometryMatchesGrid(grid);

            Transform walls =
                root.transform.Find("Generated_Dungeon/Walls");

            Assert.IsNull(walls.Find("Wall_0_0_East"));
            Assert.IsNull(walls.Find("Wall_1_0_West"));

            Assert.AreEqual(2, generator.GeneratedFloorCount);
            Assert.AreEqual(6, generator.GeneratedWallCount);
        }

        [Test]
        public void TurningCorridor_HasBothPassagesOpen()
        {
            var grid = new DungeonGrid(3, 3);

            grid.SetCell(
                new GridPosition(0, 1),
                CellType.Room);

            grid.SetCell(
                new GridPosition(1, 1),
                CellType.Corridor);

            grid.SetCell(
                new GridPosition(1, 2),
                CellType.Corridor);

            generator.GenerateGeometry(grid);

            AssertGeometryMatchesGrid(grid);

            Transform walls =
                root.transform.Find("Generated_Dungeon/Walls");

            Assert.IsNull(walls.Find("Wall_0_1_East"));
            Assert.IsNull(walls.Find("Wall_1_1_West"));

            Assert.IsNull(walls.Find("Wall_1_1_North"));
            Assert.IsNull(walls.Find("Wall_1_2_South"));

            Assert.AreEqual(3, generator.GeneratedFloorCount);
            Assert.AreEqual(8, generator.GeneratedWallCount);
        }

        [Test]
        public void GeneratedDungeon_AllCellBoundariesAreCorrect()
        {
            var layoutGenerator = new DungeonLayoutGenerator();

            DungeonLayout layout = layoutGenerator.Generate(
                seed: 12345,
                gridWidth: 60,
                gridHeight: 60,
                roomCount: 10,
                minRoomSize: 3,
                maxRoomSize: 6,
                roomSpacing: 1,
                maxAttempts: 10000,
                loopProbability: 0.15
            );

            generator.GenerateGeometry(layout.Grid);

            AssertGeometryMatchesGrid(layout.Grid);

            var connectivityValidator =
                new DungeonConnectivityValidator();

            Assert.IsTrue(
                connectivityValidator.AreAllRoomsReachable(
                    layout.Grid,
                    layout.Rooms));
        }

        private void AssertGeometryMatchesGrid(DungeonGrid grid)
        {
            Transform floors =
                root.transform.Find("Generated_Dungeon/Floors");

            Transform walls =
                root.transform.Find("Generated_Dungeon/Walls");

            Assert.IsNotNull(floors);
            Assert.IsNotNull(walls);

            int expectedFloors = 0;
            int expectedWalls = 0;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var position = new GridPosition(x, y);

                    bool walkable =
                        analyzer.IsWalkable(grid, position);

                    Transform floor =
                        floors.Find($"Floor_{x}_{y}");

                    Assert.AreEqual(
                        walkable,
                        floor != null,
                        $"Incorrect floor at ({x}, {y}).");

                    if (!walkable)
                        continue;

                    expectedFloors++;

                    float cellSize = moduleSet.CellSize;

                    Vector3 expectedFloorPosition = new Vector3(
                        (x + 0.5f) * cellSize,
                        0f,
                        (y + 0.5f) * cellSize
                    );

                    Assert.Less(
                        Vector3.Distance(
                            floor.position,
                            expectedFloorPosition),
                        0.001f,
                        $"Incorrect floor position at ({x}, {y}).");

                    foreach (CellDirection direction in Directions)
                    {
                        bool needsWall = analyzer.NeedsWall(
                            grid, position, direction);

                        Transform wall = walls.Find(
                            $"Wall_{x}_{y}_{direction}");

                        Assert.AreEqual(
                            needsWall,
                            wall != null,
                            $"Incorrect {direction} wall at ({x}, {y}).");

                        if (needsWall)
                        {
                            expectedWalls++;

                            AssertWallTransform(
                                wall,
                                position,
                                direction,
                                cellSize);
                        }
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
        }

        private static void AssertWallTransform(
            Transform wall,
            GridPosition position,
            CellDirection direction,
            float cellSize)
        {
            float centerX =
                (position.X + 0.5f) * cellSize;

            float centerZ =
                (position.Y + 0.5f) * cellSize;

            float halfCell = cellSize * 0.5f;

            Vector3 expectedPosition;
            float expectedRotation;

            switch (direction)
            {
                case CellDirection.North:
                    expectedPosition = new Vector3(
                        centerX, 0f, centerZ + halfCell);
                    expectedRotation = 0f;
                    break;

                case CellDirection.East:
                    expectedPosition = new Vector3(
                        centerX + halfCell, 0f, centerZ);
                    expectedRotation = 90f;
                    break;

                case CellDirection.South:
                    expectedPosition = new Vector3(
                        centerX, 0f, centerZ - halfCell);
                    expectedRotation = 0f;
                    break;

                case CellDirection.West:
                    expectedPosition = new Vector3(
                        centerX - halfCell, 0f, centerZ);
                    expectedRotation = 90f;
                    break;

                default:
                    Assert.Fail("Unsupported cell direction.");
                    return;
            }

            Assert.Less(
                Vector3.Distance(
                    wall.position,
                    expectedPosition),
                0.001f);

            Assert.Less(
                Quaternion.Angle(
                    wall.rotation,
                    Quaternion.Euler(0f, expectedRotation, 0f)),
                0.001f);
        }

        private static void SetPrivateField(
            object target,
            string fieldName,
            object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance |
                BindingFlags.NonPublic);

            Assert.IsNotNull(field);
            field.SetValue(target, value);
        }
    }
}
