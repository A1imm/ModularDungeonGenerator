
using System;
using System.Collections.Generic;

namespace DungeonGeneration.Generation
{
    public sealed class DisjointSet
    {
        private readonly Dictionary<int, int> parent;
        private readonly Dictionary<int, int> rank;

        public DisjointSet(IEnumerable<int> elements)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            parent = new Dictionary<int, int>();
            rank = new Dictionary<int, int>();

            foreach (int element in elements)
            {
                if (parent.ContainsKey(element))
                    throw new ArgumentException("Duplicate element.");

                parent.Add(element, element);
                rank.Add(element, 0);
            }
        }

        public int Find(int element)
        {
            if (!parent.ContainsKey(element))
                throw new ArgumentException(
                    $"Unknown element: {element}");

            if (parent[element] != element)
            {
                parent[element] = Find(parent[element]);
            }

            return parent[element];
        }

        public bool Union(int a, int b)
        {
            int rootA = Find(a);
            int rootB = Find(b);

            if (rootA == rootB)
                return false;

            if (rank[rootA] < rank[rootB])
            {
                parent[rootA] = rootB;
            }
            else if (rank[rootA] > rank[rootB])
            {
                parent[rootB] = rootA;
            }
            else
            {
                parent[rootB] = rootA;
                rank[rootA]++;
            }

            return true;
        }
    }
}
