
using DungeonGeneration.Core;
using DungeonGeneration.Generation;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class DungeonDeterminismTests
    {
        [Test]
        public void Generate_SameSeed_ProducesIdenticalLayout()
        {
            var generator = new DungeonLayoutGenerator();

            for (int seed = 0; seed < 20; seed++)
            {
                DungeonLayout first = Generate(generator, seed);
                DungeonLayout second = Generate(generator, seed);

                Assert.AreEqual(
                    first.Rooms.Count,
                    second.Rooms.Count
                );

                for (int i = 0; i < first.Rooms.Count; i++)
                {
                    Assert.AreEqual(
                        first.Rooms[i].Position,
                        second.Rooms[i].Position
                    );

                    Assert.AreEqual(
                        first.Rooms[i].Width,
                        second.Rooms[i].Width
                    );

                    Assert.AreEqual(
                        first.Rooms[i].Height,
                        second.Rooms[i].Height
                    );
                }

                for (int x = 0; x < first.Grid.Width; x++)
                {
                    for (int y = 0; y < first.Grid.Height; y++)
                    {
                        var position = new GridPosition(x, y);

                        Assert.AreEqual(
                            first.Grid.GetCell(position),
                            second.Grid.GetCell(position),
                            $"Grid mismatch at seed {seed}, {x}, {y}"
                        );
                    }
                }

                CollectionAssert.AreEqual(
                    first.FinalGraph.Connections,
                    second.FinalGraph.Connections
                );
            }
        }

        private static DungeonLayout Generate(
            DungeonLayoutGenerator generator,
            int seed)
        {
            return generator.Generate(
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
    }
}
