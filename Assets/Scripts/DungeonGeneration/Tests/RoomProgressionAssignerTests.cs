
using System;
using System.Linq;
using DungeonGeneration.Core;
using DungeonGeneration.Roles;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class RoomProgressionAssignerTests
    {
        private RoomProgressionAssigner assigner;

        [SetUp]
        public void SetUp()
        {
            assigner = new RoomProgressionAssigner();
        }

        [Test]
        public void LinearGraph_BossSeparatesStartFromExit()
        {
            DungeonRoom[] rooms = CreateRooms(5);
            RoomGraph graph = CreateLinearGraph(rooms);

            var roles = assigner.AssignRoles(graph, rooms);

            int start = FindRole(roles, RoomRole.Start);
            int boss = FindRole(roles, RoomRole.Boss);
            int exit = FindRole(roles, RoomRole.Exit);

            Assert.AreEqual(0, start);
            Assert.AreEqual(3, boss);
            Assert.AreEqual(4, exit);

            Assert.IsTrue(
                assigner.MustPassThroughBoss(
                    graph, start, boss, exit));
        }

        [Test]
        public void CycleWithoutArticulation_Throws()
        {
            DungeonRoom[] rooms = CreateRooms(4);
            var graph = new RoomGraph(rooms);

            graph.AddConnection(new RoomConnection(0, 1, 1));
            graph.AddConnection(new RoomConnection(1, 2, 1));
            graph.AddConnection(new RoomConnection(2, 3, 1));
            graph.AddConnection(new RoomConnection(3, 0, 1));

            Assert.Throws<InvalidOperationException>(() =>
                assigner.AssignRoles(graph, rooms));
        }

        [Test]
        public void Cycle_DoesNotTreatBossAsMandatory()
        {
            DungeonRoom[] rooms = CreateRooms(4);
            var graph = new RoomGraph(rooms);

            graph.AddConnection(new RoomConnection(0, 1, 1));
            graph.AddConnection(new RoomConnection(1, 2, 1));
            graph.AddConnection(new RoomConnection(2, 3, 1));
            graph.AddConnection(new RoomConnection(3, 0, 1));

            Assert.IsFalse(
                assigner.MustPassThroughBoss(graph, 0, 1, 2));
        }

        [Test]
        public void StarGraph_SelectsMandatoryCentralBoss()
        {
            DungeonRoom[] rooms = CreateRooms(5);
            var graph = new RoomGraph(rooms);

            for (int id = 1; id < 5; id++)
            {
                graph.AddConnection(
                    new RoomConnection(0, id, 1));
            }

            var roles = assigner.AssignRoles(graph, rooms);

            int start = FindRole(roles, RoomRole.Start);
            int boss = FindRole(roles, RoomRole.Boss);
            int exit = FindRole(roles, RoomRole.Exit);

            Assert.AreEqual(0, boss);

            Assert.IsTrue(
                assigner.MustPassThroughBoss(
                    graph, start, boss, exit));
        }

        [Test]
        public void ReversedRoomOrder_DoesNotChangeRoles()
        {
            DungeonRoom[] rooms = CreateRooms(5);
            RoomGraph graph = CreateLinearGraph(rooms);

            var first = assigner.AssignRoles(graph, rooms);
            var second = assigner.AssignRoles(
                graph,
                rooms.Reverse().ToArray());

            CollectionAssert.AreEqual(first, second);
        }

        [Test]
        public void ThreeRoomChain_HasValidProgression()
        {
            DungeonRoom[] rooms = CreateRooms(3);
            RoomGraph graph = CreateLinearGraph(rooms);

            var roles = assigner.AssignRoles(graph, rooms);

            Assert.AreEqual(3, roles.Count);

            int start = FindRole(roles, RoomRole.Start);
            int boss = FindRole(roles, RoomRole.Boss);
            int exit = FindRole(roles, RoomRole.Exit);

            Assert.IsTrue(
                assigner.MustPassThroughBoss(
                    graph, start, boss, exit));
        }

        private static int FindRole(
            System.Collections.Generic.IReadOnlyList<RoomRoleAssignment> roles,
            RoomRole role)
        {
            return roles.Single(a => a.Role == role).RoomId;
        }

        private static DungeonRoom[] CreateRooms(int count)
        {
            var rooms = new DungeonRoom[count];

            for (int id = 0; id < count; id++)
            {
                rooms[id] = new DungeonRoom(
                    id,
                    new GridPosition(id * 5, 0),
                    3,
                    3);
            }

            return rooms;
        }

        private static RoomGraph CreateLinearGraph(
            DungeonRoom[] rooms)
        {
            var graph = new RoomGraph(rooms);

            for (int i = 0; i < rooms.Length - 1; i++)
            {
                graph.AddConnection(
                    new RoomConnection(
                        rooms[i].Id,
                        rooms[i + 1].Id,
                        1));
            }

            return graph;
        }
    }
}
