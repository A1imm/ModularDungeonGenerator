
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DungeonGeneration.Core;
using DungeonGeneration.Generation;
using UnityEditor;

namespace DungeonGeneration.Editor
{
    public static class DungeonStressTestRunner
    {
        private const int TotalSeeds = 10000;
        private const int SeedsPerUpdate = 10;
        private const int MaxLoggedFailures = 20;

        private const int GridWidth = 60;
        private const int GridHeight = 60;
        private const int RoomCount = 10;
        private const int MinRoomSize = 3;
        private const int MaxRoomSize = 6;
        private const int RoomSpacing = 1;
        private const int MaxAttempts = 10000;
        private const double LoopProbability = 0.15;

        private static readonly DungeonLayoutGenerator Generator =
            new DungeonLayoutGenerator();

        private static readonly DungeonConnectivityValidator Validator =
            new DungeonConnectivityValidator();

        private static readonly Stopwatch Timer = new Stopwatch();

        private static readonly List<string> FailureDetails =
            new List<string>();

        private static bool isRunning;
        private static int currentSeed;
        private static int successful;
        private static int generationFailures;
        private static int connectivityFailures;

        private static double totalGenerationMs;
        private static double maxGenerationMs;
        private static int maxTimeSeed;

        [MenuItem("Tools/Dungeon Generator/Run 10,000 Seed Test")]
        private static void StartTest()
        {
            if (isRunning)
                return;

            ResetStatistics();

            isRunning = true;
            EditorApplication.update += ProcessBatch;

            UnityEngine.Debug.Log(
                $"Starting dungeon stress test: {TotalSeeds} seeds.");
        }

        [MenuItem(
            "Tools/Dungeon Generator/Run 10,000 Seed Test",
            true)]
        private static bool ValidateStartTest()
        {
            return !isRunning && !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        private static void ResetStatistics()
        {
            currentSeed = 0;
            successful = 0;
            generationFailures = 0;
            connectivityFailures = 0;

            totalGenerationMs = 0;
            maxGenerationMs = 0;
            maxTimeSeed = 0;

            FailureDetails.Clear();
        }

        private static void ProcessBatch()
        {
            try
            {
                int batchEnd = Math.Min(
                    currentSeed + SeedsPerUpdate,
                    TotalSeeds
                );

                while (currentSeed < batchEnd)
                {
                    TestSeed(currentSeed);
                    currentSeed++;
                }

                bool cancelled =
                    EditorUtility.DisplayCancelableProgressBar(
                        "Dungeon Stress Test",
                        $"Testing seed {currentSeed} / {TotalSeeds}",
                        (float)currentSeed / TotalSeeds
                    );

                if (cancelled || currentSeed == TotalSeeds)
                    FinishTest(cancelled);
            }
            catch (Exception exception)
            {
                FinishTest(true);
                UnityEngine.Debug.LogException(exception);
            }
        }

        private static void TestSeed(int seed)
        {
            DungeonLayout layout;

            try
            {
                Timer.Restart();

                layout = Generator.Generate(
                    seed,
                    GridWidth,
                    GridHeight,
                    RoomCount,
                    MinRoomSize,
                    MaxRoomSize,
                    RoomSpacing,
                    MaxAttempts,
                    LoopProbability
                );

                Timer.Stop();

                double elapsedMs = Timer.Elapsed.TotalMilliseconds;

                totalGenerationMs += elapsedMs;

                if (elapsedMs > maxGenerationMs)
                {
                    maxGenerationMs = elapsedMs;
                    maxTimeSeed = seed;
                }
            }
            catch (Exception exception)
            {
                Timer.Stop();
                generationFailures++;

                RecordFailure(
                    seed,
                    "Generation",
                    $"{exception.GetType().Name}: {exception.Message}"
                );

                return;
            }

            try
            {
                if (!Validator.AreAllRoomsReachable(
                    layout.Grid,
                    layout.Rooms))
                {
                    connectivityFailures++;

                    RecordFailure(
                        seed,
                        "Connectivity",
                        "At least one room is unreachable."
                    );

                    return;
                }

                successful++;
            }
            catch (Exception exception)
            {
                connectivityFailures++;

                RecordFailure(
                    seed,
                    "Validation",
                    $"{exception.GetType().Name}: {exception.Message}"
                );
            }
        }

        private static void RecordFailure(
            int seed,
            string category,
            string reason)
        {
            if (FailureDetails.Count >= MaxLoggedFailures)
                return;

            FailureDetails.Add(
                $"Seed {seed} [{category}]: {reason}"
            );
        }

        private static void FinishTest(bool cancelled)
        {
            if (!isRunning)
                return;

            isRunning = false;

            EditorApplication.update -= ProcessBatch;
            EditorUtility.ClearProgressBar();

            PrintReport(cancelled);
        }

        private static void PrintReport(bool cancelled)
        {
            int completed = currentSeed;
            int completedGenerations =
                completed - generationFailures;

            double successRate = completed > 0
                ? 100.0 * successful / completed
                : 0.0;

            double averageMs = completedGenerations > 0
                ? totalGenerationMs / completedGenerations
                : 0.0;

            var report = new StringBuilder();

            report.AppendLine("=== DUNGEON STRESS TEST ===");
            report.AppendLine(
                $"Status: {(cancelled ? "INCOMPLETE" : "COMPLETED")}");

            report.AppendLine($"Tested seeds: {completed}/{TotalSeeds}");
            report.AppendLine($"Successful: {successful}");
            report.AppendLine($"Generation failures: {generationFailures}");
            report.AppendLine($"Connectivity failures: {connectivityFailures}");
            report.AppendLine($"Success rate: {successRate:F2}%");

            report.AppendLine(
                $"Average generation time: {averageMs:F3} ms");

            report.AppendLine(
                $"Maximum generation time: {maxGenerationMs:F3} ms");

            if (completedGenerations > 0)
                report.AppendLine($"Slowest seed: {maxTimeSeed}");

            if (FailureDetails.Count > 0)
            {
                report.AppendLine("--- Sample failures ---");

                foreach (string failure in FailureDetails)
                    report.AppendLine(failure);

                report.AppendLine(
                    $"Showing first {FailureDetails.Count} failures.");
            }

            if (!cancelled &&
                generationFailures == 0 &&
                connectivityFailures == 0)
            {
                report.AppendLine(
                    "RESULT: PASS - All tested seeds generated " +
                    "connected dungeon layouts.");
            }
            else
            {
                report.AppendLine(
                    "RESULT: NOT PASSED or incomplete.");
            }

            UnityEngine.Debug.Log(report.ToString());
        }
    }
}
