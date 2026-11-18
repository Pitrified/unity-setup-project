namespace Game.Tests.PlayMode
{
    using System;
    using System.Collections;
    using System.IO;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.TestTools;
    using Game.Core;
    using Game.Systems;

    /// <summary>
    /// The diagnostics recorder in the running game: after a few seconds of play its report has the
    /// header, statistics with frames and memory, and the boot's log lines. With REPORT_OUT set it
    /// writes the report there, to read a real one without a phone.
    /// </summary>
    public sealed class DiagnosticsReportTests
    {
        private const int WaitFrames = 900;
        private const int PlayFrames = 120; // 4 s at 30 fps

        [SetUp]
        public void SetUp()
        {
            Time.captureDeltaTime = 1f / 30f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.captureDeltaTime = 0f;
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown() => PlayModeTestReset.DestroyPersistentGameManager();

        [UnityTest]
        public IEnumerator Report_FromTheRunningGame_HasEverySection()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.Boot, LoadSceneMode.Single);
            for (int i = 0; i < WaitFrames && (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Menu); i++)
            {
                yield return null;
            }
            var start = GameManager.Instance.StartNewGameAsync();
            while (!start.IsCompleted)
            {
                yield return null;
            }
            for (int i = 0; i < PlayFrames; i++)
            {
                yield return null;
            }

            var recorder = UnityEngine.Object.FindAnyObjectByType<DiagnosticsRecorder>();
            Assert.IsNotNull(recorder, "The Persistent scene has a DiagnosticsRecorder");
            string report = recorder.BuildReport(detailed: true);

            string output = Environment.GetEnvironmentVariable("REPORT_OUT");
            if (!string.IsNullOrEmpty(output))
            {
                File.WriteAllText(output, report);
            }

            StringAssert.Contains(Application.productName, report.Split('\n')[0]);
            Assert.That(recorder.FrameMs.Count, Is.GreaterThan(PlayFrames - 1), "every frame counted");
            Assert.That(recorder.MemoryMb.Count, Is.GreaterThan(1), "memory sampled once a second");
            StringAssert.Contains("frame ms", report);
            StringAssert.Contains("memory MB", report);
            StringAssert.Contains("Game scene ready.", report);
            StringAssert.Contains("detail: one line a second", report);
        }
    }
}
