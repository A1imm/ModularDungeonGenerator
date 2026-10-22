
using DungeonGeneration.Geometry;
using NUnit.Framework;
using UnityEngine;

namespace DungeonGeneration.Tests
{
    public class DungeonModuleSetTests
    {
        [Test]
        public void IsValid_WithoutPrefabs_ReturnsFalse()
        {
            DungeonModuleSet moduleSet =
                ScriptableObject.CreateInstance<DungeonModuleSet>();

            try
            {
                bool result =
                    moduleSet.IsValid(out string errorMessage);

                Assert.IsFalse(result);
                Assert.AreEqual(
                    "Floor prefab is not assigned.",
                    errorMessage);
            }
            finally
            {
                Object.DestroyImmediate(moduleSet);
            }
        }

        [Test]
        public void IsValid_WithPrefabs_ReturnsTrue()
        {
            DungeonModuleSet moduleSet =
                ScriptableObject.CreateInstance<DungeonModuleSet>();

            GameObject floor = new GameObject("TestFloor");
            GameObject wall = new GameObject("TestWall");

            try
            {
                SetPrivateField(
                    moduleSet,
                    "floorPrefab",
                    floor);

                SetPrivateField(
                    moduleSet,
                    "wallPrefab",
                    wall);

                bool result =
                    moduleSet.IsValid(out string errorMessage);

                Assert.IsTrue(result, errorMessage);
                Assert.IsEmpty(errorMessage);
            }
            finally
            {
                Object.DestroyImmediate(floor);
                Object.DestroyImmediate(wall);
                Object.DestroyImmediate(moduleSet);
            }
        }

        private static void SetPrivateField(
            DungeonModuleSet target,
            string fieldName,
            object value)
        {
            var field = typeof(DungeonModuleSet).GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);

            Assert.IsNotNull(field);
            field.SetValue(target, value);
        }
    }
}
