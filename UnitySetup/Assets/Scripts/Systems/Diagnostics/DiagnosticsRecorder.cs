namespace Game.Systems
{
    using System;
    using System.IO;
    using Unity.Profiling;
    using UnityEngine;
    using UnityEngine.Profiling;

    /// <summary>
    /// Collects what the diagnostics report needs, in every build, with fixed buffers and no
    /// allocation per frame: frame times into a session histogram and a last-minute ring, memory
    /// and managed heap once a second into histograms and a 10-minute per-second series, and the
    /// session's exceptions and errors apart from the log ring. <see cref="BuildReport"/> writes it
    /// as text (docs/systems/logging.md, "Diagnostics report"). Lives in the Persistent scene.
    /// </summary>
    public sealed class DiagnosticsRecorder : MonoBehaviour
    {
        /// <summary>Frames in the last-minute window at most (two minutes at 60 fps of headroom).</summary>
        internal const int RecentFrames = 7200;

        /// <summary>Seconds in the per-second series.</summary>
        internal const int SeriesSeconds = 600;

        /// <summary>The window of the "last minute" statistics, seconds.</summary>
        internal const float RecentWindow = 60f;

        /// <summary>A frame slower than this (ms) counts as slow: under 30 fps.</summary>
        internal const float SlowFrameMs = 1000f / 30f;

        private const int FrameBins = 400;     // 0 to 200 ms
        private const float FrameBinMs = 0.5f;
        private const int MemoryBins = 4096;   // 0 to 4 GB
        private const int HeapBins = 2048;     // 0 to 2 GB
        private const float BytesPerMb = 1024f * 1024f;
        private const int GcAllocBins = 1024;    // 0 to 256 kB a frame
        private const float GcAllocBinKb = 0.25f;

        /// <summary>Seconds between writes of the kept report.</summary>
        internal const float SaveInterval = 30f;

        /// <summary>One second of the per-second series.</summary>
        internal struct Second
        {
            public float Time;
            public int Frames;
            public float MeanMs;
            public float MaxMs;
            public float MemoryMb;
            public float HeapMb;
        }

        [SerializeField] private SO_BuildInfo _build;

        private readonly Histogram _frameMs = new Histogram(FrameBins, FrameBinMs);
        private readonly Histogram _memoryMb = new Histogram(MemoryBins, 1f);
        private readonly Histogram _heapMb = new Histogram(HeapBins, 1f);
        private readonly Histogram _gcAllocKb = new Histogram(GcAllocBins, GcAllocBinKb);
        private readonly float[] _recentMs = new float[RecentFrames];
        private readonly float[] _recentAt = new float[RecentFrames];
        private readonly Second[] _series = new Second[SeriesSeconds];
        private readonly ProblemLog _problems = new ProblemLog();

        private int _recentHead;
        private int _recentCount;
        private int _seriesHead;
        private int _seriesCount;
        private long _slowFrames;
        private int _gcAtStart;
        private float _secondStart;
        private int _secondFrames;
        private float _secondSumMs;
        private float _secondMaxMs;
        private DiagnosticsFiles _files;
        private float _nextSave;
        private bool _skipNextFrame;
        private ProfilerRecorder _gcAlloc;

        /// <summary>The build stamp this report names.</summary>
        public SO_BuildInfo Build => _build;

        internal Histogram FrameMs => _frameMs;
        internal Histogram MemoryMb => _memoryMb;
        internal Histogram HeapMb => _heapMb;

        /// <summary>Managed allocation per frame in kB, from the Profiler's counter; empty where it is not available (Release).</summary>
        internal Histogram GcAllocKb => _gcAllocKb;
        internal ProblemLog Problems => _problems;
        internal long SlowFrames => _slowFrames;
        internal int GcCollections => GC.CollectionCount(0) - _gcAtStart;

        /// <summary>The whole report as text; <paramref name="detailed"/> adds the per-second series.</summary>
        public string BuildReport(bool detailed) => DiagnosticsReport.Build(this, detailed);

        /// <summary>Sets the build stamp (tests, and the scene step that places the recorder).</summary>
        internal void UseBuild(SO_BuildInfo build) => _build = build;

        /// <summary>The previous session's kept report, or null when there is none.</summary>
        public string PreviousReport => Guard(() => Files.ReadPrevious(), "reading the previous report");

        /// <summary>Writes the kept report now (the panel, and tests).</summary>
        public void SaveNow(bool detailed)
        {
            string report = BuildReport(detailed);
            Guard(() => { Files.WriteCurrent(report); return report; }, "writing the report");
        }

        /// <summary>Keeps the reports in <paramref name="folder"/> instead of the default (tests).</summary>
        internal void UseFolder(string folder) => _files = new DiagnosticsFiles(folder);

        private DiagnosticsFiles Files => _files ??= new DiagnosticsFiles(Path.Combine(Application.persistentDataPath, "diagnostics"));

        private void OnEnable()
        {
            _gcAtStart = GC.CollectionCount(0);
            _secondStart = Time.realtimeSinceStartup;
            _nextSave = Time.realtimeSinceStartup + SaveInterval;
            Application.logMessageReceived += OnLogMessage;
            _gcAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            Guard(() => { Files.Rotate(); return string.Empty; }, "keeping the last session's report");
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= OnLogMessage;
            _gcAlloc.Dispose();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                SaveNow(detailed: true);
            }
            else
            {
                _skipNextFrame = true;
            }
        }

        private void OnApplicationFocus(bool focused)
        {
            if (focused)
            {
                _skipNextFrame = true;
            }
        }

        private void OnApplicationQuit()
        {
            SaveNow(detailed: true);
        }

        /// <summary>Runs a file step; a failure is a warning, never an exception in the game.</summary>
        private static string Guard(Func<string> step, string what)
        {
            try
            {
                return step();
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Log.Warn(LogCat.Save, "Diagnostics: {0} failed: {1}", what, e.Message);
                return null;
            }
        }

        private void Update()
        {
            float now = Time.realtimeSinceStartup;
            if (_skipNextFrame)
            {
                // The first frame after the app comes back spans the time it was in the background
                // (a share sheet, another app): not a frame the game took.
                _skipNextFrame = false;
            }
            else
            {
                RecordFrame(Time.unscaledDeltaTime * 1000f, now);
                if (_gcAlloc.Valid)
                {
                    // LastValue is the frame before this one: the counter closes at the end of a frame.
                    _gcAllocKb.Add(_gcAlloc.LastValue / 1024f);
                }
            }
            if (now - _secondStart >= 1f)
            {
                RecordSecond(now, Profiler.GetTotalAllocatedMemoryLong() / BytesPerMb, GC.GetTotalMemory(false) / BytesPerMb);
            }
            if (now >= _nextSave)
            {
                _nextSave = now + SaveInterval;
                SaveNow(detailed: false);
            }
        }

        /// <summary>Adds one frame's time (ms) at <paramref name="now"/> seconds.</summary>
        internal void RecordFrame(float ms, float now)
        {
            _frameMs.Add(ms);
            if (ms > SlowFrameMs)
            {
                _slowFrames++;
            }
            _recentMs[_recentHead] = ms;
            _recentAt[_recentHead] = now;
            _recentHead = (_recentHead + 1) % RecentFrames;
            if (_recentCount < RecentFrames)
            {
                _recentCount++;
            }
            _secondFrames++;
            _secondSumMs += ms;
            if (ms > _secondMaxMs)
            {
                _secondMaxMs = ms;
            }
        }

        /// <summary>Closes the current second with the memory readings, into the series and histograms.</summary>
        internal void RecordSecond(float now, float memoryMb, float heapMb)
        {
            _memoryMb.Add(memoryMb);
            _heapMb.Add(heapMb);
            _series[_seriesHead] = new Second
            {
                Time = now,
                Frames = _secondFrames,
                MeanMs = _secondFrames > 0 ? _secondSumMs / _secondFrames : 0f,
                MaxMs = _secondMaxMs,
                MemoryMb = memoryMb,
                HeapMb = heapMb,
            };
            _seriesHead = (_seriesHead + 1) % SeriesSeconds;
            if (_seriesCount < SeriesSeconds)
            {
                _seriesCount++;
            }
            _secondStart = now;
            _secondFrames = 0;
            _secondSumMs = 0f;
            _secondMaxMs = 0f;
        }

        /// <summary>A histogram of the frame times in the last <see cref="RecentWindow"/> seconds before <paramref name="now"/>.</summary>
        internal Histogram RecentFrameMs(float now)
        {
            var recent = new Histogram(FrameBins, FrameBinMs);
            for (int i = 0; i < _recentCount; i++)
            {
                int idx = (_recentHead - 1 - i + RecentFrames) % RecentFrames;
                if (now - _recentAt[idx] > RecentWindow)
                {
                    break;
                }
                recent.Add(_recentMs[idx]);
            }
            return recent;
        }

        /// <summary>Copies the per-second series, oldest first, into <paramref name="dest"/>; returns how many.</summary>
        internal int GetSeries(Second[] dest)
        {
            int count = Math.Min(_seriesCount, dest.Length);
            for (int i = 0; i < count; i++)
            {
                dest[i] = _series[(_seriesHead - count + i + SeriesSeconds) % SeriesSeconds];
            }
            return count;
        }

        private void OnLogMessage(string message, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
            {
                _problems.Add(type.ToString(), message, stackTrace, Time.realtimeSinceStartup);
            }
        }
    }
}
