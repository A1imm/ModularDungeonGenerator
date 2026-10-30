
using System;
using System.Collections.Generic;
using DungeonGeneration.Core;

namespace DungeonGeneration.Roles
{
    public sealed class RoomProgressionAssigner
    {
        private readonly RoomStartSelector startSelector =
            new RoomStartSelector();

        private readonly RoomGraphDistanceAnalyzer distanceAnalyzer =
            new RoomGraphDistanceAnalyzer();

        public IReadOnlyList<RoomRoleAssignment> AssignRoles(
            DungeonLayout layout)
        {
            if (layout == null)
                throw new ArgumentNullException(nameof(layout));

            return AssignRoles(layout.FinalGraph, layout.Rooms);
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
                throw new ArgumentException(
                    "At least three rooms are required.",
                    nameof(rooms));

            var roomIds = new SortedSet<int>();

            foreach (DungeonRoom room in rooms)
            {
                if (room == null ||
                    graph.GetRoom(room.Id) == null)
                {
                    throw new ArgumentException(
                        "Rooms must exist in the graph.",
                        nameof(rooms));
                }

                if (!roomIds.Add(room.Id))
                    throw new ArgumentException(
                        "Duplicate room ID.",
                        nameof(rooms));
            }

            int startId = startSelector.SelectStartRoomId(
                graph, rooms);

            Dictionary<int, int> distances =
                distanceAnalyzer.CalculateDistances(
                    graph, startId);

            int bestBoss = -1;
            int bestExit = -1;
            int bestExitDistance = -1;
            int bestBossDistance = -1;

            foreach (int bossId in roomIds)
            {
                if (bossId == startId)
                    continue;

                HashSet<int> reachable =
                    GetReachableWithoutRoom(
                        graph, startId, bossId);

                foreach (int exitId in roomIds)
                {
                    if (exitId == startId ||
                        exitId == bossId ||
                        reachable.Contains(exitId))
                    {
                        continue;
                    }

                    int exitDistance = distances[exitId];
                    int bossDistance = distances[bossId];

                    bool better =
                        exitDistance > bestExitDistance ||
                        (exitDistance == bestExitDistance &&
                         bossDistance > bestBossDistance) ||
                        (exitDistance == bestExitDistance &&
                         bossDistance == bestBossDistance &&
                         (bestBoss == -1 ||
                          bossId < bestBoss)) ||
                        (exitDistance == bestExitDistance &&
                         bossDistance == bestBossDistance &&
                         bossId == bestBoss &&
                         exitId < bestExit);

                    if (!better)
                        continue;

                    bestBoss = bossId;
                    bestExit = exitId;
                    bestExitDistance = exitDistance;
                    bestBossDistance = bossDistance;
                }
            }

            if (bestBoss == -1 || bestExit == -1)
            {
                throw new InvalidOperationException(
                    "No mandatory Boss-to-Exit progression " +
                    "exists in the current room graph.");
            }

            var assignments =
                new List<RoomRoleAssignment>(roomIds.Count);

            foreach (int roomId in roomIds)
            {
                RoomRole role = RoomRole.Normal;

                if (roomId == startId)
                    role = RoomRole.Start;
                else if (roomId == bestBoss)
                    role = RoomRole.Boss;
                else if (roomId == bestExit)
                    role = RoomRole.Exit;

                assignments.Add(
                    new RoomRoleAssignment(roomId, role));
            }

            return assignments.AsReadOnly();
        }

        public bool MustPassThroughBoss(
            RoomGraph graph,
            int startId,
            int bossId,
            int exitId)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            if (startId == bossId ||
                startId == exitId ||
                bossId == exitId)
            {
                return false;
            }

            if (graph.GetRoom(startId) == null ||
                graph.GetRoom(bossId) == null ||
                graph.GetRoom(exitId) == null)
            {
                return false;
            }

            Dictionary<int, int> distances =
                distanceAnalyzer.CalculateDistances(graph, startId);

            if (!distances.ContainsKey(bossId) ||
                !distances.ContainsKey(exitId))
            {
                return false;
            }

            HashSet<int> reachable =
                GetReachableWithoutRoom(
                    graph, startId, bossId);

            return !reachable.Contains(exitId);
        }

        private static HashSet<int> GetReachableWithoutRoom(
            RoomGraph graph,
            int startId,
            int blockedRoomId)
        {
            var visited = new HashSet<int>();

            if (startId == blockedRoomId)
                return visited;

            var queue = new Queue<int>();

            visited.Add(startId);
            queue.Enqueue(startId);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();

                foreach (int neighbor in graph.GetNeighbors(current))
                {
                    if (neighbor == blockedRoomId ||
                        !visited.Add(neighbor))
                    {
                        continue;
                    }

                    queue.Enqueue(neighbor);
                }
            }

            return visited;
        }
    }
}
