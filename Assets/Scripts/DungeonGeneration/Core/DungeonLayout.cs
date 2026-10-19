
using System;
using System.Collections.Generic;

namespace DungeonGeneration.Core
{
    public sealed class DungeonLayout
    {
        public int Seed { get; }

        public IReadOnlyList<DungeonRoom> Rooms { get; }

        public RoomGraph CandidateGraph { get; }
        public RoomGraph MinimumSpanningTree { get; }
        public RoomGraph FinalGraph { get; }

        public DungeonGrid Grid { get; }

        public DungeonLayout(
            int seed,
            IReadOnlyList<DungeonRoom> rooms,
            RoomGraph candidateGraph,
            RoomGraph minimumSpanningTree,
            RoomGraph finalGraph,
            DungeonGrid grid)
        {
            Seed = seed;
            Rooms = Array.AsReadOnly(
                new List<DungeonRoom>(rooms).ToArray()
            );
            CandidateGraph = candidateGraph;
            MinimumSpanningTree = minimumSpanningTree;
            FinalGraph = finalGraph;
            Grid = grid;
        }
    }
}
