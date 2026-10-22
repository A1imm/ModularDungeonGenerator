
using System;
using UnityEngine;

namespace DungeonGeneration.Geometry
{
    public sealed class DungeonGeometryHierarchy
    {
        public const string RootName = "Generated_Dungeon";
        public const string PendingRootName =
            "Generated_Dungeon_Pending";

        public const string FloorsName = "Floors";
        public const string WallsName = "Walls";
        public const string DecorationsName = "Decorations";

        public Transform Root { get; }
        public Transform Floors { get; }
        public Transform Walls { get; }
        public Transform Decorations { get; }

        public DungeonGeometryHierarchy(
            Transform parent,
            string rootName = RootName)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            if (string.IsNullOrWhiteSpace(rootName))
                throw new ArgumentException(
                    "Root name cannot be empty.",
                    nameof(rootName));

            GameObject root = new GameObject(rootName);

            Root = root.transform;
            Root.SetParent(parent, false);

            Floors = CreateChild(FloorsName);
            Walls = CreateChild(WallsName);
            Decorations = CreateChild(DecorationsName);
        }

        private Transform CreateChild(string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(Root, false);
            return child.transform;
        }
    }
}
