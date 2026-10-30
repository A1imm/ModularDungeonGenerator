
using System;
using System.Linq;
using System.Reflection;
using DungeonGeneration.Core;
using DungeonGeneration.Generation;
using DungeonGeneration.Geometry;
using DungeonGeneration.Roles;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DungeonGeneration.Tests
{
    public class DungeonGenerationControllerRoleTests
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

            DungeonGeometryGenerator geometryGenerator =
                root.GetComponent<DungeonGeometryGenerator>();

            floorPrefab = new GameObject("TestFloor");
            wallPrefab = new GameObject("TestWall");

            moduleSet =
                ScriptableObject.CreateInstance<DungeonModuleSet>();

            SetField(moduleSet, "floorPrefab", floorPrefab);
            SetField(moduleSet, "wallPrefab", wallPrefab);

            SetField(
                geometryGenerator,
                "moduleSet",
                moduleSet);
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
        public void Generate_AssignsRoleToEveryRoom()
        {
            controller.Generate();

            Assert.IsNotNull(controller.CurrentLayout);

            Assert.AreEqual(
                controller.CurrentLayout.Rooms.Count,
                controller.CurrentRoles.Count);

            Assert.AreEqual(
                1,
                controller.CurrentRoles.Count(
                    r => r.Role == RoomRole.Start));

            Assert.AreEqual(
                1,
                controller.CurrentRoles.Count(
                    r => r.Role == RoomRole.Boss));

            Assert.AreEqual(
                1,
                controller.CurrentRoles.Count(
                    r => r.Role == RoomRole.Exit));

            Assert.AreEqual(
                controller.CurrentLayout.Rooms.Count - 3,
                controller.CurrentRoles.Count(
                    r => r.Role == RoomRole.Normal));

            int startId =
                controller.GetRoomId(RoomRole.Start);

            int bossId =
                controller.GetRoomId(RoomRole.Boss);

            int exitId =
                controller.GetRoomId(RoomRole.Exit);

            Assert.AreNotEqual(startId, bossId);
            Assert.AreNotEqual(startId, exitId);
            Assert.AreNotEqual(bossId, exitId);

            Assert.AreEqual(
                RoomRole.Start,
                controller.GetRoomRole(startId));

            Assert.AreEqual(
                RoomRole.Boss,
                controller.GetRoomRole(bossId));

            Assert.AreEqual(
                RoomRole.Exit,
                controller.GetRoomRole(exitId));
        }

        [Test]
        public void GenerateTwice_ProducesSameRoles()
        {
            controller.Generate();

            RoomRoleAssignment[] firstRoles =
                controller.CurrentRoles.ToArray();

            controller.Generate();

            CollectionAssert.AreEqual(
                firstRoles,
                controller.CurrentRoles.ToArray());

            Assert.IsNotNull(
                root.transform.Find("Generated_Dungeon"));

            Assert.IsNull(
                root.transform.Find(
                    "Generated_Dungeon_Pending"));
        }

        [Test]
        public void Clear_RemovesLayoutGeometryAndRoles()
        {
            controller.Generate();
            controller.Clear();

            Assert.IsNull(controller.CurrentLayout);

            Assert.AreEqual(
                0,
                controller.CurrentRoles.Count);

            Assert.AreEqual(
                -1,
                controller.GetRoomId(RoomRole.Start));

            Assert.AreEqual(
                0,
                controller.GeneratedFloorCount);

            Assert.AreEqual(
                0,
                controller.GeneratedWallCount);

            Assert.IsNull(
                root.transform.Find("Generated_Dungeon"));
        }

        [Test]
        public void InvalidRoomCount_PreservesPreviousDungeon()
        {
            controller.Generate();

            DungeonLayout previousLayout =
                controller.CurrentLayout;

            RoomRoleAssignment[] previousRoles =
                controller.CurrentRoles.ToArray();

            Transform previousGeometry =
                root.transform.Find("Generated_Dungeon");

            int previousFloors =
                controller.GeneratedFloorCount;

            int previousWalls =
                controller.GeneratedWallCount;

            // Use reflection to simulate invalid serialized data.
            SetField(controller, "roomCount", 2);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => controller.Generate());

            Assert.AreSame(
                previousLayout,
                controller.CurrentLayout);

            CollectionAssert.AreEqual(
                previousRoles,
                controller.CurrentRoles.ToArray());

            Assert.AreSame(
                previousGeometry,
                root.transform.Find("Generated_Dungeon"));

            Assert.AreEqual(
                previousFloors,
                controller.GeneratedFloorCount);

            Assert.AreEqual(
                previousWalls,
                controller.GeneratedWallCount);
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
                $"Missing private field: {fieldName}");

            field.SetValue(target, value);
        }
    }
}
