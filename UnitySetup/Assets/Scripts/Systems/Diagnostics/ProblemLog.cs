namespace Game.Systems
{
    using System.Collections.Generic;

    /// <summary>
    /// Exceptions and errors of a session, kept apart from the log ring so that a busy log never
    /// pushes them out of the diagnostics report. Distinct by message and the first line of the
    /// stack; each keeps a count, the first and last time, and the top of its stack. At most
    /// <see cref="MaxDistinct"/>; later distinct ones are only counted.
    /// </summary>
    public sealed class ProblemLog
    {
        /// <summary>Distinct problems kept.</summary>
        public const int MaxDistinct = 10;

        /// <summary>Stack lines kept per problem.</summary>
        public const int StackLines = 6;

        /// <summary>One distinct problem.</summary>
        public sealed class Entry
        {
            /// <summary>"Exception", "Error" or "Assert".</summary>
            public string Kind;

            /// <summary>The message, as logged.</summary>
            public string Message;

            /// <summary>The top <see cref="StackLines"/> lines of the stack.</summary>
            public string Stack;

            /// <summary>Times seen.</summary>
            public int Count;

            /// <summary>Seconds since start, first and last time seen.</summary>
            public float First, Last;
        }

        private readonly List<Entry> _entries = new List<Entry>(MaxDistinct);
        private readonly Dictionary<string, Entry> _byKey = new Dictionary<string, Entry>(MaxDistinct);

        /// <summary>The distinct problems, in the order first seen.</summary>
        public IReadOnlyList<Entry> Entries => _entries;

        /// <summary>Distinct problems seen after the cap was reached, not kept.</summary>
        public int Dropped { get; private set; }

        /// <summary>Records one problem.</summary>
        public void Add(string kind, string message, string stack, float time)
        {
            string top = TopLines(stack ?? string.Empty, StackLines);
            int firstBreak = top.IndexOf('\n');
            string key = kind + "|" + message + "|" + (firstBreak < 0 ? top : top.Substring(0, firstBreak));
            if (_byKey.TryGetValue(key, out Entry entry))
            {
                entry.Count++;
                entry.Last = time;
                return;
            }
            if (_entries.Count >= MaxDistinct)
            {
                Dropped++;
                return;
            }
            entry = new Entry { Kind = kind, Message = message, Stack = top, Count = 1, First = time, Last = time };
            _entries.Add(entry);
            _byKey[key] = entry;
        }

        /// <summary>Empties it.</summary>
        public void Clear()
        {
            _entries.Clear();
            _byKey.Clear();
            Dropped = 0;
        }

        private static string TopLines(string text, int lines)
        {
            int index = 0;
            for (int i = 0; i < lines; i++)
            {
                int next = text.IndexOf('\n', index);
                if (next < 0)
                {
                    return text.TrimEnd();
                }
                index = next + 1;
            }
            return text.Substring(0, index).TrimEnd();
        }
    }
}
