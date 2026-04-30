namespace Game.Systems
{
    /// <summary>Saved ship position and heading.</summary>
    [System.Serializable]
    public struct ShipSaveData
    {
        /// <summary>World-space X position.</summary>
        public float x;

        /// <summary>World-space Y position.</summary>
        public float y;

        /// <summary>World-space Z position.</summary>
        public float z;

        /// <summary>Yaw angle in degrees.</summary>
        public float yawDeg;
    }

    /// <summary>
    /// Top-level save payload. Schema version: 1.
    /// Must stay in sync with the JSON schema in docs/functional-specs.md §9.
    /// </summary>
    [System.Serializable]
    public struct SaveData
    {
        /// <summary>Schema version. A mismatch triggers a save reset.</summary>
        public int version;

        /// <summary>Ship state at the time of save.</summary>
        public ShipSaveData ship;

        /// <summary>ISO-8601 UTC timestamp written by <see cref="SaveSystem"/>.</summary>
        public string lastSavedUtc;

        /// <summary>Current schema version. Increment on any breaking field change.</summary>
        public const int CurrentVersion = 1;

        /// <summary>
        /// Returns a new <see cref="SaveData"/> with zeroed ship state and the current version.
        /// </summary>
        public static SaveData CreateDefault()
        {
            return new SaveData
            {
                version = CurrentVersion,
                ship = default,
                lastSavedUtc = string.Empty,
            };
        }
    }
}
