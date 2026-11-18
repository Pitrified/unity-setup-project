namespace Game.UI
{
    using System;
    using System.Globalization;
    using Game.Systems;
    using UnityEngine.UIElements;

    /// <summary>
    /// The Diagnostics panel's buttons: Copy (the report), Copy detailed (with the per-second
    /// series), Share (the detailed report to the platform's share sheet), Previous session (the
    /// last session's kept report, to the clipboard), Close. A status line says what happened and
    /// how big it was; the preview shows the report's start. Plain class, built by UISystem.
    /// See docs/systems/logging.md, "Diagnostics report".
    /// </summary>
    public sealed class DiagnosticsPanel
    {
        private const int PreviewLines = 40;
        private const string ShareSubject = "Diagnostics";

        private readonly DiagnosticsRecorder _recorder;
        private readonly Label _status;
        private readonly Label _preview;
        private IDiagnosticsExport _export;

        /// <summary>Raised when Close is pressed.</summary>
        public event Action CloseRequested;

        /// <summary>The status line's text, for tests.</summary>
        public string Status => _status?.text;

        /// <summary>Wires the panel under <paramref name="root"/> to the recorder and the exporter.</summary>
        public DiagnosticsPanel(VisualElement root, DiagnosticsRecorder recorder, IDiagnosticsExport export)
        {
            _recorder = recorder;
            _export = export;
            _status = root.Q<Label>("diag-status");
            _preview = root.Q<Label>("diag-preview");
            Wire(root, "btn-diag-copy", Copy);
            Wire(root, "btn-diag-detailed", CopyDetailed);
            Wire(root, "btn-diag-share", Share);
            Wire(root, "btn-diag-previous", CopyPrevious);
            Wire(root, "btn-diag-close", () => CloseRequested?.Invoke());
        }

        /// <summary>Replaces the exporter (tests).</summary>
        internal void UseExport(IDiagnosticsExport export) => _export = export;

        /// <summary>Called when the panel is shown: a fresh preview, and a status that says what to do.</summary>
        public void Refresh()
        {
            if (_recorder == null)
            {
                SetStatus("no recorder in this scene");
                return;
            }
            string report = _recorder.BuildReport(detailed: false);
            if (_preview != null)
            {
                _preview.text = FirstLines(report, PreviewLines);
            }
            SetStatus($"{Size(report)}: Copy to paste it in a chat, Share to save or send it");
        }

        private void Copy() => Export(detailed: false, share: false);

        private void CopyDetailed() => Export(detailed: true, share: false);

        private void Share() => Export(detailed: true, share: true);

        private void Export(bool detailed, bool share)
        {
            if (_recorder == null)
            {
                return;
            }
            string report = _recorder.BuildReport(detailed);
            if (share)
            {
                _export.Share(ShareSubject, report);
                SetStatus($"shared {Size(report)}");
            }
            else
            {
                _export.Copy(report);
                SetStatus($"copied {Size(report)}{(detailed ? " with the per-second detail" : "")}");
            }
            Log.Info(LogCat.UI, "Diagnostics: {0} {1} characters", share ? "shared" : "copied", report.Length);
        }

        private void CopyPrevious()
        {
            string previous = _recorder != null ? _recorder.PreviousReport : null;
            if (string.IsNullOrEmpty(previous))
            {
                SetStatus("no report from a previous session");
                return;
            }
            _export.Copy(previous);
            SetStatus($"copied the previous session's report, {Size(previous)}");
        }

        private void SetStatus(string text)
        {
            if (_status != null)
            {
                _status.text = text;
            }
        }

        private static void Wire(VisualElement root, string name, Action action)
        {
            Button button = root.Q<Button>(name);
            if (button == null)
            {
                Log.Warn(LogCat.UI, "DiagnosticsPanel: no button '{0}' in the panel.", name);
                return;
            }
            button.clicked += action;
        }

        private static string FirstLines(string text, int lines)
        {
            int index = 0;
            for (int i = 0; i < lines; i++)
            {
                int next = text.IndexOf('\n', index);
                if (next < 0)
                {
                    return text;
                }
                index = next + 1;
            }
            return text.Substring(0, index) + "...";
        }

        private static string Size(string text)
        {
            int lines = 1;
            foreach (char c in text)
            {
                if (c == '\n') lines++;
            }
            return (text.Length / 1024f).ToString("0.0", CultureInfo.InvariantCulture) + " kB, " + lines + " lines";
        }
    }
}
