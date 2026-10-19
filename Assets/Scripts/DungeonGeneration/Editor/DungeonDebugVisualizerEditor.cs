
using DungeonGeneration.Debugging;
using UnityEditor;
using UnityEngine;

namespace DungeonGeneration.Editor
{
    [CustomEditor(typeof(DungeonDebugVisualizer))]
    public sealed class DungeonDebugVisualizerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var visualizer = (DungeonDebugVisualizer)target;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Generate Dungeon Rooms"))
            {
                visualizer.Generate();
                FrameDungeon(visualizer);
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Randomize Seed"))
            {
                Undo.RecordObject(visualizer, "Randomize Dungeon Seed");

                int newSeed = System.Guid.NewGuid().GetHashCode();
                visualizer.Seed = newSeed;

                EditorUtility.SetDirty(visualizer);
                visualizer.Generate();
                FrameDungeon(visualizer);

                SceneView.RepaintAll();
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField(
                "Generated Rooms",
                visualizer.GeneratedRoomCount.ToString()
            );

            EditorGUILayout.LabelField(
                "Generation Time",
                $"{visualizer.GenerationTimeMs:F3} ms"
            );
        }
        private static void FrameDungeon(DungeonDebugVisualizer visualizer)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;

            if (sceneView == null)
                return;

            Vector3 center = visualizer.transform.position +
                new Vector3(
                    visualizer.GridWidth * visualizer.CellSize / 2f,
                    0f,
                    visualizer.GridHeight * visualizer.CellSize / 2f
                );

            Vector3 size = new Vector3(
                visualizer.GridWidth * visualizer.CellSize,
                1f,
                visualizer.GridHeight * visualizer.CellSize
            );

            Bounds bounds = new Bounds(center, size);

            sceneView.Frame(bounds, false);
            sceneView.Repaint();
        }
    }
}
