
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
    public class DungeonGenerationDifficultyIntegrationTests
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
        public void Generate_AssignsDifficultyToEveryRoom()
        {
            controller.Generate();

            Assert.IsNotNull(controller.CurrentLayout);

            Assert.AreEqual(
                controller.CurrentLayout.Rooms.Count,
                controller.CurrentDifficulties.Count);

            foreach (DungeonRoom room in controller.CurrentLayout.Rooms)
            {
                int difficulty =
                    controller.GetRoomDifficulty(room.Id);

                Assert.That(
                    difficulty,
                    Is.InRange(1, 5));
            }

            Assert.AreEqual(
                controller.CurrentLayout.Rooms.Count,
                controller.CurrentDifficulties
                    .Select(a => a.RoomId)
                    .Distinct()
                    .Count());
        }

        [Test]
        public void StartHasMinimumDifficulty_BossHasMaximum()
        {
            controller.Generate();

            int startId =
                controller.GetRoomId(RoomRole.Start);

            int bossId =
                controller.GetRoomId(RoomRole.Boss);

            Assert.AreEqual(
                1,
                controller.GetRoomDifficulty(startId));

            Assert.AreEqual(
                0,
                controller.GetRoomDistanceFromStart(startId));

            Assert.AreEqual(
                5,
                controller.GetRoomDifficulty(bossId));

            Assert.Greater(
                controller.GetRoomDistanceFromStart(bossId),
                0);
        }

        [TestCase(12345)]
        [TestCase(54321)]
        [TestCase(99999)]
        [TestCase(2026)]
        public void GeneratedDungeon_RolesAndDifficultyAreConsistent(
            int seed)
        {
            SetField(controller, "seed", seed);

            controller.Generate();

            Assert.AreEqual(
                seed,
                controller.CurrentLayout.Seed);

            int startId =
                controller.GetRoomId(RoomRole.Start);

            int bossId =
                controller.GetRoomId(RoomRole.Boss);

            int exitId =
                controller.GetRoomId(RoomRole.Exit);

            Assert.AreNotEqual(startId, bossId);
            Assert.AreNotEqual(startId, exitId);
            Assert.AreNotEqual(bossId, exitId);

            var distanceAnalyzer =
                new RoomGraphDistanceAnalyzer();

            var expectedDistances =
                distanceAnalyzer.CalculateDistances(
                    controller.CurrentLayout.FinalGraph,
                    startId);

            Assert.AreEqual(
                controller.CurrentLayout.Rooms.Count,
                expectedDistances.Count);

            foreach (RoomDifficultyAssignment assignment
                     in controller.CurrentDifficulties)
            {
                Assert.AreEqual(
                    expectedDistances[assignment.RoomId],
                    assignment.DistanceFromStart);

                Assert.That(
                    assignment.DifficultyLevel,
                    Is.InRange(1, 5));
            }

            Assert.AreEqual(
                5,
                controller.GetRoomDifficulty(bossId));
        }

        [Test]
        public void GenerateTwice_PreservesDifficultyAssignments()
        {
            controller.Generate();

            RoomDifficultyAssignment[] first =
                controller.CurrentDifficulties.ToArray();

            controller.Generate();

            CollectionAssert.AreEqual(
                first,
                controller.CurrentDifficulties.ToArray());
        }

        [Test]
        public void Clear_RemovesDifficultyData()
        {
            controller.Generate();
            controller.Clear();

            Assert.IsNull(controller.CurrentLayout);
            Assert.AreEqual(0, controller.CurrentRoles.Count);
            Assert.AreEqual(0, controller.CurrentDifficulties.Count);

            Assert.Throws<ArgumentException>(() =>
                controller.GetRoomDifficulty(0));

            Assert.IsNull(
                root.transform.Find("Generated_Dungeon"));
        }

        [Test]
        public void FailedGeneration_PreservesPreviousDifficulty()
        {
            controller.Generate();

            DungeonLayout previousLayout =
                controller.CurrentLayout;

            RoomDifficultyAssignment[] previousDifficulties =
                controller.CurrentDifficulties.ToArray();

            RoomRoleAssignment[] previousRoles =
                controller.CurrentRoles.ToArray();

            Transform previousGeometry =
                root.transform.Find("Generated_Dungeon");

            SetField(controller, "roomCount", 2);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                controller.Generate());

            Assert.AreSame(
                previousLayout,
                controller.CurrentLayout);

            CollectionAssert.AreEqual(
                previousDifficulties,
                controller.CurrentDifficulties.ToArray());

            CollectionAssert.AreEqual(
                previousRoles,
                controller.CurrentRoles.ToArray());

            Assert.AreSame(
                previousGeometry,
                root.transform.Find("Generated_Dungeon"));
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
