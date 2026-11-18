namespace Game.Systems
{
    using System;
    using UnityEngine;

    /// <summary>Severity level of a log entry.</summary>
    internal enum LogLevel { Verbose, Info, Warn, Error }

    /// <summary>A single immutable log entry stored in the ring buffer.</summary>
    internal readonly struct LogEntry
    {
        public readonly LogLevel Level;
        public readonly LogCat Category;
        public readonly string Text;
        public readonly float Timestamp;

        internal LogEntry(LogLevel level, LogCat category, string text, float timestamp)
        {
            Level = level;
            Category = category;
            Text = text;
            Timestamp = timestamp;
        }
    }

    /// <summary>
    /// Project-wide logging facade. All code must call this instead of UnityEngine.Debug directly.
    /// Verbose and Info call sites are stripped from Release IL2CPP output via
    /// <see cref="ConditionalAttribute"/>.
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// Optional callback supplying the current game state string for the debug overlay.
        /// Bind this in GameManager.
        /// </summary>
        public static Func<string> GetGameState;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>Invoked by the debug overlay's Reset Position button.</summary>
        public static Action OnDebugResetPosition;

        /// <summary>Invoked by the debug overlay's Reload Scene button.</summary>
        public static Action OnDebugReloadScene;

        /// <summary>Invoked by the debug overlay's Skip Loading button.</summary>
        public static Action OnDebugSkipLoading;
#endif

        /// <summary>
        /// Lines the ring keeps, in every build: what the diagnostics report shows (docs/systems/logging.md).
        /// Release keeps warnings and errors only, since Verbose and Info are compiled out there.
        /// </summary>
        internal const int RingCapacity = 500;

        private static readonly LogEntry[] _ring = new LogEntry[RingCapacity];
        private static int _ringHead;
        private static int _ringCount;

        // ------------------------------------------------------------------ public API

        /// <summary>Detailed trace. Stripped from non-development builds.</summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Verbose(LogCat cat, string message, params object[] args)
        {
            string text = args.Length > 0 ? string.Format(message, args) : message;
            Append(LogLevel.Verbose, cat, text);
            UnityEngine.Debug.Log($"[V][{cat}] {text}");
        }

        /// <summary>Informational message. Stripped from non-development builds.</summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Info(LogCat cat, string message, params object[] args)
        {
            string text = args.Length > 0 ? string.Format(message, args) : message;
            Append(LogLevel.Info, cat, text);
            UnityEngine.Debug.Log($"[{cat}] {text}");
        }

        /// <summary>Warning: unexpected but recoverable. Always compiled.</summary>
        public static void Warn(LogCat cat, string message, params object[] args)
        {
            string text = args.Length > 0 ? string.Format(message, args) : message;
            Append(LogLevel.Warn, cat, text);
            UnityEngine.Debug.LogWarning($"[WARN][{cat}] {text}");
        }

        /// <summary>Error: data or system integrity failure. Always compiled.</summary>
        public static void Error(LogCat cat, string message, params object[] args)
        {
            string text = args.Length > 0 ? string.Format(message, args) : message;
            Append(LogLevel.Error, cat, text);
            UnityEngine.Debug.LogError($"[ERR][{cat}] {text}");
        }

        // ------------------------------------------------------------------ overlay read

        /// <summary>
        /// Copies the newest log entries into <paramref name="dest"/>, oldest of them first:
        /// as many as fit, or as the ring holds. Returns the number written. No allocation.
        /// </summary>
        internal static int GetRecentEntries(LogEntry[] dest)
        {
            int count = Math.Min(_ringCount, dest.Length);
            for (int i = 0; i < count; i++)
            {
                // (_ringHead - count) is the oldest of the newest `count`; adding RingCapacity keeps it positive.
                int idx = (_ringHead - count + i + RingCapacity) % RingCapacity;
                dest[i] = _ring[idx];
            }
            return count;
        }

        /// <summary>Number of entries the ring holds now, up to <see cref="RingCapacity"/>.</summary>
        internal static int Count => _ringCount;

        // ------------------------------------------------------------------ testing

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>Resets the ring buffer to empty. For tests only.</summary>
        public static void ResetForTesting()
        {
            Array.Clear(_ring, 0, _ring.Length);
            _ringHead = 0;
            _ringCount = 0;
        }
#endif

        // ------------------------------------------------------------------ private

        private static void Append(LogLevel level, LogCat cat, string text)
        {
            _ring[_ringHead] = new LogEntry(level, cat, text, Time.realtimeSinceStartup);
            _ringHead = (_ringHead + 1) % RingCapacity;
            if (_ringCount < RingCapacity)
                _ringCount++;
        }
    }
}
