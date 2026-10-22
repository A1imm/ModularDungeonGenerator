
using DungeonGeneration.Geometry;
using NUnit.Framework;
using UnityEngine;

namespace DungeonGeneration.Tests
{
    public class DungeonGeometryHierarchyTests
    {
        private GameObject parent;

        [SetUp]
        public void SetUp()
        {
            parent = new GameObject("DungeonGeometry");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(parent);
        }

        [Test]
        public void Constructor_CreatesExpectedHierarchy()
        {
            var hierarchy =
                new DungeonGeometryHierarchy(parent.transform);

            Assert.AreSame(
                hierarchy.Root,
                parent.transform.Find("Generated_Dungeon"));

            Assert.AreSame(
                hierarchy.Floors,
                parent.transform.Find("Generated_Dungeon/Floors"));

            Assert.AreSame(
                hierarchy.Walls,
                parent.transform.Find("Generated_Dungeon/Walls"));

            Assert.AreSame(
                hierarchy.Decorations,
                parent.transform.Find("Generated_Dungeon/Decorations"));

            Assert.AreEqual(3, hierarchy.Root.childCount);
        }

        [Test]
        public void Constructor_ContainersHaveIdentityLocalTransforms()
        {
            var hierarchy =
                new DungeonGeometryHierarchy(parent.transform);

            Transform[] containers =
            {
                hierarchy.Root,
                hierarchy.Floors,
                hierarchy.Walls,
                hierarchy.Decorations
            };

            foreach (Transform container in containers)
            {
                Assert.AreEqual(
                    Vector3.zero,
                    container.localPosition);

                Assert.Less(
                    Quaternion.Angle(
                        Quaternion.identity,
                        container.localRotation),
                    0.001f);

                Assert.AreEqual(
                    Vector3.one,
                    container.localScale);
            }
        }
    }
}
