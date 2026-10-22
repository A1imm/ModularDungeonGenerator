
using UnityEngine;

namespace DungeonGeneration.Geometry
{
    [CreateAssetMenu(
        fileName = "DungeonModuleSet",
        menuName = "Dungeon Generator/Module Set")]
    public sealed class DungeonModuleSet : ScriptableObject
    {
        [Header("Base Prefabs")]
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject wallPrefab;

        [Header("Optional Variants")]
        [SerializeField]
        private GameObject[] floorVariants = new GameObject[0];

        [SerializeField]
        private GameObject[] wallVariants = new GameObject[0];

        [Header("Module Dimensions")]
        [SerializeField, Min(0.1f)]
        private float cellSize = 4f;

        [SerializeField, Min(0.1f)]
        private float wallHeight = 3f;

        [SerializeField, Min(0.01f)]
        private float wallThickness = 0.2f;

        [SerializeField, Min(0.01f)]
        private float floorThickness = 0.2f;

        public GameObject FloorPrefab => floorPrefab;
        public GameObject WallPrefab => wallPrefab;

        public float CellSize => cellSize;
        public float WallHeight => wallHeight;
        public float WallThickness => wallThickness;
        public float FloorThickness => floorThickness;

        public int FloorVariantCount =>
            1 + (floorVariants?.Length ?? 0);

        public int WallVariantCount =>
            1 + (wallVariants?.Length ?? 0);

        public GameObject GetFloorPrefab(
            int seed,
            int x,
            int y)
        {
            int index = SelectIndex(
                seed, x, y, 0, FloorVariantCount);

            if (index == 0)
                return floorPrefab;

            return floorVariants[index - 1];
        }

        public GameObject GetWallPrefab(
            int seed,
            int x,
            int y,
            CellDirection direction)
        {
            int index = SelectIndex(
                seed, x, y, (int)direction + 1,
                WallVariantCount);

            if (index == 0)
                return wallPrefab;

            return wallVariants[index - 1];
        }

        private static int SelectIndex(
            int seed,
            int x,
            int y,
            int salt,
            int count)
        {
            unchecked
            {
                uint hash = (uint)seed;

                hash ^= (uint)x * 0x9E3779B9u;
                hash ^= (uint)y * 0x85EBCA6Bu;
                hash ^= (uint)salt * 0xC2B2AE35u;

                hash ^= hash >> 16;
                hash *= 0x7FEB352Du;
                hash ^= hash >> 15;
                hash *= 0x846CA68Bu;
                hash ^= hash >> 16;

                return (int)(hash % (uint)count);
            }
        }

        public bool IsValid(out string errorMessage)
        {
            if (floorPrefab == null)
            {
                errorMessage = "Floor prefab is not assigned.";
                return false;
            }

            if (wallPrefab == null)
            {
                errorMessage = "Wall prefab is not assigned.";
                return false;
            }

            if (HasNullEntry(floorVariants))
            {
                errorMessage = "Floor variants contain a missing prefab.";
                return false;
            }

            if (HasNullEntry(wallVariants))
            {
                errorMessage = "Wall variants contain a missing prefab.";
                return false;
            }

            if (cellSize <= 0f)
            {
                errorMessage = "Cell size must be positive.";
                return false;
            }

            if (wallHeight <= 0f ||
                wallThickness <= 0f ||
                floorThickness <= 0f)
            {
                errorMessage =
                    "All module dimensions must be positive.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private static bool HasNullEntry(GameObject[] prefabs)
        {
            if (prefabs == null)
                return false;

            foreach (GameObject prefab in prefabs)
            {
                if (prefab == null)
                    return true;
            }

            return false;
        }

        private void OnValidate()
        {
            cellSize = Mathf.Max(0.1f, cellSize);
            wallHeight = Mathf.Max(0.1f, wallHeight);
            wallThickness = Mathf.Max(0.01f, wallThickness);
            floorThickness = Mathf.Max(0.01f, floorThickness);
        }
    }
}
