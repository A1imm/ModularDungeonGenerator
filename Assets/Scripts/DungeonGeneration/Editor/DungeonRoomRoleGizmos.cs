
using DungeonGeneration.Core;
using DungeonGeneration.Generation;
using DungeonGeneration.Roles;
using UnityEditor;
using UnityEngine;

namespace DungeonGeneration.Editor
{
    public static class DungeonRoomRoleGizmos
    {
        [DrawGizmo(GizmoType.Selected)]
        private static void DrawRoomRoles(
            DungeonGenerationController controller,
            GizmoType gizmoType)
        {
            if (!controller.ShowRoomRoles)
                return;

            if (controller.CurrentLayout == null)
                return;

            if (controller.CurrentRoles == null)
                return;

            float cellSize = controller.CellSize;

            float markerHeight = 4.0f;
            float markerRadius = cellSize * 0.18f;

            foreach (RoomRoleAssignment assignment
                     in controller.CurrentRoles)
            {
                if (assignment.Role == RoomRole.Normal &&
                    !controller.ShowNormalRoomRoles)
                {
                    continue;
                }

                DungeonRoom room =
                    controller.CurrentLayout.FinalGraph.GetRoom(
                        assignment.RoomId);

                if (room == null)
                    continue;

                Vector3 center = new Vector3(
                    (room.Center.X + 0.5f) * cellSize,
                    markerHeight,
                    (room.Center.Y + 0.5f) * cellSize
                );

                Color roleColor = GetRoleColor(
                    assignment.Role);

                DrawMarker(
                    center,
                    markerRadius,
                    roleColor);

                DrawLabel(
                    center + Vector3.up * (markerRadius + 0.35f),
                    assignment,
                    roleColor);
            }
        }

        private static void DrawMarker(
            Vector3 center,
            float radius,
            Color color)
        {
            Color previousColor = Gizmos.color;

            Gizmos.color = color;
            Gizmos.DrawSphere(center, radius);

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(center, radius * 1.15f);

            Gizmos.color = previousColor;
        }

        private static void DrawLabel(
            Vector3 position,
            RoomRoleAssignment assignment,
            Color color)
        {
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13
            };

            style.normal.textColor = color;

            string label =
                $"{assignment.Role.ToString().ToUpperInvariant()}\n" +
                $"Room ID: {assignment.RoomId}";

            Handles.Label(position, label, style);
        }

        private static Color GetRoleColor(RoomRole role)
        {
            switch (role)
            {
                case RoomRole.Start:
                    return new Color(0.15f, 0.90f, 0.25f);

                case RoomRole.Boss:
                    return new Color(1.00f, 0.20f, 0.20f);

                case RoomRole.Exit:
                    return new Color(0.20f, 0.55f, 1.00f);

                case RoomRole.Normal:
                    return new Color(0.70f, 0.70f, 0.70f);

                default:
                    return Color.white;
            }
        }
    }
}
