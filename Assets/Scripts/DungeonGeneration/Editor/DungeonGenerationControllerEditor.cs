
using System;
using System.Collections.Generic;
using DungeonGeneration.Generation;
using DungeonGeneration.Roles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DungeonGeneration.Editor
{
    [CustomEditor(typeof(DungeonGenerationController))]
    public sealed class DungeonGenerationControllerEditor
        : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            var controller =
                (DungeonGenerationController)target;

            using (new EditorGUI.DisabledScope(
                EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (GUILayout.Button("Generate Dungeon"))
                {
                    Execute(
                        controller,
                        () => controller.Generate());
                }

                if (GUILayout.Button("Clear Dungeon"))
                {
                    Execute(
                        controller,
                        () => controller.Clear());
                }
            }

            EditorGUILayout.Space();

            DrawGeometrySummary(controller);

            EditorGUILayout.Space();

            DrawRoomRoles(controller);
            
            EditorGUILayout.Space();

            DrawRoomDifficulty(controller);
        }

        private static void DrawRoomDifficulty(
            DungeonGenerationController controller)
        {
            EditorGUILayout.LabelField(
                "Room Difficulty",
                EditorStyles.boldLabel);

            if (controller.CurrentLayout == null ||
                controller.CurrentDifficulties.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Generate a dungeon to calculate room difficulty.",
                    MessageType.Info);

                return;
            }

            int[] counts = new int[6];

            foreach (var assignment in controller.CurrentDifficulties)
            {
                counts[assignment.DifficultyLevel]++;
            }

            for (int level = 1; level <= 5; level++)
            {
                EditorGUILayout.LabelField(
                    $"Difficulty {level}",
                    counts[level].ToString());
            }

            int startId = controller.GetRoomId(RoomRole.Start);
            int bossId = controller.GetRoomId(RoomRole.Boss);
            int exitId = controller.GetRoomId(RoomRole.Exit);

            EditorGUILayout.Space();

            DrawSpecialRoomDifficulty(
                controller,
                "Start",
                startId);

            DrawSpecialRoomDifficulty(
                controller,
                "Boss",
                bossId);

            DrawSpecialRoomDifficulty(
                controller,
                "Exit",
                exitId);
        }

        private static void DrawSpecialRoomDifficulty(
            DungeonGenerationController controller,
            string label,
            int roomId)
        {
            if (roomId < 0)
                return;

            int difficulty =
                controller.GetRoomDifficulty(roomId);

            int distance =
                controller.GetRoomDistanceFromStart(roomId);

            EditorGUILayout.LabelField(
                $"{label} Room",
                $"ID {roomId} | Difficulty {difficulty} | Distance {distance}");
        }

        private static void DrawGeometrySummary(
            DungeonGenerationController controller)
        {
            EditorGUILayout.LabelField(
                "Generated Geometry",
                EditorStyles.boldLabel);

            EditorGUILayout.LabelField(
                "Floors",
                controller.GeneratedFloorCount.ToString());

            EditorGUILayout.LabelField(
                "Walls",
                controller.GeneratedWallCount.ToString());
        }

        private static void DrawRoomRoles(
            DungeonGenerationController controller)
        {
            EditorGUILayout.LabelField(
                "Room Roles",
                EditorStyles.boldLabel);

            if (controller.CurrentLayout == null)
            {
                EditorGUILayout.HelpBox(
                    "Generate a dungeon to calculate room roles.",
                    MessageType.Info);

                return;
            }

            IReadOnlyList<RoomRoleAssignment> roles =
                controller.CurrentRoles;

            int normalCount = 0;

            foreach (RoomRoleAssignment assignment in roles)
            {
                if (assignment.Role == RoomRole.Normal)
                    normalCount++;
            }

            EditorGUILayout.LabelField(
                "Total Rooms",
                roles.Count.ToString());

            EditorGUILayout.LabelField(
                "Start Room ID",
                controller.GetRoomId(RoomRole.Start).ToString());

            EditorGUILayout.LabelField(
                "Boss Room ID",
                controller.GetRoomId(RoomRole.Boss).ToString());

            EditorGUILayout.LabelField(
                "Exit Room ID",
                controller.GetRoomId(RoomRole.Exit).ToString());

            EditorGUILayout.LabelField(
                "Normal Rooms",
                normalCount.ToString());

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(
                "Progression Topology",
                controller.UsedTreeFallback
                    ? "Spanning tree fallback"
                    : "Original final graph");

            int startId = controller.GetRoomId(RoomRole.Start);
            int bossId = controller.GetRoomId(RoomRole.Boss);
            int exitId = controller.GetRoomId(RoomRole.Exit);

            var progressionAssigner = new RoomProgressionAssigner();

            bool mandatoryBoss =
                progressionAssigner.MustPassThroughBoss(
                    controller.CurrentLayout.FinalGraph,
                    startId,
                    bossId,
                    exitId);

            EditorGUILayout.LabelField(
                "Boss Required for Exit",
                mandatoryBoss ? "Yes" : "No");
        }

        private static void Execute(
            DungeonGenerationController controller,
            Action action)
        {
            try
            {
                action();

                EditorSceneManager.MarkSceneDirty(
                    controller.gameObject.scene);

                SceneView.RepaintAll();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
