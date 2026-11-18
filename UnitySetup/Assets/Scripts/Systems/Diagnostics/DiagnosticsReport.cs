namespace Game.Systems
{
    using System.Globalization;
    using System.Text;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Writes the diagnostics report as plain text, the shape of abyss's log button: header lines
    /// (app, build, device, settings, state), a blank line, then the statistics, the problems, the
    /// log lines, and with <c>detailed</c> the per-second series. Built on demand only, so it may
    /// allocate. See docs/systems/logging.md, "Diagnostics report".
    /// </summary>
    public static class DiagnosticsReport
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        /// <summary>The full report from a recorder, with the log ring and the device's description.</summary>
        public static string Build(DiagnosticsRecorder recorder, bool detailed)
        {
            var sb = new StringBuilder(8 * 1024);
            AppendHeader(sb, recorder.Build);
            sb.Append('\n');
            float now = Time.realtimeSinceStartup;
            AppendStats(sb, "session", recorder.FrameMs, recorder.MemoryMb, recorder.HeapMb, now);
            sb.Append("gc ").Append(recorder.GcCollections).Append(" collections | slow frames (over 33 ms) ")
              .Append(recorder.SlowFrames)
              .Append(" (").Append(Percent(recorder.SlowFrames, recorder.FrameMs.Count)).Append(")\n");
            AppendStats(sb, "last 60 s", recorder.RecentFrameMs(now), null, null, now);
            sb.Append('\n');
            AppendProblems(sb, recorder.Problems);
            sb.Append('\n');
            AppendLog(sb);
            if (detailed)
            {
                sb.Append('\n');
                AppendSeries(sb, recorder);
            }
            return sb.ToString();
        }

        /// <summary>App, build, device, screen, settings and state, one topic a line.</summary>
        internal static void AppendHeader(StringBuilder sb, SO_BuildInfo build)
        {
            string commit = build != null ? build.Commit : "unknown";
            string kind = build != null ? build.Kind : "unknown";
            string built = build != null && build.BuiltAtUtc.Length > 0 ? " built " + build.BuiltAtUtc : "";
            string code = build != null && build.VersionCode > 0 ? " (" + build.VersionCode + ")" : "";
            sb.Append(Application.productName).Append(' ').Append(Application.version).Append(code)
              .Append(" | ").Append(kind).Append(' ').Append(commit).Append(built)
              .Append(" | ").Append(Debug.isDebugBuild ? "development" : "release")
              .Append(" | Unity ").Append(Application.unityVersion).Append('\n');
            sb.Append("device ").Append(SystemInfo.deviceModel).Append(" | ").Append(SystemInfo.operatingSystem)
              .Append(" | ").Append(SystemInfo.processorType).Append(" x").Append(SystemInfo.processorCount)
              .Append(" | ").Append(SystemInfo.systemMemorySize).Append(" MB\n");
            sb.Append("gpu ").Append(SystemInfo.graphicsDeviceName).Append(" | ").Append(SystemInfo.graphicsDeviceType)
              .Append(" | ").Append(SystemInfo.graphicsMemorySize).Append(" MB\n");
            sb.Append("screen ").Append(Screen.width).Append('x').Append(Screen.height)
              .Append(" @ ").Append(F0(Screen.currentResolution.refreshRateRatio.value)).Append(" Hz | dpi ")
              .Append(F0(Screen.dpi)).Append(" | ").Append(Screen.orientation).Append('\n');
            string[] qualities = QualitySettings.names;
            int level = QualitySettings.GetQualityLevel();
            sb.Append("quality ").Append(level >= 0 && level < qualities.Length ? qualities[level] : "?")
              .Append(" | target ").Append(Application.targetFrameRate).Append(" fps | vsync ")
              .Append(QualitySettings.vSyncCount).Append('\n');
            sb.Append("session ").Append(F1(Time.realtimeSinceStartup)).Append(" s | state ")
              .Append(Log.GetGameState != null ? Log.GetGameState() : "?").Append(" | scenes");
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                sb.Append(' ').Append(SceneManager.GetSceneAt(i).name);
            }
            sb.Append('\n');
            sb.Append("battery ").Append(SystemInfo.batteryLevel < 0f ? "?" : F0(SystemInfo.batteryLevel * 100f) + "%")
              .Append(' ').Append(SystemInfo.batteryStatus).Append('\n');
        }

        /// <summary>
        /// A statistics block: frames and the time it covers, then a row per metric of min, p05,
        /// median, mean, p95, max. Frame rate is read off the frame times (its p05 is 1000 over the
        /// frame time's p95). Memory rows are left out when null.
        /// </summary>
        internal static void AppendStats(StringBuilder sb, string title, Histogram frameMs, Histogram memoryMb, Histogram heapMb, float now)
        {
            sb.Append("stats ").Append(title).Append(": ").Append(frameMs.Count).Append(" frames");
            if (title == "session")
            {
                sb.Append(" over ").Append(F1(now)).Append(" s");
            }
            sb.Append('\n');
            sb.Append("               min    p05    p50   mean    p95    max\n");
            if (frameMs.Count == 0)
            {
                sb.Append("  (no frames)\n");
                return;
            }
            Row(sb, "frame ms", frameMs.Min, frameMs.Quantile(0.05f), frameMs.Quantile(0.5f), frameMs.Mean, frameMs.Quantile(0.95f), frameMs.Max);
            Row(sb, "fps", Fps(frameMs.Max), Fps(frameMs.Quantile(0.95f)), Fps(frameMs.Quantile(0.5f)),
                1000f * frameMs.Count / (float)frameMs.Sum, Fps(frameMs.Quantile(0.05f)), Fps(frameMs.Min));
            if (memoryMb != null && memoryMb.Count > 0)
            {
                Row(sb, "memory MB", memoryMb.Min, memoryMb.Quantile(0.05f), memoryMb.Quantile(0.5f), memoryMb.Mean, memoryMb.Quantile(0.95f), memoryMb.Max);
            }
            if (heapMb != null && heapMb.Count > 0)
            {
                Row(sb, "heap MB", heapMb.Min, heapMb.Quantile(0.05f), heapMb.Quantile(0.5f), heapMb.Mean, heapMb.Quantile(0.95f), heapMb.Max);
            }
        }

        /// <summary>The distinct problems with count, times and the top of the stack.</summary>
        internal static void AppendProblems(StringBuilder sb, ProblemLog problems)
        {
            sb.Append("problems: ").Append(problems.Entries.Count).Append(" distinct");
            if (problems.Dropped > 0)
            {
                sb.Append(", ").Append(problems.Dropped).Append(" more not kept");
            }
            sb.Append('\n');
            foreach (ProblemLog.Entry e in problems.Entries)
            {
                sb.Append("  ").Append(e.Count).Append("x ").Append(F1(e.First)).Append("..").Append(F1(e.Last))
                  .Append(" s ").Append(e.Kind).Append(": ").Append(e.Message).Append('\n');
                if (e.Stack.Length > 0)
                {
                    foreach (string line in e.Stack.Split('\n'))
                    {
                        sb.Append("     ").Append(line.Trim()).Append('\n');
                    }
                }
            }
        }

        /// <summary>The log ring, oldest first: seconds since start, level, category, text.</summary>
        internal static void AppendLog(StringBuilder sb)
        {
            var lines = new LogEntry[Log.RingCapacity];
            int count = Log.GetRecentEntries(lines);
            sb.Append("log: last ").Append(count).Append(" lines\n");
            for (int i = 0; i < count; i++)
            {
                LogEntry e = lines[i];
                sb.Append(F2(e.Timestamp)).Append(' ').Append(Level(e.Level)).Append(' ')
                  .Append(e.Category).Append(' ').Append(e.Text).Append('\n');
            }
        }

        private static void AppendSeries(StringBuilder sb, DiagnosticsRecorder recorder)
        {
            var series = new DiagnosticsRecorder.Second[DiagnosticsRecorder.SeriesSeconds];
            int count = recorder.GetSeries(series);
            sb.Append("detail: one line a second, last ").Append(count).Append(" s\n");
            sb.Append("  time s  frames  mean ms  max ms  memory MB  heap MB\n");
            for (int i = 0; i < count; i++)
            {
                DiagnosticsRecorder.Second s = series[i];
                sb.Append(F1(s.Time).PadLeft(8)).Append(s.Frames.ToString(Invariant).PadLeft(8))
                  .Append(F1(s.MeanMs).PadLeft(9)).Append(F1(s.MaxMs).PadLeft(8))
                  .Append(F0(s.MemoryMb).PadLeft(11)).Append(F0(s.HeapMb).PadLeft(9)).Append('\n');
            }
        }

        private static void Row(StringBuilder sb, string name, float min, float p05, float p50, float mean, float p95, float max)
        {
            sb.Append(name.PadRight(12));
            foreach (float v in new[] { min, p05, p50, mean, p95, max })
            {
                sb.Append(F1(v).PadLeft(7));
            }
            sb.Append('\n');
        }

        private static float Fps(float ms) => ms > 0f ? 1000f / ms : 0f;

        private static string Percent(long part, long whole) =>
            whole == 0 ? "0%" : (100.0 * part / whole).ToString("0.00", Invariant) + "%";

        private static char Level(LogLevel level) => level switch
        {
            LogLevel.Verbose => 'V',
            LogLevel.Info => 'I',
            LogLevel.Warn => 'W',
            _ => 'E',
        };

        private static string F0(double v) => v.ToString("0", Invariant);
        private static string F1(float v) => v.ToString("0.0", Invariant);
        private static string F2(float v) => v.ToString("0.00", Invariant);
    }
}
