
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DungeonGeneration.Generation;
using DungeonGeneration.Geometry;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonGeneration.Editor
{
    public sealed class DungeonGeometryBenchmarkWindow : EditorWindow
    {
        private const int SamplesPerScenario = 10;

        private sealed class Scenario
        {
            public string Name;
            public int GridSize;
            public int RoomCount;

            public int Failures;
            public readonly List<double> LayoutTimes = new List<double>();
            public readonly List<double> GeometryTimes = new List<double>();
            public readonly List<double> TotalTimes = new List<double>();

            public int FloorCount;
            public int WallCount;
            public int GameObjectCount;
            public int ColliderCount;
        }

        private DungeonModuleSet moduleSet;
        private DungeonLayoutGenerator layoutGenerator;
        private DungeonGeometryGenerator geometryGenerator;

        private Scene previewScene;
        private GameObject previewRoot;

        private Scenario[] scenarios;
        private int scenarioIndex;
        private int sampleIndex;
        private bool running;

        private readonly List<string> errors = new List<string>();

        [MenuItem("Tools/Dungeon Generator/Geometry Benchmark")]
        private static void OpenWindow()
        {
            GetWindow<DungeonGeometryBenchmarkWindow>(
                "Dungeon Geometry Benchmark");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(
                "Dungeon Geometry Benchmark",
                EditorStyles.boldLabel);

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(running))
            {
                moduleSet = (DungeonModuleSet)
                    EditorGUILayout.ObjectField(
                        "Module Set",
                        moduleSet,
                        typeof(DungeonModuleSet),
                        false);
            }

            EditorGUILayout.HelpBox(
                "Runs 10 measurements for each of three dungeon sizes. " +
                "Uses a temporary preview scene and does not replace " +
                "the dungeon in your working scene.",
                MessageType.Info);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(
                "Small: 40 x 40, 6 rooms");

            EditorGUILayout.LabelField(
                "Medium: 60 x 60, 10 rooms");

            EditorGUILayout.LabelField(
                "Large: 100 x 100, 20 rooms");

            EditorGUILayout.Space();

            if (!running)
            {
                if (GUILayout.Button("Run Benchmark"))
                    StartBenchmark();
            }
            else
            {
                EditorGUILayout.LabelField(
                    $"Scenario {scenarioIndex + 1}/3, " +
                    $"sample {sampleIndex + 1}/{SamplesPerScenario}");

                if (GUILayout.Button("Cancel Benchmark"))
                    FinishBenchmark(true);
            }
        }

        private void StartBenchmark()
        {
            if (moduleSet == null)
            {
                UnityEngine.Debug.LogError(
                    "Assign a DungeonModuleSet first.");
                return;
            }

            if (!moduleSet.IsValid(out string errorMessage))
            {
                UnityEngine.Debug.LogError(errorMessage);
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                UnityEngine.Debug.LogWarning(
                    "Run the benchmark outside Play Mode.");
                return;
            }

            try
            {
                scenarios = new[]
                {
                    new Scenario
                    {
                        Name = "Small",
                        GridSize = 40,
                        RoomCount = 6
                    },
                    new Scenario
                    {
                        Name = "Medium",
                        GridSize = 60,
                        RoomCount = 10
                    },
                    new Scenario
                    {
                        Name = "Large",
                        GridSize = 100,
                        RoomCount = 20
                    }
                };

                scenarioIndex = 0;
                sampleIndex = 0;
                errors.Clear();

                layoutGenerator = new DungeonLayoutGenerator();

                previewScene =
                    EditorSceneManager.NewPreviewScene();

                previewRoot =
                    new GameObject("GeometryBenchmarkRoot");

                SceneManager.MoveGameObjectToScene(
                    previewRoot,
                    previewScene);

                geometryGenerator =
                    previewRoot.AddComponent<DungeonGeometryGenerator>();

                var serializedGenerator =
                    new SerializedObject(geometryGenerator);

                serializedGenerator
                    .FindProperty("moduleSet")
                    .objectReferenceValue = moduleSet;

                serializedGenerator.ApplyModifiedPropertiesWithoutUndo();

                running = true;
                EditorApplication.update += RunNextSample;

                UnityEngine.Debug.Log(
                    "Dungeon geometry benchmark started.");
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogException(exception);
                Cleanup();
            }
        }

        private void RunNextSample()
        {
            if (!running)
                return;

            try
            {
                Scenario scenario = scenarios[scenarioIndex];

                int seed = 1000 + sampleIndex;

                RunSample(scenario, seed);

                sampleIndex++;

                if (sampleIndex >= SamplesPerScenario)
                {
                    sampleIndex = 0;
                    scenarioIndex++;
                }

                if (scenarioIndex >= scenarios.Length)
                {
                    FinishBenchmark(false);
                    return;
                }

                float progress =
                    (scenarioIndex * SamplesPerScenario + sampleIndex) /
                    (float)(scenarios.Length * SamplesPerScenario);

                bool cancelled =
                    EditorUtility.DisplayCancelableProgressBar(
                        "Dungeon Geometry Benchmark",
                        $"Completed {progress * 100f:F0}%",
                        progress);

                if (cancelled)
                    FinishBenchmark(true);

                Repaint();
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogException(exception);
                FinishBenchmark(true);
            }
        }

        private void RunSample(Scenario scenario, int seed)
        {
            try
            {
                // Cleanup is excluded from the measured generation time.
                geometryGenerator.ClearGeometry();

                var timer = Stopwatch.StartNew();

                var layout = layoutGenerator.Generate(
                    seed: seed,
                    gridWidth: scenario.GridSize,
                    gridHeight: scenario.GridSize,
                    roomCount: scenario.RoomCount,
                    minRoomSize: 3,
                    maxRoomSize: 6,
                    roomSpacing: 1,
                    maxAttempts: 10000,
                    loopProbability: 0.15
                );

                timer.Stop();
                double layoutMs = timer.Elapsed.TotalMilliseconds;

                timer.Restart();

                geometryGenerator.GenerateGeometry(
                    layout.Grid,
                    seed);

                timer.Stop();
                double geometryMs = timer.Elapsed.TotalMilliseconds;

                scenario.LayoutTimes.Add(layoutMs);
                scenario.GeometryTimes.Add(geometryMs);
                scenario.TotalTimes.Add(layoutMs + geometryMs);

                Transform generated = previewRoot.transform.Find(
                    DungeonGeometryHierarchy.RootName);

                if (generated == null)
                    throw new InvalidOperationException(
                        "Generated_Dungeon was not created.");

                scenario.FloorCount =
                    geometryGenerator.GeneratedFloorCount;

                scenario.WallCount =
                    geometryGenerator.GeneratedWallCount;

                scenario.GameObjectCount =
                    generated.GetComponentsInChildren<Transform>(true).Length;

                scenario.ColliderCount =
                    generated.GetComponentsInChildren<Collider>(true).Length;
            }
            catch (Exception exception)
            {
                scenario.Failures++;

                string message =
                    $"{scenario.Name}, seed {seed}: " +
                    $"{exception.GetType().Name}: {exception.Message}";

                errors.Add(message);

                UnityEngine.Debug.LogWarning(message);
            }
        }

        private void FinishBenchmark(bool cancelled)
        {
            if (!running)
                return;

            EditorApplication.update -= RunNextSample;
            running = false;

            EditorUtility.ClearProgressBar();

            PrintReport(cancelled);
            Cleanup();
            Repaint();
        }

        private void PrintReport(bool cancelled)
        {
            var report = new StringBuilder();

            report.AppendLine("=== DUNGEON GEOMETRY BENCHMARK ===");

            report.AppendLine(
                $"Status: {(cancelled ? "INCOMPLETE" : "COMPLETED")}");

            report.AppendLine(
                $"Unity version: {Application.unityVersion}");

            report.AppendLine(
                $"Module set: {moduleSet.name}");

            foreach (Scenario scenario in scenarios)
            {
                report.AppendLine();
                report.AppendLine($"--- {scenario.Name} ---");

                report.AppendLine(
                    $"Grid: {scenario.GridSize} x {scenario.GridSize}");

                report.AppendLine(
                    $"Rooms: {scenario.RoomCount}");

                report.AppendLine(
                    $"Successful samples: {scenario.TotalTimes.Count}");

                report.AppendLine(
                    $"Failures: {scenario.Failures}");

                if (scenario.TotalTimes.Count == 0)
                    continue;

                report.AppendLine(
                    $"Layout avg: {Average(scenario.LayoutTimes):F3} ms");

                report.AppendLine(
                    $"Geometry avg: {Average(scenario.GeometryTimes):F3} ms");

                report.AppendLine(
                    $"Geometry p95: {Percentile(scenario.GeometryTimes, 0.95):F3} ms");

                report.AppendLine(
                    $"Geometry max: {Maximum(scenario.GeometryTimes):F3} ms");

                report.AppendLine(
                    $"Total avg: {Average(scenario.TotalTimes):F3} ms");

                report.AppendLine(
                    $"Floors (last success): {scenario.FloorCount}");

                report.AppendLine(
                    $"Walls (last success): {scenario.WallCount}");

                report.AppendLine(
                    $"GameObjects (last success): {scenario.GameObjectCount}");

                report.AppendLine(
                    $"Colliders (last success): {scenario.ColliderCount}");
            }

            if (errors.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("--- FAILURES ---");

                foreach (string error in errors)
                    report.AppendLine(error);
            }

            UnityEngine.Debug.Log(report.ToString());
        }

        private static double Average(List<double> values)
        {
            double sum = 0;

            foreach (double value in values)
                sum += value;

            return sum / values.Count;
        }

        private static double Maximum(List<double> values)
        {
            double maximum = double.MinValue;

            foreach (double value in values)
                maximum = Math.Max(maximum, value);

            return maximum;
        }

        private static double Percentile(
            List<double> values,
            double percentile)
        {
            var sorted = new List<double>(values);
            sorted.Sort();

            int index = Mathf.Clamp(
                Mathf.CeilToInt((float)(sorted.Count * percentile)) - 1,
                0,
                sorted.Count - 1);

            return sorted[index];
        }

        private void Cleanup()
        {
            EditorApplication.update -= RunNextSample;
            EditorUtility.ClearProgressBar();

            running = false;

            if (previewRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(previewRoot);
                previewRoot = null;
            }

            if (previewScene.IsValid())
            {
                EditorSceneManager.ClosePreviewScene(previewScene);
                previewScene = default(Scene);
            }

            geometryGenerator = null;
            layoutGenerator = null;
        }

        private void OnDisable()
        {
            Cleanup();
        }
    }
}
