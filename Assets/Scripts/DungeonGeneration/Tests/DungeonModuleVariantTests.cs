
using System.Reflection;
using DungeonGeneration.Core;
using DungeonGeneration.Geometry;
using NUnit.Framework;
using UnityEngine;

namespace DungeonGeneration.Tests
{
    public class DungeonModuleVariantTests
    {
        private DungeonModuleSet moduleSet;

        private GameObject baseFloor;
        private GameObject variantFloor;
        private GameObject baseWall;
        private GameObject variantWall;

        [SetUp]
        public void SetUp()
        {
            moduleSet =
                ScriptableObject.CreateInstance<DungeonModuleSet>();

            baseFloor = new GameObject("BaseFloor");
            variantFloor = new GameObject("VariantFloor");
            baseWall = new GameObject("BaseWall");
            variantWall = new GameObject("VariantWall");

            // Mark variants to identify them after instantiation.
            new GameObject("VariantMarker")
                .transform.SetParent(variantFloor.transform);

            new GameObject("VariantMarker")
                .transform.SetParent(variantWall.transform);

            SetField(moduleSet, "floorPrefab", baseFloor);
            SetField(moduleSet, "wallPrefab", baseWall);

            SetField(moduleSet, "floorVariants",
                new[] { variantFloor });

            SetField(moduleSet, "wallVariants",
                new[] { variantWall });
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(moduleSet);
            UnityEngine.Object.DestroyImmediate(baseFloor);
            UnityEngine.Object.DestroyImmediate(variantFloor);
            UnityEngine.Object.DestroyImmediate(baseWall);
            UnityEngine.Object.DestroyImmediate(variantWall);
        }

        [Test]
        public void SameSeed_ReturnsSamePrefabSelections()
        {
            const int seed = 12345;

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    Assert.AreSame(
                        moduleSet.GetFloorPrefab(seed, x, y),
                        moduleSet.GetFloorPrefab(seed, x, y));

                    foreach (CellDirection direction in
                        new[]
                        {
                            CellDirection.North,
                            CellDirection.East,
                            CellDirection.South,
                            CellDirection.West
                        })
                    {
                        Assert.AreSame(
                            moduleSet.GetWallPrefab(
                                seed, x, y, direction),
                            moduleSet.GetWallPrefab(
                                seed, x, y, direction));
                    }
                }
            }
        }

        [Test]
        public void VariantSelection_UsesBaseAndAdditionalPrefabs()
        {
            bool foundBaseFloor = false;
            bool foundVariantFloor = false;
            bool foundBaseWall = false;
            bool foundVariantWall = false;

            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    GameObject floor =
                        moduleSet.GetFloorPrefab(12345, x, y);

                    foundBaseFloor |= floor == baseFloor;
                    foundVariantFloor |= floor == variantFloor;

                    GameObject wall = moduleSet.GetWallPrefab(
                        12345, x, y, CellDirection.North);

                    foundBaseWall |= wall == baseWall;
                    foundVariantWall |= wall == variantWall;
                }
            }

            Assert.IsTrue(foundBaseFloor);
            Assert.IsTrue(foundVariantFloor);
            Assert.IsTrue(foundBaseWall);
            Assert.IsTrue(foundVariantWall);
        }

        [Test]
        public void GeometryRegeneration_SameSeedPreservesVariants()
        {
            GameObject root = new GameObject("TestGeometry");

            try
            {
                var generator =
                    root.AddComponent<DungeonGeometryGenerator>();

                SetField(generator, "moduleSet", moduleSet);

                var grid = new DungeonGrid(4, 4);

                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        grid.SetCell(
                            new GridPosition(x, y),
                            CellType.Room);
                    }
                }

                const int seed = 12345;

                generator.GenerateGeometry(grid, seed);

                bool[,] firstFloorVariants = new bool[4, 4];

                Transform floors = root.transform.Find(
                    "Generated_Dungeon/Floors");

                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        Transform floor =
                            floors.Find($"Floor_{x}_{y}");

                        Assert.IsNotNull(floor);

                        firstFloorVariants[x, y] =
                            floor.Find("VariantMarker") != null;
                    }
                }

                generator.GenerateGeometry(grid, seed);

                floors = root.transform.Find(
                    "Generated_Dungeon/Floors");

                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        Transform floor =
                            floors.Find($"Floor_{x}_{y}");

                        Assert.IsNotNull(floor);

                        bool isVariant =
                            floor.Find("VariantMarker") != null;

                        Assert.AreEqual(
                            firstFloorVariants[x, y],
                            isVariant,
                            $"Variant changed at ({x}, {y}).");
                    }
                }

                Assert.AreEqual(16, generator.GeneratedFloorCount);
                Assert.AreEqual(16, generator.GeneratedWallCount);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
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

            Assert.IsNotNull(field);
            field.SetValue(target, value);
        }
    }
}
