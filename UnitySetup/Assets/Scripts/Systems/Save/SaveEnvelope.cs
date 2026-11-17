namespace Game.Systems
{
    /// <summary>
    /// What <see cref="SaveSystem"/> writes to disk: a schema version and a timestamp around
    /// the game's payload. The save system owns the envelope; the game owns
    /// <typeparamref name="T"/> and its version, and this assembly knows nothing about either.
    /// Serialized with <c>JsonUtility</c>, so <typeparamref name="T"/> must be <c>[Serializable]</c>.
    /// </summary>
    /// <typeparam name="T">The game's save payload type.</typeparam>
    [System.Serializable]
    internal sealed class SaveEnvelope<T>
    {
        /// <summary>The payload's schema version, as passed by the caller. A mismatch on load resets the save.</summary>
        public int version;

        /// <summary>ISO-8601 UTC timestamp of the write.</summary>
        public string lastSavedUtc;

        /// <summary>The game's state.</summary>
        public T payload;
    }
}
