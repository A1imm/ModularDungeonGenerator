
using System.Reflection;
using DungeonGeneration.Core;
using DungeonGeneration.Geometry;
using NUnit.Framework;
using UnityEngine;

namespace DungeonGeneration.Tests
{
    public class DungeonGeometryGeneratorTests
    {
        private GameObject root;
        private GameObject floorPrefab;
        private GameObject wallPrefab;

        private DungeonModuleSet moduleSet;
        private DungeonGeometryGenerator generator;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("TestDungeonGeometry");

            generator =
                root.AddComponent<DungeonGeometryGenerator>();

            floorPrefab = new GameObject("TestFloorPrefab");
            wallPrefab = new GameObject("TestWallPrefab");

            moduleSet =
                ScriptableObject.CreateInstance<DungeonModuleSet>();

            SetPrivateField(moduleSet, "floorPrefab", floorPrefab);
            SetPrivateField(moduleSet, "wallPrefab", wallPrefab);

            SetPrivateField(generator, "moduleSet", moduleSet);
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
        public void GenerateGeometry_SingleRoom_CreatesFourWalls()
        {
            var grid = new DungeonGrid(1, 1);

            grid.SetCell(
                new GridPosition(0, 0),
                CellType.Room);

            generator.GenerateGeometry(grid);

            Assert.AreEqual(1, generator.GeneratedFloorCount);
            Assert.AreEqual(4, generator.GeneratedWallCount);

            Assert.AreEqual(
                1,
                root.transform.Find("Generated_Dungeon/Floors").childCount);

            Assert.AreEqual(
                4,
                root.transform.Find("Generated_Dungeon/Walls").childCount);
        }

        [Test]
        public void GenerateGeometry_AdjacentCells_ShareOpenEdge()
        {
            var grid = new DungeonGrid(2, 1);

            grid.SetCell(
                new GridPosition(0, 0),
                CellType.Room);

            grid.SetCell(
                new GridPosition(1, 0),
                CellType.Corridor);

            generator.GenerateGeometry(grid);

            Assert.AreEqual(2, generator.GeneratedFloorCount);
            Assert.AreEqual(6, generator.GeneratedWallCount);

            Transform walls =
                root.transform.Find("Generated_Dungeon/Walls");

            Assert.IsNull(
                walls.Find("Wall_0_0_East"));

            Assert.IsNull(
                walls.Find("Wall_1_0_West"));
        }

        [Test]
        public void GenerateGeometry_SingleRoom_PlacesWallsCorrectly()
        {
            var grid = new DungeonGrid(1, 1);

            grid.SetCell(
                new GridPosition(0, 0),
                CellType.Room);

            generator.GenerateGeometry(grid);

            Transform walls =
                root.transform.Find("Generated_Dungeon/Walls");

            AssertWall(
                walls.Find("Wall_0_0_North"),
                new Vector3(2f, 0f, 4f),
                0f);

            AssertWall(
                walls.Find("Wall_0_0_East"),
                new Vector3(4f, 0f, 2f),
                90f);

            AssertWall(
                walls.Find("Wall_0_0_South"),
                new Vector3(2f, 0f, 0f),
                0f);

            AssertWall(
                walls.Find("Wall_0_0_West"),
                new Vector3(0f, 0f, 2f),
                90f);
        }

        [Test]
        public void GenerateGeometry_EmptyGrid_CreatesNoModules()
        {
            var grid = new DungeonGrid(3, 3);

            generator.GenerateGeometry(grid);

            Assert.AreEqual(0, generator.GeneratedFloorCount);
            Assert.AreEqual(0, generator.GeneratedWallCount);
        }

        [Test]
        public void GenerateGeometry_RepeatedCall_DoesNotDuplicate()
        {
            var grid = new DungeonGrid(1, 1);

            grid.SetCell(
                new GridPosition(0, 0),
                CellType.Room);

            generator.GenerateGeometry(grid);
            generator.GenerateGeometry(grid);

            Assert.AreEqual(1, generator.GeneratedFloorCount);
            Assert.AreEqual(4, generator.GeneratedWallCount);

            Assert.AreEqual(
                1,
                root.transform.Find("Generated_Dungeon/Floors").childCount);

            Assert.AreEqual(
                4,
                root.transform.Find("Generated_Dungeon/Walls").childCount);
        }

        [Test]
        public void ClearGeometry_RemovesFloorsAndWalls()
        {
            var grid = new DungeonGrid(1, 1);

            grid.SetCell(
                new GridPosition(0, 0),
                CellType.Room);

            generator.GenerateGeometry(grid);
            generator.ClearGeometry();

            Assert.AreEqual(0, generator.GeneratedFloorCount);
            Assert.AreEqual(0, generator.GeneratedWallCount);

            Assert.IsNull(root.transform.Find("Generated_Dungeon"));
        }

        private static void AssertWall(
            Transform wall,
            Vector3 expectedPosition,
            float expectedYRotation)
        {
            Assert.IsNotNull(wall);

            Assert.Less(
                Vector3.Distance(
                    wall.position,
                    expectedPosition),
                0.001f);

            Assert.Less(
                Quaternion.Angle(
                    wall.rotation,
                    Quaternion.Euler(
                        0f,
                        expectedYRotation,
                        0f)),
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
