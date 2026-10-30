
using System;
using System.Collections.Generic;
using DungeonGeneration.Core;

namespace DungeonGeneration.Roles
{
    public sealed class RoomRoleAssigner
    {
        private readonly RoomStartSelector startSelector =
            new RoomStartSelector();

        private readonly RoomBossSelector bossSelector =
            new RoomBossSelector();

        private readonly RoomExitSelector exitSelector =
            new RoomExitSelector();

        public IReadOnlyList<RoomRoleAssignment> AssignRoles(
            DungeonLayout layout)
        {
            if (layout == null)
                throw new ArgumentNullException(nameof(layout));

            return AssignRoles(
                layout.FinalGraph,
                layout.Rooms);
        }

        public IReadOnlyList<RoomRoleAssignment> AssignRoles(
            RoomGraph graph,
            IReadOnlyList<DungeonRoom> rooms)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            if (rooms == null)
                throw new ArgumentNullException(nameof(rooms));

            if (rooms.Count < 3)
            {
                throw new ArgumentException(
                    "Role assignment requires at least three rooms.",
                    nameof(rooms));
            }

            // Step 1: select the three special rooms.
            int startRoomId = startSelector.SelectStartRoomId(
                graph, rooms);

            int bossRoomId = bossSelector.SelectBossRoomId(
                graph, rooms, startRoomId);

            int exitRoomId = exitSelector.SelectExitRoomId(
                graph, rooms, startRoomId, bossRoomId);

            // Defensive check: special rooms must be distinct.
            if (startRoomId == bossRoomId ||
                startRoomId == exitRoomId ||
                bossRoomId == exitRoomId)
            {
                throw new InvalidOperationException(
                    "Special room IDs must be unique.");
            }

            // Step 2: create exactly one assignment per room.
            var assignments = new List<RoomRoleAssignment>(
                rooms.Count);

            // Sorted IDs ensure stable output order,
            // regardless of the input collection order.
            var sortedRoomIds = new SortedSet<int>();

            foreach (DungeonRoom room in rooms)
            {
                if (room == null)
                {
                    throw new ArgumentException(
                        "Room collection contains null.",
                        nameof(rooms));
                }

                if (!sortedRoomIds.Add(room.Id))
                {
                    throw new ArgumentException(
                        $"Duplicate room ID: {room.Id}.",
                        nameof(rooms));
                }
            }

            foreach (int roomId in sortedRoomIds)
            {
                RoomRole role = RoomRole.Normal;

                if (roomId == startRoomId)
                {
                    role = RoomRole.Start;
                }
                else if (roomId == bossRoomId)
                {
                    role = RoomRole.Boss;
                }
                else if (roomId == exitRoomId)
                {
                    role = RoomRole.Exit;
                }

                assignments.Add(
                    new RoomRoleAssignment(roomId, role));
            }

            return assignments.AsReadOnly();
        }
    }
}
