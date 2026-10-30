
using DungeonGeneration.Roles;
using NUnit.Framework;

namespace DungeonGeneration.Tests
{
    public class RoomRoleTests
    {
        [Test]
        public void RoomRole_HasExpectedValues()
        {
            Assert.AreEqual(0, (int)RoomRole.Normal);
            Assert.AreEqual(1, (int)RoomRole.Start);
            Assert.AreEqual(2, (int)RoomRole.Boss);
            Assert.AreEqual(3, (int)RoomRole.Exit);
        }

        [Test]
        public void RoomRole_ContainsFourDefinedRoles()
        {
            var roles = System.Enum.GetValues(
                typeof(RoomRole));

            Assert.AreEqual(4, roles.Length);
        }
    }
}
