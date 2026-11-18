namespace Game.Systems
{
    using UnityEngine;

    /// <summary>
    /// Which build this is, for the diagnostics report: the commit and the time, written by the
    /// Editor's build pipeline just before each build ("editor" when running in the Editor).
    /// </summary>
    [CreateAssetMenu(menuName = "unity-setup/Build Info", fileName = "SO_BuildInfo")]
    public sealed class SO_BuildInfo : ScriptableObject
    {
        [SerializeField] private string _commit = "editor";
        [SerializeField] private string _builtAtUtc = "";
        [SerializeField] private string _kind = "editor";
        [SerializeField] private int _versionCode;

        /// <summary>Short commit hash, "+dirty" when the tree had changes; "editor" in the Editor.</summary>
        public string Commit => _commit;

        /// <summary>Build time, ISO 8601 UTC; empty in the Editor.</summary>
        public string BuiltAtUtc => _builtAtUtc;

        /// <summary>"dev", "profile", "release" or "editor".</summary>
        public string Kind => _kind;

        /// <summary>Android version code of the build (not readable at runtime without Java); 0 in the Editor.</summary>
        public int VersionCode => _versionCode;

        /// <summary>Sets the stamp (the build pipeline, before building).</summary>
        public void Stamp(string commit, string builtAtUtc, string kind, int versionCode)
        {
            _commit = commit;
            _builtAtUtc = builtAtUtc;
            _kind = kind;
            _versionCode = versionCode;
        }
    }
}
