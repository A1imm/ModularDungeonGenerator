
using System;
using System.Reflection;
using DungeonGeneration.Core;
using DungeonGeneration.Generation;
using DungeonGeneration.Geometry;
using NUnit.Framework;
using UnityEngine;

namespace DungeonGeneration.Tests
{
    public class DungeonRegenerationTests
    {
        private GameObject root;
        private GameObject floorPrefab;
        private GameObject wallPrefab;

        private DungeonModuleSet moduleSet;
        private DungeonGeometryGenerator geometryGenerator;
        private DungeonGenerationController controller;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("TestDungeon");

            controller =
                root.AddComponent<DungeonGenerationController>();

            geometryGenerator =
                root.GetComponent<DungeonGeometryGenerator>();

            floorPrefab = new GameObject("TestFloor");
            wallPrefab = new GameObject("TestWall");

            moduleSet =
                ScriptableObject.CreateInstance<DungeonModuleSet>();

            SetPrivateField(
                moduleSet, "floorPrefab", floorPrefab);

            SetPrivateField(
                moduleSet, "wallPrefab", wallPrefab);

            SetPrivateField(
                geometryGenerator, "moduleSet", moduleSet);
        }


        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(floorPrefab);
            UnityEngine.Object.DestroyImmediate(wallPrefab);
            UnityEngine.Object.DestroyImmediate(moduleSet);
        }


        [Test]
        public void GenerateRepeatedly_KeepsOnlyOneDungeon()
        {
            controller.Generate();

            int firstFloorCount =
                controller.GeneratedFloorCount;

            int firstWallCount =
                controller.GeneratedWallCount;

            controller.Generate();
            controller.Generate();

            Assert.AreEqual(
                firstFloorCount,
                controller.GeneratedFloorCount);

            Assert.AreEqual(
                firstWallCount,
                controller.GeneratedWallCount);

            AssertSingleGeneratedDungeon();
        }

        [Test]
        public void ChangeSeed_ReplacesPreviousDungeon()
        {
            controller.Generate();

            Transform previousRoot =
                root.transform.Find("Generated_Dungeon");

            Assert.IsNotNull(previousRoot);

            SetPrivateField(controller, "seed", 54321);

            controller.Generate();

            Assert.AreEqual(54321, controller.CurrentLayout.Seed);

            Transform currentRoot =
                root.transform.Find("Generated_Dungeon");

            Assert.IsNotNull(currentRoot);

            Assert.AreNotSame(
                previousRoot,
                currentRoot);

            AssertSingleGeneratedDungeon();
        }

        [Test]
        public void InvalidConfiguration_PreservesPreviousDungeon()
        {
            controller.Generate();

            DungeonLayout previousLayout =
                controller.CurrentLayout;

            Transform previousRoot =
                root.transform.Find("Generated_Dungeon");

            int previousFloors =
                controller.GeneratedFloorCount;

            int previousWalls =
                controller.GeneratedWallCount;

            // Simulate a broken module configuration.
            SetPrivateField(moduleSet, "floorPrefab", null);

            Assert.Throws<InvalidOperationException>(
                () => controller.Generate());

            Assert.AreSame(
                previousLayout,
                controller.CurrentLayout);

            Assert.AreSame(
                previousRoot,
                root.transform.Find("Generated_Dungeon"));

            Assert.AreEqual(
                previousFloors,
                controller.GeneratedFloorCount);

            Assert.AreEqual(
                previousWalls,
                controller.GeneratedWallCount);
        }

        [Test]
        public void ClearRepeatedly_LeavesCleanHierarchy()
        {
            controller.Generate();

            controller.Clear();
            controller.Clear();

            Assert.IsNull(controller.CurrentLayout);

            Assert.AreEqual(0, controller.GeneratedFloorCount);
            Assert.AreEqual(0, controller.GeneratedWallCount);

            Assert.IsNull(
                root.transform.Find("Generated_Dungeon"));

            Assert.IsNull(
                root.transform.Find(
                    "Generated_Dungeon_Pending"));
        }

        private void AssertSingleGeneratedDungeon()
        {
            int generatedRootCount = 0;

            foreach (Transform child in root.transform)
            {
                if (child.name == "Generated_Dungeon")
                    generatedRootCount++;
            }

            Assert.AreEqual(1, generatedRootCount);

            Assert.IsNull(
                root.transform.Find(
                    "Generated_Dungeon_Pending"));

            Assert.IsNotNull(
                root.transform.Find("Generated_Dungeon/Floors"));

            Assert.IsNotNull(
                root.transform.Find("Generated_Dungeon/Walls"));

            Assert.IsNotNull(
                root.transform.Find(
                    "Generated_Dungeon/Decorations"));
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
