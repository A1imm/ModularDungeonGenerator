
using System;
using DungeonGeneration.Roles;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class RoomRoleAssignmentTests
    {
        [Test]
        public void Constructor_StoresRoomIdAndRole()
        {
            var assignment = new RoomRoleAssignment(
                roomId: 3,
                role: RoomRole.Boss);

            Assert.AreEqual(3, assignment.RoomId);
            Assert.AreEqual(RoomRole.Boss, assignment.Role);
        }

        [Test]
        public void Constructor_NegativeRoomId_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new RoomRoleAssignment(-1, RoomRole.Start));
        }

        [Test]
        public void Constructor_InvalidRole_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new RoomRoleAssignment(
                    0,
                    (RoomRole)999));
        }

        [Test]
        public void SameValues_AreEqual()
        {
            var first = new RoomRoleAssignment(
                2, RoomRole.Exit);

            var second = new RoomRoleAssignment(
                2, RoomRole.Exit);

            Assert.IsTrue(first.Equals(second));
            Assert.IsTrue(first.Equals((object)second));

            Assert.AreEqual(first, second);
            Assert.AreEqual(
                first.GetHashCode(),
                second.GetHashCode());
        }

        [Test]
        public void DifferentRoomIds_AreNotEqual()
        {
            var first = new RoomRoleAssignment(
                1, RoomRole.Normal);

            var second = new RoomRoleAssignment(
                2, RoomRole.Normal);

            Assert.AreNotEqual(first, second);
        }

        [Test]
        public void DifferentRoles_AreNotEqual()
        {
            var first = new RoomRoleAssignment(
                1, RoomRole.Start);

            var second = new RoomRoleAssignment(
                1, RoomRole.Boss);

            Assert.AreNotEqual(first, second);
        }

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            var assignment = new RoomRoleAssignment(
                0, RoomRole.Normal);

            Assert.IsFalse(assignment.Equals(null));
        }

        [Test]
        public void ToString_ReturnsReadableDescription()
        {
            var assignment = new RoomRoleAssignment(
                5, RoomRole.Exit);

            Assert.AreEqual(
                "Room 5: Exit",
                assignment.ToString());
        }
    }
}
