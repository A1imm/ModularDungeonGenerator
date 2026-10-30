
using System.Linq;
using System.Reflection;
using DungeonGeneration.Generation;
using DungeonGeneration.Geometry;
using DungeonGeneration.Roles;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DungeonGeneration.Tests
{
    public class DungeonProgressionIntegrationTests
    {
        [TestCase(12345)]
        [TestCase(54321)]
        [TestCase(99999)]
        [TestCase(2026)]
        public void GeneratedDungeon_RequiresBossBeforeExit(int seed)
        {
            var root = new GameObject("ProgressionTest");
            var floor = new GameObject("TestFloor");
            var wall = new GameObject("TestWall");

            var modules =
                ScriptableObject.CreateInstance<DungeonModuleSet>();

            try
            {
                var controller =
                    root.AddComponent<DungeonGenerationController>();

                var geometry =
                    root.GetComponent<DungeonGeometryGenerator>();

                SetField(modules, "floorPrefab", floor);
                SetField(modules, "wallPrefab", wall);
                SetField(geometry, "moduleSet", modules);
                SetField(controller, "seed", seed);

                controller.Generate();

                int start =
                    controller.GetRoomId(RoomRole.Start);

                int boss =
                    controller.GetRoomId(RoomRole.Boss);

                int exit =
                    controller.GetRoomId(RoomRole.Exit);

                var verifier = new RoomProgressionAssigner();

                Assert.IsTrue(
                    verifier.MustPassThroughBoss(
                        controller.CurrentLayout.FinalGraph,
                        start,
                        boss,
                        exit));

                Assert.AreEqual(
                    5,
                    controller.GetRoomDifficulty(boss));

                Assert.AreEqual(
                    controller.CurrentLayout.Rooms.Count,
                    controller.CurrentRoles.Count);

                Assert.AreEqual(
                    controller.CurrentLayout.Rooms.Count,
                    controller.CurrentDifficulties.Count);

                Assert.IsNotNull(
                    root.transform.Find("Generated_Dungeon"));
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(floor);
                Object.DestroyImmediate(wall);
                Object.DestroyImmediate(modules);
            }
        }

        private static void SetField(
            object target,
            string name,
            object value)
        {
            FieldInfo field = target.GetType().GetField(
                name,
                BindingFlags.Instance |
                BindingFlags.NonPublic);

            Assert.IsNotNull(field);
            field.SetValue(target, value);
        }
    }
}
