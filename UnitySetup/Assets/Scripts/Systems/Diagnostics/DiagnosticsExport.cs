namespace Game.Systems
{
    using System.IO;
    using UnityEngine;

    /// <summary>Where a diagnostics report goes when a person asks for it.</summary>
    public interface IDiagnosticsExport
    {
        /// <summary>Puts the text on the clipboard.</summary>
        void Copy(string text);

        /// <summary>Hands the text to the platform's share sheet (or a file where there is none).</summary>
        void Share(string subject, string text);
    }

    /// <summary>
    /// The real export: the clipboard everywhere; on Android, the share sheet through an
    /// <c>ACTION_SEND</c> text intent (no plugin, no permission), so the report can be saved to
    /// Files or Drive or sent to a chat; elsewhere Share writes the text to a file and logs where.
    /// </summary>
    public sealed class PlatformDiagnosticsExport : IDiagnosticsExport
    {
        /// <inheritdoc />
        public void Copy(string text)
        {
            GUIUtility.systemCopyBuffer = text;
        }

        /// <inheritdoc />
        public void Share(string subject, string text)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var intentClass = new AndroidJavaClass("android.content.Intent");
            using var intent = new AndroidJavaObject("android.content.Intent");
            intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND")).Dispose();
            intent.Call<AndroidJavaObject>("setType", "text/plain").Dispose();
            intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), subject).Dispose();
            intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), text).Dispose();
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            using var chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intent, subject);
            activity.Call("startActivity", chooser);
#else
            string path = Path.Combine(Application.persistentDataPath, "diagnostics", "shared.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text);
            Log.Warn(LogCat.UI, "Diagnostics: no share sheet on this platform; wrote {0}", path);
#endif
        }
    }

    /// <summary>
    /// The report kept on the device: <c>current.txt</c> rewritten while the game runs, and at the
    /// next start moved to <c>previous.txt</c>, so after a crash the next session can still copy or
    /// share what happened before it. Writes go to a temporary file and replace, so a crash during
    /// a write leaves the last good report.
    /// </summary>
    public sealed class DiagnosticsFiles
    {
        private readonly string _folder;

        /// <summary>Files in <paramref name="folder"/>, created if missing.</summary>
        public DiagnosticsFiles(string folder)
        {
            _folder = folder;
            Directory.CreateDirectory(folder);
        }

        /// <summary>The report of the running session.</summary>
        public string CurrentPath => Path.Combine(_folder, "current.txt");

        /// <summary>The report of the session before this one.</summary>
        public string PreviousPath => Path.Combine(_folder, "previous.txt");

        /// <summary>At start: the last session's report becomes the previous one.</summary>
        public void Rotate()
        {
            if (!File.Exists(CurrentPath))
            {
                return;
            }
            if (File.Exists(PreviousPath))
            {
                File.Delete(PreviousPath);
            }
            File.Move(CurrentPath, PreviousPath);
        }

        /// <summary>Writes the running session's report, atomically.</summary>
        public void WriteCurrent(string text)
        {
            string temp = CurrentPath + ".tmp";
            File.WriteAllText(temp, text);
            if (File.Exists(CurrentPath))
            {
                File.Replace(temp, CurrentPath, null);
            }
            else
            {
                File.Move(temp, CurrentPath);
            }
        }

        /// <summary>The previous session's report, or null when there is none.</summary>
        public string ReadPrevious() => File.Exists(PreviousPath) ? File.ReadAllText(PreviousPath) : null;
    }
}
