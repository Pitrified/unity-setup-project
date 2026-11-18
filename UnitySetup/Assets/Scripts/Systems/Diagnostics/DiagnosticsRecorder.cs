namespace Game.Systems
{
    using System;
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

        /// <summary>The build stamp this report names.</summary>
        public SO_BuildInfo Build => _build;

        internal Histogram FrameMs => _frameMs;
        internal Histogram MemoryMb => _memoryMb;
        internal Histogram HeapMb => _heapMb;
        internal ProblemLog Problems => _problems;
        internal long SlowFrames => _slowFrames;
        internal int GcCollections => GC.CollectionCount(0) - _gcAtStart;

        /// <summary>The whole report as text; <paramref name="detailed"/> adds the per-second series.</summary>
        public string BuildReport(bool detailed) => DiagnosticsReport.Build(this, detailed);

        /// <summary>Sets the build stamp (tests, and the scene step that places the recorder).</summary>
        internal void UseBuild(SO_BuildInfo build) => _build = build;

        private void OnEnable()
        {
            _gcAtStart = GC.CollectionCount(0);
            _secondStart = Time.realtimeSinceStartup;
            Application.logMessageReceived += OnLogMessage;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= OnLogMessage;
        }

        private void Update()
        {
            float now = Time.realtimeSinceStartup;
            RecordFrame(Time.unscaledDeltaTime * 1000f, now);
            if (now - _secondStart >= 1f)
            {
                RecordSecond(now, Profiler.GetTotalAllocatedMemoryLong() / BytesPerMb, GC.GetTotalMemory(false) / BytesPerMb);
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
