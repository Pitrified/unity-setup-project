namespace Game.Systems
{
    /// <summary>Contract for reading and writing the player save file.</summary>
    public interface ISaveSystem
    {
        /// <summary>True when a valid save file exists on disk.</summary>
        bool HasSave { get; }

        /// <summary>
        /// Loads and deserializes the save file.
        /// Returns <see cref="SaveData.CreateDefault"/> when no file exists or the file is corrupt.
        /// </summary>
        SaveData Load();

        /// <summary>
        /// Atomically persists <paramref name="data"/> to disk.
        /// The implementation stamps <see cref="SaveData.lastSavedUtc"/> and
        /// <see cref="SaveData.version"/> before writing.
        /// </summary>
        void Save(SaveData data);

        /// <summary>Deletes the save file. Used for debug flows and future "new game" support.</summary>
        void DeleteSave();
    }
}
