namespace Game.Systems
{
    /// <summary>
    /// Contract for reading and writing the player save file.
    /// The file holds a payload type the game owns, with the game's schema version;
    /// the save system adds a timestamp and handles atomic writes, corruption and version mismatches.
    /// </summary>
    public interface ISaveSystem
    {
        /// <summary>True when a save file exists on disk.</summary>
        bool HasSave { get; }

        /// <summary>
        /// Loads and deserializes the payload.
        /// Returns <c>new T()</c> when no file exists, when the file is corrupt, or when its version
        /// is not <paramref name="version"/>; in the last two cases the file is backed up first.
        /// </summary>
        /// <typeparam name="T">The game's <c>[Serializable]</c> payload type.</typeparam>
        /// <param name="version">The payload schema version the caller expects.</param>
        T Load<T>(int version) where T : new();

        /// <summary>
        /// Atomically persists <paramref name="payload"/> with <paramref name="version"/>
        /// and the current UTC time.
        /// </summary>
        /// <typeparam name="T">The game's <c>[Serializable]</c> payload type.</typeparam>
        /// <param name="payload">The state to write.</param>
        /// <param name="version">The payload schema version to stamp.</param>
        void Save<T>(T payload, int version);

        /// <summary>Deletes the save file. Used for debug flows and future "new game" support.</summary>
        void DeleteSave();
    }
}
