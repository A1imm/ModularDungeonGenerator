
using DungeonGeneration.Core;
using DungeonGeneration.Generation;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class DungeonLayoutGeneratorTests
    {
        private DungeonLayout Generate(int seed)
        {
            return new DungeonLayoutGenerator().Generate(
                seed: seed,
                gridWidth: 60,
                gridHeight: 60,
                roomCount: 10,
                minRoomSize: 3,
                maxRoomSize: 6,
                roomSpacing: 1,
                maxAttempts: 10000,
                loopProbability: 0.15
            );
        }

        [Test]
        public void Generate_CreatesCompleteLayout()
        {
            DungeonLayout layout = Generate(12345);

            Assert.AreEqual(10, layout.Rooms.Count);
            Assert.AreEqual(60, layout.Grid.Width);
            Assert.AreEqual(60, layout.Grid.Height);

            Assert.AreEqual(
                45,
                layout.CandidateGraph.ConnectionCount
            );

            Assert.AreEqual(
                9,
                layout.MinimumSpanningTree.ConnectionCount
            );

            Assert.GreaterOrEqual(
                layout.FinalGraph.ConnectionCount,
                9
            );
        }

        [Test]
        public void Generate_ResultIsFullyConnected()
        {
            DungeonLayout layout = Generate(12345);

            var validator = new DungeonConnectivityValidator();

            Assert.IsTrue(
                validator.AreAllRoomsReachable(
                    layout.Grid,
                    layout.Rooms
                )
            );
        }

        [Test]
        public void Generate_SameSeed_ReturnsIdenticalGrid()
        {
            DungeonLayout first = Generate(12345);
            DungeonLayout second = Generate(12345);

            for (int x = 0; x < first.Grid.Width; x++)
            {
                for (int y = 0; y < first.Grid.Height; y++)
                {
                    var position = new GridPosition(x, y);

                    Assert.AreEqual(
                        first.Grid.GetCell(position),
                        second.Grid.GetCell(position)
                    );
                }
            }

            CollectionAssert.AreEqual(
                first.FinalGraph.Connections,
                second.FinalGraph.Connections
            );
        }
    }
}
