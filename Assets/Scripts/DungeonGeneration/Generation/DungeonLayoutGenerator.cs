
using System;
using DungeonGeneration.Core;

namespace DungeonGeneration.Generation
{
    public sealed class DungeonLayoutGenerator
    {
        private readonly RoomPlacementGenerator roomPlacement =
            new RoomPlacementGenerator();

        private readonly RoomGraphBuilder graphBuilder =
            new RoomGraphBuilder();

        private readonly MinimumSpanningTreeBuilder mstBuilder =
            new MinimumSpanningTreeBuilder();

        private readonly RoomLoopGenerator loopGenerator =
            new RoomLoopGenerator();

        private readonly CorridorGenerator corridorGenerator =
            new CorridorGenerator();

        private readonly DungeonConnectivityValidator validator =
            new DungeonConnectivityValidator();

        public DungeonLayout Generate(
            int seed,
            int gridWidth,
            int gridHeight,
            int roomCount,
            int minRoomSize,
            int maxRoomSize,
            int roomSpacing,
            int maxAttempts,
            double loopProbability)
        {
            if (roomCount < 1)
                throw new ArgumentOutOfRangeException(
                    nameof(roomCount),
                    "Dungeon must contain at least one room.");

            // 1. Generate room positions and dimensions.
            var rooms = roomPlacement.Generate(
                seed,
                gridWidth,
                gridHeight,
                roomCount,
                minRoomSize,
                maxRoomSize,
                roomSpacing,
                maxAttempts
            );

            // 2. Build possible room connections.
            RoomGraph candidates =
                graphBuilder.BuildCompleteGraph(rooms);

            // 3. Select the minimum spanning tree.
            RoomGraph mst = mstBuilder.Build(
                candidates,
                rooms
            );

            // 4. Add optional loops using an independent seed.
            int loopSeed = unchecked(seed ^ 0x5F3759DF);

            RoomGraph finalGraph = loopGenerator.AddLoops(
                candidates,
                mst,
                rooms,
                loopSeed,
                loopProbability
            );

            // 5. Create room and corridor cells.
            DungeonGrid grid = corridorGenerator.Generate(
                gridWidth,
                gridHeight,
                rooms,
                finalGraph
            );

            // 6. Validate the physical grid.
            if (!validator.AreAllRoomsReachable(grid, rooms))
            {
                throw new InvalidOperationException(
                    "Generated dungeon is not fully connected.");
            }

            return new DungeonLayout(
                seed,
                rooms,
                candidates,
                mst,
                finalGraph,
                grid
            );
        }
    }
}
