
using DungeonGeneration.Geometry;
using UnityEditor;

namespace DungeonGeneration.Editor
{
    [CustomEditor(typeof(DungeonGeometryGenerator))]
    public sealed class DungeonGeometryGeneratorEditor
        : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "Module configuration is assigned here. " +
                "Use DungeonGenerationController on the same " +
                "GameObject to generate or clear the dungeon.",
                MessageType.Info);
        }
    }
}
