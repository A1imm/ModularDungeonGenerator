
using System;
using DungeonGeneration.Generation;
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
                    Execute(controller, () => controller.Generate());
                }

                if (GUILayout.Button("Clear Dungeon"))
                {
                    Execute(controller, () => controller.Clear());
                }
            }

            EditorGUILayout.Space();

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
