namespace Game.Gameplay
{
    /// <summary>Saved avatar position and heading.</summary>
    [System.Serializable]
    public struct AvatarSaveData
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
    /// The game's save payload, written by <c>Game.Systems.SaveSystem</c> inside its envelope.
    /// Must stay in sync with the JSON schema in docs/functional-specs.md §9.
    /// </summary>
    [System.Serializable]
    public struct GameSaveData
    {
        /// <summary>
        /// Payload schema version, passed to the save system on every load and save.
        /// Increment on any breaking field change; a mismatch resets the save.
        /// 2: the payload moved inside the save system's envelope (1 had the ship at the top level).
        /// 3: the ship's section became the avatar's, when the game moved to artificial-pirate.
        /// </summary>
        public const int CurrentVersion = 3;

        /// <summary>Avatar state at the time of save.</summary>
        public AvatarSaveData avatar;
    }
}
