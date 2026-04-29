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

        private const int RingCapacity = 20;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static readonly LogEntry[] _ring = new LogEntry[RingCapacity];
        private static int _ringHead;
        private static int _ringCount;
#endif

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
        /// Copies the most recent log entries (oldest first) into <paramref name="dest"/>.
        /// Returns the number of entries written. No allocation.
        /// </summary>
        internal static int GetRecentEntries(LogEntry[] dest)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            int count = Math.Min(_ringCount, dest.Length);
            for (int i = 0; i < count; i++)
            {
                // (_ringHead - _ringCount) is the oldest slot; adding RingCapacity keeps it positive.
                int idx = (_ringHead - _ringCount + i + RingCapacity) % RingCapacity;
                dest[i] = _ring[idx];
            }
            return count;
#else
            return 0;
#endif
        }

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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _ring[_ringHead] = new LogEntry(level, cat, text, Time.realtimeSinceStartup);
            _ringHead = (_ringHead + 1) % RingCapacity;
            if (_ringCount < RingCapacity)
                _ringCount++;
#endif
        }
    }
}
