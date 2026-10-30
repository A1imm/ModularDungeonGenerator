
using System;
using System.Collections.Generic;
using System.Linq;
using DungeonGeneration.Core;
using DungeonGeneration.Generation;
using DungeonGeneration.Roles;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class RoomRoleAssignerTests
    {
        private RoomRoleAssigner assigner;

        [SetUp]
        public void SetUp()
        {
            assigner = new RoomRoleAssigner();
        }

        [Test]
        public void AssignRoles_CreatesOneAssignmentPerRoom()
        {
            DungeonRoom[] rooms = CreateRooms(5);
            RoomGraph graph = CreateLinearGraph(rooms);

            IReadOnlyList<RoomRoleAssignment> result =
                assigner.AssignRoles(graph, rooms);

            Assert.AreEqual(rooms.Length, result.Count);

            Assert.AreEqual(
                rooms.Length,
                result.Select(a => a.RoomId).Distinct().Count());
        }

        [Test]
        public void AssignRoles_CreatesExactlyThreeSpecialRooms()
        {
            DungeonRoom[] rooms = CreateRooms(5);
            RoomGraph graph = CreateLinearGraph(rooms);

            var result = assigner.AssignRoles(graph, rooms);

            Assert.AreEqual(
                1,
                result.Count(a => a.Role == RoomRole.Start));

            Assert.AreEqual(
                1,
                result.Count(a => a.Role == RoomRole.Boss));

            Assert.AreEqual(
                1,
                result.Count(a => a.Role == RoomRole.Exit));

            Assert.AreEqual(
                2,
                result.Count(a => a.Role == RoomRole.Normal));
        }

        [Test]
        public void LinearGraph_AssignsExpectedRoles()
        {
            DungeonRoom[] rooms = CreateRooms(5);
            RoomGraph graph = CreateLinearGraph(rooms);

            var result = assigner.AssignRoles(graph, rooms);

            Assert.AreEqual(
                RoomRole.Start,
                GetRole(result, 0));

            Assert.AreEqual(
                RoomRole.Boss,
                GetRole(result, 4));

            Assert.AreEqual(
                RoomRole.Exit,
                GetRole(result, 3));

            Assert.AreEqual(
                RoomRole.Normal,
                GetRole(result, 1));

            Assert.AreEqual(
                RoomRole.Normal,
                GetRole(result, 2));
        }

        [Test]
        public void SameGraph_ProducesIdenticalAssignments()
        {
            DungeonRoom[] rooms = CreateRooms(5);
            RoomGraph graph = CreateLinearGraph(rooms);

            var first = assigner.AssignRoles(graph, rooms);
            var second = assigner.AssignRoles(graph, rooms);

            CollectionAssert.AreEqual(first, second);
        }

        [Test]
        public void ReversedRoomOrder_DoesNotChangeAssignments()
        {
            DungeonRoom[] rooms = CreateRooms(5);
            RoomGraph graph = CreateLinearGraph(rooms);

            var reversedRooms = rooms.Reverse().ToArray();

            var first = assigner.AssignRoles(graph, rooms);
            var second = assigner.AssignRoles(
                graph, reversedRooms);

            CollectionAssert.AreEqual(first, second);
        }

        [Test]
        public void NonConsecutiveIds_AreSupported()
        {
            var rooms = new[]
            {
                CreateRoom(10),
                CreateRoom(20),
                CreateRoom(30)
            };

            var graph = new RoomGraph(rooms);

            graph.AddConnection(
                new RoomConnection(10, 20, 1));

            graph.AddConnection(
                new RoomConnection(20, 30, 1));

            var result = assigner.AssignRoles(graph, rooms);

            Assert.AreEqual(3, result.Count);

            Assert.AreEqual(RoomRole.Start, GetRole(result, 10));
            Assert.AreEqual(RoomRole.Exit, GetRole(result, 20));
            Assert.AreEqual(RoomRole.Boss, GetRole(result, 30));
        }

        [Test]
        public void TwoRooms_ThrowsArgumentException()
        {
            DungeonRoom[] rooms = CreateRooms(2);
            RoomGraph graph = CreateLinearGraph(rooms);

            Assert.Throws<ArgumentException>(() =>
                assigner.AssignRoles(graph, rooms));
        }

        [Test]
        public void DisconnectedGraph_ThrowsException()
        {
            DungeonRoom[] rooms = CreateRooms(5);
            var graph = new RoomGraph(rooms);

            graph.AddConnection(
                new RoomConnection(0, 1, 1));

            graph.AddConnection(
                new RoomConnection(2, 3, 1));

            Assert.Throws<InvalidOperationException>(() =>
                assigner.AssignRoles(graph, rooms));
        }

        [Test]
        public void NullInputs_ThrowArgumentNullException()
        {
            DungeonRoom[] rooms = CreateRooms(5);
            RoomGraph graph = CreateLinearGraph(rooms);

            Assert.Throws<ArgumentNullException>(() =>
                assigner.AssignRoles(
                    (DungeonLayout)null));

            Assert.Throws<ArgumentNullException>(() =>
                assigner.AssignRoles(null, rooms));

            Assert.Throws<ArgumentNullException>(() =>
                assigner.AssignRoles(graph, null));
        }

        private static RoomRole GetRole(
            IReadOnlyList<RoomRoleAssignment> assignments,
            int roomId)
        {
            return assignments
                .Single(a => a.RoomId == roomId)
                .Role;
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

        private static DungeonRoom[] CreateRooms(int count)
        {
            var rooms = new DungeonRoom[count];

            for (int i = 0; i < count; i++)
            {
                rooms[i] = CreateRoom(i);
            }

            return rooms;
        }

        private static DungeonRoom CreateRoom(int id)
        {
            return new DungeonRoom(
                id,
                new GridPosition(id * 5, 0),
                3,
                3);
        }
    }
}
