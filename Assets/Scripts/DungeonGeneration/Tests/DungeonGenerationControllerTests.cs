
using System.Reflection;
using DungeonGeneration.Generation;
using DungeonGeneration.Geometry;
using NUnit.Framework;
using UnityEngine;

namespace DungeonGeneration.Tests
{
    public class DungeonGenerationControllerTests
    {
        private GameObject root;
        private GameObject floorPrefab;
        private GameObject wallPrefab;

        private DungeonModuleSet moduleSet;
        private DungeonGenerationController controller;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("TestDungeon");

            controller =
                root.AddComponent<DungeonGenerationController>();

            var geometryGenerator =
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
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(floorPrefab);
            Object.DestroyImmediate(wallPrefab);
            Object.DestroyImmediate(moduleSet);
        }

        [Test]
        public void Generate_CreatesLayoutAndGeometry()
        {
            controller.Generate();

            Assert.IsNotNull(controller.CurrentLayout);

            Assert.AreEqual(
                10,
                controller.CurrentLayout.Rooms.Count);

            Assert.Greater(controller.GeneratedFloorCount, 0);
            Assert.Greater(controller.GeneratedWallCount, 0);

            Assert.IsNotNull(
                root.transform.Find("Generated_Dungeon/Floors"));

            Assert.IsNotNull(
                root.transform.Find("Generated_Dungeon/Walls"));
        }

        [Test]
        public void Clear_RemovesGeneratedDungeon()
        {
            controller.Generate();
            controller.Clear();

            Assert.IsNull(controller.CurrentLayout);

            Assert.AreEqual(
                0,
                controller.GeneratedFloorCount);

            Assert.AreEqual(
                0,
                controller.GeneratedWallCount);

            Assert.IsNull(
                root.transform.Find("Generated_Dungeon"));
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
