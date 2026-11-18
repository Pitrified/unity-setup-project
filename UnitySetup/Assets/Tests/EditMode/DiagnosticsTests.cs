namespace Game.Tests.EditMode
{
    using System;
    using NUnit.Framework;
    using UnityEngine;
    using Game.Systems;

    /// <summary>
    /// The diagnostics report's parts: histogram statistics against exact ones, problems kept apart
    /// and de-duplicated, and the report's sections in order.
    /// </summary>
    public sealed class DiagnosticsTests
    {
        [SetUp]
        public void SetUp()
        {
            Log.ResetForTesting();
        }

        [Test]
        public void Histogram_QuantilesWithinOneBinOfExact()
        {
            var h = new Histogram(400, 0.5f);
            var rng = new System.Random(5);
            var values = new float[5000];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = (float)(rng.NextDouble() * rng.NextDouble() * 120.0);
                h.Add(values[i]);
            }
            Array.Sort(values);
            foreach (float q in new[] { 0.05f, 0.5f, 0.95f })
            {
                float exact = values[(int)Math.Ceiling(q * values.Length) - 1];
                Assert.AreEqual(exact, h.Quantile(q), 0.5f, $"q {q}");
            }
            Assert.AreEqual(values[0], h.Min, 1e-5f);
            Assert.AreEqual(values[values.Length - 1], h.Max, 1e-5f);
        }

        [Test]
        public void Histogram_EmptyIsNaN_OverflowIsExactMax()
        {
            var h = new Histogram(10, 1f);
            Assert.IsNaN(h.Quantile(0.5f));
            Assert.IsNaN(h.Mean);
            h.Add(2f);
            h.Add(250f);
            Assert.AreEqual(250f, h.Quantile(1f), 1e-5f, "past the last bin, the quantile is the exact max");
        }

        [Test]
        public void ProblemLog_DeduplicatesCountsAndCaps()
        {
            var log = new ProblemLog();
            string stack = "at A.B ()\nat C.D ()\nat E.F ()\nat G.H ()\nat I.J ()\nat K.L ()\nat M.N ()\nat O.P ()\n";
            log.Add("Exception", "NullReferenceException: x", stack, 1f);
            log.Add("Exception", "NullReferenceException: x", stack, 4f);
            Assert.AreEqual(1, log.Entries.Count);
            Assert.AreEqual(2, log.Entries[0].Count);
            Assert.AreEqual(1f, log.Entries[0].First);
            Assert.AreEqual(4f, log.Entries[0].Last);
            Assert.AreEqual(ProblemLog.StackLines, log.Entries[0].Stack.Split('\n').Length, "the top of the stack only");

            for (int i = 0; i < ProblemLog.MaxDistinct + 3; i++)
            {
                log.Add("Error", $"error {i}", "", 5f);
            }
            Assert.AreEqual(ProblemLog.MaxDistinct, log.Entries.Count);
            Assert.AreEqual(4, log.Dropped, "the 1 exception + 9 errors were kept; 4 more counted only");
        }

        [Test]
        public void Files_RotateTheLastSessionToPrevious()
        {
            string folder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "diagnostics-test-" + Guid.NewGuid().ToString("N"));
            try
            {
                var files = new DiagnosticsFiles(folder);
                Assert.IsNull(files.ReadPrevious(), "no previous session yet");
                files.WriteCurrent("session one, first");
                files.WriteCurrent("session one, last");
                files.Rotate();
                Assert.AreEqual("session one, last", files.ReadPrevious(), "the last write of the last session");
                Assert.IsFalse(System.IO.File.Exists(files.CurrentPath), "the new session starts without a report");
                files.WriteCurrent("session two");
                files.Rotate();
                Assert.AreEqual("session two", files.ReadPrevious(), "a rotation replaces the older previous");
            }
            finally
            {
                if (System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.Delete(folder, true);
                }
            }
        }

        [Test]
        public void Share_WithoutAShareSheet_WritesTheFile()
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, "diagnostics", "shared.txt");
            new PlatformDiagnosticsExport().Share("Diagnostics", "the report");
            Assert.AreEqual("the report", System.IO.File.ReadAllText(path));
        }

        [Test]
        public void Report_HasHeaderStatsProblemsAndLogInOrder()
        {
            var go = new GameObject("Recorder");
            try
            {
                var recorder = go.AddComponent<DiagnosticsRecorder>();
                var build = ScriptableObject.CreateInstance<SO_BuildInfo>();
                build.Stamp("abc1234", "2026-11-18T03:00:00Z", "dev", 7);
                recorder.UseBuild(build);
                for (int i = 0; i < 120; i++)
                {
                    recorder.RecordFrame(16f + i % 5, Time.realtimeSinceStartup);
                }
                recorder.RecordSecond(Time.realtimeSinceStartup, 300f, 40f);
                recorder.Problems.Add("Exception", "InvalidOperationException: boom", "at X.Y ()\n", 2f);
                Log.Warn(LogCat.Gameplay, "something odd");

                string report = recorder.BuildReport(detailed: false);
                string[] lines = report.Split('\n');
                StringAssert.Contains(Application.version, lines[0]);
                StringAssert.Contains("dev abc1234 built 2026-11-18T03:00:00Z", lines[0]);
                StringAssert.Contains("(7)", lines[0]);

                int stats = report.IndexOf("stats session: 120 frames", StringComparison.Ordinal);
                int recent = report.IndexOf("stats last 60 s", StringComparison.Ordinal);
                int problems = report.IndexOf("problems: 1 distinct", StringComparison.Ordinal);
                int log = report.IndexOf("log: last 1 lines", StringComparison.Ordinal);
                Assert.That(stats, Is.GreaterThan(0), "session statistics");
                Assert.That(recent, Is.GreaterThan(stats), "then the last minute");
                Assert.That(problems, Is.GreaterThan(recent), "then the problems");
                Assert.That(log, Is.GreaterThan(problems), "then the log");
                StringAssert.Contains("memory MB", report);
                StringAssert.Contains("1x 2.0..2.0 s Exception: InvalidOperationException: boom", report);
                StringAssert.Contains(" W Gameplay something odd", report);
                StringAssert.DoesNotContain("detail:", report);

                StringAssert.Contains("detail: one line a second, last 1 s", recorder.BuildReport(detailed: true));
                UnityEngine.Object.DestroyImmediate(build);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
